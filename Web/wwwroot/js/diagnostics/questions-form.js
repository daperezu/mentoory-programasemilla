(function () {
    'use strict';

    // Answer types that support options
    const TYPES_WITH_OPTIONS = [3, 4]; // SingleChoice = 3, MultiChoice = 4

    let answerOptionIndex = 0;

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        initializeAnswerTypeHandler();
        initializeAnswerOptionsHandlers();
        
        // If editing, set the initial index based on existing options
        const existingOptions = document.querySelectorAll('.answer-option-item');
        if (existingOptions.length > 0) {
            answerOptionIndex = existingOptions.length;
        }

        // Check initial answer type if editing
        if (window.initialAnswerType !== undefined) {
            checkAnswerTypeForOptions(window.initialAnswerType);
        }
    });

    function initializeAnswerTypeHandler() {
        const answerTypeSelect = document.querySelector('select[name="AnswerType"]');
        if (answerTypeSelect) {
            answerTypeSelect.addEventListener('change', function () {
                const selectedType = parseInt(this.value);
                checkAnswerTypeForOptions(selectedType);
            });
        }
    }

    function checkAnswerTypeForOptions(answerType) {
        const answerOptionsSection = document.getElementById('answerOptionsSection');
        const answerOptionsContainer = document.getElementById('answerOptionsContainer');
        
        if (!answerOptionsSection) return;

        if (TYPES_WITH_OPTIONS.includes(answerType)) {
            // Show options section
            answerOptionsSection.style.display = 'block';
            
            // If no options exist, add one by default
            if (answerOptionsContainer && answerOptionsContainer.children.length === 0) {
                addAnswerOption();
            }
        } else {
            // Hide options section and clear options
            answerOptionsSection.style.display = 'none';
            if (answerOptionsContainer) {
                answerOptionsContainer.innerHTML = '';
            }
            answerOptionIndex = 0;
        }
    }

    function initializeAnswerOptionsHandlers() {
        // Add answer option button
        const addButton = document.getElementById('addAnswerOption');
        if (addButton) {
            addButton.addEventListener('click', function () {
                addAnswerOption();
            });
        }

        // Remove answer option buttons (for existing options)
        document.addEventListener('click', function (e) {
            if (e.target.classList.contains('remove-answer-option') || 
                e.target.closest('.remove-answer-option')) {
                e.preventDefault();
                const optionItem = e.target.closest('.answer-option-item');
                if (optionItem) {
                    removeAnswerOption(optionItem);
                }
            }
        });
    }

    function addAnswerOption() {
        const container = document.getElementById('answerOptionsContainer');
        if (!container) return;

        const optionHtml = createAnswerOptionHtml(answerOptionIndex);
        container.insertAdjacentHTML('beforeend', optionHtml);
        
        // Re-index all options to maintain order
        reindexAnswerOptions();
        
        answerOptionIndex++;
    }

    function createAnswerOptionHtml(index) {
        return `
            <div class="answer-option-item border rounded p-3 mb-2" data-index="${index}">
                <div class="row g-2">
                    <div class="col-md-6">
                        <label class="form-label">Texto de la opción</label>
                        <input type="text" name="AnswerOptions[${index}].Text" class="form-control" required />
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Puntaje</label>
                        <input type="number" name="AnswerOptions[${index}].Score" value="0" class="form-control" />
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Orden</label>
                        <input type="number" name="AnswerOptions[${index}].Order" value="${index}" class="form-control" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Tipo FODA</label>
                        <select name="AnswerOptions[${index}].Foda" class="form-select">
                            <option value="0">None</option>
                            <option value="1">Fortaleza</option>
                            <option value="2">Oportunidad</option>
                            <option value="3">Debilidad</option>
                            <option value="4">Amenaza</option>
                        </select>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Explicación FODA</label>
                        <input type="text" name="AnswerOptions[${index}].FodaExplanation" class="form-control" required />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Tipo ODSR</label>
                        <select name="AnswerOptions[${index}].Odsr" class="form-select">
                            <option value="0">None</option>
                            <option value="1">Objetivos</option>
                            <option value="2">Diseños y Actuaciones</option>
                            <option value="3">Seguimiento y Reevaluación</option>
                        </select>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Explicación ODSR</label>
                        <input type="text" name="AnswerOptions[${index}].OdsrExplanation" class="form-control" required />
                    </div>
                    <div class="col-12">
                        <label class="form-label">Pregunta de seguimiento (opcional)</label>
                        <input type="text" name="AnswerOptions[${index}].FollowupQuestionText" class="form-control" />
                    </div>
                    <div class="col-12">
                        <button type="button" class="btn btn-danger btn-sm remove-answer-option">
                            <i class="fa-solid fa-trash"></i> Eliminar
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    function removeAnswerOption(optionItem) {
        optionItem.remove();
        reindexAnswerOptions();
    }

    function reindexAnswerOptions() {
        const container = document.getElementById('answerOptionsContainer');
        if (!container) return;

        const options = container.querySelectorAll('.answer-option-item');
        options.forEach((option, index) => {
            // Update data-index
            option.setAttribute('data-index', index);
            
            // Update all input names within this option
            const inputs = option.querySelectorAll('input, select');
            inputs.forEach(input => {
                const name = input.getAttribute('name');
                if (name) {
                    // Replace the index in the name (e.g., AnswerOptions[0].Text -> AnswerOptions[1].Text)
                    const newName = name.replace(/\[\d+\]/, `[${index}]`);
                    input.setAttribute('name', newName);
                }
            });
            
            // Update order field if it's empty
            const orderInput = option.querySelector('input[name*=".Order"]');
            if (orderInput && !orderInput.value) {
                orderInput.value = index;
            }
        });
    }
})();