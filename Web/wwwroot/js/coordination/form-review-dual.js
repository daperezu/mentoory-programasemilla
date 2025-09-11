// Dual Answer Review Manager for Coordinator Journey - REQ-008
window.DualAnswerReviewManager = (function() {
    'use strict';

    // State management
    let submissionId = null;
    let coordinatorAnswers = {};
    let starterAnswers = {};
    let questionMetadata = {}; // Store question metadata (answerType, moduleInfo, topicInfo)
    let preferenceSelections = {};
    let isDirty = false;
    let autoSaveInterval = 30000; // 30 seconds
    let autoSaveTimer = null;
    let saveInProgress = false;

    // Initialize the dual answer system
    function init(subId) {
        submissionId = subId;
        setupAutoSave();
        attachEventHandlers();
        loadExistingCoordinatorData();
        console.log('[DualAnswerReview] Initialized for submission:', submissionId);
    }

    // Setup auto-save functionality
    function setupAutoSave() {
        // Auto-save every 30 seconds if dirty
        autoSaveTimer = setInterval(() => {
            if (isDirty && !saveInProgress) {
                saveDraft(false);
            }
        }, autoSaveInterval);

        // Save on page unload
        window.addEventListener('beforeunload', (e) => {
            if (isDirty) {
                saveDraft(false);
                e.preventDefault();
                e.returnValue = 'Tienes cambios sin guardar. ¿Estás seguro de que quieres salir?';
            }
        });
    }

    // Attach event handlers
    function attachEventHandlers() {
        // Delegate event handling for dynamic content
        document.addEventListener('click', (e) => {
            // Handle copy from starter button
            if (e.target.closest('.copy-from-starter-btn')) {
                const btn = e.target.closest('.copy-from-starter-btn');
                const questionId = btn.getAttribute('data-question-id');
                copyFromStarter(questionId);
            }
        });

        document.addEventListener('change', (e) => {
            // Handle coordinator answer inputs
            if (e.target.classList.contains('coordinator-answer-input')) {
                const questionId = e.target.getAttribute('data-question-id');
                
                // Handle checkboxes (MultiChoice)
                if (e.target.type === 'checkbox') {
                    // Get all checked checkboxes for this question
                    const checkboxes = document.querySelectorAll(`input[type="checkbox"][data-question-id="${questionId}"]`);
                    const checkedValues = Array.from(checkboxes)
                        .filter(cb => cb.checked)
                        .map(cb => cb.value);
                    const value = checkedValues.join(',');
                    updateCoordinatorAnswer(questionId, value);
                } else {
                    // Handle radio buttons and other inputs
                    const value = e.target.value;
                    updateCoordinatorAnswer(questionId, value);
                }
            }

            // Handle use for diagnosis switches
            if (e.target.classList.contains('use-for-diagnosis')) {
                const questionId = e.target.getAttribute('data-question-id');
                const useCoordinator = e.target.checked;
                updatePreference(questionId, useCoordinator);
            }
        });

        document.addEventListener('input', (e) => {
            // Handle text inputs
            if (e.target.classList.contains('coordinator-answer-input') && 
                (e.target.type === 'text' || e.target.type === 'textarea')) {
                const questionId = e.target.getAttribute('data-question-id');
                const value = e.target.value;
                updateCoordinatorAnswer(questionId, value);
            }
        });
    }

    // Load existing coordinator data if available
    async function loadExistingCoordinatorData() {
        try {
            const response = await fetch(`/Coordination/FormReview/GetCoordinatorAnswers/${submissionId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.coordinatorData) {
                    coordinatorAnswers = data.coordinatorData;
                    preferenceSelections = data.preferenceSelections || {};
                    applyCoordinatorData();
                    updateProgress();
                }
            }
        } catch (error) {
            console.error('[DualAnswerReview] Error loading coordinator data:', error);
        }
    }

    // Apply loaded coordinator data to UI
    function applyCoordinatorData() {
        Object.keys(coordinatorAnswers).forEach(questionId => {
            const input = document.querySelector(`.coordinator-answer-input[data-question-id="${questionId}"]`);
            if (input) {
                if (input.type === 'radio') {
                    // Handle radio buttons
                    const specificInput = document.querySelector(
                        `.coordinator-answer-input[data-question-id="${questionId}"][value="${coordinatorAnswers[questionId]}"]`
                    );
                    if (specificInput) {
                        specificInput.checked = true;
                    }
                } else if (input.type === 'checkbox') {
                    // Handle checkboxes - multiple values
                    const values = coordinatorAnswers[questionId].split(',').map(v => v.trim());
                    values.forEach(value => {
                        const checkbox = document.querySelector(
                            `.coordinator-answer-input[data-question-id="${questionId}"][value="${value}"]`
                        );
                        if (checkbox) {
                            checkbox.checked = true;
                        }
                    });
                } else {
                    // Handle text, select, and other input types
                    input.value = coordinatorAnswers[questionId];
                }
            }
        });

        // Apply preference selections
        Object.keys(preferenceSelections).forEach(questionId => {
            const checkbox = document.querySelector(`.use-for-diagnosis[data-question-id="${questionId}"]`);
            if (checkbox) {
                checkbox.checked = preferenceSelections[questionId];
            }
        });
    }

    // Copy answer from starter to coordinator
    function copyFromStarter(questionId) {
        const starterValue = starterAnswers[questionId];
        if (!starterValue) {
            console.warn('[DualAnswerReview] No starter answer found for question:', questionId);
            return;
        }

        // Update coordinator answer
        coordinatorAnswers[questionId] = starterValue;
        
        // Update UI based on input type
        const inputs = document.querySelectorAll(`.coordinator-answer-input[data-question-id="${questionId}"]`);
        
        if (inputs.length > 0) {
            const firstInput = inputs[0];
            
            if (firstInput.type === 'radio') {
                // Handle radio buttons - select the matching value
                const targetRadio = document.querySelector(
                    `.coordinator-answer-input[data-question-id="${questionId}"][value="${starterValue}"]`
                );
                if (targetRadio) {
                    targetRadio.checked = true;
                }
            } else if (firstInput.type === 'checkbox') {
                // Handle checkboxes - select multiple values
                const selectedValues = starterValue.split(',').map(v => v.trim());
                inputs.forEach(checkbox => {
                    checkbox.checked = selectedValues.includes(checkbox.value);
                });
            } else if (firstInput.tagName === 'SELECT') {
                // Handle select dropdowns
                firstInput.value = starterValue;
            } else {
                // Handle text, number, date, email, tel, url inputs
                firstInput.value = starterValue;
            }
        }

        markDirty();
        updateProgress();
        checkForDifferences(questionId);
        showToast('Respuesta copiada exitosamente', 'success');
    }

    // Update coordinator answer
    function updateCoordinatorAnswer(questionId, value) {
        coordinatorAnswers[questionId] = value;
        markDirty();
        updateProgress();
        checkForDifferences(questionId);
    }

    // Update preference selection
    function updatePreference(questionId, useCoordinator) {
        preferenceSelections[questionId] = useCoordinator;
        markDirty();
    }

    // Check for differences between starter and coordinator answers
    function checkForDifferences(questionId) {
        const starterValue = starterAnswers[questionId];
        const coordinatorValue = coordinatorAnswers[questionId];
        
        const diffAlert = document.querySelector(`.answer-diff-alert[data-question-id="${questionId}"]`);
        if (diffAlert) {
            if (starterValue && coordinatorValue && starterValue !== coordinatorValue) {
                diffAlert.classList.add('visible');
            } else {
                diffAlert.classList.remove('visible');
            }
        }
    }

    // Update progress indicators
    function updateProgress() {
        const totalQuestions = Object.keys(starterAnswers).length;
        const answeredQuestions = Object.keys(coordinatorAnswers).filter(id => 
            coordinatorAnswers[id] && coordinatorAnswers[id].trim() !== ''
        ).length;

        const percentage = totalQuestions > 0 ? Math.round((answeredQuestions / totalQuestions) * 100) : 0;

        // Update coordinator progress bar
        const progressFill = document.getElementById('coordinatorProgressFill');
        const progressText = document.getElementById('coordinatorProgressText');
        const progressContainer = document.getElementById('coordinatorProgress');

        if (progressFill) {
            progressFill.style.width = `${percentage}%`;
        }
        if (progressText) {
            progressText.textContent = `${percentage}%`;
        }
        if (progressContainer && totalQuestions > 0) {
            progressContainer.style.display = 'block';
        }

        // Enable/disable approve button based on completion
        const btnApprove = document.getElementById('btnApprove');
        if (btnApprove) {
            if (percentage === 100) {
                btnApprove.removeAttribute('disabled');
            } else {
                btnApprove.setAttribute('disabled', 'disabled');
            }
        }

        return percentage;
    }

    // Mark form as dirty
    function markDirty() {
        isDirty = true;
    }

    // Save draft
    async function saveDraft(showNotification = true) {
        if (saveInProgress) {
            return;
        }

        saveInProgress = true;
        showSaving();

        try {
            const draftData = buildDraftData();
            const response = await fetch('/Coordination/FormReview/SaveCoordinatorAnswers', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    submissionId: submissionId,
                    coordinatorData: draftData,
                    preferenceSelections: preferenceSelections
                })
            });

            if (response.ok) {
                isDirty = false;
                const timestamp = new Date().toLocaleTimeString('es-ES', { 
                    hour: '2-digit', 
                    minute: '2-digit' 
                });
                showSaved(timestamp);
                
                if (showNotification) {
                    showToast('Respuestas guardadas exitosamente', 'success');
                }
            } else {
                throw new Error('Error al guardar las respuestas');
            }
        } catch (error) {
            console.error('[DualAnswerReview] Save error:', error);
            showSaveError();
            if (showNotification) {
                showToast('Error al guardar las respuestas', 'danger');
            }
        } finally {
            saveInProgress = false;
        }
    }

    // Build draft data structure matching DraftDataDto
    function buildDraftData() {
        const blockResponses = [];
        
        // Group answers by block
        document.querySelectorAll('.form-block').forEach(blockElement => {
            const blockId = blockElement.getAttribute('data-block-id');
            const blockName = blockElement.querySelector('h4')?.textContent?.trim() || '';
            
            const questionResponses = [];
            blockElement.querySelectorAll('.question-item').forEach(questionElement => {
                const questionId = questionElement.getAttribute('data-question-id');
                const questionText = questionElement.querySelector('.question-label span:not(.required-indicator)')?.textContent?.trim() || '';
                
                if (coordinatorAnswers[questionId]) {
                    const metadata = questionMetadata[questionId] || {};
                    questionResponses.push({
                        questionId: parseInt(questionId),
                        questionText: questionText,
                        answer: coordinatorAnswers[questionId],
                        answerType: metadata.answerType || 3, // Use actual type or default to FreeText
                        isAnswered: true,
                        moduleInfo: metadata.moduleInfo || null,
                        topicInfo: metadata.topicInfo || null
                    });
                }
            });

            if (questionResponses.length > 0) {
                blockResponses.push({
                    blockId: parseInt(blockId),
                    blockName: blockName,
                    questionResponses: questionResponses
                });
            }
        });

        return {
            formVersion: 1,
            blockResponses: blockResponses,
            progressPercentage: updateProgress()
        };
    }

    // Show saving indicator
    function showSaving() {
        const indicator = document.getElementById('autoSaveIndicator');
        const spinner = document.getElementById('saveSpinner');
        const check = document.getElementById('saveCheck');
        const text = document.getElementById('saveText');

        if (indicator) {
            indicator.classList.add('visible');
            if (spinner) spinner.style.display = 'block';
            if (check) check.style.display = 'none';
            if (text) text.textContent = 'Guardando...';
        }
    }

    // Show saved indicator
    function showSaved(timestamp) {
        const indicator = document.getElementById('autoSaveIndicator');
        const spinner = document.getElementById('saveSpinner');
        const check = document.getElementById('saveCheck');
        const text = document.getElementById('saveText');
        const time = document.getElementById('saveTime');

        if (spinner) spinner.style.display = 'none';
        if (check) check.style.display = 'block';
        if (text) text.textContent = 'Guardado';
        if (time) time.textContent = timestamp;

        setTimeout(() => {
            if (indicator) {
                indicator.classList.remove('visible');
            }
        }, 3000);
    }

    // Show save error
    function showSaveError() {
        const indicator = document.getElementById('autoSaveIndicator');
        const spinner = document.getElementById('saveSpinner');
        const text = document.getElementById('saveText');

        if (spinner) spinner.style.display = 'none';
        if (text) {
            text.textContent = 'Error al guardar';
            text.classList.add('text-danger');
        }

        setTimeout(() => {
            if (indicator) {
                indicator.classList.remove('visible');
            }
            if (text) {
                text.classList.remove('text-danger');
            }
        }, 3000);
    }

    // Render dual answer layout for a question
    function renderDualAnswerLayout(question, block) {
        const starterAnswerValue = question.answer || '';
        const questionId = question.questionId;
        const answerType = question.answerType || 3; // Default to FreeText if not specified
        
        // Store starter answer
        starterAnswers[questionId] = starterAnswerValue;
        
        // Store question metadata
        questionMetadata[questionId] = {
            answerType: answerType,
            moduleInfo: question.moduleInfo || null,
            topicInfo: question.topicInfo || null
        };

        // Format starter answer display based on type
        let starterAnswerDisplay = 'Sin respuesta';
        let coordinatorInput = '';

        // Build the appropriate input based on answer type
        switch (answerType) {
            case 1: // SingleChoice
                if (question.answerOptions && question.answerOptions.length > 0) {
                    // Find selected option text for starter
                    const selectedOption = question.answerOptions.find(opt => 
                        opt.answerOptionId && opt.answerOptionId.toString() === starterAnswerValue
                    );
                    starterAnswerDisplay = selectedOption ? selectedOption.answerOptionText : starterAnswerValue || 'Sin respuesta';

                    // Build radio buttons for coordinator
                    coordinatorInput = `
                        <div class="radio-group">
                            ${question.answerOptions.map(option => `
                                <div class="form-check">
                                    <input class="form-check-input coordinator-answer-input" 
                                           type="radio" 
                                           name="coord-q-${questionId}" 
                                           id="coord-q-${questionId}-${option.answerOptionId}"
                                           value="${option.answerOptionId}"
                                           data-question-id="${questionId}">
                                    <label class="form-check-label" for="coord-q-${questionId}-${option.answerOptionId}">
                                        ${option.answerOptionText}
                                    </label>
                                </div>
                            `).join('')}
                        </div>
                    `;
                }
                break;

            case 2: // MultiChoice
                if (question.answerOptions && question.answerOptions.length > 0) {
                    // Parse selected options for starter
                    const selectedIds = starterAnswerValue ? starterAnswerValue.split(',').map(id => id.trim()) : [];
                    const selectedOptions = question.answerOptions.filter(opt => 
                        selectedIds.includes(opt.answerOptionId.toString())
                    );
                    starterAnswerDisplay = selectedOptions.length > 0 
                        ? selectedOptions.map(opt => opt.answerOptionText).join(', ')
                        : 'Sin respuesta';

                    // Build checkboxes for coordinator
                    coordinatorInput = `
                        <div class="checkbox-group">
                            ${question.answerOptions.map(option => `
                                <div class="form-check">
                                    <input class="form-check-input coordinator-answer-input" 
                                           type="checkbox" 
                                           id="coord-q-${questionId}-${option.answerOptionId}"
                                           value="${option.answerOptionId}"
                                           data-question-id="${questionId}">
                                    <label class="form-check-label" for="coord-q-${questionId}-${option.answerOptionId}">
                                        ${option.answerOptionText}
                                    </label>
                                </div>
                            `).join('')}
                        </div>
                    `;
                }
                break;

            case 3: // FreeText
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <textarea class="form-control coordinator-answer-input" 
                              data-question-id="${questionId}"
                              rows="3"
                              placeholder="Ingrese su respuesta como coordinador...">${coordinatorAnswers[questionId] || ''}</textarea>
                `;
                break;

            case 4: // Numeric
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <input type="number" 
                           class="form-control coordinator-answer-input" 
                           data-question-id="${questionId}"
                           value="${coordinatorAnswers[questionId] || ''}"
                           placeholder="Ingrese un valor numérico...">
                `;
                break;

            case 5: // Date
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <input type="date" 
                           class="form-control coordinator-answer-input" 
                           data-question-id="${questionId}"
                           value="${coordinatorAnswers[questionId] || ''}">
                `;
                break;

            case 6: // PersonId
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <input type="text" 
                           class="form-control coordinator-answer-input" 
                           data-question-id="${questionId}"
                           value="${coordinatorAnswers[questionId] || ''}"
                           pattern="[0-9]*"
                           placeholder="Número de identificación...">
                `;
                break;

            case 7: // IdType
                // Map code to text for starter display
                const idTypeOptions = {
                    'CC': 'Cédula de Ciudadanía',
                    'CE': 'Cédula de Extranjería',
                    'PA': 'Pasaporte',
                    'TI': 'Tarjeta de Identidad'
                };
                starterAnswerDisplay = idTypeOptions[starterAnswerValue] || starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <select class="form-control coordinator-answer-input" 
                            data-question-id="${questionId}">
                        <option value="">Seleccione tipo de identificación</option>
                        <option value="CC" ${coordinatorAnswers[questionId] === 'CC' ? 'selected' : ''}>Cédula de Ciudadanía</option>
                        <option value="CE" ${coordinatorAnswers[questionId] === 'CE' ? 'selected' : ''}>Cédula de Extranjería</option>
                        <option value="PA" ${coordinatorAnswers[questionId] === 'PA' ? 'selected' : ''}>Pasaporte</option>
                        <option value="TI" ${coordinatorAnswers[questionId] === 'TI' ? 'selected' : ''}>Tarjeta de Identidad</option>
                    </select>
                `;
                break;

            case 8: // Gender
                // Map code to text for starter display
                const genderOptions = {
                    'M': 'Masculino',
                    'F': 'Femenino',
                    'O': 'Otro'
                };
                starterAnswerDisplay = genderOptions[starterAnswerValue] || starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <select class="form-control coordinator-answer-input" 
                            data-question-id="${questionId}">
                        <option value="">Seleccione género</option>
                        <option value="M" ${coordinatorAnswers[questionId] === 'M' ? 'selected' : ''}>Masculino</option>
                        <option value="F" ${coordinatorAnswers[questionId] === 'F' ? 'selected' : ''}>Femenino</option>
                        <option value="O" ${coordinatorAnswers[questionId] === 'O' ? 'selected' : ''}>Otro</option>
                    </select>
                `;
                break;

            case 9: // MaritalStatus
                // Map code to text for starter display
                const maritalOptions = {
                    'S': 'Soltero(a)',
                    'C': 'Casado(a)',
                    'U': 'Unión Libre',
                    'D': 'Divorciado(a)',
                    'V': 'Viudo(a)'
                };
                starterAnswerDisplay = maritalOptions[starterAnswerValue] || starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <select class="form-control coordinator-answer-input" 
                            data-question-id="${questionId}">
                        <option value="">Seleccione estado civil</option>
                        <option value="S" ${coordinatorAnswers[questionId] === 'S' ? 'selected' : ''}>Soltero(a)</option>
                        <option value="C" ${coordinatorAnswers[questionId] === 'C' ? 'selected' : ''}>Casado(a)</option>
                        <option value="U" ${coordinatorAnswers[questionId] === 'U' ? 'selected' : ''}>Unión Libre</option>
                        <option value="D" ${coordinatorAnswers[questionId] === 'D' ? 'selected' : ''}>Divorciado(a)</option>
                        <option value="V" ${coordinatorAnswers[questionId] === 'V' ? 'selected' : ''}>Viudo(a)</option>
                    </select>
                `;
                break;

            case 10: // Email
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <input type="email" 
                           class="form-control coordinator-answer-input" 
                           data-question-id="${questionId}"
                           value="${coordinatorAnswers[questionId] || ''}"
                           placeholder="correo@ejemplo.com">
                `;
                break;

            case 11: // PhoneNumber
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <input type="tel" 
                           class="form-control coordinator-answer-input" 
                           data-question-id="${questionId}"
                           value="${coordinatorAnswers[questionId] || ''}"
                           pattern="[0-9+()-]*"
                           placeholder="+57 300 123 4567">
                `;
                break;

            case 12: // Nationality
                // Complete nationality list matching participant form exactly
                const nationalities = [
                    // Americas
                    { value: 'AR', text: 'Argentina' },
                    { value: 'BO', text: 'Bolivia' },
                    { value: 'BR', text: 'Brasil' },
                    { value: 'CA', text: 'Canadá' },
                    { value: 'CL', text: 'Chile' },
                    { value: 'CO', text: 'Colombia' },
                    { value: 'CR', text: 'Costa Rica' },
                    { value: 'CU', text: 'Cuba' },
                    { value: 'DO', text: 'República Dominicana' },
                    { value: 'EC', text: 'Ecuador' },
                    { value: 'SV', text: 'El Salvador' },
                    { value: 'US', text: 'Estados Unidos' },
                    { value: 'GT', text: 'Guatemala' },
                    { value: 'HT', text: 'Haití' },
                    { value: 'HN', text: 'Honduras' },
                    { value: 'JM', text: 'Jamaica' },
                    { value: 'MX', text: 'México' },
                    { value: 'NI', text: 'Nicaragua' },
                    { value: 'PA', text: 'Panamá' },
                    { value: 'PY', text: 'Paraguay' },
                    { value: 'PE', text: 'Perú' },
                    { value: 'PR', text: 'Puerto Rico' },
                    { value: 'UY', text: 'Uruguay' },
                    { value: 'VE', text: 'Venezuela' },
                    
                    // Europe
                    { value: 'AL', text: 'Albania' },
                    { value: 'DE', text: 'Alemania' },
                    { value: 'AD', text: 'Andorra' },
                    { value: 'AT', text: 'Austria' },
                    { value: 'BE', text: 'Bélgica' },
                    { value: 'BY', text: 'Bielorrusia' },
                    { value: 'BA', text: 'Bosnia y Herzegovina' },
                    { value: 'BG', text: 'Bulgaria' },
                    { value: 'HR', text: 'Croacia' },
                    { value: 'DK', text: 'Dinamarca' },
                    { value: 'SK', text: 'Eslovaquia' },
                    { value: 'SI', text: 'Eslovenia' },
                    { value: 'ES', text: 'España' },
                    { value: 'EE', text: 'Estonia' },
                    { value: 'FI', text: 'Finlandia' },
                    { value: 'FR', text: 'Francia' },
                    { value: 'GR', text: 'Grecia' },
                    { value: 'HU', text: 'Hungría' },
                    { value: 'IE', text: 'Irlanda' },
                    { value: 'IS', text: 'Islandia' },
                    { value: 'IT', text: 'Italia' },
                    { value: 'LV', text: 'Letonia' },
                    { value: 'LI', text: 'Liechtenstein' },
                    { value: 'LT', text: 'Lituania' },
                    { value: 'LU', text: 'Luxemburgo' },
                    { value: 'MK', text: 'Macedonia del Norte' },
                    { value: 'MT', text: 'Malta' },
                    { value: 'MD', text: 'Moldavia' },
                    { value: 'MC', text: 'Mónaco' },
                    { value: 'ME', text: 'Montenegro' },
                    { value: 'NO', text: 'Noruega' },
                    { value: 'NL', text: 'Países Bajos' },
                    { value: 'PL', text: 'Polonia' },
                    { value: 'PT', text: 'Portugal' },
                    { value: 'GB', text: 'Reino Unido' },
                    { value: 'CZ', text: 'República Checa' },
                    { value: 'RO', text: 'Rumania' },
                    { value: 'RU', text: 'Rusia' },
                    { value: 'SM', text: 'San Marino' },
                    { value: 'RS', text: 'Serbia' },
                    { value: 'SE', text: 'Suecia' },
                    { value: 'CH', text: 'Suiza' },
                    { value: 'UA', text: 'Ucrania' },
                    { value: 'VA', text: 'Vaticano' },
                    
                    // Asia
                    { value: 'AF', text: 'Afganistán' },
                    { value: 'SA', text: 'Arabia Saudita' },
                    { value: 'AM', text: 'Armenia' },
                    { value: 'AZ', text: 'Azerbaiyán' },
                    { value: 'BH', text: 'Baréin' },
                    { value: 'BD', text: 'Bangladesh' },
                    { value: 'BT', text: 'Bután' },
                    { value: 'BN', text: 'Brunéi' },
                    { value: 'KH', text: 'Camboya' },
                    { value: 'CN', text: 'China' },
                    { value: 'CY', text: 'Chipre' },
                    { value: 'KP', text: 'Corea del Norte' },
                    { value: 'KR', text: 'Corea del Sur' },
                    { value: 'AE', text: 'Emiratos Árabes Unidos' },
                    { value: 'PH', text: 'Filipinas' },
                    { value: 'GE', text: 'Georgia' },
                    { value: 'IN', text: 'India' },
                    { value: 'ID', text: 'Indonesia' },
                    { value: 'IQ', text: 'Irak' },
                    { value: 'IR', text: 'Irán' },
                    { value: 'IL', text: 'Israel' },
                    { value: 'JP', text: 'Japón' },
                    { value: 'JO', text: 'Jordania' },
                    { value: 'KZ', text: 'Kazajistán' },
                    { value: 'KW', text: 'Kuwait' },
                    { value: 'KG', text: 'Kirguistán' },
                    { value: 'LA', text: 'Laos' },
                    { value: 'LB', text: 'Líbano' },
                    { value: 'MY', text: 'Malasia' },
                    { value: 'MV', text: 'Maldivas' },
                    { value: 'MN', text: 'Mongolia' },
                    { value: 'MM', text: 'Myanmar' },
                    { value: 'NP', text: 'Nepal' },
                    { value: 'OM', text: 'Omán' },
                    { value: 'PK', text: 'Pakistán' },
                    { value: 'PS', text: 'Palestina' },
                    { value: 'QA', text: 'Qatar' },
                    { value: 'SG', text: 'Singapur' },
                    { value: 'SY', text: 'Siria' },
                    { value: 'LK', text: 'Sri Lanka' },
                    { value: 'TJ', text: 'Tayikistán' },
                    { value: 'TH', text: 'Tailandia' },
                    { value: 'TL', text: 'Timor Oriental' },
                    { value: 'TM', text: 'Turkmenistán' },
                    { value: 'TR', text: 'Turquía' },
                    { value: 'UZ', text: 'Uzbekistán' },
                    { value: 'VN', text: 'Vietnam' },
                    { value: 'YE', text: 'Yemen' },
                    
                    // Africa
                    { value: 'DZ', text: 'Argelia' },
                    { value: 'AO', text: 'Angola' },
                    { value: 'BJ', text: 'Benín' },
                    { value: 'BW', text: 'Botsuana' },
                    { value: 'BF', text: 'Burkina Faso' },
                    { value: 'BI', text: 'Burundi' },
                    { value: 'CV', text: 'Cabo Verde' },
                    { value: 'CM', text: 'Camerún' },
                    { value: 'TD', text: 'Chad' },
                    { value: 'KM', text: 'Comoras' },
                    { value: 'CG', text: 'Congo' },
                    { value: 'CD', text: 'República Democrática del Congo' },
                    { value: 'CI', text: 'Costa de Marfil' },
                    { value: 'DJ', text: 'Yibuti' },
                    { value: 'EG', text: 'Egipto' },
                    { value: 'GQ', text: 'Guinea Ecuatorial' },
                    { value: 'ER', text: 'Eritrea' },
                    { value: 'SZ', text: 'Esuatini' },
                    { value: 'ET', text: 'Etiopía' },
                    { value: 'GA', text: 'Gabón' },
                    { value: 'GM', text: 'Gambia' },
                    { value: 'GH', text: 'Ghana' },
                    { value: 'GN', text: 'Guinea' },
                    { value: 'GW', text: 'Guinea-Bisáu' },
                    { value: 'KE', text: 'Kenia' },
                    { value: 'LS', text: 'Lesoto' },
                    { value: 'LR', text: 'Liberia' },
                    { value: 'LY', text: 'Libia' },
                    { value: 'MG', text: 'Madagascar' },
                    { value: 'MW', text: 'Malaui' },
                    { value: 'ML', text: 'Malí' },
                    { value: 'MR', text: 'Mauritania' },
                    { value: 'MU', text: 'Mauricio' },
                    { value: 'MA', text: 'Marruecos' },
                    { value: 'MZ', text: 'Mozambique' },
                    { value: 'NA', text: 'Namibia' },
                    { value: 'NE', text: 'Níger' },
                    { value: 'NG', text: 'Nigeria' },
                    { value: 'CF', text: 'República Centroafricana' },
                    { value: 'RW', text: 'Ruanda' },
                    { value: 'ST', text: 'Santo Tomé y Príncipe' },
                    { value: 'SN', text: 'Senegal' },
                    { value: 'SC', text: 'Seychelles' },
                    { value: 'SL', text: 'Sierra Leona' },
                    { value: 'SO', text: 'Somalia' },
                    { value: 'ZA', text: 'Sudáfrica' },
                    { value: 'SS', text: 'Sudán del Sur' },
                    { value: 'SD', text: 'Sudán' },
                    { value: 'TZ', text: 'Tanzania' },
                    { value: 'TG', text: 'Togo' },
                    { value: 'TN', text: 'Túnez' },
                    { value: 'UG', text: 'Uganda' },
                    { value: 'ZM', text: 'Zambia' },
                    { value: 'ZW', text: 'Zimbabue' },
                    
                    // Oceania
                    { value: 'AU', text: 'Australia' },
                    { value: 'FJ', text: 'Fiyi' },
                    { value: 'KI', text: 'Kiribati' },
                    { value: 'MH', text: 'Islas Marshall' },
                    { value: 'FM', text: 'Micronesia' },
                    { value: 'NR', text: 'Nauru' },
                    { value: 'NZ', text: 'Nueva Zelanda' },
                    { value: 'PW', text: 'Palaos' },
                    { value: 'PG', text: 'Papúa Nueva Guinea' },
                    { value: 'WS', text: 'Samoa' },
                    { value: 'SB', text: 'Islas Salomón' },
                    { value: 'TO', text: 'Tonga' },
                    { value: 'TV', text: 'Tuvalu' },
                    { value: 'VU', text: 'Vanuatu' }
                ];
                
                // Sort countries alphabetically by text (matching participant form)
                nationalities.sort((a, b) => a.text.localeCompare(b.text, 'es'));
                
                // Find the text for the starter's answer
                const selectedNationality = nationalities.find(n => n.value === starterAnswerValue);
                starterAnswerDisplay = selectedNationality ? selectedNationality.text : starterAnswerValue || 'Sin respuesta';
                
                coordinatorInput = `
                    <select class="form-control coordinator-answer-input nationality-select-coord" 
                            data-question-id="${questionId}">
                        <option value="">Seleccione nacionalidad</option>
                        ${nationalities.map(opt => 
                            `<option value="${opt.value}" ${coordinatorAnswers[questionId] === opt.value ? 'selected' : ''}>${opt.text}</option>`
                        ).join('')}
                    </select>
                `;
                break;

            default:
                starterAnswerDisplay = starterAnswerValue || 'Sin respuesta';
                coordinatorInput = `
                    <textarea class="form-control coordinator-answer-input" 
                              data-question-id="${questionId}"
                              rows="3"
                              placeholder="Ingrese su respuesta...">${coordinatorAnswers[questionId] || ''}</textarea>
                `;
        }

        return `
            <div class="dual-answer-container" data-question-id="${questionId}">
                <!-- Starter Answer Column -->
                <div class="answer-column starter-column">
                    <div class="answer-column-header">
                        <div class="answer-column-title">
                            <i class="fas fa-user text-primary"></i>
                            <span>Respuesta del Emprendedor</span>
                        </div>
                    </div>
                    <div class="starter-answer-content">
                        ${starterAnswerDisplay}
                    </div>
                </div>

                <!-- Coordinator Answer Column -->
                <div class="answer-column coordinator-column">
                    <div class="answer-column-header">
                        <div class="answer-column-title">
                            <i class="fas fa-user-tie text-success"></i>
                            <span>Tu Respuesta como Coordinador</span>
                        </div>
                        <button class="copy-from-starter-btn" 
                                data-question-id="${questionId}"
                                type="button">
                            <i class="fas fa-copy me-1"></i>Copiar
                        </button>
                    </div>
                    <div class="coordinator-answer-content">
                        ${coordinatorInput}
                        
                        <div class="use-for-diagnosis-switch">
                            <div class="form-check form-switch">
                                <input class="form-check-input use-for-diagnosis" 
                                       type="checkbox" 
                                       id="use-diagnosis-${questionId}"
                                       data-question-id="${questionId}">
                                <label class="form-check-label" for="use-diagnosis-${questionId}">
                                    Usar mi respuesta para el diagnóstico
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- Difference Alert -->
            <div class="answer-diff-alert" data-question-id="${questionId}">
                <i class="fas fa-exclamation-triangle"></i>
                <span>Tu respuesta difiere de la del emprendedor</span>
            </div>
        `;
    }

    // Validate completion
    function validateCompletion() {
        const totalQuestions = Object.keys(starterAnswers).length;
        const answeredQuestions = Object.keys(coordinatorAnswers).filter(id => 
            coordinatorAnswers[id] && coordinatorAnswers[id].trim() !== ''
        ).length;

        return totalQuestions === answeredQuestions;
    }

    // Show toast notification
    function showToast(message, type = 'info') {
        // Use the Phoenix Admin toast utility if available
        if (window.phoenixIsRTL !== undefined && window.toast) {
            window.toast[type](message);
        } else {
            // Fallback to console
            console.log(`[Toast ${type}]:`, message);
        }
    }

    // Cleanup on destroy
    function destroy() {
        if (autoSaveTimer) {
            clearInterval(autoSaveTimer);
        }
        if (isDirty) {
            saveDraft(false);
        }
    }

    // Public API
    return {
        init: init,
        renderDualAnswerLayout: renderDualAnswerLayout,
        saveDraft: saveDraft,
        validateCompletion: validateCompletion,
        destroy: destroy
    };
})();