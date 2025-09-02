// Form Review Module for Coordinator Journey
window.FormReview = (function() {
    'use strict';

    // State management
    let submissionData = null;
    let currentReviewId = null;
    let feedbackList = [];
    let isDirty = false;

    // Initialize the review system
    function init(submissionId) {
        if (!submissionId) {
            console.error('Submission ID is required');
            return;
        }

        loadSubmissionDetails(submissionId);
        setupEventHandlers();
        setupAutoSave();
    }

    // Setup event handlers
    function setupEventHandlers() {
        // Action buttons
        document.getElementById('btnApprove').addEventListener('click', handleApprove);
        document.getElementById('btnRequestChanges').addEventListener('click', showRequestChangesModal);
        document.getElementById('btnFlag').addEventListener('click', handleFlag);
        
        // Feedback modal
        document.getElementById('btnSaveFeedback').addEventListener('click', saveFeedback);
        document.getElementById('btnAddGeneralFeedback').addEventListener('click', () => showFeedbackModal(null, null));
        
        // Request changes modal
        document.getElementById('btnConfirmChanges').addEventListener('click', handleRequestChanges);
        
        // Set minimum date for deadline
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        document.getElementById('newDeadline').min = tomorrow.toISOString().split('T')[0];
        
        // Track changes
        document.getElementById('generalComments').addEventListener('input', () => isDirty = true);
    }

    // Setup auto-save for general comments
    function setupAutoSave() {
        let saveTimeout;
        document.getElementById('generalComments').addEventListener('input', function() {
            clearTimeout(saveTimeout);
            saveTimeout = setTimeout(() => {
                // Auto-save logic could be implemented here
                console.log('Auto-saving comments...');
            }, 2000);
        });
    }

    // Load submission details
    async function loadSubmissionDetails(submissionId) {
        try {
            const response = await fetch(`/Coordination/FormReview/GetSubmissionDetails/${submissionId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load submission details');
            }

            submissionData = await response.json();
            renderSubmissionContent();
            renderReviewHistory();
            enableActionButtons();
            
        } catch (error) {
            console.error('Error loading submission:', error);
            toastr.error('Error al cargar los detalles del formulario', 'Error');
            showErrorState();
        }
    }

    // Render submission content
    function renderSubmissionContent() {
        // Hide loader, show content
        document.getElementById('formContentLoader').style.display = 'none';
        document.getElementById('formContentContainer').style.display = 'block';
        
        // Update participant info
        const initials = getInitials(submissionData.userName);
        document.getElementById('participantAvatar').textContent = initials;
        document.getElementById('participantName').textContent = submissionData.userName;
        document.getElementById('participantEmail').textContent = submissionData.userEmail;
        
        // Update progress
        const progress = submissionData.completionPercentage || 0;
        document.getElementById('progressFill').style.width = `${progress}%`;
        document.getElementById('progressText').textContent = `${progress}%`;
        
        // Update review status
        updateReviewStatus(submissionData.currentReviewStatus);
        
        // Render form blocks
        renderFormBlocks();
    }

    // Render form blocks and questions
    function renderFormBlocks() {
        const container = document.getElementById('formBlocks');
        
        if (!submissionData.blocks || submissionData.blocks.length === 0) {
            container.innerHTML = `
                <div class="alert alert-info">
                    <i class="fas fa-info-circle me-2"></i>
                    No hay bloques de preguntas disponibles para revisar.
                </div>
            `;
            return;
        }
        
        let html = '';
        submissionData.blocks.forEach(block => {
            const blockFeedback = getFeedbackSummary(block.feedback);
            html += `
                <div class="form-block" data-block-id="${block.blockId}">
                    <h4>
                        ${block.blockName}
                        ${blockFeedback.count > 0 ? `
                            <span class="block-feedback-indicator ${blockFeedback.class}">
                                ${blockFeedback.count}
                            </span>
                        ` : ''}
                        <button class="feedback-button ms-auto" onclick="FormReview.showFeedbackModal(${block.blockId}, null)">
                            <i class="fas fa-comment me-1"></i> Comentar Bloque
                        </button>
                    </h4>
                    ${block.blockDescription ? `<p class="text-muted small mb-3">${block.blockDescription}</p>` : ''}
                    
                    <div class="questions-container">
                        ${renderQuestions(block)}
                    </div>
                    
                    ${renderBlockFeedback(block.feedback)}
                </div>
            `;
        });
        
        container.innerHTML = html;
    }

    // Render questions within a block
    function renderQuestions(block) {
        if (!block.questions || block.questions.length === 0) {
            return '<p class="text-muted">No hay preguntas en este bloque.</p>';
        }
        
        let html = '';
        block.questions.forEach(question => {
            const hasFeedback = question.feedback && question.feedback.length > 0;
            html += `
                <div class="question-item ${hasFeedback ? 'has-feedback' : ''}" data-question-id="${question.questionId}">
                    <div class="question-label">
                        ${question.isRequired ? '<span class="required-indicator">*</span>' : ''}
                        <span>${question.questionText}</span>
                    </div>
                    
                    <div class="question-answer ${!question.answer ? 'empty' : ''}">
                        ${question.answer || 'Sin respuesta'}
                    </div>
                    
                    <button class="feedback-button" onclick="FormReview.showFeedbackModal(${block.blockId}, ${question.questionId})">
                        <i class="fas fa-comment me-1"></i> Comentar
                    </button>
                    
                    ${renderQuestionFeedback(question.feedback)}
                </div>
            `;
        });
        
        return html;
    }

    // Render feedback for a block
    function renderBlockFeedback(feedbackItems) {
        if (!feedbackItems || feedbackItems.length === 0) {
            return '';
        }
        
        let html = '<div class="feedback-list">';
        feedbackItems.forEach(feedback => {
            html += renderFeedbackItem(feedback);
        });
        html += '</div>';
        
        return html;
    }

    // Render feedback for a question
    function renderQuestionFeedback(feedbackItems) {
        if (!feedbackItems || feedbackItems.length === 0) {
            return '';
        }
        
        let html = '<div class="feedback-list">';
        feedbackItems.forEach(feedback => {
            html += renderFeedbackItem(feedback);
        });
        html += '</div>';
        
        return html;
    }

    // Render individual feedback item
    function renderFeedbackItem(feedback) {
        const typeClass = getFeedbackTypeClass(feedback.feedbackType);
        const typeIcon = getFeedbackTypeIcon(feedback.feedbackType);
        
        return `
            <div class="feedback-item ${typeClass}">
                <div class="d-flex align-items-start gap-2">
                    <i class="${typeIcon}"></i>
                    <div class="flex-grow-1">
                        <div>${feedback.feedbackText}</div>
                        <div class="feedback-meta">
                            ${feedback.reviewerName} • ${formatDate(feedback.createdAt)}
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Render review history
    function renderReviewHistory() {
        const container = document.getElementById('reviewTimeline');
        
        if (!submissionData.reviewHistory || submissionData.reviewHistory.length === 0) {
            return;
        }
        
        let html = '';
        submissionData.reviewHistory.forEach(review => {
            const statusIcon = getStatusIcon(review.status);
            const statusText = getStatusText(review.status);
            
            html += `
                <div class="timeline-item">
                    <div class="timeline-date">${formatDate(review.reviewedAt)}</div>
                    <div class="timeline-content">
                        <div class="d-flex align-items-center gap-2 mb-1">
                            <i class="${statusIcon}"></i>
                            <strong>${statusText}</strong>
                        </div>
                        <div class="small">Por: ${review.reviewerName}</div>
                        ${review.comments ? `<div class="mt-2">${review.comments}</div>` : ''}
                        ${review.feedbackCount > 0 ? `
                            <div class="mt-2">
                                <span class="badge bg-secondary">${review.feedbackCount} comentarios</span>
                            </div>
                        ` : ''}
                    </div>
                </div>
            `;
        });
        
        container.innerHTML = html;
    }

    // Show feedback modal
    function showFeedbackModal(blockId, questionId) {
        document.getElementById('feedbackBlockId').value = blockId || '';
        document.getElementById('feedbackQuestionId').value = questionId || '';
        document.getElementById('feedbackText').value = '';
        document.getElementById('typeInfo').checked = true;
        
        const modal = new bootstrap.Modal(document.getElementById('feedbackModal'));
        modal.show();
    }

    // Save feedback
    async function saveFeedback() {
        const blockId = document.getElementById('feedbackBlockId').value;
        const questionId = document.getElementById('feedbackQuestionId').value;
        const feedbackText = document.getElementById('feedbackText').value.trim();
        const feedbackType = parseInt(document.querySelector('input[name="feedbackType"]:checked').value);
        
        if (!feedbackText) {
            document.getElementById('feedbackText').classList.add('is-invalid');
            return;
        }
        
        try {
            const response = await fetch('/Coordination/FormReview/AddFeedback', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({
                    reviewId: currentReviewId || 0,
                    blockId: blockId || null,
                    questionId: questionId || null,
                    feedbackText: feedbackText,
                    feedbackType: feedbackType
                })
            });
            
            if (!response.ok) {
                throw new Error('Failed to save feedback');
            }
            
            const result = await response.json();
            
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById('feedbackModal')).hide();
            
            // Show success message
            toastr.success('Retroalimentación agregada exitosamente', 'Éxito');
            
            // Reload submission to show new feedback
            await loadSubmissionDetails(window.submissionId);
            
        } catch (error) {
            console.error('Error saving feedback:', error);
            toastr.error('Error al guardar la retroalimentación', 'Error');
        }
    }

    // Handle approve action
    async function handleApprove() {
        const result = await Swal.fire({
            title: '¿Aprobar formulario?',
            text: 'El participante será notificado y el formulario quedará marcado como aprobado.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sí, aprobar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#198754'
        });
        
        if (!result.isConfirmed) return;
        
        const comments = document.getElementById('generalComments').value.trim();
        
        try {
            const response = await fetch('/Coordination/FormReview/Approve', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({
                    submissionId: window.submissionId,
                    comments: comments || null
                })
            });
            
            if (!response.ok) {
                throw new Error('Failed to approve submission');
            }
            
            await Swal.fire({
                title: 'Aprobado',
                text: 'El formulario ha sido aprobado exitosamente.',
                icon: 'success',
                confirmButtonText: 'OK'
            });
            
            // Redirect back to review list
            window.location.href = '/Coordination/FormReview';
            
        } catch (error) {
            console.error('Error approving submission:', error);
            toastr.error('Error al aprobar el formulario', 'Error');
        }
    }

    // Show request changes modal
    function showRequestChangesModal() {
        document.getElementById('changesComments').value = '';
        
        // Set default deadline to 7 days from now
        const defaultDeadline = new Date();
        defaultDeadline.setDate(defaultDeadline.getDate() + 7);
        document.getElementById('newDeadline').value = defaultDeadline.toISOString().split('T')[0];
        
        const modal = new bootstrap.Modal(document.getElementById('requestChangesModal'));
        modal.show();
    }

    // Handle request changes
    async function handleRequestChanges() {
        const comments = document.getElementById('changesComments').value.trim();
        const newDeadline = document.getElementById('newDeadline').value;
        
        if (!comments || !newDeadline) {
            toastr.warning('Por favor complete todos los campos requeridos', 'Atención');
            return;
        }
        
        try {
            const response = await fetch('/Coordination/FormReview/RequestChanges', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({
                    submissionId: window.submissionId,
                    comments: comments,
                    newDeadline: new Date(newDeadline).toISOString()
                })
            });
            
            if (!response.ok) {
                throw new Error('Failed to request changes');
            }
            
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById('requestChangesModal')).hide();
            
            await Swal.fire({
                title: 'Cambios Solicitados',
                text: 'Se han solicitado cambios al participante. Será notificado por correo electrónico.',
                icon: 'success',
                confirmButtonText: 'OK'
            });
            
            // Redirect back to review list
            window.location.href = '/Coordination/FormReview';
            
        } catch (error) {
            console.error('Error requesting changes:', error);
            toastr.error('Error al solicitar cambios', 'Error');
        }
    }

    // Handle flag for review
    async function handleFlag() {
        const result = await Swal.fire({
            title: 'Marcar para revisión especial',
            text: 'Este formulario será marcado para una revisión más detallada.',
            icon: 'info',
            showCancelButton: true,
            confirmButtonText: 'Marcar',
            cancelButtonText: 'Cancelar'
        });
        
        if (!result.isConfirmed) return;
        
        // TODO: Implement flag functionality
        toastr.info('Formulario marcado para revisión especial', 'Marcado');
    }

    // Helper functions
    function getInitials(name) {
        if (!name) return '?';
        const parts = name.split(' ');
        if (parts.length >= 2) {
            return parts[0][0] + parts[1][0];
        }
        return name.substring(0, 2).toUpperCase();
    }

    function updateReviewStatus(status) {
        const statusElement = document.getElementById('reviewStatus');
        let statusClass = 'status-pending';
        let statusText = 'Pendiente de Revisión';
        let statusIcon = 'fas fa-clock';
        
        switch (status) {
            case 'Approved':
                statusClass = 'status-approved';
                statusText = 'Aprobado';
                statusIcon = 'fas fa-check-circle';
                break;
            case 'ChangesRequested':
                statusClass = 'status-changes-requested';
                statusText = 'Cambios Solicitados';
                statusIcon = 'fas fa-edit';
                break;
            case 'Flagged':
                statusClass = 'status-flagged';
                statusText = 'Marcado para Revisión';
                statusIcon = 'fas fa-flag';
                break;
        }
        
        statusElement.className = `review-status ${statusClass}`;
        statusElement.innerHTML = `<i class="${statusIcon} me-1"></i> ${statusText}`;
    }

    function getFeedbackSummary(feedbackItems) {
        if (!feedbackItems || feedbackItems.length === 0) {
            return { count: 0, class: '' };
        }
        
        let hasError = false;
        let hasWarning = false;
        
        feedbackItems.forEach(item => {
            if (item.feedbackType === 'Error' || item.feedbackType === 2) hasError = true;
            if (item.feedbackType === 'Warning' || item.feedbackType === 1) hasWarning = true;
        });
        
        let className = 'has-feedback-info';
        if (hasError) className = 'has-feedback-error';
        else if (hasWarning) className = 'has-feedback-warning';
        
        return {
            count: feedbackItems.length,
            class: className
        };
    }

    function getFeedbackTypeClass(type) {
        if (type === 'Error' || type === 2) return 'feedback-error';
        if (type === 'Warning' || type === 1) return 'feedback-warning';
        return 'feedback-info';
    }

    function getFeedbackTypeIcon(type) {
        if (type === 'Error' || type === 2) return 'fas fa-times-circle text-danger';
        if (type === 'Warning' || type === 1) return 'fas fa-exclamation-triangle text-warning';
        return 'fas fa-info-circle text-info';
    }

    function getStatusIcon(status) {
        switch (status) {
            case 'Approved': return 'fas fa-check-circle text-success';
            case 'ChangesRequested': return 'fas fa-edit text-warning';
            case 'Flagged': return 'fas fa-flag text-danger';
            default: return 'fas fa-clock text-secondary';
        }
    }

    function getStatusText(status) {
        switch (status) {
            case 'Approved': return 'Aprobado';
            case 'ChangesRequested': return 'Cambios Solicitados';
            case 'Flagged': return 'Marcado para Revisión';
            default: return 'Pendiente';
        }
    }

    function formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        const options = { 
            year: 'numeric', 
            month: 'short', 
            day: 'numeric', 
            hour: '2-digit', 
            minute: '2-digit' 
        };
        return date.toLocaleDateString('es-ES', options);
    }

    function enableActionButtons() {
        document.getElementById('btnApprove').disabled = false;
        document.getElementById('btnRequestChanges').disabled = false;
        document.getElementById('btnFlag').disabled = false;
    }

    function showErrorState() {
        document.getElementById('formContentLoader').innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Error al cargar el formulario. Por favor, intente nuevamente.
            </div>
            <a href="/Coordination/FormReview" class="btn btn-primary">
                <i class="fas fa-arrow-left me-1"></i> Volver a la lista
            </a>
        `;
    }

    // Public API
    return {
        init: init,
        showFeedbackModal: showFeedbackModal
    };
})();