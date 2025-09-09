function showToast(message = '', type = 'info', header = null) {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    // Auto-dismiss durations (in milliseconds)
    const durations = {
        success: 4000,
        info: 5000,
        warning: 8000,
        danger: 0  // Sticky for errors
    };

    // Phoenix-aligned configuration
    const config = {
        info: { icon: 'info-circle', header: 'Información', colorClass: 'toast-info' },
        success: { icon: 'check-circle', header: 'Éxito', colorClass: 'toast-success' },
        warning: { icon: 'alert-triangle', header: 'Atención', colorClass: 'toast-warning' },
        danger: { icon: 'x-octagon', header: 'Error', colorClass: 'toast-danger' }
    };

    const typeConfig = config[type] || config.info;
    const toastHeader = header || typeConfig.header;
    const toastId = `toast-${Date.now()}`;
    const duration = durations[type] || durations.info;

    const toastEl = document.createElement('div');
    toastEl.className = `toast phoenix-toast ${typeConfig.colorClass} mb-2`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', type === 'danger' ? 'assertive' : 'polite');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.setAttribute('data-bs-autohide', duration > 0 ? 'true' : 'false');
    if (duration > 0) {
        toastEl.setAttribute('data-bs-delay', duration);
    }

    // Build toast HTML with progress bar
    toastEl.innerHTML = `
        <div class="toast-header phoenix-toast-header">
            <i data-feather="${typeConfig.icon}" class="toast-icon me-2"></i>
            <strong class="me-auto">${toastHeader}</strong>
            <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Cerrar"></button>
        </div>
        ${message ? `<div class="toast-body">${message}</div>` : ''}
        ${duration > 0 ? `<div class="toast-progress" style="animation-duration: ${duration}ms;"></div>` : ''}
    `;

    container.appendChild(toastEl);

    // Replace Feather icons
    if (window.feather) {
        feather.replace();
    }

    // Initialize Bootstrap Toast
    const toast = new bootstrap.Toast(toastEl, {
        autohide: duration > 0,
        delay: duration
    });

    // Handle hover to pause progress bar
    if (duration > 0) {
        const progressBar = toastEl.querySelector('.toast-progress');
        let isPaused = false;
        let remainingTime = duration;
        let startTime = Date.now();
        let animationId;

        toastEl.addEventListener('mouseenter', () => {
            if (!isPaused && progressBar) {
                isPaused = true;
                const elapsed = Date.now() - startTime;
                remainingTime = duration - elapsed;
                progressBar.style.animationPlayState = 'paused';
                
                // Cancel auto-hide
                toast._config.autohide = false;
                clearTimeout(toast._timeout);
            }
        });

        toastEl.addEventListener('mouseleave', () => {
            if (isPaused && progressBar) {
                isPaused = false;
                startTime = Date.now();
                progressBar.style.animationPlayState = 'running';
                
                // Resume auto-hide with remaining time
                toast._config.autohide = true;
                toast._timeout = setTimeout(() => {
                    toast.hide();
                }, remainingTime);
            }
        });
    }

    // Show the toast
    toast.show();

    // Clean up on hide
    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });

    return toast;
}


document.addEventListener('DOMContentLoaded', () => {
    // Initialize Feather icons on page load
    if (window.feather) {
        feather.replace();
    }

    // Process any queued toast messages
    if (window._toastMessages && Array.isArray(window._toastMessages)) {
        window._toastMessages.forEach(t => showToast(t.message, t.type, t.icon));
        window._toastMessages = []; // optional cleanup
    }
});
