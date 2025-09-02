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
        toastr.error('Error al cargar las estadísticas', 'Error');
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
            toastr.success('Datos actualizados correctamente', 'Éxito');
        }
    }
    
    /**
     * Export participants data
     */
    function exportParticipants(format = 'excel') {
        const exportUrl = `/Coordination/Participant/Export?format=${format}`;
        
        // Show loading indicator
        toastr.info('Generando archivo de exportación...', 'Procesando');
        
        // Create a temporary link and trigger download
        const link = document.createElement('a');
        link.href = exportUrl;
        link.download = `participantes_${new Date().toISOString().split('T')[0]}.${format}`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Show success message after a delay
        setTimeout(() => {
            toastr.success('Archivo descargado correctamente', 'Éxito');
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