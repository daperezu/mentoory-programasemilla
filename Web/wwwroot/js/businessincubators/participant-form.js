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
                    formId: config.formId,
                    progressPercentage: 0,
                    blockResponses: []
                };
            }
        }

        async initialize() {
            try {
                // Load form structure
                await this.loadFormStructure();
                
                // Initialize auto-save
                this.autoSaveManager = new FormAutoSaveManager(this.config);
                this.autoSaveManager.initialize();
                
                // Setup navigation
                this.setupNavigation();
                
                // Load current block
                this.loadBlock(this.currentBlockIndex);
                
                // Setup event handlers
                this.setupEventHandlers();
                
                // Update progress
                this.updateProgress();
                
            } catch (error) {
                console.error('Error initializing form:', error);
                toastr.error('Error al cargar el formulario', 'Error');
            }
        }

        async loadFormStructure() {
            try {
                // Load form structure from API
                const response = await fetch(`/BusinessIncubators/${this.config.businessIncubatorId}/Projects/${this.config.projectId}/ParticipantForm/GetFormStructure?formId=${this.config.formId}`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) {
                    throw new Error('Error al cargar la estructura del formulario');
                }

                this.formStructure = await response.json();

                // Render navigation
                this.renderNavigation();
                
            } catch (error) {
                throw new Error('Failed to load form structure: ' + error.message);
            }
        }

        renderNavigation() {
            const navContainer = document.getElementById('blockNavigation');
            navContainer.innerHTML = '';

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

                navItem.innerHTML = `
                    <span class="nav-item-circle-parent">
                        <span class="nav-item-circle">
                            ${isCompleted ? '<i class="fas fa-check"></i>' : index + 1}
                        </span>
                    </span>
                    <span class="nav-item-title">${block.blockName}</span>
                `;

                navContainer.appendChild(navItem);
            });
        }

        loadBlock(index) {
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
            
            let html = `
                <div class="card" data-block-id="${block.blockId}">
                    <div class="card-header bg-body-tertiary">
                        <h5 class="mb-0">${block.blockName}</h5>
                        ${block.description ? `<p class="mb-0 mt-1 text-muted">${block.description}</p>` : ''}
                    </div>
                    <div class="card-body">
            `;

            block.questions.forEach(question => {
                const questionResponse = blockResponse?.questionResponses?.find(q => q.questionId === question.questionId);
                html += this.renderQuestion(question, questionResponse);
            });

            html += `
                    </div>
                </div>
            `;

            contentContainer.innerHTML = html;

            // Show action buttons
            document.getElementById('actionButtons').style.display = 'flex';

            // Re-initialize auto-save for new inputs
            this.autoSaveManager.attachListeners();
        }

        renderQuestion(question, questionResponse) {
            let html = `<div class="form-question" data-question-id="${question.questionId}">`;
            
            html += `
                <label class="question-label">
                    ${question.questionText}
                    ${question.isRequired ? '<span class="question-required">*</span>' : ''}
                </label>
            `;

            if (question.helpText) {
                html += `<div class="question-help">${question.helpText}</div>`;
            }

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

            html += '</div>';
            return html;
        }

        renderTextInput(question, response) {
            const value = response?.textAnswer || '';
            return `
                <input type="text" 
                       class="form-control" 
                       id="q_${question.questionId}"
                       data-question-id="${question.questionId}"
                       data-answer-type="text"
                       ${question.isRequired ? 'required' : ''}
                       value="${this.escapeHtml(value)}"
                       placeholder="Su respuesta aquí">
            `;
        }

        renderSingleChoice(question, response) {
            let html = '<div class="radio-group" data-question-id="' + question.questionId + '">';
            
            question.answerOptions.forEach(option => {
                const isChecked = response?.answerOptionId === option.answerOptionId;
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
                const isChecked = response?.answerOptionIds?.includes(option.answerOptionId);
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
            const value = response?.textAnswer || '';
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
            const value = response?.numericAnswer || '';
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
            const value = response?.dateAnswer ? response.dateAnswer.split('T')[0] : '';
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
            const value = response?.textAnswer || '';
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
            const value = response?.textAnswer || '';
            const options = [
                { value: 'CC', text: 'Cédula de Ciudadanía' },
                { value: 'CE', text: 'Cédula de Extranjería' },
                { value: 'PA', text: 'Pasaporte' },
                { value: 'TI', text: 'Tarjeta de Identidad' }
            ];
            
            let html = `<select class="form-select" 
                               id="q_${question.questionId}"
                               data-question-id="${question.questionId}"
                               data-answer-type="text"
                               ${question.isRequired ? 'required' : ''}>
                           <option value="">Seleccione tipo de identificación</option>`;
            
            options.forEach(opt => {
                html += `<option value="${opt.value}" ${value === opt.value ? 'selected' : ''}>${opt.text}</option>`;
            });
            
            html += '</select>';
            return html;
        }

        renderGenderSelect(question, response) {
            const value = response?.textAnswer || '';
            const options = [
                { value: 'M', text: 'Masculino' },
                { value: 'F', text: 'Femenino' },
                { value: 'O', text: 'Otro' }
            ];
            
            let html = `<select class="form-select" 
                               id="q_${question.questionId}"
                               data-question-id="${question.questionId}"
                               data-answer-type="text"
                               ${question.isRequired ? 'required' : ''}>
                           <option value="">Seleccione género</option>`;
            
            options.forEach(opt => {
                html += `<option value="${opt.value}" ${value === opt.value ? 'selected' : ''}>${opt.text}</option>`;
            });
            
            html += '</select>';
            return html;
        }

        renderMaritalStatusSelect(question, response) {
            const value = response?.textAnswer || '';
            const options = [
                { value: 'S', text: 'Soltero(a)' },
                { value: 'C', text: 'Casado(a)' },
                { value: 'U', text: 'Unión Libre' },
                { value: 'D', text: 'Divorciado(a)' },
                { value: 'V', text: 'Viudo(a)' }
            ];
            
            let html = `<select class="form-select" 
                               id="q_${question.questionId}"
                               data-question-id="${question.questionId}"
                               data-answer-type="text"
                               ${question.isRequired ? 'required' : ''}>
                           <option value="">Seleccione estado civil</option>`;
            
            options.forEach(opt => {
                html += `<option value="${opt.value}" ${value === opt.value ? 'selected' : ''}>${opt.text}</option>`;
            });
            
            html += '</select>';
            return html;
        }

        renderEmailInput(question, response) {
            const value = response?.textAnswer || '';
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
            const value = response?.textAnswer || '';
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
            const value = response?.textAnswer || '';
            const options = [
                { value: 'CO', text: 'Colombia' },
                { value: 'VE', text: 'Venezuela' },
                { value: 'EC', text: 'Ecuador' },
                { value: 'PE', text: 'Perú' },
                { value: 'BR', text: 'Brasil' },
                { value: 'AR', text: 'Argentina' },
                { value: 'CL', text: 'Chile' },
                { value: 'MX', text: 'México' },
                { value: 'US', text: 'Estados Unidos' },
                { value: 'OTHER', text: 'Otra' }
            ];
            
            let html = `<select class="form-select" 
                               id="q_${question.questionId}"
                               data-question-id="${question.questionId}"
                               data-answer-type="text"
                               ${question.isRequired ? 'required' : ''}>
                           <option value="">Seleccione nacionalidad</option>`;
            
            options.forEach(opt => {
                html += `<option value="${opt.value}" ${value === opt.value ? 'selected' : ''}>${opt.text}</option>`;
            });
            
            html += '</select>';
            return html;
        }

        setupNavigation() {
            // Block navigation clicks
            document.getElementById('blockNavigation').addEventListener('click', (e) => {
                e.preventDefault();
                const navLink = e.target.closest('.nav-link');
                if (navLink) {
                    const index = parseInt(navLink.dataset.blockIndex);
                    if (!isNaN(index)) {
                        // Save current block before navigating
                        this.saveCurrentBlock();
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
                    this.loadBlock(this.currentBlockIndex - 1);
                }
            });

            // Next button
            document.getElementById('btnNext').addEventListener('click', () => {
                if (this.validateCurrentBlock()) {
                    this.saveCurrentBlock();
                    if (this.currentBlockIndex < this.formStructure.blocks.length - 1) {
                        this.loadBlock(this.currentBlockIndex + 1);
                    }
                }
            });

            // Save draft button
            document.getElementById('btnSaveDraft').addEventListener('click', async () => {
                this.saveCurrentBlock();
                await this.autoSaveManager.save();
                toastr.success('Borrador guardado exitosamente', 'Éxito');
            });

            // Submit button
            document.getElementById('btnSubmit').addEventListener('click', async () => {
                if (this.isSubmitting) return;
                
                if (this.validateCurrentBlock() && this.validateAllBlocks()) {
                    this.saveCurrentBlock();
                    await this.submitForm();
                }
            });
        }

        validateCurrentBlock() {
            const block = this.formStructure.blocks[this.currentBlockIndex];
            const errors = this.validator.validateBlock(document.querySelector(`[data-block-id="${block.blockId}"]`));
            
            if (errors.length > 0) {
                // Show first error
                const firstError = errors[0];
                firstError.field.classList.add('is-invalid');
                firstError.field.focus();
                toastr.error(firstError.message, 'Error de validación');
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
                toastr.error('Por favor complete todas las preguntas requeridas antes de enviar', 'Formulario incompleto');
                if (firstIncompleteBlock !== null) {
                    this.loadBlock(firstIncompleteBlock);
                }
            }

            return allValid;
        }

        saveCurrentBlock() {
            const block = this.formStructure.blocks[this.currentBlockIndex];
            const blockElement = document.querySelector(`[data-block-id="${block.blockId}"]`);
            
            if (!blockElement) return;

            // Get or create block response
            let blockResponse = this.getBlockResponse(block.blockId);
            if (!blockResponse) {
                blockResponse = {
                    blockId: block.blockId,
                    blockName: block.blockName,
                    questionResponses: []
                };
                draftData.blockResponses.push(blockResponse);
            }

            // Save each question
            block.questions.forEach(question => {
                const response = this.collectQuestionResponse(question);
                if (response) {
                    // Update or add response
                    const existingIndex = blockResponse.questionResponses.findIndex(q => q.questionId === question.questionId);
                    if (existingIndex >= 0) {
                        blockResponse.questionResponses[existingIndex] = response;
                    } else {
                        blockResponse.questionResponses.push(response);
                    }
                }
            });

            // Update progress
            this.updateProgressPercentage();
        }

        collectQuestionResponse(question) {
            const response = {
                questionId: question.questionId,
                questionText: question.questionText,
                answerType: question.answerType,
                isAnswered: false,
                moduleInfo: question.moduleInfo,
                topicInfo: question.topicInfo
            };

            switch (question.answerType) {
                case 1: // SingleChoice
                    const selectedRadio = document.querySelector(`input[name="q_${question.questionId}"]:checked`);
                    if (selectedRadio) {
                        response.answerOptionId = parseInt(selectedRadio.value);
                        response.isAnswered = true;
                    }
                    break;

                case 2: // MultiChoice
                    const checkedBoxes = document.querySelectorAll(`input[data-question-id="${question.questionId}"]:checked`);
                    if (checkedBoxes.length > 0) {
                        response.answerOptionIds = Array.from(checkedBoxes).map(cb => parseInt(cb.value));
                        response.isAnswered = true;
                    }
                    break;

                case 3: // FreeText
                case 6: // PersonId
                case 7: // IdType
                case 8: // Gender
                case 9: // MaritalStatus
                case 10: // Email
                case 11: // PhoneNumber
                case 12: // Nationality
                    const textInput = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="text"]`);
                    if (textInput && textInput.value.trim()) {
                        response.textAnswer = textInput.value.trim();
                        response.isAnswered = true;
                    }
                    break;

                case 4: // Numeric
                    const numericInput = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="numeric"]`);
                    if (numericInput && numericInput.value) {
                        response.numericAnswer = parseFloat(numericInput.value);
                        response.isAnswered = true;
                    }
                    break;

                case 5: // Date
                    const dateInput = document.querySelector(`[data-question-id="${question.questionId}"][data-answer-type="date"]`);
                    if (dateInput && dateInput.value) {
                        response.dateAnswer = dateInput.value;
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
                btnSubmit.style.display = 'inline-block';
            } else {
                btnNext.style.display = 'inline-block';
                btnSubmit.style.display = 'none';
            }
        }

        updateProgress() {
            const percentage = draftData.progressPercentage || 0;
            const currentBlock = this.formStructure.blocks[this.currentBlockIndex];
            
            document.getElementById('progressBar').style.width = percentage + '%';
            document.getElementById('progressPercentage').textContent = percentage + '%';
            document.getElementById('progressText').textContent = 
                `Bloque ${this.currentBlockIndex + 1} de ${this.formStructure.blocks.length} - ${currentBlock.blockName}`;
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

        async submitForm() {
            if (this.isSubmitting) return;
            
            // Confirm submission
            const result = await Swal.fire({
                title: '¿Enviar formulario?',
                text: 'Una vez enviado, no podrá realizar más cambios hasta que sea revisado.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Sí, enviar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#198754'
            });

            if (!result.isConfirmed) return;

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
                    body: JSON.stringify({
                        projectId: this.config.projectIdNumeric,
                        submissionId: this.config.submissionId || 0
                    })
                });

                if (response.ok) {
                    const result = await response.json();
                    
                    await Swal.fire({
                        title: '¡Formulario enviado!',
                        text: result.message || 'Su formulario ha sido enviado exitosamente.',
                        icon: 'success',
                        confirmButtonText: 'Aceptar'
                    });

                    // Redirect
                    window.location.href = result.redirectUrl || '/';
                } else {
                    const error = await response.json();
                    throw new Error(error.message || 'Error al enviar el formulario');
                }
            } catch (error) {
                console.error('Error submitting form:', error);
                toastr.error(error.message || 'Error al enviar el formulario', 'Error');
            } finally {
                this.isSubmitting = false;
            }
        }

        escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
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
            
            // Save on page unload
            window.addEventListener('beforeunload', (e) => {
                if (this.hasUnsavedChanges()) {
                    e.preventDefault();
                    e.returnValue = '¿Está seguro de que desea salir? Los cambios no guardados se perderán.';
                }
            });

            // Auto-save every 30 seconds
            setInterval(() => {
                if (this.hasUnsavedChanges()) {
                    this.save();
                }
            }, 30000);
        }

        attachListeners() {
            document.querySelectorAll('input, textarea, select').forEach(element => {
                element.addEventListener('change', () => this.scheduleSave());
                if (element.type === 'text' || element.tagName === 'TEXTAREA') {
                    element.addEventListener('input', () => this.scheduleSave());
                }
            });
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
                    body: JSON.stringify({
                        projectId: this.config.projectIdNumeric,
                        formId: this.config.formId,
                        draftData: draftData
                    })
                });

                if (response.ok) {
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
            const indicator = document.getElementById('autoSaveIndicator');
            indicator.className = 'auto-save-indicator show saving';
            document.getElementById('saveSpinner').style.display = 'inline-block';
            document.getElementById('saveCheck').style.display = 'none';
            document.getElementById('saveError').style.display = 'none';
            document.getElementById('saveText').textContent = 'Guardando...';
        }

        showSavedIndicator() {
            const indicator = document.getElementById('autoSaveIndicator');
            indicator.className = 'auto-save-indicator show saved';
            document.getElementById('saveSpinner').style.display = 'none';
            document.getElementById('saveCheck').style.display = 'inline-block';
            document.getElementById('saveError').style.display = 'none';
            document.getElementById('saveText').textContent = 'Guardado';
            
            setTimeout(() => {
                indicator.classList.remove('show');
            }, 3000);
        }

        showErrorIndicator() {
            const indicator = document.getElementById('autoSaveIndicator');
            indicator.className = 'auto-save-indicator show error';
            document.getElementById('saveSpinner').style.display = 'none';
            document.getElementById('saveCheck').style.display = 'none';
            document.getElementById('saveError').style.display = 'inline-block';
            document.getElementById('saveText').textContent = 'Error al guardar';
            
            setTimeout(() => {
                indicator.classList.remove('show');
            }, 5000);
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
        formManager.initialize();
    });
})();