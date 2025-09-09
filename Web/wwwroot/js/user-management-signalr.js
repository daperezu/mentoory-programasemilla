// User Management SignalR Client
(function () {
    'use strict';

    // Configuration
    const hubUrl = '/hubs/user-management';
    let connection = null;
    let reconnectAttempts = 0;
    const maxReconnectAttempts = 5;
    const reconnectDelay = 3000; // 3 seconds

    // Initialize SignalR connection
    function initializeConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount >= maxReconnectAttempts) {
                        return null; // Stop reconnecting
                    }
                    return Math.min(retryContext.previousRetryCount * 2000, 10000);
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Connection event handlers
        connection.onreconnecting(error => {
            console.warn('SignalR reconnecting...', error);
            showConnectionStatus('warning', 'Reconectando...');
        });

        connection.onreconnected(connectionId => {
            console.log('SignalR reconnected:', connectionId);
            showConnectionStatus('success', 'Reconectado');
            reconnectAttempts = 0;
            
            // Refresh the DataTable to get latest data
            if (window.userDataTable) {
                window.userDataTable.ajax.reload(null, false);
            }
        });

        connection.onclose(error => {
            if (error) {
                console.error('SignalR connection closed with error:', error);
                showConnectionStatus('danger', 'Conexión perdida');
                attemptManualReconnect();
            } else {
                console.log('SignalR connection closed');
                showConnectionStatus('info', 'Desconectado');
            }
        });

        // Register event handlers
        registerEventHandlers();

        // Start the connection
        startConnection();
    }

    // Start connection with error handling
    async function startConnection() {
        try {
            await connection.start();
            console.log('SignalR connected successfully');
            showConnectionStatus('success', 'Conectado');
            reconnectAttempts = 0;
        } catch (err) {
            console.error('SignalR connection failed:', err);
            showConnectionStatus('danger', 'Error de conexión');
            attemptManualReconnect();
        }
    }

    // Manual reconnection logic
    function attemptManualReconnect() {
        if (reconnectAttempts < maxReconnectAttempts) {
            reconnectAttempts++;
            console.log(`Attempting reconnection ${reconnectAttempts}/${maxReconnectAttempts}...`);
            setTimeout(() => {
                startConnection();
            }, reconnectDelay * reconnectAttempts);
        } else {
            console.error('Max reconnection attempts reached');
            showConnectionStatus('danger', 'No se pudo reconectar. Por favor, recarga la página.');
        }
    }

    // Register SignalR event handlers
    function registerEventHandlers() {
        // User created event
        connection.on('UserCreated', function (data) {
            console.log('User created:', data);
            
            // Show toast notification
            showToast(`Nuevo usuario creado: ${data.email}`, 'success');
            
            // Refresh DataTable if it exists
            if (window.userDataTable) {
                window.userDataTable.ajax.reload(null, false);
            }
            
            // Update user count if element exists
            updateUserCount(1);
        });

        // User updated event
        connection.on('UserUpdated', function (data) {
            console.log('User updated:', data);
            
            // Show toast notification with changed fields
            const changedFields = Object.keys(data.changes).join(', ');
            showToast(`Usuario actualizado: ${changedFields}`, 'info');
            
            // Refresh specific row in DataTable if possible
            if (window.userDataTable) {
                // Try to find and update the specific row
                const row = window.userDataTable.row(`#user-${data.userId}`);
                if (row.node()) {
                    row.invalidate().draw(false);
                } else {
                    // Fallback to full reload
                    window.userDataTable.ajax.reload(null, false);
                }
            }
        });

        // User status changed event
        connection.on('UserStatusChanged', function (data) {
            console.log('User status changed:', data);
            
            const status = data.isActive ? 'activado' : 'desactivado';
            showToast(`Usuario ${status}`, 'warning');
            
            // Update DataTable
            if (window.userDataTable) {
                window.userDataTable.ajax.reload(null, false);
            }
        });

        // Bulk operation progress event
        connection.on('BulkOperationProgress', function (data) {
            console.log('Bulk operation progress:', data);
            updateBulkOperationProgress(data);
        });
    }

    // Show connection status
    function showConnectionStatus(type, message) {
        const statusElement = document.getElementById('signalr-status');
        if (statusElement) {
            statusElement.className = `badge bg-${type}`;
            statusElement.textContent = message;
        }
    }

    // Show toast notification
    function showToast(message, type) {
        // Check if the showToast function exists (from the main application)
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
        } else {
            // Fallback to console
            console.log(`[${type.toUpperCase()}] ${message}`);
        }
    }

    // Update user count display
    function updateUserCount(delta) {
        const countElement = document.getElementById('user-count');
        if (countElement) {
            const currentCount = parseInt(countElement.textContent) || 0;
            countElement.textContent = currentCount + delta;
        }
    }

    // Update bulk operation progress modal
    function updateBulkOperationProgress(data) {
        // Update progress bar
        const progressBar = document.getElementById('bulk-progress-bar');
        if (progressBar) {
            progressBar.style.width = `${data.progress}%`;
            progressBar.setAttribute('aria-valuenow', data.progress);
            progressBar.textContent = `${data.progress}%`;
        }

        // Update status message
        const statusMessage = document.getElementById('bulk-status-message');
        if (statusMessage) {
            statusMessage.textContent = data.message;
        }

        // Update counters
        const currentItem = document.getElementById('bulk-current-item');
        if (currentItem) {
            currentItem.textContent = data.currentItem;
        }

        const totalItems = document.getElementById('bulk-total-items');
        if (totalItems) {
            totalItems.textContent = data.totalItems;
        }

        const successCount = document.getElementById('bulk-success-count');
        if (successCount) {
            successCount.textContent = data.successCount;
            successCount.className = 'text-success fw-bold';
        }

        const failureCount = document.getElementById('bulk-failure-count');
        if (failureCount) {
            failureCount.textContent = data.failureCount;
            failureCount.className = data.failureCount > 0 ? 'text-danger fw-bold' : 'text-muted';
        }

        // Show/hide modal if needed
        const progressModal = document.getElementById('bulkProgressModal');
        if (progressModal) {
            const modal = bootstrap.Modal.getInstance(progressModal) || new bootstrap.Modal(progressModal);
            
            if (data.progress < 100 && data.progress > 0) {
                modal.show();
            } else if (data.progress >= 100) {
                // Operation complete
                setTimeout(() => {
                    modal.hide();
                    
                    // Show completion notification
                    if (data.failureCount > 0) {
                        showToast(`Operación completada con ${data.failureCount} errores`, 'warning');
                    } else {
                        showToast('Operación completada exitosamente', 'success');
                    }
                    
                    // Refresh DataTable
                    if (window.userDataTable) {
                        window.userDataTable.ajax.reload();
                    }
                }, 1500);
            }
        }
    }

    // Public API
    window.UserManagementSignalR = {
        initialize: initializeConnection,
        getConnection: () => connection,
        isConnected: () => connection && connection.state === signalR.HubConnectionState.Connected,
        
        // Manual methods for testing
        notifyUserCreated: async (userId, email, role) => {
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                await connection.invoke('NotifyUserCreated', userId, email, role);
            }
        },
        
        notifyUserUpdated: async (userId, changes) => {
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                await connection.invoke('NotifyUserUpdated', userId, changes);
            }
        },
        
        notifyUserStatusChanged: async (userId, isActive) => {
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                await connection.invoke('NotifyUserStatusChanged', userId, isActive);
            }
        },
        
        subscribeToIncubator: async (incubatorId) => {
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                await connection.invoke('SubscribeToIncubator', incubatorId);
            }
        },
        
        unsubscribeFromIncubator: async (incubatorId) => {
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                await connection.invoke('UnsubscribeFromIncubator', incubatorId);
            }
        }
    };

    // Auto-initialize when document is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeConnection);
    } else {
        initializeConnection();
    }
})();