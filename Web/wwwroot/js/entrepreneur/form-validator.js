class FormValidator {
    constructor() {
        this.rules = {
            email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
            number: /^\d+$/,
            date: /^\d{4}-\d{2}-\d{2}$/
        };
    }
    
    validateBlock(form) {
        const errors = [];
        
        // Check required fields
        $(form).find('.question-input[required]').each((index, input) => {
            const $input = $(input);
            
            if ($input.is(':checkbox') || $input.is(':radio')) {
                const name = $input.attr('name');
                if ($(`input[name="${name}"]:checked`).length === 0) {
                    errors.push({
                        field: name,
                        message: 'Este campo es requerido'
                    });
                }
            } else {
                const value = $input.val();
                if (!value || value.trim() === '') {
                    errors.push({
                        field: $input.attr('name'),
                        message: 'Este campo es requerido'
                    });
                }
            }
        });
        
        // Check format validations
        $(form).find('input[type="email"]').each((index, input) => {
            const value = $(input).val();
            if (value && !this.rules.email.test(value)) {
                errors.push({
                    field: $(input).attr('name'),
                    message: 'Formato de email inválido'
                });
            }
        });
        
        // Show errors if any
        if (errors.length > 0) {
            console.log('Validation errors:', errors);
            return false;
        }
        
        return true;
    }
    
    validateField(field) {
        const type = field.attr('type');
        const value = field.val();
        
        if (field.attr('required') && (!value || value.trim() === '')) {
            return { valid: false, message: 'Este campo es requerido' };
        }
        
        if (type === 'email' && value && !this.rules.email.test(value)) {
            return { valid: false, message: 'Formato de email inválido' };
        }
        
        if (type === 'number') {
            const min = field.attr('min');
            const max = field.attr('max');
            
            if (min && parseFloat(value) < parseFloat(min)) {
                return { valid: false, message: `El valor mínimo es ${min}` };
            }
            
            if (max && parseFloat(value) > parseFloat(max)) {
                return { valid: false, message: `El valor máximo es ${max}` };
            }
        }
        
        return { valid: true };
    }
}