class EntrepreneurFormWizard {
    constructor() {
        // Configuration
        this.submissionExternalId = $('#submissionExternalId').val();
        this.phase = $('#currentPhase').val();
        
        // State
        this.currentStep = 1;
        this.totalSteps = $('.nav-wizard .nav-item').length;
        this.isDirty = false;
        this.isSubmitting = false;
        
        // Managers
        this.autoSaveManager = new FormAutoSaveManager(this);
        this.validator = new FormValidator();
        
        // Initialize
        this.init();
    }
    
    init() {
        this.loadDraftData();
        this.initializeWizard();
        this.attachEventHandlers();
        this.updateProgress();
        this.startAutoSave();
    }
    
    initializeWizard() {
        // Phoenix wizard is initialized via data attributes
        // We enhance it with our custom logic
        
        // Listen to tab changes
        $('.nav-wizard .nav-link').on('shown.bs.tab', (e) => {
            this.onStepChange(e);
        });
    }
    
    attachEventHandlers() {
        // Input changes
        $(document).on('change', '.question-input', () => {
            this.isDirty = true;
            this.updateProgress();
        });
        
        // Navigation buttons
        $('#btnPrevious').on('click', () => this.navigateToPrevious());
        $('#btnNext').on('click', () => this.navigateToNext());
        $('#btnSaveDraft').on('click', () => this.saveDraft(true));
        $('#btnSubmit').on('click', () => this.submitForm());
        
        // Prevent navigation if dirty
        window.addEventListener('beforeunload', (e) => {
            if (this.isDirty && !this.isSubmitting) {
                e.preventDefault();
                e.returnValue = '¿Está seguro de salir? Los cambios no guardados se perderán.';
            }
        });
        
        // Keyboard navigation
        $(document).on('keydown', (e) => {
            if (e.ctrlKey && e.key === 's') {
                e.preventDefault();
                this.saveDraft(true);
            }
        });
    }
    
    onStepChange(event) {
        const newStep = parseInt($(event.target).data('wizard-step'));
        const previousStep = this.currentStep;
        
        // Validate if moving forward
        if (newStep > previousStep) {
            if (!this.validateStep(previousStep)) {
                event.preventDefault();
                $(`.nav-wizard .nav-link[data-wizard-step="${previousStep}"]`).tab('show');
                return false;
            }
        }
        
        this.currentStep = newStep;
        this.updateNavigationButtons();
        
        // Auto-save when changing steps
        if (this.isDirty) {
            this.saveDraft(false);
        }
    }
    
    validateStep(stepNumber) {
        const form = document.querySelector(`#form-step-${stepNumber}`);
        if (!form) return true;
        
        // Bootstrap validation
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            
            // Focus first invalid field
            const firstInvalid = form.querySelector(':invalid');
            if (firstInvalid) {
                firstInvalid.focus();
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            
            showToast('Por favor complete todos los campos requeridos', 'warning');
            return false;
        }
        
        // Custom validation
        return this.validator.validateBlock(form);
    }
    
    updateNavigationButtons() {
        const btnPrevious = $('#btnPrevious');
        const btnNext = $('#btnNext');
        const btnSubmit = $('#btnSubmit');
        
        // Previous button
        btnPrevious.prop('disabled', this.currentStep === 1);
        
        // Next/Submit buttons
        if (this.currentStep === this.totalSteps) {
            btnNext.addClass('d-none');
            btnSubmit.removeClass('d-none');
        } else {
            btnNext.removeClass('d-none');
            btnSubmit.addClass('d-none');
        }
    }
    
    navigateToPrevious() {
        if (this.currentStep > 1) {
            const targetStep = this.currentStep - 1;
            $(`.nav-wizard .nav-link[data-wizard-step="${targetStep}"]`).tab('show');
        }
    }
    
    navigateToNext() {
        if (this.currentStep < this.totalSteps) {
            if (!this.validateStep(this.currentStep)) {
                return;
            }
            
            const targetStep = this.currentStep + 1;
            $(`.nav-wizard .nav-link[data-wizard-step="${targetStep}"]`).tab('show');
        }
    }
    
    updateProgress() {
        const stats = this.calculateProgress();
        
        // Update progress bar
        $('#progressBar')
            .css('width', stats.percentage + '%')
            .attr('aria-valuenow', stats.percentage);
        
        // Update counters
        $('#progressPercent').text(stats.percentage);
        $('#answeredCount').text(stats.answered);
        $('#totalCount').text(stats.total);
        
        // Update step completion status
        this.updateStepCompletionStatus();
        
        return stats;
    }
    
    calculateProgress() {
        const allInputs = $('.question-input');
        const answeredInputs = allInputs.filter(function() {
            const input = $(this);
            if (input.is(':checkbox') || input.is(':radio')) {
                return input.is(':checked');
            }
            return input.val() && input.val().trim() !== '';
        });
        
        const total = allInputs.length;
        const answered = answeredInputs.length;
        const percentage = total > 0 ? Math.round((answered / total) * 100) : 0;
        
        return { total, answered, percentage };
    }
    
    updateStepCompletionStatus() {
        $('.tab-pane').each((index, element) => {
            const block = $(element);
            const blockId = block.data('block-id');
            const isComplete = this.isBlockComplete(block);
            
            const navItem = $(`.nav-wizard .nav-link[href="#block-${blockId}"]`).parent();
            if (isComplete) {
                navItem.addClass('done');
                navItem.find('.fas')
                    .removeClass('fa-circle')
                    .addClass('fa-check-circle');
            } else {
                navItem.removeClass('done');
                navItem.find('.fas')
                    .removeClass('fa-check-circle')
                    .addClass('fa-circle');
            }
        });
    }
    
    isBlockComplete(block) {
        const blockInputs = block.find('.question-input[required]');
        if (blockInputs.length === 0) return true;
        
        const answeredRequired = blockInputs.filter(function() {
            const input = $(this);
            if (input.is(':checkbox') || input.is(':radio')) {
                const name = input.attr('name');
                return $(`input[name="${name}"]:checked`).length > 0;
            }
            return input.val() && input.val().trim() !== '';
        });
        
        return answeredRequired.length === blockInputs.length;
    }
    
    collectFormData() {
        const blockResponses = [];
        
        $('.tab-pane').each((index, block) => {
            const $block = $(block);
            const blockId = $block.data('block-id');
            const blockName = $block.find('h5').first().text();
            
            const questionResponses = [];
            
            $block.find('.question-container').each((qIndex, container) => {
                const $container = $(container);
                const questionId = $container.data('question-id');
                const questionText = $container.find('.form-label').first().text().replace('*', '').trim();
                const answerType = $container.data('answer-type');
                
                const response = this.collectQuestionAnswer($container);
                
                questionResponses.push({
                    questionId: questionId,
                    questionText: questionText,
                    answer: response.answer,
                    answerType: answerType,
                    isAnswered: response.isAnswered
                });
            });
            
            blockResponses.push({
                blockId: blockId,
                blockName: blockName,
                questionResponses: questionResponses
            });
        });
        
        return {
            formVersion: 1,
            lastSavedAt: new Date().toISOString(),
            currentBlockIndex: this.currentStep - 1,
            progressPercentage: this.calculateProgress().percentage,
            blockResponses: blockResponses
        };
    }
    
    collectQuestionAnswer(container) {
        const inputs = container.find('.question-input');
        let answer = '';
        let isAnswered = false;
        
        if (inputs.is(':checkbox')) {
            const checked = inputs.filter(':checked');
            if (checked.length > 0) {
                answer = checked.map(function() {
                    return $(this).val();
                }).get().join(',');
                isAnswered = true;
            }
        } else if (inputs.is(':radio')) {
            const checked = inputs.filter(':checked');
            if (checked.length > 0) {
                answer = checked.val();
                isAnswered = true;
            }
        } else {
            answer = inputs.val() || '';
            isAnswered = answer.trim() !== '';
        }
        
        return { answer, isAnswered };
    }
    
    async saveDraft(showNotification = true) {
        if (!this.isDirty) {
            if (showNotification) {
                showToast('No hay cambios para guardar', 'info');
            }
            return;
        }
        
        const progress = this.updateProgress();
        const draftData = this.collectFormData();
        
        try {
            this.autoSaveManager.showSaving();
            
            const response = await $.ajax({
                url: `/Entrepreneur/Form/${this.submissionExternalId}/SaveDraft`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    draftData: JSON.stringify(draftData),
                    answeredQuestions: progress.answered,
                    totalQuestions: progress.total
                })
            });
            
            if (response.success) {
                this.isDirty = false;
                this.autoSaveManager.showSaved(response.lastSaved);
                
                if (showNotification) {
                    showToast('Borrador guardado exitosamente', 'success');
                }
            } else {
                throw new Error(response.message || 'Error al guardar');
            }
        } catch (error) {
            console.error('Error saving draft:', error);
            this.autoSaveManager.showError();
            
            if (showNotification) {
                showToast('Error al guardar el borrador', 'error');
            }
        }
    }
    
    async submitForm() {
        // Validate all steps
        for (let i = 1; i <= this.totalSteps; i++) {
            if (!this.validateStep(i)) {
                // Navigate to first invalid step
                $(`.nav-wizard .nav-link[data-wizard-step="${i}"]`).tab('show');
                showToast('Por favor complete todos los campos requeridos', 'warning');
                return;
            }
        }
        
        // Confirm submission using Bootstrap modal
        const confirmed = await showConfirmModal(
            '¿Enviar formulario?',
            'Una vez enviado, no podrá editarlo hasta que sea revisado.',
            'Sí, enviar',
            'Cancelar'
        );
        
        if (!confirmed) {
            return;
        }
        
        this.isSubmitting = true;
        
        // Save draft first
        await this.saveDraft(false);
        
        try {
            const response = await $.ajax({
                url: `/Entrepreneur/Form/${this.submissionExternalId}/Submit`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({})
            });
            
            if (response.success) {
                this.isDirty = false;
                
                // Show success modal
                await showSuccessModal(
                    '¡Formulario enviado!',
                    'Su formulario ha sido enviado exitosamente.'
                );
                
                // Redirect to dashboard
                window.location.href = response.redirectUrl || '/Starter/Dashboard';
            } else {
                throw new Error(response.message || 'Error al enviar');
            }
        } catch (error) {
            console.error('Error submitting form:', error);
            this.isSubmitting = false;
            
            showErrorModal(
                'Error',
                'No se pudo enviar el formulario. Por favor intente nuevamente.'
            );
        }
    }
    
    loadDraftData() {
        const draftDataElement = $('#draftData');
        if (!draftDataElement.length || !draftDataElement.val()) {
            return;
        }
        
        try {
            const draftData = JSON.parse(draftDataElement.val());
            this.restoreDraftData(draftData);
        } catch (error) {
            console.error('Error loading draft data:', error);
        }
    }
    
    restoreDraftData(draftData) {
        if (!draftData.blockResponses) return;
        
        draftData.blockResponses.forEach(block => {
            block.questionResponses.forEach(question => {
                if (question.isAnswered && question.answer) {
                    this.restoreQuestionAnswer(question);
                }
            });
        });
        
        this.updateProgress();
        this.isDirty = false;
    }
    
    restoreQuestionAnswer(question) {
        const container = $(`.question-container[data-question-id="${question.questionId}"]`);
        const inputs = container.find('.question-input');
        
        if (inputs.is(':checkbox')) {
            inputs.prop('checked', false);
            const values = question.answer.split(',');
            values.forEach(value => {
                inputs.filter(`[value="${value.trim()}"]`).prop('checked', true);
            });
        } else if (inputs.is(':radio')) {
            inputs.filter(`[value="${question.answer}"]`).prop('checked', true);
        } else {
            inputs.val(question.answer);
        }
    }
    
    startAutoSave() {
        // Auto-save every 30 seconds
        setInterval(() => {
            if (this.isDirty && !this.isSubmitting) {
                this.saveDraft(false);
            }
        }, 30000);
    }
}

// Helper function for toast notifications
function showToast(message, type = 'info') {
    // This should integrate with your existing toast system
    const toastContainer = $('#toastContainer');
    if (toastContainer.length === 0) {
        // Create toast container if it doesn't exist
        $('body').append('<div id="toastContainer" class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
    }
    
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0" 
             role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                        data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    const toastElement = $(toastHtml);
    $('#toastContainer').append(toastElement);
    const toast = new bootstrap.Toast(toastElement[0]);
    toast.show();
}

// Helper function for confirmation modal using Bootstrap
function showConfirmModal(title, message, confirmText = 'Confirmar', cancelText = 'Cancelar') {
    return new Promise((resolve) => {
        // Create modal HTML
        const modalHtml = `
            <div class="modal fade" id="confirmModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p>${message}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
                            <button type="button" class="btn btn-primary" id="confirmBtn">${confirmText}</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        // Remove any existing modal
        $('#confirmModal').remove();
        
        // Add modal to body
        $('body').append(modalHtml);
        
        const modalElement = document.getElementById('confirmModal');
        const modal = new bootstrap.Modal(modalElement);
        
        // Handle confirm button
        $('#confirmBtn').on('click', () => {
            modal.hide();
            resolve(true);
        });
        
        // Handle modal hidden event
        modalElement.addEventListener('hidden.bs.modal', () => {
            $('#confirmModal').remove();
            resolve(false);
        });
        
        modal.show();
    });
}

// Helper function for success modal
function showSuccessModal(title, message) {
    return new Promise((resolve) => {
        const modalHtml = `
            <div class="modal fade" id="successModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header border-0">
                            <h5 class="modal-title">${title}</h5>
                        </div>
                        <div class="modal-body text-center">
                            <div class="mb-3">
                                <i class="fas fa-check-circle text-success" style="font-size: 4rem;"></i>
                            </div>
                            <p>${message}</p>
                        </div>
                        <div class="modal-footer border-0 justify-content-center">
                            <button type="button" class="btn btn-success" data-bs-dismiss="modal">Aceptar</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        $('#successModal').remove();
        $('body').append(modalHtml);
        
        const modalElement = document.getElementById('successModal');
        const modal = new bootstrap.Modal(modalElement);
        
        modalElement.addEventListener('hidden.bs.modal', () => {
            $('#successModal').remove();
            resolve();
        });
        
        modal.show();
    });
}

// Helper function for error modal
function showErrorModal(title, message) {
    const modalHtml = `
        <div class="modal fade" id="errorModal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header border-0">
                        <h5 class="modal-title text-danger">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body text-center">
                        <div class="mb-3">
                            <i class="fas fa-exclamation-circle text-danger" style="font-size: 4rem;"></i>
                        </div>
                        <p>${message}</p>
                    </div>
                    <div class="modal-footer border-0 justify-content-center">
                        <button type="button" class="btn btn-danger" data-bs-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    $('#errorModal').remove();
    $('body').append(modalHtml);
    
    const modalElement = document.getElementById('errorModal');
    const modal = new bootstrap.Modal(modalElement);
    
    modalElement.addEventListener('hidden.bs.modal', () => {
        $('#errorModal').remove();
    });
    
    modal.show();
}