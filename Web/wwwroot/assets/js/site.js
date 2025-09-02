function showToast(message = '', type = 'info', icon = null, delay = 0, header = null) {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const defaultIcons = {
        info: 'info',
        success: 'check-circle',
        warning: 'alert-triangle',
        danger: 'x-circle'
    };

    const defaultHeaders = {
        info: 'Info',
        success: 'Éxito',
        warning: 'Atención',
        danger: 'Error'
    };

    const iconName = icon ?? defaultIcons[type] ?? 'info';
    const toastHeader = header ?? defaultHeaders[type] ?? 'Info';
    const toastId = `toast-${Date.now()}`;

    const toastEl = document.createElement('div');
    toastEl.className = `toast text-bg-${type} border-0 mb-2 shadow-sm`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    toastEl.innerHTML = `
    <div class="toast-header text-white bg-transparent border-0">
      <i data-feather="${iconName}" class="me-2"></i>
      <strong class="me-auto">${toastHeader}</strong>
      <span id="${toastId}-timer" class="badge bg-dark-subtle text-white rounded-pill me-2 small" style="display:none;"></span>
      <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
    ${message ? `<div class="toast-body">${message}</div>` : ''}
  `;

    container.appendChild(toastEl);

    // Replace Feather icons
    if (window.feather) {
        feather.replace();
    }

    const toast = new bootstrap.Toast(toastEl, {
        autohide: false
    });

    toast.show();

    // Timer logic
    let seconds = 0;
    const timerLabel = document.getElementById(`${toastId}-timer`);
    const interval = setInterval(() => {
        seconds++;

        if (seconds >= 15 && seconds % 15 === 0) {
            let label = '';

            if (seconds >= 300) {
                label = 'hace más de 5 minutos';
                clearInterval(interval);
            } else if (seconds >= 60) {
                const mins = Math.floor(seconds / 60);
                const secs = seconds % 60;
                label = `hace ${mins}min${secs > 0 ? ` ${secs}s` : ''}`;
            } else {
                label = `hace ${seconds}s`;
            }

            timerLabel.textContent = label;
            timerLabel.style.display = 'inline-block';
        }
    }, 1000);

    toastEl.addEventListener('hidden.bs.toast', () => {
        clearInterval(interval);
        toastEl.remove();
    });
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
