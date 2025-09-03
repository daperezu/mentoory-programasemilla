/**
 * Coordination Reports Management
 * Handles report template gallery, generation, and export functionality
 */

class CoordinatorReports {
    constructor() {
        this.projectId = null;
        this.currentFilter = '';
        this.templates = [];
        this.stats = {};
        
        this.init();
    }

    init() {
        // Get project ID from the page
        this.projectId = window.projectId || this.getProjectIdFromUrl();
        
        // Initialize event handlers
        this.initEventHandlers();
        
        // Load initial data
        this.loadReportStats();
        this.loadReportTemplates();
        
        // Set default date ranges
        this.setDefaultDates();
    }

    initEventHandlers() {
        // Filter tabs
        $('#reportTypeTabs button[data-bs-toggle="tab"]').on('shown.bs.tab', (e) => {
            const filter = $(e.target).data('filter');
            this.currentFilter = filter;
            this.filterTemplates(filter);
        });

        // Refresh button
        $('#refreshReports').on('click', () => {
            this.refreshAllData();
        });

        // Quick export buttons
        $('#quickExportProgress').on('click', () => {
            this.quickExport('Progress');
        });

        $('#quickExportParticipation').on('click', () => {
            this.quickExport('Participation');
        });

        // Create template form
        $('#createTemplateForm').on('submit', (e) => {
            e.preventDefault();
            this.createTemplate();
        });

        // Generate report form
        $('#generateReportForm').on('submit', (e) => {
            e.preventDefault();
            this.generateReport();
        });

        // Generate and export button
        $('#generateAndExport').on('click', () => {
            this.generateAndExport();
        });

        // Template type change - show/hide configuration
        $('#templateType').on('change', (e) => {
            const isCustom = $(e.target).val() === 'Custom';
            $('#configurationSection').toggle(isCustom);
        });
    }

    async loadReportStats() {
        try {
            const response = await $.get('/Coordination/Reports/GetStats');
            
            if (response.success) {
                this.stats = response.data;
                this.updateStatsDisplay();
            } else {
                this.showError('Error al cargar estadísticas: ' + response.message);
            }
        } catch (error) {
            console.error('Error loading report stats:', error);
            this.showError('Error al cargar estadísticas de reportes');
        }
    }

    async loadReportTemplates(filterByType = null) {
        try {
            this.showTemplatesLoading();

            const url = filterByType 
                ? `/Coordination/Reports/GetTemplates?filterByType=${filterByType}`
                : '/Coordination/Reports/GetTemplates';
                
            const response = await $.get(url);
            
            if (response.success) {
                this.templates = response.data.templates || [];
                this.renderTemplatesGallery();
            } else {
                this.showError('Error al cargar plantillas: ' + response.message);
                this.showEmptyTemplates();
            }
        } catch (error) {
            console.error('Error loading templates:', error);
            this.showError('Error al cargar plantillas de reportes');
            this.showEmptyTemplates();
        }
    }

    updateStatsDisplay() {
        $('#totalTemplates').text(this.stats.totalTemplates || 0);
        $('#totalGenerated').text(this.stats.totalGenerated || 0);
        
        if (this.stats.lastGenerated) {
            const lastGenerated = new Date(this.stats.lastGenerated);
            $('#lastGenerated').text(this.formatRelativeDate(lastGenerated));
        } else {
            $('#lastGenerated').text('Nunca');
        }
    }

    renderTemplatesGallery() {
        const container = $('#templatesContainer');
        
        if (!this.templates || this.templates.length === 0) {
            this.showEmptyTemplates();
            return;
        }

        const filteredTemplates = this.currentFilter 
            ? this.templates.filter(t => t.type === this.currentFilter)
            : this.templates;

        if (filteredTemplates.length === 0) {
            this.showEmptyTemplates();
            return;
        }

        const templatesHtml = `
            <div class="template-grid">
                ${filteredTemplates.map(template => this.renderTemplateCard(template)).join('')}
            </div>
        `;

        container.html(templatesHtml);
        
        // Bind card click events
        $('.report-card').on('click', (e) => {
            const templateId = $(e.currentTarget).data('template-id');
            this.openGenerateModal(templateId);
        });
    }

    renderTemplateCard(template) {
        const typeColors = {
            'Progress': 'primary',
            'Completion': 'success', 
            'Participation': 'info',
            'Custom': 'warning'
        };

        const typeIcons = {
            'Progress': 'fa-chart-line',
            'Completion': 'fa-check-circle',
            'Participation': 'fa-users',
            'Custom': 'fa-cog'
        };

        const color = typeColors[template.type] || 'secondary';
        const icon = typeIcons[template.type] || 'fa-file';
        const isGlobal = template.isGlobal;

        return `
            <div class="card report-card h-100" data-template-id="${template.templateId}">
                <div class="card-body position-relative">
                    <div class="report-type-badge">
                        <span class="badge badge-phoenix badge-phoenix-${color}">
                            <span class="fas ${icon} me-1"></span>${template.typeDescription}
                        </span>
                        ${isGlobal ? '<span class="badge badge-phoenix badge-phoenix-secondary ms-1">Global</span>' : ''}
                    </div>
                    
                    <div class="mb-3">
                        <h5 class="card-title mb-2">${template.name}</h5>
                        <p class="card-text text-muted small">${template.description || 'Sin descripción'}</p>
                    </div>

                    <div class="template-preview mb-3">
                        <small class="text-muted">
                            <span class="fas fa-code me-1"></span>
                            ${template.configurationPreview}
                        </small>
                    </div>

                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <small class="text-muted">
                                <span class="fas fa-calendar me-1"></span>
                                ${this.formatDate(template.createdAt)}
                            </small>
                            ${template.activeScheduleCount > 0 
                                ? `<br><small class="text-info"><span class="fas fa-clock me-1"></span>${template.activeScheduleCount} programados</small>`
                                : ''}
                        </div>
                        <div class="btn-group btn-group-sm" role="group">
                            <button type="button" class="btn btn-outline-primary" title="Generar">
                                <span class="fas fa-play"></span>
                            </button>
                            <div class="btn-group btn-group-sm" role="group">
                                <button type="button" class="btn btn-outline-success dropdown-toggle" 
                                        data-bs-toggle="dropdown" title="Exportar">
                                    <span class="fas fa-download"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a class="dropdown-item" href="#" onclick="coordinatorReports.quickExportTemplate(${template.templateId}, 'Excel')">
                                        <span class="fas fa-file-excel text-success me-1"></span>Excel
                                    </a></li>
                                    <li><a class="dropdown-item" href="#" onclick="coordinatorReports.quickExportTemplate(${template.templateId}, 'CSV')">
                                        <span class="fas fa-file-csv text-info me-1"></span>CSV
                                    </a></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    showTemplatesLoading() {
        $('#templatesContainer').html(`
            <div class="text-center py-5">
                <div class="spinner-border" role="status">
                    <span class="visually-hidden">Cargando plantillas...</span>
                </div>
                <p class="mt-2 text-muted">Cargando plantillas de reportes...</p>
            </div>
        `);
    }

    showEmptyTemplates() {
        const filterText = this.currentFilter 
            ? ` de tipo "${this.getFilterDescription(this.currentFilter)}"`
            : '';

        $('#templatesContainer').html(`
            <div class="text-center py-5">
                <div class="mb-3">
                    <span class="fas fa-file-alt fa-3x text-muted-light"></span>
                </div>
                <h5 class="text-muted">No hay plantillas${filterText}</h5>
                <p class="text-muted">Cree una nueva plantilla para comenzar a generar reportes</p>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#createTemplateModal">
                    <span class="fas fa-plus me-1"></span>Nueva Plantilla
                </button>
            </div>
        `);
    }

    filterTemplates(type) {
        this.renderTemplatesGallery();
    }

    openGenerateModal(templateId) {
        const template = this.templates.find(t => t.templateId == templateId);
        if (!template) return;

        $('#generateTemplateId').val(templateId);
        $('#selectedTemplateName').text(template.name);
        $('#selectedTemplateDescription').text(template.description || 'Sin descripción');
        
        $('#generateReportModal').modal('show');
    }

    async createTemplate() {
        const form = $('#createTemplateForm')[0];
        const formData = new FormData(form);
        
        try {
            this.showButtonLoading('#createTemplateForm button[type="submit"]');
            
            const response = await $.ajax({
                url: '/Coordination/Reports/CreateTemplate',
                type: 'POST',
                data: Object.fromEntries(formData),
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            if (response.success) {
                this.showSuccess('Plantilla creada exitosamente');
                $('#createTemplateModal').modal('hide');
                $('#createTemplateForm')[0].reset();
                await this.refreshAllData();
            } else {
                this.showError('Error al crear plantilla: ' + response.message);
            }
        } catch (error) {
            console.error('Error creating template:', error);
            this.showError('Error al crear la plantilla');
        } finally {
            this.hideButtonLoading('#createTemplateForm button[type="submit"]', 'Crear Plantilla');
        }
    }

    async generateReport() {
        const templateId = $('#generateTemplateId').val();
        const startDate = $('#startDate').val() || null;
        const endDate = $('#endDate').val() || null;
        
        try {
            this.showButtonLoading('#generateReportForm button[type="submit"]');
            
            const response = await $.ajax({
                url: '/Coordination/Reports/Generate',
                type: 'POST',
                data: { 
                    templateId: templateId,
                    startDate: startDate,
                    endDate: endDate
                },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            if (response.success) {
                this.showSuccess('Reporte generado exitosamente');
                $('#generateReportModal').modal('hide');
                // You could show the generated report data here
                console.log('Generated report:', response.data);
            } else {
                this.showError('Error al generar reporte: ' + response.message);
            }
        } catch (error) {
            console.error('Error generating report:', error);
            this.showError('Error al generar el reporte');
        } finally {
            this.hideButtonLoading('#generateReportForm button[type="submit"]', 'Generar');
        }
    }

    async generateAndExport() {
        const templateId = $('#generateTemplateId').val();
        const startDate = $('#startDate').val() || null;
        const endDate = $('#endDate').val() || null;
        
        await this.exportReport(templateId, 'Excel', startDate, endDate);
        $('#generateReportModal').modal('hide');
    }

    async quickExport(reportType) {
        // Find a template of the specified type
        const template = this.templates.find(t => t.type === reportType && t.isGlobal);
        
        if (!template) {
            this.showWarning(`No hay plantillas globales de tipo ${this.getFilterDescription(reportType)} disponibles`);
            return;
        }

        await this.exportReport(template.templateId, 'Excel');
    }

    async quickExportTemplate(templateId, format) {
        await this.exportReport(templateId, format);
    }

    async exportReport(templateId, format, startDate = null, endDate = null) {
        try {
            const btn = event?.target;
            if (btn) {
                $(btn).prop('disabled', true);
                $(btn).html('<span class="spinner-border spinner-border-sm me-1"></span>Exportando...');
            }

            // Create a form to submit the export request
            const form = $('<form>', {
                'method': 'POST',
                'action': '/Coordination/Reports/Export'
            });

            // Add CSRF token
            form.append($('<input>', {
                'type': 'hidden',
                'name': '__RequestVerificationToken',
                'value': $('input[name="__RequestVerificationToken"]').val()
            }));

            // Add parameters
            form.append($('<input>', { 'type': 'hidden', 'name': 'templateId', 'value': templateId }));
            form.append($('<input>', { 'type': 'hidden', 'name': 'format', 'value': format }));
            
            if (startDate) {
                form.append($('<input>', { 'type': 'hidden', 'name': 'startDate', 'value': startDate }));
            }
            if (endDate) {
                form.append($('<input>', { 'type': 'hidden', 'name': 'endDate', 'value': endDate }));
            }

            // Submit form to trigger download
            form.appendTo('body').submit().remove();

            this.showSuccess(`Exportando reporte en formato ${format}...`);
            
        } catch (error) {
            console.error('Error exporting report:', error);
            this.showError('Error al exportar el reporte');
        } finally {
            // Re-enable button after delay
            setTimeout(() => {
                const btn = event?.target;
                if (btn) {
                    $(btn).prop('disabled', false);
                    $(btn).html(`<span class="fas fa-download me-1"></span>${format}`);
                }
            }, 2000);
        }
    }

    async refreshAllData() {
        const btn = $('#refreshReports');
        btn.prop('disabled', true);
        btn.find('.fas').addClass('fa-spin');

        try {
            await Promise.all([
                this.loadReportStats(),
                this.loadReportTemplates(this.currentFilter)
            ]);
        } finally {
            btn.prop('disabled', false);
            btn.find('.fas').removeClass('fa-spin');
        }
    }

    setDefaultDates() {
        const today = new Date();
        const monthAgo = new Date(today);
        monthAgo.setMonth(monthAgo.getMonth() - 1);

        $('#startDate').val(monthAgo.toISOString().split('T')[0]);
        $('#endDate').val(today.toISOString().split('T')[0]);
    }

    getFilterDescription(type) {
        const descriptions = {
            'Progress': 'Progreso',
            'Completion': 'Finalización',
            'Participation': 'Participación',
            'Custom': 'Personalizado'
        };
        return descriptions[type] || type;
    }

    getProjectIdFromUrl() {
        // Try to extract project ID from URL or other sources
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get('projectId') || 
               window.ViewBag?.ProjectId || 
               document.querySelector('[data-project-id]')?.dataset.projectId;
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('es-ES', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    formatRelativeDate(date) {
        const now = new Date();
        const diffTime = Math.abs(now - date);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays === 0) {
            return 'Hoy';
        } else if (diffDays === 1) {
            return 'Ayer';
        } else if (diffDays < 7) {
            return `Hace ${diffDays} días`;
        } else {
            return this.formatDate(date);
        }
    }

    showButtonLoading(selector) {
        const btn = $(selector);
        btn.prop('disabled', true);
        btn.data('original-text', btn.html());
        btn.html('<span class="spinner-border spinner-border-sm me-1"></span>Procesando...');
    }

    hideButtonLoading(selector, originalText) {
        const btn = $(selector);
        btn.prop('disabled', false);
        btn.html(btn.data('original-text') || originalText);
    }

    showSuccess(message) {
        this.showToast(message, 'success');
    }

    showError(message) {
        this.showToast(message, 'danger');
    }

    showWarning(message) {
        this.showToast(message, 'warning');
    }

    showToast(message, type) {
        // Use Phoenix Toast notification system if available
        if (window.phoenix && window.phoenix.toast) {
            window.phoenix.toast(message, type);
        } else {
            // Fallback to simple alert or console
            console.log(`${type.toUpperCase()}: ${message}`);
            
            // Create simple toast notification
            const toast = $(`
                <div class="toast align-items-center text-white bg-${type} border-0 position-fixed" 
                     style="top: 20px; right: 20px; z-index: 9999;" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">${message}</div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `);
            
            $('body').append(toast);
            
            // Initialize and show toast
            const bsToast = new bootstrap.Toast(toast[0]);
            bsToast.show();
            
            // Remove from DOM after hiding
            toast.on('hidden.bs.toast', () => toast.remove());
        }
    }
}

// Initialize when document is ready
$(document).ready(function() {
    window.coordinatorReports = new CoordinatorReports();
});