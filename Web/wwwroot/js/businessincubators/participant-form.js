// Participant Form Manager
(function () {
    'use strict';

    // Form Manager Class
    class ParticipantFormManager {
        constructor(config) {
            this.config = config;
            this.formStructure = null;
            this.currentBlockIndex = 0;
            this.autoSaveManager = null;
            this.validator = new FormValidator();
            this.isSubmitting = false;
            
            // Initialize draft data if not exists
            if (!draftData) {
                draftData = {
                    formVersion: 1,
                    lastSavedAt: new Date().toISOString(),
                    currentBlockIndex: 0,
                    blockResponses: [],
                    progressPercentage: 0
                };
            } else {
                // Ensure blockResponses is always an array even if draft exists
                if (!draftData.blockResponses) {
                    draftData.blockResponses = [];
                }
                // Use the saved block index if available
                if (typeof draftData.currentBlockIndex === 'number') {
                    this.currentBlockIndex = draftData.currentBlockIndex;
                }
                console.log('Loaded existing draft with', draftData.blockResponses.length, 'block responses');
                console.log('Draft data:', draftData);
            }
        }

        showLoadingSpinner() {
            // Loading spinner is already visible by default in HTML
            // Just ensure wizard is hidden
            const wizardContainer = document.getElementById('wizardContainer');
            if (wizardContainer) {
                wizardContainer.style.display = 'none';
            }
        }

        hideLoadingSpinner() {
            // Hide loading spinner
            const loadingContainer = document.getElementById('formLoadingContainer');
            if (loadingContainer) {
                loadingContainer.style.display = 'none';
            }
            // Show wizard container
            const wizardContainer = document.getElementById('wizardContainer');
            if (wizardContainer) {
                wizardContainer.style.display = 'block';
            }
        }

        async initialize() {
            try {
                // Loading spinner is already visible from HTML
                
                // Load form structure
                await this.loadFormStructure();
                
                // Initialize auto-save
                this.autoSaveManager = new FormAutoSaveManager(this.config);
                this.autoSaveManager.initialize();
                
                // Setup navigation
                this.setupNavigation();
                
                // Hide loading spinner
                this.hideLoadingSpinner();
                
                // Load current block
                this.loadBlock(this.currentBlockIndex);
                
                // Setup event handlers
                this.setupEventHandlers();
                
                // Update progress
                this.updateProgress();
                
                // Update all blocks' completion status on load
                if (this.formStructure && this.formStructure.blocks) {
                    for (let i = 0; i < this.formStructure.blocks.length; i++) {
                        this.updateBlockCompletionStatus(i);
                    }
                }
                
            } catch (error) {
                console.error('Error initializing form:', error);
                showToast('Error al cargar el formulario', 'danger');
            }
        }

        async loadFormStructure() {
            try {
                // Load form structure from API
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/GetFormStructure`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    credentials: 'same-origin' // Include cookies for authentication
                });

                if (!response.ok) {
                    if (response.status === 401) {
                        throw new Error('Su sesión ha expirado. Por favor, vuelva a iniciar sesión.');
                    }
                    throw new Error('Error al cargar la estructura del formulario');
                }

                this.formStructure = await response.json();
                
                // Validate form structure
                if (!this.formStructure || !this.formStructure.blocks || this.formStructure.blocks.length === 0) {
                    throw new Error('El formulario no tiene bloques configurados. Por favor, contacte al administrador.');
                }

                // Render navigation
                this.renderNavigation();
                
                // Re-render navigation and current block after a small delay to ensure feedback data is loaded
                setTimeout(() => {
                    console.log('[Feedback Debug] Re-rendering after delay, feedback data:', window.formFeedback);
                    this.renderNavigation();
                    // Re-render current block to show feedback
                    if (this.currentBlockIndex >= 0) {
                        this.loadBlock(this.currentBlockIndex);
                    }
                }, 500);
                
            } catch (error) {
                throw new Error('Failed to load form structure: ' + error.message);
            }
        }

        renderNavigation() {
            const navContainer = document.getElementById('blockNavigation');
            navContainer.innerHTML = '';
            
            if (!this.formStructure || !this.formStructure.blocks || this.formStructure.blocks.length === 0) {
                navContainer.innerHTML = '<div class="alert alert-warning">No hay bloques disponibles en este formulario.</div>';
                return;
            }

            this.formStructure.blocks.forEach((block, index) => {
                const navItem = document.createElement('a');
                navItem.className = 'nav-link';
                navItem.href = '#';
                navItem.dataset.blockIndex = index;

                // Check if block is completed
                const blockResponse = this.getBlockResponse(block.blockId);
                const isCompleted = this.isBlockCompleted(block, blockResponse);
                
                if (isCompleted) {
                    navItem.classList.add('completed');
                }

                if (index === this.currentBlockIndex) {
                    navItem.classList.add('active');
                }

                // Count feedback for this block
                let feedbackCount = 0;
                if (window.formFeedback) {
                    // Count feedback for all questions in this block
                    block.questions?.forEach(question => {
                        if (window.formFeedback[question.questionId]) {
                            const openFeedback = window.formFeedback[question.questionId].filter(f => f.status === 0);
                            feedbackCount += openFeedback.length;
                        }
                    });
                    // Also check for block-level feedback
                    const blockKey = `block_${block.blockId}`;
                    if (window.formFeedback[blockKey]) {
                        const openFeedback = window.formFeedback[blockKey].filter(f => f.status === 0);
                        feedbackCount += openFeedback.length;
                    }
                }

                navItem.innerHTML = `
                    <span class="nav-item-circle-parent">
                        <span class="nav-item-circle">
                            ${isCompleted ? '<i class="fas fa-check"></i>' : index + 1}
                        </span>
                    </span>
                    <span class="nav-item-title">
                        ${block.blockName}
                        ${feedbackCount > 0 ? `<span class="badge bg-danger ms-2" title="Retroalimentación pendiente"><i class="fas fa-comment-dots"></i> ${feedbackCount}</span>` : ''}
                    </span>
                `;
                
                // Add click handler to navigate to block
                navItem.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.loadBlock(index);
                });

                navContainer.appendChild(navItem);
            });
        }

        loadBlock(index) {
            if (!this.formStructure || !this.formStructure.blocks || this.formStructure.blocks.length === 0) {
                console.error('No blocks available to load');
                return;
            }
            if (index < 0 || index >= this.formStructure.blocks.length) return;

            this.currentBlockIndex = index;
            const block = this.formStructure.blocks[index];
            const blockResponse = this.getBlockResponse(block.blockId);

            // Update navigation
            this.updateNavigation();

            // Render block content
            this.renderBlock(block, blockResponse);

            // Update buttons
            this.updateButtons();

            // Update progress
            this.updateProgress();

            // Scroll to top
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }

        renderBlock(block, blockResponse) {
            const contentContainer = document.getElementById('formContent');
            
            // Update card header with block info
            const blockTitle = document.getElementById('blockTitle');
            const currentBlockNum = document.getElementById('currentBlockNum');
            const totalBlocksNum = document.getElementById('totalBlocksNum');
            
            if (blockTitle) blockTitle.textContent = block.blockName;
            if (currentBlockNum) currentBlockNum.textContent = this.currentBlockIndex + 1;
            if (totalBlocksNum) totalBlocksNum.textContent = this.formStructure.blocks.length;
            
            let html = `
                <div class="form-block-content" data-block-id="${block.blockId}">
                    ${block.description ? `<div class="alert alert-info mb-4"><i class="fas fa-info-circle me-2"></i>${block.description}</div>` : ''}
            `;

            block.questions.forEach(question => {
                const questionResponse = blockResponse?.questionResponses?.find(q => q.questionId === question.questionId);
                html += this.renderQuestion(question, questionResponse);
            });

            html += `
                </div>
            `;

            contentContainer.innerHTML = html;

            // Initialize Choices.js for nationality selects
            this.initializeEnhancedSelects();

            // Show action buttons container by default
            const actionButtons = document.getElementById('actionButtons');
            if (actionButtons) {
                actionButtons.style.display = 'flex';
            }
            
            // Hide save and submit buttons in read-only mode or when submission is not allowed
            if (this.config.isReadOnly || !this.config.canSubmit) {
                const saveDraftBtn = document.getElementById('btnSaveDraft');
                const submitBtn = document.getElementById('btnSubmit');
                if (saveDraftBtn) saveDraftBtn.style.display = 'none';
                if (submitBtn) submitBtn.style.display = 'none';
                
                // If in read-only mode and can't submit, hide the entire action buttons footer
                // except when there's navigation (previous/next buttons)
                if (this.config.isReadOnly && !this.config.canSubmit) {
                    // Check if we have multiple blocks that require navigation
                    if (!this.formStructure || !this.formStructure.blocks || this.formStructure.blocks.length <= 1) {
                        // Hide entire footer if there's only one block (no navigation needed)
                        if (actionButtons) {
                            actionButtons.style.display = 'none';
                        }
                    }
                }
            }

            // Re-initialize auto-save for new inputs (only if not read-only)
            if (!this.config.isReadOnly) {
                this.autoSaveManager.attachListeners();
            }
        }

        renderQuestion(question, questionResponse) {
            // Debug: Log question being rendered and check for feedback
            console.log(`[Feedback Debug] Rendering question ${question.questionId}, has feedback: ${window.formFeedback && window.formFeedback[question.questionId] ? 'YES' : 'NO'}`);
            
            // Determine if question is complete
            const isComplete = questionResponse && questionResponse.isAnswered;
            const completeClass = isComplete ? 'question-complete' : '';
            
            let html = `<div class="form-question question-card ${completeClass}" data-question-id="${question.questionId}">`;
            
            // Add icon based on answer type
            const iconMap = {
                1: 'fa-dot-circle',      // SingleChoice
                2: 'fa-check-square',    // MultiChoice
                3: 'fa-align-left',      // FreeText
                4: 'fa-sort-numeric-up', // Numeric
                5: 'fa-calendar-alt',    // Date
                6: 'fa-link'             // Url
            };
            
            const icon = iconMap[question.answerType] || 'fa-question-circle';
            
            html += `
                <div class="d-flex align-items-start justify-content-between">
                    <div class="flex-grow-1">
                        <label class="question-label">
                            <i class="fas ${icon} me-2 text-primary"></i>
                            ${question.questionText}
                            ${question.isRequired ? '<span class="question-required">*</span>' : ''}
                        </label>
                        ${question.helpText ? `<div class="question-help"><i class="fas fa-info-circle me-1"></i>${question.helpText}</div>` : ''}
                    </div>
                    ${isComplete ? '<span class="question-checkmark animated"><i class="fas fa-check-circle"></i></span>' : ''}
                </div>
            `;

            // Render based on answer type
            switch (question.answerType) {
                case 1: // SingleChoice
                    html += this.renderSingleChoice(question, questionResponse);
                    break;
                case 2: // MultiChoice
                    html += this.renderMultiChoice(question, questionResponse);
                    break;
                case 3: // FreeText
                    html += this.renderTextarea(question, questionResponse);
                    break;
                case 4: // Numeric
                    html += this.renderNumericInput(question, questionResponse);
                    break;
                case 5: // Date
                    html += this.renderDateInput(question, questionResponse);
                    break;
                case 6: // PersonId
                    html += this.renderPersonIdInput(question, questionResponse);
                    break;
                case 7: // IdType
                    html += this.renderIdTypeSelect(question, questionResponse);
                    break;
                case 8: // Gender
                    html += this.renderGenderSelect(question, questionResponse);
                    break;
                case 9: // MaritalStatus
                    html += this.renderMaritalStatusSelect(question, questionResponse);
                    break;
                case 10: // Email
                    html += this.renderEmailInput(question, questionResponse);
                    break;
                case 11: // PhoneNumber
                    html += this.renderPhoneInput(question, questionResponse);
                    break;
                case 12: // Nationality
                    html += this.renderNationalitySelect(question, questionResponse);
                    break;
                default:
                    html += this.renderTextInput(question, questionResponse);
                    break;
            }

            // Add feedback if exists for this question
            if (window.formFeedback && window.formFeedback[question.questionId]) {
                html += this.renderQuestionFeedback(window.formFeedback[question.questionId]);
            }

            html += '</div>';
            return html;
        }

        renderQuestionFeedback(feedbackList) {
            if (!feedbackList || feedbackList.length === 0) return '';
            
            let html = '<div class="mt-3">';
            
            feedbackList.forEach(feedback => {
                const isOpen = feedback.status === 0; // ReviewNeeded = 0, ReviewClosed = 1
                const statusClass = isOpen ? 'border-warning' : 'border-success';
                const statusBadge = isOpen ? 
                    '<span class="badge bg-warning">Requiere Revisión</span>' : 
                    '<span class="badge bg-success">Cerrado</span>';
                
                html += `
                    <div class="question-feedback-box">
                        <div class="feedback-header mb-2 d-flex justify-content-between align-items-center">
                            <div>
                                <i class="fas fa-comment-dots text-warning me-2"></i>
                                <strong>Retroalimentación</strong>
                            </div>
                            ${statusBadge}
                        </div>
                        <div class="feedback-messages">`;
                
                // Original feedback
                html += `
                    <div class="feedback-message mb-2 p-2 rounded ${feedback.isFromParticipant ? 'bg-primary-subtle' : 'bg-warning-subtle'}">
                        <div class="feedback-author small">
                            <strong>${this.escapeHtml(feedback.authorName)}</strong>
                            <span class="text-muted ms-2">
                                <i class="far fa-clock"></i> ${new Date(feedback.createdAt).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                            </span>
                        </div>
                        <div class="feedback-content mt-1">
                            ${this.escapeHtml(feedback.feedbackText)}
                        </div>
                    </div>`;
                
                // Replies
                if (feedback.replies && feedback.replies.length > 0) {
                    feedback.replies.forEach(reply => {
                        html += `
                            <div class="feedback-message mb-2 ms-3 p-2 rounded ${reply.isFromParticipant ? 'bg-primary-subtle' : 'bg-warning-subtle'}">
                                <div class="feedback-author small">
                                    <strong>${this.escapeHtml(reply.authorName)}</strong>
                                    <span class="text-muted ms-2">
                                        <i class="far fa-clock"></i> ${new Date(reply.createdAt).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                                    </span>
                                </div>
                                <div class="feedback-content mt-1">
                                    ${this.escapeHtml(reply.feedbackText)}
                                </div>
                            </div>`;
                    });
                }
                
                html += '</div>'; // Close feedback-messages
                
                // Add reply form if feedback is open and feedback is not read-only
                // Use feedbackReadOnly instead of isReadOnly to allow feedback interaction on submitted forms
                if (isOpen && !this.config.feedbackReadOnly) {
                    html += `
                        <div class="feedback-actions mt-2">
                            <div class="input-group input-group-sm">
                                <input type="text" 
                                       class="form-control feedback-reply-input" 
                                       placeholder="Escribir respuesta..." 
                                       data-feedback-id="${feedback.id}" />
                                <button class="btn btn-primary btn-send-reply" type="button" data-feedback-id="${feedback.id}">
                                    <i class="fas fa-paper-plane"></i> Enviar
                                </button>
                            </div>
                        </div>`;
                }
                
                html += '</div>'; // Close question-feedback-box
            });
            
            html += '</div>';
            return html;
        }

        renderTextInput(question, response) {
            const value = response?.answer || '';
            const disabled = this.config.isReadOnly ? 'disabled' : '';
            return `
                <input type="text" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="text"
                       ${question.isRequired ? 'required' : ''}
                       value="${this.escapeHtml(value)}"
                       placeholder="Su respuesta aquí"
                       ${disabled}>
            `;
        }

        renderSingleChoice(question, response) {
            let html = '<div class="radio-group" data-question-id="' + question.questionId + '">';
            
            question.answerOptions.forEach(option => {
                const isChecked = response?.answer === option.answerOptionId.toString();
                const inputId = `q_${question.questionId}_opt_${option.answerOptionId}`;
                
                html += `
                    <div class="form-check">
                        <input class="form-check-input" 
                               type="radio" 
                               name="q_${question.questionId}" 
                               id="${inputId}"
                               value="${option.answerOptionId}"
                               data-question-id="${question.questionId}"
                               data-answer-type="single"
                               ${question.isRequired ? 'required' : ''}
                               ${isChecked ? 'checked' : ''}>
                        <label class="form-check-label" for="${inputId}">
                            ${this.escapeHtml(option.optionText)}
                        </label>
                    </div>
                `;
            });
            
            html += '</div>';
            return html;
        }

        renderMultiChoice(question, response) {
            let html = `<div class="checkbox-group" data-question-id="${question.questionId}" ${question.isRequired ? 'data-required="true"' : ''}>`;
            
            question.answerOptions.forEach(option => {
                const answerIds = response?.answer ? response.answer.split(',').map(id => parseInt(id)) : [];
                const isChecked = answerIds.includes(option.answerOptionId);
                const inputId = `q_${question.questionId}_opt_${option.answerOptionId}`;
                
                html += `
                    <div class="form-check">
                        <input class="form-check-input" 
                               type="checkbox" 
                               id="${inputId}"
                               value="${option.answerOptionId}"
                               data-question-id="${question.questionId}"
                               data-answer-type="multi"
                               ${isChecked ? 'checked' : ''}>
                        <label class="form-check-label" for="${inputId}">
                            ${this.escapeHtml(option.optionText)}
                        </label>
                    </div>
                `;
            });
            
            html += '</div>';
            return html;
        }

        renderTextarea(question, response) {
            const value = response?.answer || '';
            return `
                <textarea class="form-control" 
                          id="q_${question.questionId}"
                          data-question-id="${question.questionId}"
                          data-answer-type="textarea"
                          rows="4"
                          ${question.isRequired ? 'required' : ''}
                          placeholder="Su respuesta detallada aquí">${this.escapeHtml(value)}</textarea>
            `;
        }

        renderNumericInput(question, response) {
            const value = response?.answer || '';
            return `
                <input type="number" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="numeric"
                       ${question.isRequired ? 'required' : ''}
                       value="${value}"
                       min="0"
                       step="1"
                       placeholder="0">
            `;
        }

        renderDateInput(question, response) {
            const value = response?.answer ? response.answer.split('T')[0] : '';
            return `
                <input type="date" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="date"
                       ${question.isRequired ? 'required' : ''}
                       value="${value}">
            `;
        }

        renderPersonIdInput(question, response) {
            const value = response?.answer || '';
            return `
                <input type="text" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="text"
                       ${question.isRequired ? 'required' : ''}
                       value="${this.escapeHtml(value)}"
                       pattern="[0-9]{6,15}"
                       placeholder="Número de identificación">
            `;
        }

        renderIdTypeSelect(question, response) {
            const value = response?.answer || '';
            const { FormUtils } = window.FormConstants || {};
            
            if (!FormUtils) {
                console.error('FormConstants module not loaded');
                return '<p class="text-danger">Error: FormConstants module not loaded</p>';
            }
            
            return `<select class="form-select" 
                           id="q_${question.questionId}"
                           data-question-id="${question.questionId}"
                           data-answer-type="text"
                           ${question.isRequired ? 'required' : ''}>
                       ${FormUtils.renderSelectOptions('idType', value)}
                   </select>`;
        }

        renderGenderSelect(question, response) {
            const value = response?.answer || '';
            const { FormUtils } = window.FormConstants || {};
            
            if (!FormUtils) {
                console.error('FormConstants module not loaded');
                return '<p class="text-danger">Error: FormConstants module not loaded</p>';
            }
            
            return `<select class="form-select" 
                           id="q_${question.questionId}"
                           data-question-id="${question.questionId}"
                           data-answer-type="text"
                           ${question.isRequired ? 'required' : ''}>
                       ${FormUtils.renderSelectOptions('gender', value)}
                   </select>`;
        }

        renderMaritalStatusSelect(question, response) {
            const value = response?.answer || '';
            const { FormUtils } = window.FormConstants || {};
            
            if (!FormUtils) {
                console.error('FormConstants module not loaded');
                return '<p class="text-danger">Error: FormConstants module not loaded</p>';
            }
            
            return `<select class="form-select" 
                           id="q_${question.questionId}"
                           data-question-id="${question.questionId}"
                           data-answer-type="text"
                           ${question.isRequired ? 'required' : ''}>
                       ${FormUtils.renderSelectOptions('maritalStatus', value)}
                   </select>`;
        }

        renderEmailInput(question, response) {
            const value = response?.answer || '';
            return `
                <input type="email" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="text"
                       ${question.isRequired ? 'required' : ''}
                       value="${this.escapeHtml(value)}"
                       placeholder="correo@ejemplo.com">
            `;
        }

        renderPhoneInput(question, response) {
            const value = response?.answer || '';
            return `
                <input type="tel" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="text"
                       ${question.isRequired ? 'required' : ''}
                       value="${this.escapeHtml(value)}"
                       pattern="[0-9]{7,15}"
                       placeholder="Número de teléfono">
            `;
        }

        renderNationalitySelect(question, response) {
            const value = response?.answer || '';
            const { FormUtils } = window.FormConstants || {};
            
            if (!FormUtils) {
                console.error('FormConstants module not loaded');
                return '<p class="text-danger">Error: FormConstants module not loaded</p>';
            }
            
            return `<select class="form-select nationality-select" 
                           id="q_${question.questionId}"
                           data-question-id="${question.questionId}"
                           data-answer-type="text"
                           ${question.isRequired ? 'required' : ''}>
                       ${FormUtils.renderSelectOptions('nationality', value)}
                   </select>`;
        }

        initializeEnhancedSelects() {
            // Check if Choices.js is available
            if (typeof Choices === 'undefined') {
                console.warn('Choices.js library not loaded');
                return;
            }

            // Initialize all nationality selects
            const nationalitySelects = document.querySelectorAll('.nationality-select');
            nationalitySelects.forEach(select => {
                // Check if not already initialized
                if (!select.hasAttribute('data-choice')) {
                    try {
                        const choices = new Choices(select, {
                            searchEnabled: true,
                            searchPlaceholderValue: 'Buscar país...',
                            noResultsText: 'No se encontraron resultados',
                            itemSelectText: 'Presione para seleccionar',
                            shouldSort: false,
                            position: 'auto',  // Changed from 'bottom' to 'auto' for smart positioning
                            removeItemButton: false,
                            placeholder: true,
                            placeholderValue: 'Seleccione nacionalidad',
                            shouldSortItems: false,
                            renderChoiceLimit: -1  // Show all choices
                        });
                        
                        // Mark as initialized
                        select.setAttribute('data-choice', 'initialized');
                        
                        // Re-attach change listener for auto-save
                        if (!this.config.isReadOnly) {
                            select.addEventListener('change', () => {
                                if (this.autoSaveManager) {
                                    this.autoSaveManager.onFieldChange(select);
                                    this.autoSaveManager.scheduleSave();
                                }
                            });
                        }
                    } catch (error) {
                        console.error('Error initializing Choices.js for select:', error);
                    }
                }
            });
        }

        setupNavigation() {
            // Block navigation clicks
            document.getElementById('blockNavigation').addEventListener('click', (e) => {
                e.preventDefault();
                const navLink = e.target.closest('.nav-link');
                if (navLink) {
                    const index = parseInt(navLink.dataset.blockIndex);
                    if (!isNaN(index)) {
                        // ISSUE 3 FIX: Validate current block before allowing navigation
                        if (index > this.currentBlockIndex && !this.validateCurrentBlock()) {
                            // Don't allow forward navigation if current block has validation errors
                            return;
                        }
                        // Save current block before navigating
                        this.saveCurrentBlock();
                        this.updateBlockCompletionStatus(this.currentBlockIndex);
                        this.loadBlock(index);
                    }
                }
            });
        }

        setupEventHandlers() {
            // Previous button
            document.getElementById('btnPrevious').addEventListener('click', () => {
                if (this.currentBlockIndex > 0) {
                    this.saveCurrentBlock();
                    this.updateBlockCompletionStatus(this.currentBlockIndex);
                    this.loadBlock(this.currentBlockIndex - 1);
                }
            });

            // Next button
            document.getElementById('btnNext').addEventListener('click', () => {
                if (this.validateCurrentBlock()) {
                    this.saveCurrentBlock();
                    this.updateBlockCompletionStatus(this.currentBlockIndex);
                    if (this.currentBlockIndex < this.formStructure.blocks.length - 1) {
                        this.loadBlock(this.currentBlockIndex + 1);
                    }
                }
            });

            // Save draft button
            document.getElementById('btnSaveDraft').addEventListener('click', async () => {
                this.saveCurrentBlock();
                await this.autoSaveManager.save();
                showToast('Borrador guardado exitosamente', 'success');
            });

            // Submit button
            document.getElementById('btnSubmit').addEventListener('click', async () => {
                if (this.isSubmitting) return;
                
                if (this.validateCurrentBlock() && this.validateAllBlocks()) {
                    this.saveCurrentBlock();
                    await this.submitForm();
                }
            });

            // Setup feedback event handlers using event delegation
            this.setupFeedbackHandlers();
        }

        setupFeedbackHandlers() {
            // Use event delegation for dynamically added feedback elements
            document.addEventListener('click', async (e) => {
                // Handle reply button
                if (e.target.closest('.btn-send-reply')) {
                    const button = e.target.closest('.btn-send-reply');
                    const feedbackId = button.dataset.feedbackId;
                    const input = document.querySelector(`.feedback-reply-input[data-feedback-id="${feedbackId}"]`);
                    
                    if (input && input.value.trim()) {
                        await this.sendFeedbackReply(feedbackId, input.value.trim());
                        input.value = '';
                    }
                }
                
                // Handle close feedback button
                if (e.target.closest('.btn-close-feedback')) {
                    const button = e.target.closest('.btn-close-feedback');
                    const feedbackId = button.dataset.feedbackId;
                    await this.closeFeedback(feedbackId);
                }
                
                // Handle reopen feedback button
                if (e.target.closest('.btn-reopen-feedback')) {
                    const button = e.target.closest('.btn-reopen-feedback');
                    const feedbackId = button.dataset.feedbackId;
                    await this.reopenFeedback(feedbackId);
                }
            });
            
            // Handle enter key in reply input
            document.addEventListener('keypress', async (e) => {
                if (e.key === 'Enter' && e.target.classList.contains('feedback-reply-input')) {
                    const feedbackId = e.target.dataset.feedbackId;
                    const button = document.querySelector(`.btn-send-reply[data-feedback-id="${feedbackId}"]`);
                    if (button) {
                        button.click();
                    }
                }
            });
        }

        async sendFeedbackReply(parentFeedbackId, feedbackText) {
            try {
                const formData = new FormData();
                formData.append('parentFeedbackId', parentFeedbackId);
                formData.append('feedbackText', feedbackText);
                
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/ReplyToFeedback`, {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: formData
                });
                
                if (response.ok) {
                    showToast('Respuesta enviada exitosamente', 'success');
                    // Reload the page to show updated feedback
                    window.location.reload();
                } else {
                    showToast('Error al enviar la respuesta', 'error');
                }
            } catch (error) {
                console.error('Error sending feedback reply:', error);
                showToast('Error al enviar la respuesta', 'error');
            }
        }

        async closeFeedback(feedbackId) {
            try {
                const formData = new FormData();
                formData.append('feedbackId', feedbackId);
                
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/CloseFeedback`, {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: formData
                });
                
                if (response.ok) {
                    showToast('Retroalimentación cerrada', 'success');
                    window.location.reload();
                } else {
                    showToast('Error al cerrar la retroalimentación', 'error');
                }
            } catch (error) {
                console.error('Error closing feedback:', error);
                showToast('Error al cerrar la retroalimentación', 'error');
            }
        }

        async reopenFeedback(feedbackId) {
            try {
                const formData = new FormData();
                formData.append('feedbackId', feedbackId);
                
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/ReopenFeedback`, {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: formData
                });
                
                if (response.ok) {
                    showToast('Retroalimentación reabierta', 'success');
                    window.location.reload();
                } else {
                    showToast('Error al reabrir la retroalimentación', 'error');
                }
            } catch (error) {
                console.error('Error reopening feedback:', error);
                showToast('Error al reabrir la retroalimentación', 'error');
            }
        }

        validateCurrentBlock() {
            const block = this.formStructure.blocks[this.currentBlockIndex];
            const errors = this.validator.validateBlock(document.querySelector(`[data-block-id="${block.blockId}"]`));
            
            if (errors.length > 0) {
                // Show first error
                const firstError = errors[0];
                firstError.field.classList.add('is-invalid');
                firstError.field.focus();
                showToast(firstError.message, 'danger', 'Error de validación');
                return false;
            }
            
            return true;
        }

        validateAllBlocks() {
            // Check if all required questions are answered
            let allValid = true;
            let firstIncompleteBlock = null;

            this.formStructure.blocks.forEach((block, index) => {
                const blockResponse = this.getBlockResponse(block.blockId);
                const requiredQuestions = block.questions.filter(q => q.isRequired);
                
                requiredQuestions.forEach(question => {
                    const questionResponse = blockResponse?.questionResponses?.find(q => q.questionId === question.questionId);
                    if (!questionResponse || !questionResponse.isAnswered) {
                        allValid = false;
                        if (firstIncompleteBlock === null) {
                            firstIncompleteBlock = index;
                        }
                    }
                });
            });

            if (!allValid) {
                showToast('Por favor complete todas las preguntas requeridas antes de enviar', 'danger', 'Formulario incompleto');
                if (firstIncompleteBlock !== null) {
                    this.loadBlock(firstIncompleteBlock);
                }
            }

            return allValid;
        }

        saveCurrentBlock() {
            const block = this.formStructure.blocks[this.currentBlockIndex];
            const blockElement = document.querySelector(`[data-block-id="${block.blockId}"]`);
            
            if (!blockElement) {
                console.warn('Block element not found for blockId:', block.blockId);
                return;
            }

            // Update draft metadata
            draftData.currentBlockIndex = this.currentBlockIndex;
            draftData.lastSavedAt = new Date().toISOString();

            // Get or create block response
            let blockResponse = this.getBlockResponse(block.blockId);
            if (!blockResponse) {
                blockResponse = {
                    blockId: block.blockId,
                    blockName: block.blockName,
                    questionResponses: [],
                    isCompleted: false
                };
                draftData.blockResponses.push(blockResponse);
            }

            // Clear existing responses to avoid duplicates
            blockResponse.questionResponses = [];

            // Save each question
            block.questions.forEach(question => {
                const response = this.collectQuestionResponse(question);
                if (response) {
                    blockResponse.questionResponses.push(response);
                    console.log(`Collected response for question ${question.questionId}:`, response);
                }
            });

            // Mark block as completed if all required questions are answered
            const requiredQuestions = block.questions.filter(q => q.isRequired);
            const answeredRequired = requiredQuestions.every(q => {
                const response = blockResponse.questionResponses.find(r => r.questionId === q.questionId);
                return response && response.isAnswered;
            });
            blockResponse.isCompleted = answeredRequired;

            // Update progress
            this.updateProgressPercentage();
            
            console.log('Block saved. Total responses:', blockResponse.questionResponses.length);
            console.log('Current draftData:', JSON.stringify(draftData, null, 2));
        }

        collectQuestionResponse(question) {
            const response = {
                questionId: question.questionId,
                questionText: question.questionText,
                answerType: question.answerType,
                answer: null,
                followUpAnswer: null,
                isAnswered: false,
                moduleInfo: question.moduleInfo,
                topicInfo: question.topicInfo
            };

            console.log(`Collecting response for question ${question.questionId}, type: ${question.answerType}`);

            switch (question.answerType) {
                case 1: // SingleChoice
                    const selectedRadio = document.querySelector(`input[name="q_${question.questionId}"]:checked`);
                    console.log(`Looking for: input[name="q_${question.questionId}"]:checked, found:`, selectedRadio);
                    if (selectedRadio) {
                        response.answer = selectedRadio.value;
                        response.isAnswered = true;
                    }
                    break;

                case 2: // MultiChoice
                    const checkedBoxes = document.querySelectorAll(`input[data-question-id="${question.questionId}"]:checked`);
                    if (checkedBoxes.length > 0) {
                        response.answer = Array.from(checkedBoxes).map(cb => cb.value).join(',');
                        response.isAnswered = true;
                    }
                    break;

                case 3: // FreeText
                    const textarea = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="textarea"]`);
                    if (textarea && textarea.value.trim()) {
                        response.answer = textarea.value.trim();
                        response.isAnswered = true;
                        console.log(`Collected textarea value for question ${question.questionId}:`, response.answer);
                    }
                    break;

                case 6: // PersonId
                case 7: // IdType
                case 8: // Gender
                case 9: // MaritalStatus
                case 10: // Email
                case 11: // PhoneNumber
                case 12: // Nationality
                    const textInput = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="text"]`);
                    if (textInput && textInput.value.trim()) {
                        response.answer = textInput.value.trim();
                        response.isAnswered = true;
                    }
                    break;

                case 4: // Numeric
                    const numericInput = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="numeric"]`);
                    if (numericInput && numericInput.value) {
                        response.answer = numericInput.value;
                        response.isAnswered = true;
                    }
                    break;

                case 5: // Date
                    const dateInput = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="date"]`);
                    if (dateInput && dateInput.value) {
                        response.answer = dateInput.value;
                        response.isAnswered = true;
                    }
                    break;
            }

            return response.isAnswered ? response : null;
        }

        getBlockResponse(blockId) {
            return draftData.blockResponses.find(b => b.blockId === blockId);
        }

        isBlockCompleted(block, blockResponse) {
            if (!blockResponse) return false;
            
            const requiredQuestions = block.questions.filter(q => q.isRequired);
            if (requiredQuestions.length === 0) return true;

            return requiredQuestions.every(question => {
                const response = blockResponse.questionResponses.find(r => r.questionId === question.questionId);
                return response && response.isAnswered;
            });
        }

        updateNavigation() {
            document.querySelectorAll('#blockNavigation .nav-link').forEach((link, index) => {
                link.classList.toggle('active', index === this.currentBlockIndex);
            });
        }

        updateButtons() {
            const btnPrevious = document.getElementById('btnPrevious');
            const btnNext = document.getElementById('btnNext');
            const btnSubmit = document.getElementById('btnSubmit');

            // Previous button
            btnPrevious.disabled = this.currentBlockIndex === 0;

            // Next/Submit buttons
            const isLastBlock = this.currentBlockIndex === this.formStructure.blocks.length - 1;
            
            if (isLastBlock) {
                btnNext.style.display = 'none';
                // Only show submit button if submission is allowed
                if (this.config.canSubmit && !this.config.isReadOnly) {
                    btnSubmit.style.display = 'inline-block';
                } else {
                    btnSubmit.style.display = 'none';
                }
            } else {
                btnNext.style.display = 'inline-block';
                btnSubmit.style.display = 'none';
            }
        }

        updateProgress() {
            const percentage = draftData.progressPercentage || 0;
            
            // Animate progress bar with smooth transition
            const progressBar = document.getElementById('progressBar');
            progressBar.style.width = percentage + '%';
            
            // Update percentage with animation
            const percentageElement = document.getElementById('progressPercentage');
            percentageElement.textContent = Math.round(percentage) + '%';
            
            // Milestone indicators removed to avoid visual confusion
            
            // Update progress bar color based on completion
            if (percentage >= 100) {
                progressBar.className = 'progress-bar bg-success';
            } else if (percentage >= 75) {
                progressBar.className = 'progress-bar bg-info';
            } else if (percentage >= 50) {
                progressBar.className = 'progress-bar bg-primary';
            } else {
                progressBar.className = 'progress-bar bg-primary';
            }
            
            // Progress text has been removed from UI, no need to update it
        }

        addMilestoneIndicators() {
            const progressWrapper = document.querySelector('.progress-wrapper');
            if (!progressWrapper || progressWrapper.querySelector('.milestone-indicators')) return;
            
            const milestonesContainer = document.createElement('div');
            milestonesContainer.className = 'milestone-indicators';
            milestonesContainer.style.cssText = 'position: relative; height: 20px; margin-top: -10px;';
            
            const milestones = [25, 50, 75, 100];
            milestones.forEach(milestone => {
                const marker = document.createElement('div');
                marker.className = 'milestone-marker';
                marker.style.cssText = `
                    position: absolute;
                    left: ${milestone}%;
                    transform: translateX(-50%);
                    text-align: center;
                `;
                marker.innerHTML = `
                    <div style="width: 2px; height: 10px; background: var(--phoenix-gray-400); margin: 0 auto;"></div>
                    <div style="font-size: 0.7rem; color: var(--phoenix-gray-600); margin-top: 2px;">${milestone}%</div>
                `;
                milestonesContainer.appendChild(marker);
            });
            
            progressWrapper.appendChild(milestonesContainer);
        }

        // ISSUE 1 FIX: Update block completion checkmark in real-time
        updateBlockCompletionStatus(blockIndex) {
            if (!this.formStructure || !this.formStructure.blocks) return;
            
            const block = this.formStructure.blocks[blockIndex];
            if (!block) return;
            
            const blockResponse = this.getBlockResponse(block.blockId);
            const navItem = document.querySelector(`[data-block-index="${blockIndex}"]`);
            
            if (!navItem) return;
            
            // Check if all required questions in block are answered
            let isComplete = true;
            for (const question of block.questions) {
                if (question.isRequired) {
                    const response = blockResponse?.questionResponses?.find(r => r.questionId === question.questionId);
                    if (!response || !response.isAnswered) {
                        isComplete = false;
                        break;
                    }
                }
            }
            
            // Update visual state - only add/remove completed class, no checkmark
            if (isComplete) {
                navItem.classList.add('completed');
            } else {
                navItem.classList.remove('completed');
            }
        }

        updateProgressPercentage() {
            let totalQuestions = 0;
            let answeredQuestions = 0;

            this.formStructure.blocks.forEach(block => {
                const blockResponse = this.getBlockResponse(block.blockId);
                
                block.questions.forEach(question => {
                    if (question.isRequired) {
                        totalQuestions++;
                        const response = blockResponse?.questionResponses?.find(r => r.questionId === question.questionId);
                        if (response && response.isAnswered) {
                            answeredQuestions++;
                        }
                    }
                });
            });

            draftData.progressPercentage = totalQuestions > 0 
                ? Math.round((answeredQuestions / totalQuestions) * 100)
                : 0;
        }

        animateQuestionCompletion(questionDiv) {
            if (!questionDiv) return;
            
            const questionId = questionDiv.dataset.questionId;
            const block = this.formStructure.blocks[this.currentBlockIndex];
            const question = block.questions.find(q => q.questionId === parseInt(questionId));
            
            if (!question) return;
            
            // Check if question is answered
            const response = this.collectQuestionResponse(question);
            const isAnswered = response && response.isAnswered;
            
            if (isAnswered) {
                // Add completion class with animation
                if (!questionDiv.classList.contains('question-complete')) {
                    questionDiv.classList.add('question-complete');
                    
                    // Check if checkmark already exists
                    let checkmark = questionDiv.querySelector('.question-checkmark');
                    if (!checkmark) {
                        // Create and add checkmark with animation
                        const checkmarkHtml = '<span class="question-checkmark"><i class="fas fa-check-circle"></i></span>';
                        const flexContainer = questionDiv.querySelector('.d-flex');
                        if (flexContainer) {
                            flexContainer.insertAdjacentHTML('beforeend', checkmarkHtml);
                            checkmark = questionDiv.querySelector('.question-checkmark');
                            
                            // Trigger animation after a brief delay
                            setTimeout(() => {
                                if (checkmark) checkmark.classList.add('animated');
                            }, 10);
                        }
                    }
                }
            } else {
                // Remove completion state if question is cleared
                questionDiv.classList.remove('question-complete');
                const checkmark = questionDiv.querySelector('.question-checkmark');
                if (checkmark) {
                    checkmark.classList.remove('animated');
                    setTimeout(() => checkmark.remove(), 300);
                }
            }
        }

        async submitForm() {
            if (this.isSubmitting) return;
            
            // Confirm submission using Bootstrap modal
            const confirmed = await this.showConfirmModal(
                '¿Enviar formulario?',
                'Una vez enviado, no podrá realizar más cambios hasta que sea revisado.',
                'Sí, enviar',
                'Cancelar'
            );

            if (!confirmed) return;

            this.isSubmitting = true;
            
            try {
                // First save the draft
                await this.autoSaveManager.save();

                // Then submit
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/Submit`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-TOKEN': document.querySelector('[name="__RequestVerificationToken"]').value
                    },
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        projectId: this.config.projectIdNumeric,
                        submissionId: this.config.submissionId || 0,
                        isOnBehalf: this.config.isOnBehalf === true || this.config.isOnBehalf === 'true',
                        participantUserId: this.config.participantUserId || null
                    })
                });

                if (response.ok) {
                    const result = await response.json();
                    
                    // Show success message
                    showToast(
                        result.message || 'Su formulario ha sido enviado exitosamente.',
                        'success',
                        '¡Formulario enviado!'
                    );

                    // Redirect after a short delay to let user see the success message
                    setTimeout(() => {
                        window.location.href = result.redirectUrl || '/';
                    }, 1500);
                } else {
                    const error = await response.json();
                    throw new Error(error.message || 'Error al enviar el formulario');
                }
            } catch (error) {
                console.error('Error submitting form:', error);
                showToast(error.message || 'Error al enviar el formulario', 'danger');
            } finally {
                this.isSubmitting = false;
            }
        }

        escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }

        // Bootstrap modal confirmation dialog
        showConfirmModal(title, message, confirmText = 'Confirmar', cancelText = 'Cancelar') {
            return new Promise((resolve) => {
                // Create modal HTML
                const modalHtml = `
                    <div class="modal fade" id="confirmModal" tabindex="-1">
                        <div class="modal-dialog modal-dialog-centered">
                            <div class="modal-content">
                                <div class="modal-header phoenix-gradient">
                                    <h5 class="modal-title text-white">${title}</h5>
                                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    <p>${message}</p>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
                                    <button type="button" class="btn btn-success" id="confirmBtn">${confirmText}</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                // Remove any existing modal
                const existingModal = document.getElementById('confirmModal');
                if (existingModal) {
                    existingModal.remove();
                }
                
                // Add modal to body
                document.body.insertAdjacentHTML('beforeend', modalHtml);
                
                const modalElement = document.getElementById('confirmModal');
                const modal = new bootstrap.Modal(modalElement);
                
                // Handle confirm button
                document.getElementById('confirmBtn').addEventListener('click', () => {
                    modal.hide();
                    resolve(true);
                });
                
                // Handle modal hidden event
                modalElement.addEventListener('hidden.bs.modal', () => {
                    modalElement.remove();
                    resolve(false);
                });
                
                modal.show();
            });
        }
    }

    // Auto-save Manager Class
    class FormAutoSaveManager {
        constructor(config) {
            this.config = config;
            this.saveTimeout = null;
            this.isSaving = false;
            this.lastSavedData = null;
        }

        initialize() {
            this.attachListeners();
            
            // Save on page unload (only if not in read-only mode)
            if (!this.config.isReadOnly) {
                window.addEventListener('beforeunload', (e) => {
                    if (this.hasUnsavedChanges()) {
                        e.preventDefault();
                        e.returnValue = '¿Está seguro de que desea salir? Los cambios no guardados se perderán.';
                    }
                });
            }

            // Auto-save every 30 seconds (only if not in read-only mode)
            if (!this.config.isReadOnly) {
                setInterval(() => {
                    if (this.hasUnsavedChanges()) {
                        this.save();
                    }
                }, 30000);
            }
        }

        attachListeners() {
            // Skip attaching listeners in read-only mode
            if (this.config.isReadOnly) {
                return;
            }
            
            document.querySelectorAll('input, textarea, select').forEach(element => {
                // ISSUE 2 FIX: Update progress on every change
                element.addEventListener('change', () => {
                    this.onFieldChange(element);
                    this.scheduleSave();
                });
                if (element.type === 'text' || element.tagName === 'TEXTAREA') {
                    element.addEventListener('input', () => {
                        this.onFieldChange(element);
                        this.scheduleSave();
                    });
                }
            });
        }

        // ISSUE 2 FIX: Real-time progress update when fields change
        onFieldChange(element) {
            // Get the form manager instance
            if (window.formManager) {
                // Save current block data immediately
                window.formManager.saveCurrentBlock();
                // Update progress percentage
                window.formManager.updateProgressPercentage();
                // Update visual progress
                window.formManager.updateProgress();
                // Update block completion status
                window.formManager.updateBlockCompletionStatus(window.formManager.currentBlockIndex);
                
                // Animate question completion if element is provided
                if (element) {
                    const questionDiv = element.closest('.form-question');
                    if (questionDiv) {
                        window.formManager.animateQuestionCompletion(questionDiv);
                    }
                }
            }
        }

        scheduleSave() {
            clearTimeout(this.saveTimeout);
            this.saveTimeout = setTimeout(() => this.save(), 2000);
        }

        async save() {
            if (this.isSaving) return;
            
            const currentData = JSON.stringify(draftData);
            if (currentData === this.lastSavedData) {
                return; // No changes
            }

            this.isSaving = true;
            this.showSavingIndicator();

            try {
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/SaveDraft`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-TOKEN': document.querySelector('[name="__RequestVerificationToken"]').value
                    },
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        projectId: this.config.projectIdNumeric,
                        submissionId: this.config.submissionId || 0,
                        draftData: this.convertToPascalCase(draftData),
                        isOnBehalf: this.config.isOnBehalf === true || this.config.isOnBehalf === 'true',
                        participantUserId: this.config.participantUserId || null
                    })
                });

                if (response.ok) {
                    const result = await response.json();
                    // Update submission ID if this was the first save
                    if (result.submissionId && (!this.config.submissionId || this.config.submissionId === 0)) {
                        this.config.submissionId = result.submissionId;
                    }
                    this.lastSavedData = currentData;
                    this.showSavedIndicator();
                } else {
                    this.showErrorIndicator();
                }
            } catch (error) {
                console.error('Error saving draft:', error);
                this.showErrorIndicator();
            } finally {
                this.isSaving = false;
            }
        }

        hasUnsavedChanges() {
            return JSON.stringify(draftData) !== this.lastSavedData;
        }

        showSavingIndicator() {
            // Don't show notification for saving - too frequent
        }

        showSavedIndicator() {
            // Only show success notification for saved
            window.showToast('Cambios guardados correctamente', 'success');
        }

        showErrorIndicator() {
            // Use regular toast notification for error
            window.showToast('Error al guardar los cambios', 'error');
        }

        // Convert camelCase object to PascalCase for C# compatibility
        convertToPascalCase(obj) {
            if (obj === null || obj === undefined) return obj;
            if (Array.isArray(obj)) {
                return obj.map(item => this.convertToPascalCase(item));
            }
            if (typeof obj !== 'object' || obj instanceof Date) return obj;

            const result = {};
            for (const key in obj) {
                if (obj.hasOwnProperty(key)) {
                    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
                    result[pascalKey] = this.convertToPascalCase(obj[key]);
                }
            }
            return result;
        }
    }

    // Form Validator Class
    class FormValidator {
        validateBlock(blockElement) {
            const errors = [];
            const requiredFields = blockElement.querySelectorAll('[required]');
            
            requiredFields.forEach(field => {
                if (!this.isFieldValid(field)) {
                    errors.push({
                        field: field,
                        message: field.dataset.errorMessage || 'Este campo es requerido'
                    });
                }
            });

            // Check multi-choice groups
            blockElement.querySelectorAll('.checkbox-group[data-required="true"]').forEach(group => {
                const checked = group.querySelectorAll('input[type="checkbox"]:checked');
                if (checked.length === 0) {
                    errors.push({
                        field: group.querySelector('input[type="checkbox"]'),
                        message: 'Debe seleccionar al menos una opción'
                    });
                }
            });
            
            return errors;
        }

        isFieldValid(field) {
            if (field.type === 'radio') {
                const name = field.name;
                return document.querySelector(`input[name="${name}"]:checked`) !== null;
            } else if (field.type === 'checkbox') {
                // Handled separately for groups
                return true;
            } else {
                return field.value.trim() !== '';
            }
        }
    }

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        const formManager = new ParticipantFormManager(config);
        // Make formManager globally accessible for auto-save manager
        window.formManager = formManager;
        formManager.initialize();
    });
})();