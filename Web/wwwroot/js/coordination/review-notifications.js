// Review Notifications Module
window.ReviewNotifications = (function() {
    'use strict';

    let connection = null;
    let isConnected = false;
    let currentProjectId = null;
    let reconnectAttempts = 0;
    const maxReconnectAttempts = 5;
    const reconnectDelay = 3000;

    // Initialize SignalR connection
    async function init(projectId) {
        if (!projectId) {
            console.error('Project ID is required for notifications');
            return;
        }

        currentProjectId = projectId;

        // Create SignalR connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/review-notifications')
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount >= maxReconnectAttempts) {
                        return null;
                    }
                    return Math.min(reconnectDelay * Math.pow(2, retryContext.previousRetryCount), 30000);
                }
            })
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Setup event handlers
        setupConnectionHandlers();
        setupNotificationHandlers();

        // Start connection
        await startConnection();
    }

    // Setup connection event handlers
    function setupConnectionHandlers() {
        connection.onreconnecting(error => {
            console.log('Reconnecting to notification hub...', error);
            showConnectionStatus('reconnecting');
        });

        connection.onreconnected(connectionId => {
            console.log('Reconnected to notification hub', connectionId);
            showConnectionStatus('connected');
            reconnectAttempts = 0;
            
            // Rejoin project group after reconnection
            if (currentProjectId) {
                joinProjectGroup(currentProjectId);
            }
        });

        connection.onclose(error => {
            console.log('Disconnected from notification hub', error);
            isConnected = false;
            showConnectionStatus('disconnected');
            
            // Try to reconnect manually if auto-reconnect fails
            if (reconnectAttempts < maxReconnectAttempts) {
                setTimeout(() => {
                    reconnectAttempts++;
                    startConnection();
                }, reconnectDelay * Math.pow(2, reconnectAttempts));
            }
        });
    }

    // Setup notification handlers
    function setupNotificationHandlers() {
        connection.on('ReceiveNotification', notification => {
            console.log('Received notification:', notification);
            handleNotification(notification);
        });
    }

    // Start SignalR connection
    async function startConnection() {
        try {
            await connection.start();
            isConnected = true;
            showConnectionStatus('connected');
            console.log('Connected to notification hub');
            
            // Join project group
            if (currentProjectId) {
                await joinProjectGroup(currentProjectId);
            }
        } catch (error) {
            console.error('Failed to connect to notification hub:', error);
            showConnectionStatus('error');
            
            // Retry connection
            if (reconnectAttempts < maxReconnectAttempts) {
                setTimeout(() => {
                    reconnectAttempts++;
                    startConnection();
                }, reconnectDelay * Math.pow(2, reconnectAttempts));
            }
        }
    }

    // Join project group for notifications
    async function joinProjectGroup(projectId) {
        if (!isConnected) {
            console.warn('Cannot join project group - not connected');
            return;
        }

        try {
            await connection.invoke('JoinProjectGroup', projectId);
            console.log(`Joined project group ${projectId}`);
        } catch (error) {
            console.error('Failed to join project group:', error);
        }
    }

    // Leave project group
    async function leaveProjectGroup(projectId) {
        if (!isConnected) return;

        try {
            await connection.invoke('LeaveProjectGroup', projectId);
            console.log(`Left project group ${projectId}`);
        } catch (error) {
            console.error('Failed to leave project group:', error);
        }
    }

    // Handle incoming notifications
    function handleNotification(notification) {
        switch (notification.type) {
            case 'statusChange':
                handleStatusChangeNotification(notification);
                break;
            case 'newFeedback':
                handleNewFeedbackNotification(notification);
                break;
            case 'reviewAssigned':
                handleReviewAssignedNotification(notification);
                break;
            case 'deadlineWarning':
                handleDeadlineWarningNotification(notification);
                break;
            default:
                console.warn('Unknown notification type:', notification.type);
        }

        // Update UI elements if they exist
        updateNotificationBadge();
        addToNotificationList(notification);
    }

    // Handle status change notification
    function handleStatusChangeNotification(notification) {
        const icon = getStatusIcon(notification.status);
        const toastType = getStatusToastType(notification.status);
        
        showNotificationToast(notification.message, toastType, icon);
        
        // Update submission status in UI if on review page
        if (window.submissionId && window.submissionId === notification.submissionId) {
            updateSubmissionStatus(notification.status);
        }
        
        // Refresh list if on index page
        if (typeof window.loadSubmissions === 'function') {
            window.loadSubmissions();
        }
    }

    // Handle new feedback notification
    function handleNewFeedbackNotification(notification) {
        showNotificationToast(notification.message, 'info', 'fas fa-comment');
        
        // Reload submission details if on the same review page
        if (window.submissionId && window.submissionId === notification.submissionId) {
            if (typeof window.FormReview !== 'undefined' && window.FormReview.loadSubmissionDetails) {
                window.FormReview.loadSubmissionDetails(notification.submissionId);
            }
        }
    }

    // Handle review assigned notification
    function handleReviewAssignedNotification(notification) {
        showNotificationToast(notification.message, 'warning', 'fas fa-tasks');
        playNotificationSound();
        
        // Add highlight to new assignment
        highlightNewAssignment(notification.submissionId);
    }

    // Handle deadline warning notification
    function handleDeadlineWarningNotification(notification) {
        const urgency = notification.daysRemaining <= 1 ? 'error' : 'warning';
        showNotificationToast(notification.message, urgency, 'fas fa-clock');
        
        if (notification.daysRemaining <= 1) {
            playNotificationSound();
        }
    }

    // Show notification toast
    function showNotificationToast(message, type, icon) {
        // Use toastr if available
        if (typeof toastr !== 'undefined') {
            const options = {
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right',
                timeOut: 5000
            };
            
            switch (type) {
                case 'success':
                    toastr.success(message, 'Notificación', options);
                    break;
                case 'error':
                    toastr.error(message, 'Notificación', options);
                    break;
                case 'warning':
                    toastr.warning(message, 'Notificación', options);
                    break;
                default:
                    toastr.info(message, 'Notificación', options);
            }
        } else {
            // Fallback to custom notification
            createCustomNotification(message, type, icon);
        }
    }

    // Create custom notification element
    function createCustomNotification(message, type, icon) {
        const notification = document.createElement('div');
        notification.className = `custom-notification notification-${type}`;
        notification.innerHTML = `
            <i class="${icon}"></i>
            <span>${message}</span>
            <button onclick="this.parentElement.remove()">×</button>
        `;
        
        // Add to notification container or create one
        let container = document.getElementById('notificationContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'notificationContainer';
            container.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999;';
            document.body.appendChild(container);
        }
        
        container.appendChild(notification);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            notification.remove();
        }, 5000);
    }

    // Update notification badge
    function updateNotificationBadge() {
        const badge = document.getElementById('notificationBadge');
        if (badge) {
            const currentCount = parseInt(badge.textContent) || 0;
            badge.textContent = currentCount + 1;
            badge.style.display = 'inline-block';
        }
    }

    // Add notification to list
    function addToNotificationList(notification) {
        const list = document.getElementById('notificationList');
        if (!list) return;
        
        const item = document.createElement('div');
        item.className = 'notification-item';
        item.innerHTML = `
            <div class="notification-content">
                <div class="notification-message">${notification.message}</div>
                <div class="notification-time">${formatTime(notification.timestamp)}</div>
            </div>
        `;
        
        // Add to top of list
        list.insertBefore(item, list.firstChild);
        
        // Limit to 10 items
        while (list.children.length > 10) {
            list.removeChild(list.lastChild);
        }
    }

    // Show connection status
    function showConnectionStatus(status) {
        const indicator = document.getElementById('connectionIndicator');
        if (!indicator) return;
        
        switch (status) {
            case 'connected':
                indicator.className = 'connection-status connected';
                indicator.title = 'Conectado';
                break;
            case 'reconnecting':
                indicator.className = 'connection-status reconnecting';
                indicator.title = 'Reconectando...';
                break;
            case 'disconnected':
                indicator.className = 'connection-status disconnected';
                indicator.title = 'Desconectado';
                break;
            case 'error':
                indicator.className = 'connection-status error';
                indicator.title = 'Error de conexión';
                break;
        }
    }

    // Play notification sound
    function playNotificationSound() {
        const audio = document.getElementById('notificationSound');
        if (audio) {
            audio.play().catch(error => {
                console.log('Could not play notification sound:', error);
            });
        }
    }

    // Highlight new assignment
    function highlightNewAssignment(submissionId) {
        const element = document.querySelector(`[data-submission-id="${submissionId}"]`);
        if (element) {
            element.classList.add('new-assignment');
            setTimeout(() => {
                element.classList.remove('new-assignment');
            }, 3000);
        }
    }

    // Update submission status in UI
    function updateSubmissionStatus(status) {
        const statusElement = document.getElementById('reviewStatus');
        if (statusElement && typeof window.updateReviewStatus === 'function') {
            window.updateReviewStatus(status);
        }
    }

    // Helper functions
    function getStatusIcon(status) {
        switch (status) {
            case 'Approved': return 'fas fa-check-circle';
            case 'ChangesRequested': return 'fas fa-edit';
            case 'Flagged': return 'fas fa-flag';
            default: return 'fas fa-info-circle';
        }
    }

    function getStatusToastType(status) {
        switch (status) {
            case 'Approved': return 'success';
            case 'ChangesRequested': return 'warning';
            case 'Flagged': return 'error';
            default: return 'info';
        }
    }

    function formatTime(timestamp) {
        const date = new Date(timestamp);
        const now = new Date();
        const diff = Math.floor((now - date) / 1000);
        
        if (diff < 60) return 'Ahora mismo';
        if (diff < 3600) return `Hace ${Math.floor(diff / 60)} minutos`;
        if (diff < 86400) return `Hace ${Math.floor(diff / 3600)} horas`;
        return date.toLocaleDateString('es-ES');
    }

    // Disconnect from hub
    async function disconnect() {
        if (connection && isConnected) {
            try {
                if (currentProjectId) {
                    await leaveProjectGroup(currentProjectId);
                }
                await connection.stop();
                isConnected = false;
                console.log('Disconnected from notification hub');
            } catch (error) {
                console.error('Error disconnecting from hub:', error);
            }
        }
    }

    // Public API
    return {
        init: init,
        disconnect: disconnect,
        joinProjectGroup: joinProjectGroup,
        leaveProjectGroup: leaveProjectGroup,
        isConnected: () => isConnected
    };
})();

// Auto-cleanup on page unload
window.addEventListener('beforeunload', function() {
    if (window.ReviewNotifications) {
        window.ReviewNotifications.disconnect();
    }
});