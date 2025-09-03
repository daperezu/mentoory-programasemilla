// Starter Dashboard JavaScript Module
// Extends DashboardCore for shared functionality
class StarterDashboard extends DashboardCore {
    constructor(options) {
        // Call parent constructor with starter-specific defaults
        super({
            dashboardType: 'starter',
            refreshInterval: 60000,
            enableDragDrop: true,
            enableAutoSave: true,
            apiEndpoints: {
                loadWidgets: '/BusinessIncubators/StarterDashboard/GetWidgets',
                saveLayout: '/BusinessIncubators/StarterDashboard/SaveLayout',
                refreshWidget: '/BusinessIncubators/StarterDashboard/RefreshWidget',
                updatePreferences: '/BusinessIncubators/StarterDashboard/UpdatePreferences',
                completeTask: '/BusinessIncubators/StarterDashboard/CompleteTask',
                markNotificationRead: '/BusinessIncubators/StarterDashboard/MarkNotificationRead',
                getWidgetData: '/BusinessIncubators/StarterDashboard/GetWidgetData'
            },
            ...options
        });
        
        this.refreshTimer = null;
        this.starterWidgets = new Map();
        
        this.initStarter();
    }
    
    initStarter() {
        // Register starter-specific widgets
        this.registerStarterWidgets();
        
        // Setup starter-specific event handlers
        this.setupStarterEventHandlers();
        
        // Initialize starter features
        this.initializeStarterFeatures();
        
        // Initialize project switcher
        this.initializeProjectSwitcher();
        
        // Load dashboard settings
        this.loadDashboardSettings();
    }
    
    registerStarterWidgets() {
        // Register Progress Widget
        this.registerWidget('progress', {
            name: 'Progreso General',
            description: 'Muestra el progreso del proyecto',
            refreshable: true,
            removable: false,
            configurable: true,
            defaultSettings: {
                showTrend: true,
                showDetails: true,
                refreshRate: 60000
            },
            loader: async (settings, options) => {
                const response = await fetch(`${options.apiEndpoints.getWidgetData}?widgetCode=progress&projectId=${options.projectId}`);
                return await response.json();
            }
        });
        
        // Register Tasks Widget
        this.registerWidget('tasks', {
            name: 'Tareas Pendientes',
            description: 'Lista de tareas del proyecto',
            refreshable: true,
            removable: true,
            configurable: true,
            defaultSettings: {
                showCompleted: false,
                maxItems: 10,
                sortBy: 'priority'
            },
            loader: async (settings, options) => {
                const response = await fetch(`${options.apiEndpoints.getWidgetData}?widgetCode=tasks&projectId=${options.projectId}`);
                return await response.json();
            }
        });
        
        // Register Notifications Widget
        this.registerWidget('notifications', {
            name: 'Notificaciones',
            description: 'Notificaciones del sistema',
            refreshable: true,
            removable: true,
            configurable: true,
            defaultSettings: {
                showRead: false,
                maxItems: 5,
                autoMarkRead: false
            },
            loader: async (settings, options) => {
                const response = await fetch(`${options.apiEndpoints.getWidgetData}?widgetCode=notifications&projectId=${options.projectId}`);
                return await response.json();
            }
        });
        
        // Register Milestones Widget
        this.registerWidget('milestones', {
            name: 'Hitos del Proyecto',
            description: 'Timeline de hitos importantes',
            refreshable: true,
            removable: true,
            configurable: false,
            loader: async (settings, options) => {
                const response = await fetch(`${options.apiEndpoints.getWidgetData}?widgetCode=milestones&projectId=${options.projectId}`);
                return await response.json();
            }
        });
        
        // Register Mentor Widget
        this.registerWidget('mentor', {
            name: 'Información del Mentor',
            description: 'Datos de contacto del mentor asignado',
            refreshable: false,
            removable: false,
            configurable: false,
            loader: async (settings, options) => {
                const response = await fetch(`${options.apiEndpoints.getWidgetData}?widgetCode=mentor&projectId=${options.projectId}`);
                return await response.json();
            }
        });
        
        // Register Resources Widget
        this.registerWidget('resources', {
            name: 'Recursos',
            description: 'Documentos y enlaces útiles',
            refreshable: true,
            removable: true,
            configurable: true,
            defaultSettings: {
                showCategories: true,
                maxItems: 20
            },
            loader: async (settings, options) => {
                const response = await fetch(`${options.apiEndpoints.getWidgetData}?widgetCode=resources&projectId=${options.projectId}`);
                return await response.json();
            }
        });
    }
    
    setupStarterEventHandlers() {
        // Task completion handlers
        document.addEventListener('click', (e) => {
            if (e.target.matches('.task-checkbox') || e.target.closest('.task-checkbox')) {
                this.handleTaskComplete(e);
            }
        });
        
        // Notification handlers
        document.addEventListener('click', (e) => {
            if (e.target.matches('.mark-notification-read') || e.target.closest('.mark-notification-read')) {
                this.handleNotificationRead(e);
            }
            
            if (e.target.matches('.mark-all-read') || e.target.closest('.mark-all-read')) {
                this.handleMarkAllNotificationsRead();
            }
        });
        
        // Dashboard refresh button
        window.refreshDashboard = () => this.refreshAllWidgets();
        
        // Dashboard settings
        window.applyDashboardSettings = (settings) => this.applySettings(settings);
        
        // Project switcher
        document.addEventListener('change', (e) => {
            if (e.target.matches('#project-selector')) {
                this.handleProjectSwitch(e.target.value);
            }
        });
        
        // Widget add button
        document.addEventListener('click', (e) => {
            if (e.target.matches('.add-widget-btn') || e.target.closest('.add-widget-btn')) {
                this.openAddWidgetModal();
            }
        });
    }
    
    initializeStarterFeatures() {
        // Initialize starter-specific features
        this.initializeQuickActions();
        this.initializeProjectSelector();
        this.initializeKeyboardShortcuts();
        this.initializeWidgetCatalog();
    }
    
    initializeQuickActions() {
        // Add quick action buttons for common tasks
        const quickActionsHtml = `
            <div class="quick-actions-bar d-flex gap-2 mb-3">
                <button class="btn btn-phoenix-primary btn-sm" onclick="starterDashboard.quickAction('newTask')">
                    <span class="fas fa-plus me-1"></span>Nueva Tarea
                </button>
                <button class="btn btn-phoenix-secondary btn-sm" onclick="starterDashboard.quickAction('viewProgress')">
                    <span class="fas fa-chart-line me-1"></span>Ver Progreso
                </button>
                <button class="btn btn-phoenix-info btn-sm" onclick="starterDashboard.quickAction('contactMentor')">
                    <span class="fas fa-user-tie me-1"></span>Contactar Mentor
                </button>
            </div>
        `;
        
        const dashboardHeader = document.querySelector('.dashboard-header');
        if (dashboardHeader) {
            dashboardHeader.insertAdjacentHTML('afterend', quickActionsHtml);
        }
    }
    
    initializeProjectSelector() {
        // Enhanced project selector with search
        const projectSelector = document.getElementById('project-selector');
        if (!projectSelector) return;
        
        // Add search functionality if multiple projects
        const options = projectSelector.querySelectorAll('option');
        if (options.length > 5) {
            // Convert to searchable select (using Select2 or similar if available)
            if (typeof $ !== 'undefined' && $.fn.select2) {
                $(projectSelector).select2({
                    placeholder: 'Seleccionar proyecto',
                    allowClear: false,
                    width: '100%'
                });
            }
        }
    }
    
    initializeKeyboardShortcuts() {
        // Add keyboard shortcuts for common actions
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + K: Quick search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                this.openQuickSearch();
            }
            
            // Ctrl/Cmd + R: Refresh dashboard
            if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
                e.preventDefault();
                this.refreshAllWidgets();
            }
            
            // Ctrl/Cmd + N: New task
            if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                e.preventDefault();
                this.quickAction('newTask');
            }
        });
    }
    
    initializeWidgetCatalog() {
        // Create widget catalog for adding new widgets
        this.widgetCatalog = {
            available: [
                'progress', 'tasks', 'notifications', 
                'milestones', 'mentor', 'resources'
            ],
            active: []
        };
        
        // Get currently active widgets
        document.querySelectorAll('.widget-container').forEach(widget => {
            this.widgetCatalog.active.push(widget.dataset.widgetCode);
        });
    }
    
    // Quick Actions Handler
    quickAction(action) {
        switch (action) {
            case 'newTask':
                this.openNewTaskModal();
                break;
            case 'viewProgress':
                this.openProgressDetailModal();
                break;
            case 'contactMentor':
                this.openContactMentorModal();
                break;
            default:
                console.log('Unknown action:', action);
        }
    }
    
    openNewTaskModal() {
        const modalHtml = `
            <div class="modal fade" id="newTaskModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Nueva Tarea</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="newTaskForm">
                                <div class="mb-3">
                                    <label for="taskTitle" class="form-label">Título</label>
                                    <input type="text" class="form-control" id="taskTitle" required>
                                </div>
                                <div class="mb-3">
                                    <label for="taskDescription" class="form-label">Descripción</label>
                                    <textarea class="form-control" id="taskDescription" rows="3"></textarea>
                                </div>
                                <div class="mb-3">
                                    <label for="taskPriority" class="form-label">Prioridad</label>
                                    <select class="form-select" id="taskPriority">
                                        <option value="low">Baja</option>
                                        <option value="medium" selected>Media</option>
                                        <option value="high">Alta</option>
                                    </select>
                                </div>
                                <div class="mb-3">
                                    <label for="taskDueDate" class="form-label">Fecha de vencimiento</label>
                                    <input type="date" class="form-control" id="taskDueDate">
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-phoenix-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-phoenix-primary" onclick="starterDashboard.saveNewTask()">Guardar</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        this.showModal(modalHtml, 'newTaskModal');
    }
    
    openProgressDetailModal() {
        // Show detailed progress modal
        window.location.href = `/BusinessIncubators/StarterDashboard/Progress?projectId=${this.options.projectId}`;
    }
    
    openContactMentorModal() {
        const modalHtml = `
            <div class="modal fade" id="contactMentorModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Contactar Mentor</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="contactMentorForm">
                                <div class="mb-3">
                                    <label for="messageSubject" class="form-label">Asunto</label>
                                    <input type="text" class="form-control" id="messageSubject" required>
                                </div>
                                <div class="mb-3">
                                    <label for="messageContent" class="form-label">Mensaje</label>
                                    <textarea class="form-control" id="messageContent" rows="5" required></textarea>
                                </div>
                                <div class="mb-3">
                                    <label for="messagePriority" class="form-label">Urgencia</label>
                                    <select class="form-select" id="messagePriority">
                                        <option value="normal">Normal</option>
                                        <option value="urgent">Urgente</option>
                                    </select>
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-phoenix-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-phoenix-primary" onclick="starterDashboard.sendMentorMessage()">Enviar</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        this.showModal(modalHtml, 'contactMentorModal');
    }
    
    openQuickSearch() {
        const modalHtml = `
            <div class="modal fade" id="quickSearchModal" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Búsqueda Rápida</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="input-group mb-3">
                                <span class="input-group-text"><i class="fas fa-search"></i></span>
                                <input type="text" class="form-control" id="quickSearchInput" 
                                       placeholder="Buscar tareas, documentos, hitos..." 
                                       autofocus>
                            </div>
                            <div id="quickSearchResults" class="list-group"></div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        this.showModal(modalHtml, 'quickSearchModal');
        
        // Setup search handler
        const searchInput = document.getElementById('quickSearchInput');
        if (searchInput) {
            searchInput.addEventListener('input', debounce((e) => {
                this.performQuickSearch(e.target.value);
            }, 300));
        }
    }
    
    openAddWidgetModal() {
        const availableWidgets = this.widgetCatalog.available.filter(
            w => !this.widgetCatalog.active.includes(w)
        );
        
        const widgetOptions = availableWidgets.map(code => {
            const config = this.widgetManager.registry.get(code);
            return `
                <div class="col-md-6 mb-3">
                    <div class="card widget-option" data-widget-code="${code}">
                        <div class="card-body">
                            <h6>${config.name}</h6>
                            <p class="text-muted small">${config.description}</p>
                            <button class="btn btn-phoenix-primary btn-sm" 
                                    onclick="starterDashboard.addWidgetToDashboard('${code}')">
                                Agregar
                            </button>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
        
        const modalHtml = `
            <div class="modal fade" id="addWidgetModal" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Agregar Widget</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="row">
                                ${widgetOptions || '<p class="text-center">No hay widgets disponibles para agregar</p>'}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        this.showModal(modalHtml, 'addWidgetModal');
    }
    
    initializeProjectSwitcher() {
        // Load user projects and setup project dropdown
        this.loadUserProjects();
        
        // Setup project dropdown event handler if exists
        const projectDropdown = document.getElementById('projectDropdown');
        if (projectDropdown) {
            // Dropdown items are handled via href links in the layout
            // Additional dynamic functionality can be added here if needed
        }
    }
    
    async loadUserProjects() {
        try {
            const response = await fetch('/BusinessIncubators/StarterDashboard/GetUserProjects');
            const result = await response.json();
            
            if (result.success && result.projects) {
                // Update dropdown if needed dynamically
                const dropdownMenu = document.querySelector('#projectDropdown + .dropdown-menu');
                if (dropdownMenu && result.projects.length > 0) {
                    // Projects are already rendered server-side in the layout
                    // This method can be used for dynamic updates if needed
                }
            }
        } catch (error) {
            console.error('Error loading user projects:', error);
        }
    }
    
    async handleProjectSwitch(projectId) {
        // Save current dashboard state
        await this.saveLayout();
        
        // Switch project via API
        try {
            const response = await fetch('/BusinessIncubators/StarterDashboard/SwitchProject', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ projectId: projectId })
            });
            
            const result = await response.json();
            if (result.success) {
                // Redirect to new project dashboard
                window.location.href = result.redirectUrl || `/BusinessIncubators/StarterDashboard?projectId=${projectId}`;
            } else {
                this.showToast('error', result.message || 'Error al cambiar de proyecto');
            }
        } catch (error) {
            console.error('Error switching project:', error);
            this.showToast('error', 'Error al cambiar de proyecto');
        }
    }
    
    async handleTaskComplete(event) {
        const checkbox = event.target;
        const taskId = checkbox.dataset.taskId;
        const isCompleted = checkbox.checked;
        
        try {
            const response = await fetch(this.options.apiEndpoints.completeTask, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    taskId: taskId,
                    notes: ''
                })
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Update UI
                const taskLabel = checkbox.closest('.list-group-item').querySelector('label');
                if (taskLabel) {
                    if (isCompleted) {
                        taskLabel.classList.add('text-decoration-line-through', 'text-500');
                    } else {
                        taskLabel.classList.remove('text-decoration-line-through', 'text-500');
                    }
                }
                
                this.showToast('success', 'Tarea actualizada correctamente');
            } else {
                checkbox.checked = !isCompleted; // Revert
                this.showToast('error', result.message || 'Error al actualizar la tarea');
            }
        } catch (error) {
            console.error('Error completing task:', error);
            checkbox.checked = !isCompleted; // Revert
            this.showToast('error', 'Error al conectar con el servidor');
        }
    }
    
    async handleNotificationRead(event) {
        event.preventDefault();
        const button = event.currentTarget;
        const notificationId = button.dataset.notificationId;
        
        try {
            const response = await fetch(this.options.apiEndpoints.markNotificationRead, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    notificationId: notificationId
                })
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Update UI
                const notificationItem = button.closest('.notification-item');
                if (notificationItem) {
                    notificationItem.classList.remove('bg-primary-subtle');
                    button.remove();
                }
                
                // Update badge count
                this.updateNotificationBadge(-1);
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }
    
    async handleMarkAllNotificationsRead() {
        try {
            const unreadNotifications = document.querySelectorAll('.notification-item .mark-notification-read');
            const notificationIds = Array.from(unreadNotifications).map(btn => btn.dataset.notificationId);
            
            if (notificationIds.length === 0) return;
            
            // Send batch request
            for (const id of notificationIds) {
                await fetch(this.options.apiEndpoints.markNotificationRead, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({ notificationId: id })
                });
            }
            
            // Update UI
            document.querySelectorAll('.notification-item').forEach(item => {
                item.classList.remove('bg-primary-subtle');
            });
            document.querySelectorAll('.mark-notification-read').forEach(btn => btn.remove());
            
            // Reset badge
            this.updateNotificationBadge(0);
            
            this.showToast('success', 'Todas las notificaciones marcadas como leídas');
        } catch (error) {
            console.error('Error marking all notifications as read:', error);
            this.showToast('error', 'Error al marcar las notificaciones');
        }
    }
    
    updateNotificationBadge(count) {
        const badge = document.querySelector('.notifications-badge');
        if (badge) {
            if (typeof count === 'number') {
                const currentCount = parseInt(badge.textContent) || 0;
                const newCount = count < 0 ? Math.max(0, currentCount + count) : count;
                badge.textContent = newCount;
                badge.style.display = newCount > 0 ? '' : 'none';
            }
        }
    }
    
    startAutoRefresh() {
        if (this.options.refreshInterval > 0) {
            this.refreshTimer = setInterval(() => {
                this.refreshAllWidgets();
            }, this.options.refreshInterval);
        }
    }
    
    stopAutoRefresh() {
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
        }
    }
    
    async refreshAllWidgets() {
        const refreshBtn = document.getElementById('dashboard-refresh');
        if (refreshBtn) {
            refreshBtn.disabled = true;
            const icon = refreshBtn.querySelector('.fa-rotate');
            if (icon) icon.classList.add('fa-spin');
        }
        
        try {
            // Refresh each widget
            for (const [name, widget] of this.widgets) {
                if (widget.loader) {
                    await widget.loader.call(this, widget);
                }
            }
            
            this.showToast('success', 'Dashboard actualizado');
        } catch (error) {
            console.error('Error refreshing dashboard:', error);
            this.showToast('error', 'Error al actualizar el dashboard');
        } finally {
            if (refreshBtn) {
                refreshBtn.disabled = false;
                const icon = refreshBtn.querySelector('.fa-rotate');
                if (icon) icon.classList.remove('fa-spin');
            }
        }
    }
    
    async loadProgressWidget(widget) {
        try {
            const response = await fetch(`${this.options.apiEndpoints.getWidgetData}?widgetCode=progress&projectId=${this.options.projectId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                // Update progress values
                this.updateProgressValues(result.data);
            }
        } catch (error) {
            console.error('Error loading progress widget:', error);
        }
    }
    
    async loadTasksWidget(widget) {
        try {
            const response = await fetch(`${this.options.apiEndpoints.getWidgetData}?widgetCode=tasks&projectId=${this.options.projectId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                // Update tasks list
                this.updateTasksList(result.data);
            }
        } catch (error) {
            console.error('Error loading tasks widget:', error);
        }
    }
    
    async loadNotificationsWidget(widget) {
        try {
            const response = await fetch(`${this.options.apiEndpoints.getWidgetData}?widgetCode=notifications&projectId=${this.options.projectId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                // Update notifications
                this.updateNotificationsList(result.data);
            }
        } catch (error) {
            console.error('Error loading notifications widget:', error);
        }
    }
    
    async loadMilestonesWidget(widget) {
        try {
            const response = await fetch(`${this.options.apiEndpoints.getWidgetData}?widgetCode=milestones&projectId=${this.options.projectId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                // Update milestones timeline
                this.updateMilestonesTimeline(result.data);
            }
        } catch (error) {
            console.error('Error loading milestones widget:', error);
        }
    }
    
    updateProgressValues(data) {
        // Update progress cards
        if (data.overall) {
            const overallProgress = document.querySelector('[data-progress-overall]');
            if (overallProgress) {
                overallProgress.textContent = `${data.overall}%`;
            }
        }
    }
    
    updateTasksList(data) {
        // This would update the tasks list dynamically
        // For now, we'll just update the count
        const taskCount = document.querySelector('[data-task-count]');
        if (taskCount && data.pending) {
            taskCount.textContent = data.pending;
        }
    }
    
    updateNotificationsList(data) {
        // Update notifications list and badge
        if (data.unread) {
            this.updateNotificationBadge(data.unread);
        }
    }
    
    updateMilestonesTimeline(data) {
        // Update milestones progress
        if (data.completed && data.total) {
            const progressText = document.querySelector('[data-milestones-progress]');
            if (progressText) {
                progressText.textContent = `${data.completed} / ${data.total} completados`;
            }
        }
    }
    
    initializeProgressCharts() {
        document.querySelectorAll('.echart-progress-chart').forEach(element => {
            const progress = parseInt(element.dataset.progress) || 0;
            const chart = echarts.init(element);
            
            const option = {
                series: [{
                    type: 'pie',
                    radius: ['70%', '90%'],
                    avoidLabelOverlap: false,
                    label: { show: false },
                    emphasis: { scale: false },
                    data: [
                        { value: progress, itemStyle: { color: '#3874FF' } },
                        { value: 100 - progress, itemStyle: { color: '#E3E6ED' } }
                    ]
                }]
            };
            
            chart.setOption(option);
            
            // Make chart responsive
            window.addEventListener('resize', () => chart.resize());
        });
    }
    
    loadDashboardSettings() {
        const settings = localStorage.getItem('dashboardSettings');
        if (settings) {
            try {
                const parsed = JSON.parse(settings);
                this.applySettings(parsed);
            } catch (error) {
                console.error('Error loading dashboard settings:', error);
            }
        }
    }
    
    applySettings(settings) {
        // Apply refresh interval
        if (settings.refreshInterval !== undefined) {
            this.stopAutoRefresh();
            this.options.refreshInterval = parseInt(settings.refreshInterval);
            this.startAutoRefresh();
        }
        
        // Apply theme
        if (settings.theme) {
            // This would integrate with the Phoenix theme system
            document.documentElement.setAttribute('data-theme', settings.theme);
        }
        
        // Apply notification settings
        if (settings.showNotifications !== undefined) {
            const notificationsWidget = document.querySelector('.notifications-widget');
            if (notificationsWidget) {
                notificationsWidget.style.display = settings.showNotifications ? '' : 'none';
            }
        }
    }
    
    showToast(type, message) {
        // Use Phoenix template's toast system if available
        if (window.phoenix && window.phoenix.toast) {
            window.phoenix.toast[type](message);
        } else if (window.showToast) {
            window.showToast(type, message);
        } else {
            // Fallback to console
            console.log(`[${type.toUpperCase()}] ${message}`);
        }
    }
    
    // Helper Methods
    showModal(html, modalId) {
        // Remove existing modal if present
        const existingModal = document.getElementById(modalId);
        if (existingModal) {
            existingModal.remove();
        }
        
        // Add new modal to DOM
        document.body.insertAdjacentHTML('beforeend', html);
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById(modalId));
        modal.show();
    }
    
    async saveNewTask() {
        const form = document.getElementById('newTaskForm');
        const formData = {
            title: form.querySelector('#taskTitle').value,
            description: form.querySelector('#taskDescription').value,
            priority: form.querySelector('#taskPriority').value,
            dueDate: form.querySelector('#taskDueDate').value,
            projectId: this.options.projectId
        };
        
        try {
            const response = await fetch('/BusinessIncubators/StarterDashboard/CreateTask', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(formData)
            });
            
            if (response.ok) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('newTaskModal'));
                modal.hide();
                this.showToast('success', 'Tarea creada correctamente');
                await this.refreshWidget('tasks');
            } else {
                this.showToast('error', 'Error al crear la tarea');
            }
        } catch (error) {
            console.error('Error creating task:', error);
            this.showToast('error', 'Error al conectar con el servidor');
        }
    }
    
    async sendMentorMessage() {
        const form = document.getElementById('contactMentorForm');
        const formData = {
            subject: form.querySelector('#messageSubject').value,
            content: form.querySelector('#messageContent').value,
            priority: form.querySelector('#messagePriority').value,
            projectId: this.options.projectId
        };
        
        try {
            const response = await fetch('/BusinessIncubators/StarterDashboard/ContactMentor', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(formData)
            });
            
            if (response.ok) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('contactMentorModal'));
                modal.hide();
                this.showToast('success', 'Mensaje enviado al mentor');
            } else {
                this.showToast('error', 'Error al enviar el mensaje');
            }
        } catch (error) {
            console.error('Error sending mentor message:', error);
            this.showToast('error', 'Error al conectar con el servidor');
        }
    }
    
    async performQuickSearch(query) {
        if (!query || query.length < 2) {
            document.getElementById('quickSearchResults').innerHTML = '';
            return;
        }
        
        try {
            const response = await fetch(`/BusinessIncubators/StarterDashboard/QuickSearch?q=${encodeURIComponent(query)}&projectId=${this.options.projectId}`);
            const results = await response.json();
            
            const resultsHtml = results.map(item => `
                <a href="${item.url}" class="list-group-item list-group-item-action">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${item.title}</h6>
                        <small class="badge badge-phoenix badge-phoenix-${item.typeBadge}">${item.type}</small>
                    </div>
                    <p class="mb-1 text-muted small">${item.description}</p>
                </a>
            `).join('');
            
            document.getElementById('quickSearchResults').innerHTML = resultsHtml || '<p class="text-center text-muted">No se encontraron resultados</p>';
        } catch (error) {
            console.error('Error performing search:', error);
        }
    }
    
    async addWidgetToDashboard(widgetCode) {
        const widget = await this.addWidget(widgetCode);
        if (widget) {
            this.widgetCatalog.active.push(widgetCode);
            const modal = bootstrap.Modal.getInstance(document.getElementById('addWidgetModal'));
            if (modal) modal.hide();
            this.showToast('success', 'Widget agregado correctamente');
        }
    }
    
    destroy() {
        // Call parent destroy
        super.destroy();
        
        // Clean up starter-specific resources
        this.starterWidgets.clear();
        
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
        }
    }
}

// Utility function for debouncing
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Export for use
window.StarterDashboard = StarterDashboard;