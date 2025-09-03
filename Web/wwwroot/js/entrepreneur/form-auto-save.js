class FormAutoSaveManager {
    constructor(wizard) {
        this.wizard = wizard;
        this.saveTimeout = null;
        this.lastSaveTime = null;
    }
    
    scheduleAutoSave() {
        // Debounce auto-save to avoid too many requests
        if (this.saveTimeout) {
            clearTimeout(this.saveTimeout);
        }
        
        this.saveTimeout = setTimeout(() => {
            this.wizard.saveDraft(false);
        }, 5000); // Save 5 seconds after last change
    }
    
    showSaving() {
        const indicator = $('#autoSaveIndicator');
        indicator.removeClass('d-none');
        
        $('#saveSpinner').removeClass('d-none');
        $('#saveCheck').addClass('d-none');
        $('#saveError').addClass('d-none');
        $('#saveText').text('Guardando...');
        $('#saveTime').text('');
    }
    
    showSaved(timestamp) {
        $('#saveSpinner').addClass('d-none');
        $('#saveCheck').removeClass('d-none');
        $('#saveError').addClass('d-none');
        $('#saveText').text('Guardado');
        $('#saveTime').text(timestamp || new Date().toLocaleTimeString('es-ES', { 
            hour: '2-digit', 
            minute: '2-digit' 
        }));
        
        this.lastSaveTime = new Date();
        
        // Hide after 3 seconds
        setTimeout(() => {
            $('#autoSaveIndicator').addClass('d-none');
        }, 3000);
    }
    
    showError() {
        $('#saveSpinner').addClass('d-none');
        $('#saveCheck').addClass('d-none');
        $('#saveError').removeClass('d-none');
        $('#saveText').text('Error al guardar');
        $('#saveTime').text('');
        
        // Hide after 5 seconds
        setTimeout(() => {
            $('#autoSaveIndicator').addClass('d-none');
        }, 5000);
    }
}