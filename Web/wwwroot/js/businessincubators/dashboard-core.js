/**
 * Dashboard Core Module
 * Provides shared functionality for all dashboard types
 * Implements widget manager with drag-and-drop support
 */

class DashboardCore {
    constructor(options = {}) {
        this.options = {
            dashboardId: null,
            dashboardType: 'base',
            projectId: null,
            userId: null,
            refreshInterval: 60000,
            enableDragDrop: true,
            enableAutoSave: true,
            autoSaveDelay: 2000,
            apiEndpoints: {
                loadWidgets: '/api/dashboard/widgets',
                saveLayout: '/api/dashboard/layout',
                refreshWidget: '/api/dashboard/widget/refresh',
                updatePreferences: '/api/dashboard/preferences'
            },
            ...options
        };

        this.widgets = new Map();
        this.widgetInstances = new Map();
        this.refreshTimers = new Map();
        this.layoutChanged = false;
        this.autoSaveTimer = null;
        this.draggedElement = null;

        this.init();
    }

    init() {
        this.setupWidgetManager();
        this.loadWidgets();
        this.setupEventListeners();
        this.initializeDragDrop();
        this.startGlobalRefresh();
    }

    /**
     * Widget Manager Implementation
     */
    setupWidgetManager() {
        this.widgetManager = {
            registry: new Map(),
            loaders: new Map(),
            validators: new Map(),

            register: (widgetCode, config) => {
                this.widgetManager.registry.set(widgetCode, {
                    code: widgetCode,
                    name: config.name,
                    description: config.description,
                    defaultSettings: config.defaultSettings || {},
                    supportedRoles: config.supportedRoles || ['all'],
                    refreshable: config.refreshable !== false,
                    removable: config.removable !== false,
                    configurable: config.configurable !== false,
                    minWidth: config.minWidth || 3,
                    maxWidth: config.maxWidth || 12,
                    minHeight: config.minHeight || 200,
                    maxHeight: config.maxHeight || 600
                });

                if (config.loader) {
                    this.widgetManager.loaders.set(widgetCode, config.loader);
                }

                if (config.validator) {
                    this.widgetManager.validators.set(widgetCode, config.validator);
                }
            },

            unregister: (widgetCode) => {
                this.widgetManager.registry.delete(widgetCode);
                this.widgetManager.loaders.delete(widgetCode);
                this.widgetManager.validators.delete(widgetCode);
            },

            create: async (widgetCode, container, settings = {}) => {
                const config = this.widgetManager.registry.get(widgetCode);
                if (!config) {
                    console.error(`Widget ${widgetCode} not registered`);
                    return null;
                }

                const widgetId = `widget-${widgetCode}-${Date.now()}`;
                const widget = {
                    id: widgetId,
                    code: widgetCode,
                    container: container,
                    settings: { ...config.defaultSettings, ...settings },
                    config: config,
                    state: 'loading'
                };

                this.widgets.set(widgetId, widget);

                // Create widget HTML structure
                const widgetHtml = this.createWidgetHtml(widget);
                container.innerHTML = widgetHtml;

                // Load widget data
                await this.loadWidgetData(widget);

                return widget;
            },

            destroy: (widgetId) => {
                const widget = this.widgets.get(widgetId);
                if (!widget) return;

                // Stop refresh timer if exists
                if (this.refreshTimers.has(widgetId)) {
                    clearInterval(this.refreshTimers.get(widgetId));
                    this.refreshTimers.delete(widgetId);
                }

                // Clean up widget instance
                const instance = this.widgetInstances.get(widgetId);
                if (instance && typeof instance.destroy === 'function') {
                    instance.destroy();
                }
                this.widgetInstances.delete(widgetId);

                // Remove from DOM
                if (widget.container) {
                    widget.container.remove();
                }

                // Remove from registry
                this.widgets.delete(widgetId);
            },

            refresh: async (widgetId) => {
                const widget = this.widgets.get(widgetId);
                if (!widget) return;

                widget.state = 'refreshing';
                this.updateWidgetState(widget);

                try {
                    await this.loadWidgetData(widget);
                    widget.state = 'ready';
                } catch (error) {
                    widget.state = 'error';
                    console.error(`Error refreshing widget ${widgetId}:`, error);
                }

                this.updateWidgetState(widget);
            },

            configure: (widgetId, newSettings) => {
                const widget = this.widgets.get(widgetId);
                if (!widget) return;

                // Validate settings if validator exists
                const validator = this.widgetManager.validators.get(widget.code);
                if (validator && !validator(newSettings)) {
                    throw new Error('Invalid widget settings');
                }

                widget.settings = { ...widget.settings, ...newSettings };
                this.widgetManager.refresh(widgetId);

                if (this.options.enableAutoSave) {
                    this.scheduleAutoSave();
                }
            }
        };
    }

    createWidgetHtml(widget) {
        const { config } = widget;
        return `
            <div class="widget-container" data-widget-id="${widget.id}" data-widget-code="${widget.code}">
                <div class="widget-header d-flex justify-content-between align-items-center p-2 bg-body-tertiary">
                    <h6 class="widget-title mb-0">${config.name}</h6>
                    <div class="widget-actions">
                        ${config.refreshable ? `
                            <button class="btn btn-sm btn-phoenix-secondary widget-refresh" data-widget-id="${widget.id}">
                                <span class="fas fa-sync-alt"></span>
                            </button>
                        ` : ''}
                        ${config.configurable ? `
                            <button class="btn btn-sm btn-phoenix-secondary widget-configure" data-widget-id="${widget.id}">
                                <span class="fas fa-cog"></span>
                            </button>
                        ` : ''}
                        ${config.removable ? `
                            <button class="btn btn-sm btn-phoenix-secondary widget-remove" data-widget-id="${widget.id}">
                                <span class="fas fa-times"></span>
                            </button>
                        ` : ''}
                        ${this.options.enableDragDrop ? `
                            <button class="btn btn-sm btn-phoenix-secondary widget-drag-handle">
                                <span class="fas fa-grip-vertical"></span>
                            </button>
                        ` : ''}
                    </div>
                </div>
                <div class="widget-body">
                    <div class="widget-loading text-center py-4">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Cargando...</span>
                        </div>
                    </div>
                    <div class="widget-content" style="display: none;"></div>
                    <div class="widget-error text-center py-4" style="display: none;">
                        <span class="fas fa-exclamation-triangle text-warning mb-2"></span>
                        <p class="text-muted">Error al cargar el widget</p>
                    </div>
                </div>
            </div>
        `;
    }

    async loadWidgetData(widget) {
        const loader = this.widgetManager.loaders.get(widget.code);
        
        if (loader) {
            try {
                const data = await loader(widget.settings, this.options);
                this.renderWidgetContent(widget, data);
            } catch (error) {
                console.error(`Error loading widget ${widget.code}:`, error);
                widget.state = 'error';
                this.updateWidgetState(widget);
            }
        } else {
            // Default loader - fetch from API
            try {
                const response = await fetch(`${this.options.apiEndpoints.refreshWidget}/${widget.code}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        widgetId: widget.id,
                        settings: widget.settings,
                        projectId: this.options.projectId
                    })
                });

                if (!response.ok) throw new Error('Network response was not ok');

                const data = await response.json();
                this.renderWidgetContent(widget, data);
            } catch (error) {
                console.error(`Error loading widget ${widget.code}:`, error);
                widget.state = 'error';
                this.updateWidgetState(widget);
            }
        }
    }

    renderWidgetContent(widget, data) {
        const contentElement = widget.container.querySelector('.widget-content');
        if (!contentElement) return;

        // Apply custom renderer if exists
        const renderer = this.widgetRenderers.get(widget.code);
        if (renderer) {
            contentElement.innerHTML = renderer(data, widget.settings);
        } else {
            // Default rendering
            contentElement.innerHTML = this.defaultWidgetRenderer(data);
        }

        widget.state = 'ready';
        this.updateWidgetState(widget);

        // Initialize any widget-specific JavaScript
        this.initializeWidgetScripts(widget, data);
    }

    defaultWidgetRenderer(data) {
        // Basic default renderer
        if (typeof data === 'string') {
            return data;
        } else if (data.html) {
            return data.html;
        } else {
            return `<pre>${JSON.stringify(data, null, 2)}</pre>`;
        }
    }

    updateWidgetState(widget) {
        const container = widget.container;
        const loading = container.querySelector('.widget-loading');
        const content = container.querySelector('.widget-content');
        const error = container.querySelector('.widget-error');

        switch (widget.state) {
            case 'loading':
            case 'refreshing':
                loading.style.display = 'block';
                content.style.display = 'none';
                error.style.display = 'none';
                break;
            case 'ready':
                loading.style.display = 'none';
                content.style.display = 'block';
                error.style.display = 'none';
                break;
            case 'error':
                loading.style.display = 'none';
                content.style.display = 'none';
                error.style.display = 'block';
                break;
        }

        // Update refresh button state
        const refreshBtn = container.querySelector('.widget-refresh');
        if (refreshBtn) {
            if (widget.state === 'refreshing') {
                refreshBtn.disabled = true;
                refreshBtn.querySelector('.fas').classList.add('fa-spin');
            } else {
                refreshBtn.disabled = false;
                refreshBtn.querySelector('.fas').classList.remove('fa-spin');
            }
        }
    }

    /**
     * Drag and Drop Implementation
     */
    initializeDragDrop() {
        if (!this.options.enableDragDrop) return;

        // Make widget containers sortable
        const gridContainer = document.querySelector('.dashboard-grid');
        if (!gridContainer) return;

        // Use native HTML5 drag and drop
        this.setupDragAndDrop(gridContainer);
    }

    setupDragAndDrop(container) {
        container.addEventListener('dragstart', (e) => this.handleDragStart(e));
        container.addEventListener('dragover', (e) => this.handleDragOver(e));
        container.addEventListener('drop', (e) => this.handleDrop(e));
        container.addEventListener('dragend', (e) => this.handleDragEnd(e));
        container.addEventListener('dragenter', (e) => this.handleDragEnter(e));
        container.addEventListener('dragleave', (e) => this.handleDragLeave(e));
    }

    handleDragStart(e) {
        const dragHandle = e.target.closest('.widget-drag-handle');
        if (!dragHandle) return;

        const widgetContainer = e.target.closest('.widget-container');
        if (!widgetContainer) return;

        this.draggedElement = widgetContainer.parentElement; // Get the grid column
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.draggedElement.innerHTML);
        
        this.draggedElement.classList.add('dragging');
    }

    handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';
        return false;
    }

    handleDragEnter(e) {
        const dropTarget = e.target.closest('.col-md-6, .col-lg-4, .col-xl-3');
        if (dropTarget && dropTarget !== this.draggedElement) {
            dropTarget.classList.add('drag-over');
        }
    }

    handleDragLeave(e) {
        const dropTarget = e.target.closest('.col-md-6, .col-lg-4, .col-xl-3');
        if (dropTarget) {
            dropTarget.classList.remove('drag-over');
        }
    }

    handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }

        const dropTarget = e.target.closest('.col-md-6, .col-lg-4, .col-xl-3');
        if (!dropTarget || dropTarget === this.draggedElement) return;

        // Swap elements
        const draggedClone = this.draggedElement.cloneNode(true);
        const dropClone = dropTarget.cloneNode(true);
        
        this.draggedElement.parentNode.replaceChild(dropClone, this.draggedElement);
        dropTarget.parentNode.replaceChild(draggedClone, dropTarget);

        this.layoutChanged = true;
        if (this.options.enableAutoSave) {
            this.scheduleAutoSave();
        }

        return false;
    }

    handleDragEnd(e) {
        const dragging = document.querySelectorAll('.dragging');
        dragging.forEach(el => el.classList.remove('dragging'));
        
        const dragOver = document.querySelectorAll('.drag-over');
        dragOver.forEach(el => el.classList.remove('drag-over'));
        
        this.draggedElement = null;

        // Re-initialize event handlers for the swapped elements
        this.setupEventListeners();
    }

    /**
     * Event Listeners
     */
    setupEventListeners() {
        // Widget refresh buttons
        document.querySelectorAll('.widget-refresh').forEach(btn => {
            btn.removeEventListener('click', this.handleWidgetRefresh);
            btn.addEventListener('click', (e) => this.handleWidgetRefresh(e));
        });

        // Widget configure buttons
        document.querySelectorAll('.widget-configure').forEach(btn => {
            btn.removeEventListener('click', this.handleWidgetConfigure);
            btn.addEventListener('click', (e) => this.handleWidgetConfigure(e));
        });

        // Widget remove buttons
        document.querySelectorAll('.widget-remove').forEach(btn => {
            btn.removeEventListener('click', this.handleWidgetRemove);
            btn.addEventListener('click', (e) => this.handleWidgetRemove(e));
        });

        // Make drag handles draggable
        document.querySelectorAll('.widget-drag-handle').forEach(handle => {
            const widgetContainer = handle.closest('.widget-container');
            if (widgetContainer && widgetContainer.parentElement) {
                widgetContainer.parentElement.draggable = true;
            }
        });
    }

    handleWidgetRefresh = async (e) => {
        const widgetId = e.currentTarget.dataset.widgetId;
        await this.widgetManager.refresh(widgetId);
    }

    handleWidgetConfigure = (e) => {
        const widgetId = e.currentTarget.dataset.widgetId;
        const widget = this.widgets.get(widgetId);
        if (!widget) return;

        // Open configuration modal
        this.openWidgetConfigModal(widget);
    }

    handleWidgetRemove = (e) => {
        const widgetId = e.currentTarget.dataset.widgetId;
        
        if (confirm('¿Está seguro de que desea eliminar este widget?')) {
            this.widgetManager.destroy(widgetId);
            this.layoutChanged = true;
            
            if (this.options.enableAutoSave) {
                this.scheduleAutoSave();
            }
        }
    }

    /**
     * Widget Configuration Modal
     */
    openWidgetConfigModal(widget) {
        // Create modal HTML
        const modalHtml = `
            <div class="modal fade" id="widgetConfigModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Configurar ${widget.config.name}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="widgetConfigForm">
                                ${this.generateConfigForm(widget)}
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-phoenix-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-phoenix-primary" onclick="dashboardCore.saveWidgetConfig('${widget.id}')">Guardar</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Add modal to DOM if not exists
        let modalElement = document.getElementById('widgetConfigModal');
        if (modalElement) {
            modalElement.remove();
        }
        
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('widgetConfigModal'));
        modal.show();
    }

    generateConfigForm(widget) {
        // Generate form fields based on widget settings
        let formHtml = '';
        const settings = widget.settings;

        for (const [key, value] of Object.entries(settings)) {
            const fieldType = typeof value;
            
            formHtml += `
                <div class="mb-3">
                    <label for="config_${key}" class="form-label">${this.humanizeKey(key)}</label>
            `;

            switch (fieldType) {
                case 'boolean':
                    formHtml += `
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" id="config_${key}" 
                                   name="${key}" ${value ? 'checked' : ''}>
                        </div>
                    `;
                    break;
                case 'number':
                    formHtml += `
                        <input type="number" class="form-control" id="config_${key}" 
                               name="${key}" value="${value}">
                    `;
                    break;
                default:
                    formHtml += `
                        <input type="text" class="form-control" id="config_${key}" 
                               name="${key}" value="${value}">
                    `;
            }

            formHtml += '</div>';
        }

        return formHtml;
    }

    saveWidgetConfig(widgetId) {
        const form = document.getElementById('widgetConfigForm');
        const formData = new FormData(form);
        const newSettings = {};

        for (const [key, value] of formData.entries()) {
            newSettings[key] = value;
        }

        // Handle checkboxes
        form.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
            newSettings[checkbox.name] = checkbox.checked;
        });

        try {
            this.widgetManager.configure(widgetId, newSettings);
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('widgetConfigModal'));
            modal.hide();
            
            this.showToast('success', 'Configuración guardada correctamente');
        } catch (error) {
            this.showToast('error', 'Error al guardar la configuración');
        }
    }

    /**
     * Auto-save functionality
     */
    scheduleAutoSave() {
        if (this.autoSaveTimer) {
            clearTimeout(this.autoSaveTimer);
        }

        this.autoSaveTimer = setTimeout(() => {
            this.saveLayout();
        }, this.options.autoSaveDelay);
    }

    async saveLayout() {
        if (!this.layoutChanged) return;

        const layout = this.getCurrentLayout();
        
        try {
            const response = await fetch(this.options.apiEndpoints.saveLayout, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    dashboardId: this.options.dashboardId,
                    layout: layout,
                    widgets: Array.from(this.widgets.values()).map(w => ({
                        id: w.id,
                        code: w.code,
                        settings: w.settings,
                        position: this.getWidgetPosition(w)
                    }))
                })
            });

            if (response.ok) {
                this.layoutChanged = false;
                this.showToast('success', 'Diseño guardado automáticamente');
            }
        } catch (error) {
            console.error('Error saving layout:', error);
        }
    }

    getCurrentLayout() {
        const gridContainer = document.querySelector('.dashboard-grid');
        if (!gridContainer) return [];

        const layout = [];
        gridContainer.querySelectorAll('.widget-container').forEach((widget, index) => {
            layout.push({
                widgetId: widget.dataset.widgetId,
                position: index
            });
        });

        return layout;
    }

    getWidgetPosition(widget) {
        const container = widget.container;
        if (!container) return { x: 0, y: 0 };

        const parent = container.parentElement;
        const siblings = Array.from(parent.parentElement.children);
        
        return {
            x: siblings.indexOf(parent) % 4,
            y: Math.floor(siblings.indexOf(parent) / 4)
        };
    }

    /**
     * Global refresh
     */
    startGlobalRefresh() {
        if (this.options.refreshInterval > 0) {
            setInterval(() => {
                this.refreshAllWidgets();
            }, this.options.refreshInterval);
        }
    }

    async refreshAllWidgets() {
        const promises = [];
        
        for (const [widgetId, widget] of this.widgets) {
            if (widget.config.refreshable) {
                promises.push(this.widgetManager.refresh(widgetId));
            }
        }

        await Promise.all(promises);
    }

    /**
     * Widget loading
     */
    async loadWidgets() {
        try {
            const response = await fetch(`${this.options.apiEndpoints.loadWidgets}?dashboardId=${this.options.dashboardId}`);
            const data = await response.json();

            if (data.widgets) {
                for (const widgetData of data.widgets) {
                    const container = document.querySelector(`[data-widget-placeholder="${widgetData.code}"]`);
                    if (container) {
                        await this.widgetManager.create(widgetData.code, container, widgetData.settings);
                    }
                }
            }
        } catch (error) {
            console.error('Error loading widgets:', error);
        }
    }

    /**
     * Widget Scripts Initialization
     */
    initializeWidgetScripts(widget, data) {
        // Initialize charts, tooltips, etc. based on widget type
        const container = widget.container;

        // Initialize tooltips
        const tooltips = container.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltips.forEach(el => new bootstrap.Tooltip(el));

        // Initialize popovers
        const popovers = container.querySelectorAll('[data-bs-toggle="popover"]');
        popovers.forEach(el => new bootstrap.Popover(el));

        // Widget-specific initialization
        switch (widget.code) {
            case 'progress':
                this.initializeProgressChart(container, data);
                break;
            case 'tasks':
                this.initializeTaskList(container, data);
                break;
            case 'notifications':
                this.initializeNotifications(container, data);
                break;
            // Add more widget-specific initializations
        }
    }

    initializeProgressChart(container, data) {
        // Initialize progress charts using ECharts or similar
        const chartElement = container.querySelector('.progress-chart');
        if (chartElement && typeof echarts !== 'undefined') {
            const chart = echarts.init(chartElement);
            const option = {
                // Chart configuration
            };
            chart.setOption(option);
            
            // Store chart instance for cleanup
            this.widgetInstances.set(container.dataset.widgetId, { chart });
        }
    }

    initializeTaskList(container, data) {
        // Add task interaction handlers
        container.querySelectorAll('.task-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                this.handleTaskStatusChange(e, data);
            });
        });
    }

    initializeNotifications(container, data) {
        // Add notification interaction handlers
        container.querySelectorAll('.notification-item').forEach(item => {
            item.addEventListener('click', (e) => {
                this.handleNotificationClick(e, data);
            });
        });
    }

    /**
     * Utility functions
     */
    humanizeKey(key) {
        return key
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, str => str.toUpperCase())
            .replace(/_/g, ' ');
    }

    showToast(type, message) {
        // Use Phoenix template's toast system if available
        if (window.phoenix && window.phoenix.toast) {
            window.phoenix.toast[type](message);
        } else {
            console.log(`[${type.toUpperCase()}] ${message}`);
        }
    }

    /**
     * Public API
     */
    registerWidget(code, config) {
        this.widgetManager.register(code, config);
    }

    addWidget(code, settings = {}) {
        // Find available slot or create new one
        const gridContainer = document.querySelector('.dashboard-grid');
        if (!gridContainer) return;

        const newCol = document.createElement('div');
        newCol.className = 'col-12 col-md-6 col-lg-4 col-xl-3 mb-3';
        
        const cardDiv = document.createElement('div');
        cardDiv.className = 'card h-100';
        newCol.appendChild(cardDiv);
        
        gridContainer.appendChild(newCol);

        return this.widgetManager.create(code, cardDiv, settings);
    }

    removeWidget(widgetId) {
        this.widgetManager.destroy(widgetId);
    }

    refreshWidget(widgetId) {
        return this.widgetManager.refresh(widgetId);
    }

    configureWidget(widgetId, settings) {
        this.widgetManager.configure(widgetId, settings);
    }

    /**
     * Cleanup
     */
    destroy() {
        // Clear all timers
        this.refreshTimers.forEach(timer => clearInterval(timer));
        this.refreshTimers.clear();

        if (this.autoSaveTimer) {
            clearTimeout(this.autoSaveTimer);
        }

        // Destroy all widget instances
        this.widgetInstances.forEach(instance => {
            if (instance && typeof instance.destroy === 'function') {
                instance.destroy();
            }
        });
        this.widgetInstances.clear();

        // Clear widget registry
        this.widgets.clear();
    }
}

// Widget renderers registry
DashboardCore.prototype.widgetRenderers = new Map();

// Export for global use
window.DashboardCore = DashboardCore;