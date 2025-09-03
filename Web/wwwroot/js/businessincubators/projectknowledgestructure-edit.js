// Helper functions for edit operations in Project Knowledge Structure

// Get entity ID from node data (handles both id and entityId)
function getNodeEntityId(node) {
    if (!node || !node.data) return null;
    return node.data.entityId || node.data.id || null;
}

// Show edit dialog based on node type
function showEditDialog(node) {
    const entityId = getNodeEntityId(node);
    if (!entityId) {
        showToast('No se puede editar este elemento', 'danger');
        return;
    }
    
    // Set modal title based on type
    let title = '';
    switch(node.type) {
        case 'module':
            title = 'Editar Módulo';
            $('#editElementContentGroup').hide();
            break;
        case 'topic':
            title = 'Editar Tema';
            $('#editElementContentGroup').hide();
            break;
        case 'subject':
            title = 'Editar Materia';
            $('#editElementContentGroup').show();
            break;
        case 'block':
            title = 'Editar Bloque';
            $('#editElementContentGroup').hide();
            break;
        case 'question':
            showEditQuestionDialog(node);
            return;
        case 'answer':
            showEditAnswerDialog(node);
            return;
        default:
            showToast('Tipo de elemento no válido para edición', 'danger');
            return;
    }
    
    // Populate form
    $('#editElementTitle').text(title);
    $('#editElementId').val(entityId);
    $('#editElementType').val(node.type);
    $('#editElementName').val(node.text);
    $('#editElementOrder').val(node.data.order || 1);
    $('#editElementContent').val(node.data.content || '');
    
    // Show modal
    $('#editElementModal').modal('show');
}

// Show edit question dialog
function showEditQuestionDialog(node) {
    const entityId = getNodeEntityId(node);
    if (!entityId) {
        showToast('No se puede editar esta pregunta', 'danger');
        return;
    }
    
    // Create custom dialog for question editing
    const modalHtml = `
        <div class="modal fade" id="editQuestionModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Editar Pregunta</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form id="editQuestionForm">
                            <input type="hidden" id="editQuestionId" value="${entityId}" />
                            
                            <div class="mb-3">
                                <label for="editQuestionText" class="form-label">Texto de la Pregunta</label>
                                <textarea class="form-control" id="editQuestionText" rows="3" required>${node.text}</textarea>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editQuestionAnswerType" class="form-label">Tipo de Respuesta</label>
                                <select class="form-control" id="editQuestionAnswerType" required>
                                    <option value="0">Texto Corto</option>
                                    <option value="1">Texto Largo</option>
                                    <option value="2">Sí/No</option>
                                    <option value="3">Opción Múltiple</option>
                                    <option value="4">Fecha</option>
                                    <option value="5">Número</option>
                                    <option value="6">Email</option>
                                    <option value="7">URL</option>
                                    <option value="8">Archivo</option>
                                    <option value="9">Lista</option>
                                </select>
                            </div>
                            
                            <div class="mb-3">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" id="editQuestionIsDiagnostic" ${node.data.isUsedForDiagnosis ? 'checked' : ''}>
                                    <label class="form-check-label" for="editQuestionIsDiagnostic">
                                        Usar para diagnóstico
                                    </label>
                                </div>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editQuestionPhase" class="form-label">Fase de Aplicación</label>
                                <select class="form-control" id="editQuestionPhase">
                                    <option value="0">Registro</option>
                                    <option value="1">Evaluación</option>
                                    <option value="2">Ambas</option>
                                </select>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editQuestionOrder" class="form-label">Orden</label>
                                <input type="number" class="form-control" id="editQuestionOrder" min="1" value="${node.data.order || 1}" required>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editQuestionTopicId" class="form-label">Vincular a Tema (opcional)</label>
                                <select class="form-control" id="editQuestionTopicId">
                                    <option value="">-- Sin vincular a tema --</option>
                                </select>
                                <small class="form-text text-muted">
                                    Puede cambiar o eliminar la vinculación de esta pregunta con un tema
                                </small>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnSaveQuestionChanges">
                            <i class="ti ti-device-floppy"></i> Guardar Cambios
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    $('#editQuestionModal').remove();
    $('body').append(modalHtml);
    
    // Set current values
    $('#editQuestionAnswerType').val(getAnswerTypeValue(node.data.answerType));
    $('#editQuestionPhase').val(getPhaseValue(node.data.appliesToPhase));
    
    // Load available topics and set current selection
    loadTopicsForEdit(node.data.topicId);
    
    // Bind save button event
    $(document).off('click', '#btnSaveQuestionChanges').on('click', '#btnSaveQuestionChanges', function() {
        saveQuestionChanges();
    });
    
    // Show modal
    $('#editQuestionModal').modal('show');
}

// Show edit answer dialog
function showEditAnswerDialog(node) {
    const entityId = getNodeEntityId(node);
    if (!entityId) {
        showToast('No se puede editar esta opción de respuesta', 'danger');
        return;
    }
    
    // Create custom dialog for answer editing
    const modalHtml = `
        <div class="modal fade" id="editAnswerModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Editar Opción de Respuesta</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form id="editAnswerForm">
                            <input type="hidden" id="editAnswerId" value="${entityId}" />
                            
                            <div class="mb-3">
                                <label for="editAnswerText" class="form-label">Texto de la Opción</label>
                                <input type="text" class="form-control" id="editAnswerText" value="${node.text}" required>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editAnswerScore" class="form-label">Puntaje</label>
                                <input type="number" class="form-control" id="editAnswerScore" min="0" max="100" value="${node.data.score || 0}" required>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editAnswerOrder" class="form-label">Orden</label>
                                <input type="number" class="form-control" id="editAnswerOrder" min="1" value="${node.data.order || 1}" required>
                            </div>
                            
                            <div class="mb-3">
                                <label for="editAnswerFollowUp" class="form-label">Pregunta de Seguimiento (opcional)</label>
                                <textarea class="form-control" id="editAnswerFollowUp" rows="2" placeholder="¿Alguna pregunta adicional cuando se seleccione esta opción?">${node.data.followUpQuestionText || ''}</textarea>
                            </div>
                            
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="mb-3">
                                        <label for="editAnswerFoda" class="form-label">FODA</label>
                                        <select class="form-control" id="editAnswerFoda">
                                            <option value="78">No Definido</option>
                                            <option value="70">Fortalezas</option>
                                            <option value="79">Oportunidades</option>
                                            <option value="68">Debilidades</option>
                                            <option value="65">Amenazas</option>
                                        </select>
                                    </div>
                                    <div class="mb-3">
                                        <label for="editAnswerFodaExplanation" class="form-label">Explicación FODA (opcional)</label>
                                        <textarea class="form-control" id="editAnswerFodaExplanation" rows="2">${node.data.fodaExplanation || ''}</textarea>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="mb-3">
                                        <label for="editAnswerOdsr" class="form-label">ODSR</label>
                                        <select class="form-control" id="editAnswerOdsr">
                                            <option value="78">No Definido</option>
                                            <option value="79">Oportunidades</option>
                                            <option value="68">Desafíos</option>
                                            <option value="83">Soluciones</option>
                                            <option value="82">Resultados</option>
                                        </select>
                                    </div>
                                    <div class="mb-3">
                                        <label for="editAnswerOdsrExplanation" class="form-label">Explicación ODSR (opcional)</label>
                                        <textarea class="form-control" id="editAnswerOdsrExplanation" rows="2">${node.data.odsrExplanation || ''}</textarea>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnSaveAnswerChanges">
                            <i class="ti ti-device-floppy"></i> Guardar Cambios
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    $('#editAnswerModal').remove();
    $('body').append(modalHtml);
    
    // Set current FODA and ODSR values
    if (node.data.foda) {
        const fodaValue = getFodaValue(node.data.foda);
        $('#editAnswerFoda').val(fodaValue);
    }
    if (node.data.odsr) {
        const odsrValue = getOdsrValue(node.data.odsr);
        $('#editAnswerOdsr').val(odsrValue);
    }
    
    // Bind save button event
    $(document).off('click', '#btnSaveAnswerChanges').on('click', '#btnSaveAnswerChanges', function() {
        saveAnswerChanges();
    });
    
    // Show modal
    $('#editAnswerModal').modal('show');
}

// Load topics for edit dialog
function loadTopicsForEdit(currentTopicId) {
    $.ajax({
        url: `/BusinessIncubators/${businessIncubatorId}/Projects/${projectId}/KnowledgeStructure/Tree`,
        type: 'GET',
        success: function(data) {
            const topicSelect = $('#editQuestionTopicId');
            topicSelect.empty();
            topicSelect.append('<option value="">-- Sin vincular a tema --</option>');
            
            // Extract topics from the tree structure
            if (data && data.length > 0) {
                const root = data[0];
                if (root.children) {
                    root.children.forEach(module => {
                        if (module.type === 'module' && module.children) {
                            const optgroup = $(`<optgroup label="${module.text}"></optgroup>`);
                            module.children.forEach(topic => {
                                if (topic.type === 'topic' && topic.data && topic.data.entityId) {
                                    const selected = currentTopicId && topic.data.entityId === currentTopicId ? 'selected' : '';
                                    optgroup.append(`<option value="${topic.data.entityId}" ${selected}>${topic.text}</option>`);
                                }
                            });
                            if (optgroup.children().length > 0) {
                                topicSelect.append(optgroup);
                            }
                        }
                    });
                }
            }
        },
        error: function() {
            console.error('Error loading topics for edit');
        }
    });
}

// Helper functions
function getAnswerTypeValue(answerType) {
    const types = {
        'ShortText': 0,
        'LongText': 1,
        'YesNo': 2,
        'MultipleChoice': 3,
        'Date': 4,
        'Number': 5,
        'Email': 6,
        'Url': 7,
        'File': 8,
        'List': 9
    };
    return types[answerType] || 0;
}

function getPhaseValue(phase) {
    const phases = {
        'Registration': 0,
        'Evaluation': 1,
        'Both': 2
    };
    return phases[phase] || 0;
}

function getFodaValue(foda) {
    // Convert FODA character or string to ASCII value
    if (typeof foda === 'string') {
        switch(foda.toUpperCase()) {
            case 'F':
            case 'FORTALEZAS':
                return 70;
            case 'O':
            case 'OPORTUNIDADES':
                return 79;
            case 'D':
            case 'DEBILIDADES':
                return 68;
            case 'A':
            case 'AMENAZAS':
                return 65;
            case 'N':
            case 'NONE':
            case 'NODEFINIDO':
            default:
                return 78;
        }
    }
    return 78; // Default to 'N'
}

function getOdsrValue(odsr) {
    // Convert ODSR character or string to ASCII value
    if (typeof odsr === 'string') {
        switch(odsr.toUpperCase()) {
            case 'O':
            case 'OPORTUNIDADES':
                return 79;
            case 'D':
            case 'DESAFIOS':
            case 'DESAFÍOS':
                return 68;
            case 'S':
            case 'SOLUCIONES':
                return 83;
            case 'R':
            case 'RESULTADOS':
                return 82;
            case 'N':
            case 'NONE':
            case 'NODEFINIDO':
            default:
                return 78;
        }
    }
    return 78; // Default to 'N'
}

// Save question changes
function saveQuestionChanges() {
    const questionId = $('#editQuestionId').val();
    const text = $('#editQuestionText').val();
    const answerType = $('#editQuestionAnswerType').val();
    const isDiagnostic = $('#editQuestionIsDiagnostic').is(':checked');
    const phase = $('#editQuestionPhase').val();
    const order = $('#editQuestionOrder').val();
    const topicId = $('#editQuestionTopicId').val();
    
    if (!text || !order) {
        showToast('Por favor complete todos los campos requeridos', 'danger');
        return;
    }
    
    $.ajax({
        url: `/BusinessIncubators/${businessIncubatorId}/Projects/${projectId}/KnowledgeStructure/questions/${questionId}`,
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({
            text: text,
            answerType: parseInt(answerType),
            isUsedForDiagnosis: isDiagnostic,
            appliesToPhase: parseInt(phase),
            order: parseInt(order),
            topicId: topicId ? parseInt(topicId) : null
        }),
        success: function(result) {
            if (result.success) {
                $('#editQuestionModal').modal('hide');
                showToast(result.message, 'success');
                $('#knowledgeStructureTree').jstree('refresh');
            } else {
                showToast(result.message || 'Error al actualizar la pregunta', 'danger');
            }
        },
        error: function(xhr) {
            console.error('Error updating question:', xhr.responseText);
            if (xhr.status === 400 && xhr.responseJSON) {
                const response = xhr.responseJSON;
                let errorMessage = 'Error al actualizar la pregunta';
                if (response.errors) {
                    // Show validation errors
                    const errorMessages = [];
                    for (const field in response.errors) {
                        errorMessages.push(...response.errors[field]);
                    }
                    errorMessage = errorMessages.join(', ');
                } else if (response.message) {
                    errorMessage = response.message;
                }
                showToast(errorMessage, 'danger');
            } else {
                showToast('Error al actualizar la pregunta', 'danger');
            }
        }
    });
}

// Save answer changes
function saveAnswerChanges() {
    const answerId = $('#editAnswerId').val();
    const text = $('#editAnswerText').val();
    const score = $('#editAnswerScore').val();
    const order = $('#editAnswerOrder').val();
    const followUpText = $('#editAnswerFollowUp').val();
    const fodaValue = $('#editAnswerFoda').val();
    const odsrValue = $('#editAnswerOdsr').val();
    const fodaExplanation = $('#editAnswerFodaExplanation').val();
    const odsrExplanation = $('#editAnswerOdsrExplanation').val();
    
    if (!text || !score || !order) {
        showToast('Por favor complete todos los campos requeridos', 'danger');
        return;
    }
    
    $.ajax({
        url: `/BusinessIncubators/${businessIncubatorId}/Projects/${projectId}/KnowledgeStructure/answer-options/${answerId}`,
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({
            text: text,
            score: parseInt(score),
            foda: parseInt(fodaValue),
            fodaExplanation: fodaExplanation || '',
            odsr: parseInt(odsrValue),
            odsrExplanation: odsrExplanation || '',
            order: parseInt(order),
            followUpQuestionText: followUpText || null
        }),
        success: function(result) {
            if (result.success) {
                $('#editAnswerModal').modal('hide');
                showToast(result.message, 'success');
                $('#knowledgeStructureTree').jstree('refresh');
            } else {
                showToast(result.message || 'Error al actualizar la opción de respuesta', 'danger');
            }
        },
        error: function() {
            showToast('Error al actualizar la opción de respuesta', 'danger');
        }
    });
}