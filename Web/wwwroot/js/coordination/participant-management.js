/**
 * Participant Management JavaScript
 * Handles participant list interactions, stats loading, and export functionality
 */

(function($) {
    'use strict';
    
    // Module variables
    let participantTable = null;
    let statsCache = null;
    let lastStatsUpdate = 0;
    const STATS_CACHE_DURATION = 5 * 60 * 1000; // 5 minutes
    
    // Initialize when document is ready
    $(document).ready(function() {
        initializeParticipantManagement();
    });
    
    /**
     * Initialize the participant management module
     */
    function initializeParticipantManagement() {
        // Wait for DataTable to be initialized by TagHelper
        setTimeout(function() {
            // Store reference to the already initialized DataTable
            if ($.fn.DataTable.isDataTable('#participantsTable')) {
                participantTable = $('#participantsTable').DataTable();
            }
        }, 100);
        
        // Load initial stats
        loadParticipantStats();
        
        // Set up event handlers
        setupEventHandlers();
        
        // Auto-refresh stats every 2 minutes
        setInterval(function() {
            if (shouldRefreshStats()) {
                loadParticipantStats();
            }
        }, 120000);
    }
    
    /**
     * Setup event handlers for various interactions
     */
    function setupEventHandlers() {
        // Refresh button
        $(document).on('click', '#refreshTable', function(e) {
            e.preventDefault();
            refreshTableData();
        });
        
        // Export buttons
        $(document).on('click', '[data-export]', function(e) {
            e.preventDefault();
            const format = $(this).data('export');
            exportParticipants(format);
        });
        
        // Status filter quick buttons
        $(document).on('click', '[data-status-filter]', function(e) {
            e.preventDefault();
            const status = $(this).data('status-filter');
            filterByStatus(status);
        });
        
        // Stats refresh button
        $(document).on('click', '#refreshStats', function(e) {
            e.preventDefault();
            loadParticipantStats(true);
        });
        
        // Fill form on behalf action
        $(document).on('click', 'a[href="#fill-on-behalf"]', function(e) {
            e.preventDefault();
            const row = $(this).closest('tr');
            const data = participantTable ? participantTable.row(row).data() : null;
            
            if (data) {
                handleFillOnBehalf(data);
            }
        });
    }
    
    /**
     * Load participant statistics
     */
    function loadParticipantStats(forceRefresh = false) {
        // Use cache if available and not expired
        if (!forceRefresh && statsCache && !shouldRefreshStats()) {
            updateStatsDisplay(statsCache);
            return Promise.resolve(statsCache);
        }
        
        return $.ajax({
            url: '/Coordination/Participant/GetStats',
            method: 'GET',
            cache: false
        })
        .done(function(response) {
            if (response && response.success) {
                statsCache = response.data;
                lastStatsUpdate = Date.now();
                updateStatsDisplay(response.data);
            } else {
                console.error('Failed to load participant stats:', response);
                showStatsError();
            }
        })
        .fail(function(xhr, status, error) {
            console.error('Error loading participant stats:', error);
            showStatsError();
            
            // Use cached data if available
            if (statsCache) {
                updateStatsDisplay(statsCache);
            }
        });
    }
    
    /**
     * Update stats display with provided data
     */
    function updateStatsDisplay(stats) {
        $('#totalParticipants').text(stats.totalParticipants || 0);
        $('#activeParticipants').text(stats.activeParticipants || 0);
        $('#completedForms').text(stats.completedForms || 0);
        $('#pendingForms').text(stats.pendingForms || 0);
        
        // Update progress bars if they exist
        updateProgressBars(stats);
        
        // Add animation class
        $('.stat-number').addClass('stat-updated');
        setTimeout(() => {
            $('.stat-number').removeClass('stat-updated');
        }, 1000);
    }
    
    /**
     * Update progress bars based on stats
     */
    function updateProgressBars(stats) {
        const total = stats.totalParticipants || 1;
        const activePercentage = Math.round((stats.activeParticipants / total) * 100);
        const completedPercentage = Math.round((stats.completedForms / total) * 100);
        
        // Update active participants progress
        $('.active-progress .progress-bar').css('width', activePercentage + '%');
        
        // Update form completion progress
        $('.completion-progress .progress-bar').css('width', completedPercentage + '%');
    }
    
    /**
     * Show error state for stats
     */
    function showStatsError() {
        $('.stat-number').text('-');
        showToast('Error al cargar las estadísticas', 'danger');
    }
    
    /**
     * Check if stats should be refreshed
     */
    function shouldRefreshStats() {
        return Date.now() - lastStatsUpdate > STATS_CACHE_DURATION;
    }
    
    /**
     * Refresh table data
     */
    function refreshTableData() {
        if (participantTable) {
            participantTable.ajax.reload(null, false);
            loadParticipantStats(true);
            showToast('Datos actualizados correctamente', 'success');
        }
    }
    
    /**
     * Export participants data
     */
    function exportParticipants(format = 'excel') {
        const exportUrl = `/Coordination/Participant/Export?format=${format}`;
        
        // Show loading indicator
        showToast('Generando archivo de exportación...', 'info', 'Procesando');
        
        // Create a temporary link and trigger download
        const link = document.createElement('a');
        link.href = exportUrl;
        link.download = `participantes_${new Date().toISOString().split('T')[0]}.${format}`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Show success message after a delay
        setTimeout(() => {
            showToast('Archivo descargado correctamente', 'success');
        }, 1000);
    }
    
    /**
     * Filter table by status
     */
    function filterByStatus(status) {
        if (participantTable) {
            const statusColumn = participantTable.column(3); // Status column index
            const searchValue = status === 'active' ? 'true' : status === 'inactive' ? 'false' : '';
            statusColumn.search(searchValue).draw();
        }
    }
    
    /**
     * Handle fill form on behalf action
     */
    function handleFillOnBehalf(participantData) {
        // Check if participant has a pending form
        if (!participantData.formStatus || participantData.formStatus === 'Sin formulario') {
            showToast('Este participante no tiene formularios pendientes', 'warning');
            return;
        }
        
        // Check if form is already completed or approved
        if (participantData.formStatus === 'Aprobado' || participantData.formStatus === 'Rechazado') {
            showToast('El formulario de este participante ya fue procesado', 'info');
            return;
        }
        
        // Show confirmation modal
        showConfirmModal(
            'Completar formulario en nombre de',
            `¿Está seguro que desea completar el formulario en nombre de <strong>${participantData.fullName}</strong>?<br><br>
             <small class="text-muted">Nota: El formulario seguirá el flujo de aprobación normal después de enviarlo.</small>`,
            'Continuar',
            'Cancelar'
        ).then(confirmed => {
            if (confirmed) {
                // Redirect to form editor in on-behalf mode
                const projectId = participantData.projectExternalId || window.currentProjectId;
                const participantUserId = participantData.userId;
                
                if (!projectId || !participantUserId) {
                    showToast('No se pudo obtener la información necesaria', 'danger');
                    return;
                }
                
                // Navigate to form editor with on-behalf parameters
                window.location.href = `/Coordination/Participant/FillFormOnBehalf?projectId=${projectId}&participantUserId=${participantUserId}`;
            }
        });
    }
    
    /**
     * Show confirmation modal
     */
    function showConfirmModal(title, message, confirmText, cancelText) {
        return new Promise((resolve) => {
            const modalHtml = `
                <div class="modal fade" id="confirmModal" tabindex="-1">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${title}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                <p>${message}</p>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
                                <button type="button" class="btn btn-primary" id="confirmBtn">${confirmText}</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            $('#confirmModal').remove();
            $('body').append(modalHtml);
            
            const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
            
            $('#confirmBtn').on('click', () => {
                modal.hide();
                resolve(true);
            });
            
            document.getElementById('confirmModal').addEventListener('hidden.bs.modal', () => {
                $('#confirmModal').remove();
                resolve(false);
            });
            
            modal.show();
        });
    }
    
    /**
     * Utility function to format numbers
     */
    function formatNumber(num) {
        return new Intl.NumberFormat('es-ES').format(num);
    }
    
    /**
     * Utility function to format dates
     */
    function formatDate(dateString) {
        if (!dateString) return '-';
        
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('es-ES', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    }
    
    // Expose public methods
    window.ParticipantManagement = {
        refresh: refreshTableData,
        loadStats: loadParticipantStats,
        exportData: exportParticipants,
        filterByStatus: filterByStatus
    };
    
})(jQuery);