// Dashboard Performance Optimization Module
// Implements progressive loading, caching, and parallel widget loading

(function () {
    'use strict';

    // Dashboard cache with TTL
    const DashboardCache = {
        // Default TTL in minutes
        defaultTTL: 5,

        // Set data in sessionStorage with expiry
        set: function(key, data, ttlMinutes) {
            const ttl = (ttlMinutes || this.defaultTTL) * 60 * 1000;
            const item = {
                data: data,
                expiry: Date.now() + ttl
            };
            try {
                sessionStorage.setItem(`dashboard_${key}`, JSON.stringify(item));
            } catch (e) {
                console.warn('Failed to cache dashboard data:', e);
            }
        },

        // Get data from sessionStorage if not expired
        get: function(key) {
            try {
                const item = sessionStorage.getItem(`dashboard_${key}`);
                if (!item) return null;

                const parsed = JSON.parse(item);
                if (Date.now() > parsed.expiry) {
                    sessionStorage.removeItem(`dashboard_${key}`);
                    return null;
                }
                return parsed.data;
            } catch (e) {
                console.warn('Failed to retrieve cached data:', e);
                return null;
            }
        },

        // Clear specific key or all dashboard cache
        clear: function(key) {
            if (key) {
                sessionStorage.removeItem(`dashboard_${key}`);
            } else {
                // Clear all dashboard cache
                Object.keys(sessionStorage)
                    .filter(k => k.startsWith('dashboard_'))
                    .forEach(k => sessionStorage.removeItem(k));
            }
        }
    };

    // Widget loader with progressive rendering
    const WidgetLoader = {
        // Widget priority order (critical widgets load first)
        widgetPriority: {
            'participants': 1,
            'diagnostics': 1,
            'reviews': 2,
            'activity': 3,
            'notifications': 3
        },

        // Load all dashboard widgets
        loadAllWidgets: async function() {
            const widgets = document.querySelectorAll('[data-widget-code]');
            if (!widgets.length) return;

            // Group widgets by priority
            const widgetsByPriority = {};
            widgets.forEach(widget => {
                const code = widget.dataset.widgetCode;
                const priority = this.widgetPriority[code] || 4;
                
                if (!widgetsByPriority[priority]) {
                    widgetsByPriority[priority] = [];
                }
                widgetsByPriority[priority].push(widget);
            });

            // Load widgets in priority order
            const priorities = Object.keys(widgetsByPriority).sort();
            for (const priority of priorities) {
                await this.loadWidgetGroup(widgetsByPriority[priority]);
            }
        },

        // Load a group of widgets in parallel
        loadWidgetGroup: async function(widgets) {
            const promises = widgets.map(widget => this.loadWidget(widget));
            await Promise.allSettled(promises);
        },

        // Load individual widget
        loadWidget: async function(widgetElement) {
            const code = widgetElement.dataset.widgetCode;
            const url = widgetElement.dataset.widgetUrl;
            
            if (!url) return;

            // Show loading skeleton
            this.showSkeleton(widgetElement);

            try {
                // Check cache first
                const cacheKey = `widget_${code}_${window.projectId}`;
                const cachedData = DashboardCache.get(cacheKey);
                
                if (cachedData) {
                    this.renderWidget(widgetElement, cachedData);
                    return;
                }

                // Fetch widget data
                const response = await fetch(url, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!response.ok) throw new Error(`HTTP ${response.status}`);

                const result = await response.json();
                
                if (result.success) {
                    // Cache the data
                    DashboardCache.set(cacheKey, result.data);
                    this.renderWidget(widgetElement, result.data);
                } else {
                    this.showError(widgetElement, result.message || 'Error al cargar widget');
                }
            } catch (error) {
                console.error(`Error loading widget ${code}:`, error);
                this.showError(widgetElement, 'Error al cargar widget');
            }
        },

        // Show loading skeleton
        showSkeleton: function(widgetElement) {
            const content = widgetElement.querySelector('.widget-content');
            if (!content) return;

            content.innerHTML = `
                <div class="skeleton-loader">
                    <div class="skeleton-line"></div>
                    <div class="skeleton-line"></div>
                    <div class="skeleton-line"></div>
                </div>
            `;
        },

        // Render widget with data
        renderWidget: function(widgetElement, data) {
            const content = widgetElement.querySelector('.widget-content');
            if (!content) return;

            const code = widgetElement.dataset.widgetCode;
            const renderer = this.widgetRenderers[code];
            
            if (renderer) {
                content.innerHTML = renderer(data);
            } else {
                content.innerHTML = '<pre>' + JSON.stringify(data, null, 2) + '</pre>';
            }

            // Trigger rendered event
            widgetElement.dispatchEvent(new CustomEvent('widget-rendered', { 
                detail: { code, data } 
            }));
        },

        // Show error state
        showError: function(widgetElement, message) {
            const content = widgetElement.querySelector('.widget-content');
            if (!content) return;

            content.innerHTML = `
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle"></i> ${message}
                </div>
            `;
        },

        // Widget-specific renderers
        widgetRenderers: {
            participants: function(data) {
                return `
                    <div class="row">
                        <div class="col-md-3">
                            <div class="stat-box">
                                <div class="stat-value">${data.totalCount || 0}</div>
                                <div class="stat-label">Total</div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="stat-box">
                                <div class="stat-value">${data.activeCount || 0}</div>
                                <div class="stat-label">Activos</div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="stat-box">
                                <div class="stat-value">${data.pendingInvitations || 0}</div>
                                <div class="stat-label">Invitaciones</div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="stat-box">
                                <div class="stat-value">${data.recentlyAdded || 0}</div>
                                <div class="stat-label">Nuevos (7d)</div>
                            </div>
                        </div>
                    </div>
                `;
            },

            diagnostics: function(data) {
                const completionRate = data.completionRate || 0;
                return `
                    <div class="progress mb-3" style="height: 25px;">
                        <div class="progress-bar bg-success" style="width: ${completionRate}%">
                            ${completionRate.toFixed(1)}% Completado
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <small class="text-muted">Completados</small>
                            <div class="h5">${data.completedForms || 0}</div>
                        </div>
                        <div class="col-md-4">
                            <small class="text-muted">En Progreso</small>
                            <div class="h5">${data.inProgressForms || 0}</div>
                        </div>
                        <div class="col-md-4">
                            <small class="text-muted">Sin Iniciar</small>
                            <div class="h5">${data.notStartedCount || 0}</div>
                        </div>
                    </div>
                `;
            },

            reviews: function(data) {
                if (!data.topReviews || data.topReviews.length === 0) {
                    return '<p class="text-muted">No hay revisiones pendientes</p>';
                }

                const reviewsHtml = data.topReviews.slice(0, 5).map(review => `
                    <div class="review-item mb-2 p-2 border-start border-3 border-warning">
                        <div class="d-flex justify-content-between">
                            <strong>${review.participantName || 'Usuario'}</strong>
                            <small class="text-muted">${review.daysWaiting}d esperando</small>
                        </div>
                        <small class="text-muted">${review.formType}</small>
                    </div>
                `).join('');

                return `
                    <div class="mb-2">
                        <span class="badge bg-warning text-dark">${data.totalPending} pendientes</span>
                    </div>
                    ${reviewsHtml}
                `;
            },

            activity: function(data) {
                if (!data || data.length === 0) {
                    return '<p class="text-muted">No hay actividad reciente</p>';
                }

                const activitiesHtml = data.slice(0, 10).map(activity => `
                    <div class="activity-item mb-2">
                        <div class="d-flex justify-content-between">
                            <span>
                                <i class="bi bi-person-circle"></i> 
                                ${activity.userName || 'Usuario'}
                            </span>
                            <small class="text-muted">${activity.timeAgo}</small>
                        </div>
                        <small class="text-muted">${activity.actionDescription}</small>
                    </div>
                `).join('');

                return activitiesHtml;
            }
        }
    };

    // Auto-refresh functionality
    const AutoRefresh = {
        interval: null,
        defaultInterval: 300000, // 5 minutes

        start: function(interval) {
            this.stop();
            this.interval = setInterval(() => {
                this.refresh();
            }, interval || this.defaultInterval);
        },

        stop: function() {
            if (this.interval) {
                clearInterval(this.interval);
                this.interval = null;
            }
        },

        refresh: function() {
            // Clear cache to force fresh data
            DashboardCache.clear();
            // Reload widgets
            WidgetLoader.loadAllWidgets();
        }
    };

    // Performance monitoring
    const PerformanceMonitor = {
        mark: function(name) {
            if (window.performance && window.performance.mark) {
                window.performance.mark(name);
            }
        },

        measure: function(name, startMark, endMark) {
            if (window.performance && window.performance.measure) {
                try {
                    window.performance.measure(name, startMark, endMark);
                    const measure = window.performance.getEntriesByName(name)[0];
                    console.log(`[Performance] ${name}: ${measure.duration.toFixed(2)}ms`);
                    return measure.duration;
                } catch (e) {
                    console.warn('Performance measurement failed:', e);
                }
            }
            return null;
        }
    };

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function() {
        // Mark start of dashboard loading
        PerformanceMonitor.mark('dashboard-start');

        // Load widgets progressively
        WidgetLoader.loadAllWidgets().then(() => {
            // Mark end of dashboard loading
            PerformanceMonitor.mark('dashboard-end');
            PerformanceMonitor.measure('dashboard-total-load', 'dashboard-start', 'dashboard-end');
        });

        // Start auto-refresh if enabled
        if (window.dashboardAutoRefresh) {
            AutoRefresh.start();
        }

        // Handle manual refresh button
        const refreshBtn = document.querySelector('[data-action="refresh-dashboard"]');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', function(e) {
                e.preventDefault();
                AutoRefresh.refresh();
            });
        }
    });

    // Expose API for external use
    window.DashboardPerformance = {
        cache: DashboardCache,
        loader: WidgetLoader,
        autoRefresh: AutoRefresh,
        monitor: PerformanceMonitor
    };

})();