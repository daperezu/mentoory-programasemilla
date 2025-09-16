// Phoenix Homepage - Dual Mode Project Discovery
// REQ-012: Phoenix Homepage Redesign with time-based and location-based modes

(function () {
    'use strict';

    // Configuration
    const config = {
        defaultRadius: 15,
        maxResults: 20,
        discoveryMode: 'time' // 'time' or 'location'
    };

    // State
    let userLocation = null;
    let isLoadingProjects = false;

    // Initialize on document ready
    document.addEventListener('DOMContentLoaded', function () {
        initializeEventHandlers();
        initializeTooltips();
        initializeAnimations();
    });

    // Initialize event handlers
    function initializeEventHandlers() {
        // Explore projects button (scrolls to projects)
        const btnExplore = document.getElementById('btnExploreProjects');
        if (btnExplore) {
            btnExplore.addEventListener('click', function () {
                const section = document.getElementById('latestProjectsSection');
                if (section) {
                    section.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            });
        }

        // Enable location button
        const btnLocation = document.getElementById('btnEnableLocation');
        if (btnLocation) {
            btnLocation.addEventListener('click', requestUserLocation);
        }

        // Radius selector for nearby projects
        const radiusSelect = document.getElementById('radiusSelect');
        if (radiusSelect) {
            radiusSelect.addEventListener('change', function () {
                if (config.discoveryMode === 'location' && userLocation) {
                    searchNearbyProjects(userLocation.latitude, userLocation.longitude);
                }
            });
        }
    }

    // Initialize Bootstrap tooltips
    function initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Initialize animations
    function initializeAnimations() {
        // Add animation classes when elements come into view
        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in-up');
                }
            });
        }, {
            threshold: 0.1
        });

        // Observe timeline items
        document.querySelectorAll('.timeline-item').forEach(function (item) {
            observer.observe(item);
        });
    }

    // Request user location
    function requestUserLocation() {
        if (!navigator.geolocation) {
            showLocationStatus('Tu navegador no soporta geolocalización', 'warning');
            return;
        }

        showLocationStatus('Obteniendo tu ubicación...', 'info');

        navigator.geolocation.getCurrentPosition(
            handleLocationSuccess,
            handleLocationError,
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    }

    // Handle successful location acquisition
    function handleLocationSuccess(position) {
        userLocation = {
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            accuracy: position.coords.accuracy
        };

        config.discoveryMode = 'location';

        showLocationStatus(
            `Ubicación detectada (precisión: ${Math.round(position.coords.accuracy)}m)`,
            'success'
        );

        // Switch to location-based view
        switchToLocationMode();

        // Search for nearby projects
        searchNearbyProjects(userLocation.latitude, userLocation.longitude);
    }

    // Handle location error
    function handleLocationError(error) {
        let message = 'No se pudo obtener tu ubicación';

        switch (error.code) {
            case error.PERMISSION_DENIED:
                message = 'Permiso de ubicación denegado. Por favor, habilita los permisos de ubicación.';
                break;
            case error.POSITION_UNAVAILABLE:
                message = 'Información de ubicación no disponible.';
                break;
            case error.TIMEOUT:
                message = 'La solicitud de ubicación ha expirado.';
                break;
        }

        showLocationStatus(message, 'danger');
    }

    // Switch to location-based mode
    function switchToLocationMode() {
        // Hide latest projects section
        const latestSection = document.getElementById('latestProjectsSection');
        if (latestSection) {
            latestSection.classList.add('d-none');
        }

        // Show nearby projects section
        const nearbySection = document.getElementById('nearbyProjectsSection');
        if (nearbySection) {
            nearbySection.classList.remove('d-none');
        }

        // Update button states
        const btnLocation = document.getElementById('btnEnableLocation');
        if (btnLocation) {
            btnLocation.innerHTML = '<i class="fas fa-check me-2"></i>Ubicación Activa';
            btnLocation.classList.remove('btn-outline-light');
            btnLocation.classList.add('btn-success');
            btnLocation.disabled = true;
        }
    }

    // Search for nearby projects
    function searchNearbyProjects(latitude, longitude) {
        if (isLoadingProjects) return;

        isLoadingProjects = true;
        showLoadingIndicator(true);
        hideNoProjectsMessage();

        const radiusKm = document.getElementById('radiusSelect')?.value || config.defaultRadius;
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch('/Public/Projects/GetNearbyProjects', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                latitude: latitude,
                longitude: longitude,
                radiusKm: parseFloat(radiusKm),
                maxResults: config.maxResults
            })
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error en la búsqueda de proyectos');
            }
            return response.json();
        })
        .then(data => {
            displayNearbyProjects(data);
        })
        .catch(error => {
            console.error('Error:', error);
            showLocationStatus('Error al buscar proyectos cercanos', 'danger');
            showNoProjectsMessage();
        })
        .finally(() => {
            isLoadingProjects = false;
            showLoadingIndicator(false);
        });
    }

    // Display nearby projects
    function displayNearbyProjects(data) {
        const grid = document.getElementById('nearbyProjectsGrid');
        if (!grid) return;

        if (!data.projects || data.projects.length === 0) {
            showNoProjectsMessage();
            return;
        }

        // Clear existing content
        grid.innerHTML = '';

        // Create project cards
        data.projects.forEach(project => {
            const card = createProjectCard(project, true);
            grid.appendChild(card);
        });

        // Update status
        showLocationStatus(
            `Se encontraron ${data.projects.length} proyectos en un radio de ${data.searchRadiusKm} km`,
            'success'
        );
    }

    // Create project card element
    function createProjectCard(project, showDistance = false) {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4';

        const distanceBadge = showDistance && project.distanceKm
            ? `<span class="distance-badge"><i class="fas fa-location-dot me-1"></i>${project.distanceKm.toFixed(1)} km</span>`
            : '';

        const stageBadge = project.nextStageStartDate
            ? `<span class="stage-badge">${getStageLabel(project.nextStageStartDate, project.nextStageTitle)}</span>`
            : '';

        const locationInfo = project.locationName
            ? `<p class="text-muted small mb-2">
                <i class="fas fa-map-marker-alt me-1"></i>${escapeHtml(project.locationName)}
               </p>`
            : '';

        const startDateInfo = project.nextStageStartDate
            ? `<p class="text-primary small mb-2">
                <i class="fas fa-calendar me-1"></i>Inicia ${formatDate(project.nextStageStartDate)}
               </p>`
            : '';

        col.innerHTML = `
            <div class="card project-card card-phoenix h-100">
                ${project.heroImageUrl ? `
                    <div class="position-relative">
                        <img src="${escapeHtml(project.heroImageUrl)}" class="card-img-top" alt="${escapeHtml(project.name)}">
                        ${distanceBadge}
                        ${stageBadge}
                    </div>
                ` : ''}
                <div class="card-body">
                    <h5 class="card-title">${escapeHtml(project.name)}</h5>
                    <p class="text-muted small mb-2">
                        <i class="fas fa-building me-1"></i>${escapeHtml(project.businessIncubatorName)}
                    </p>
                    ${locationInfo}
                    ${startDateInfo}
                    <p class="card-text text-truncate-2">
                        ${escapeHtml(project.description || 'Descubre más sobre este proyecto innovador')}
                    </p>
                    <div class="d-flex justify-content-between align-items-center mt-3">
                        <span class="badge bg-info-subtle text-info">
                            <i class="fas fa-users me-1"></i>${project.activeParticipants} participantes
                        </span>
                        <a href="/Public/Projects/Details/${project.externalId}" class="btn btn-sm btn-phoenix-primary">
                            Ver detalles <i class="fas fa-arrow-right ms-1"></i>
                        </a>
                    </div>
                </div>
            </div>
        `;

        return col;
    }

    // Get stage label based on date
    function getStageLabel(stageDate, stageTitle) {
        const now = new Date();
        const stage = new Date(stageDate);
        const daysUntil = Math.ceil((stage - now) / (1000 * 60 * 60 * 24));

        if (daysUntil <= 0) {
            return 'En curso';
        } else if (daysUntil <= 7) {
            return 'Inicia pronto';
        } else {
            return stageTitle || 'Próximamente';
        }
    }

    // Format date
    function formatDate(dateString) {
        const date = new Date(dateString);
        const options = { day: 'numeric', month: 'short', year: 'numeric' };
        return date.toLocaleDateString('es-CR', options);
    }

    // Show location status message
    function showLocationStatus(message, type) {
        const statusDiv = document.getElementById('locationStatus');
        const statusText = document.getElementById('locationStatusText');

        if (statusDiv && statusText) {
            statusText.textContent = message;
            statusDiv.className = `alert alert-${type} m-3`;
            statusDiv.classList.remove('d-none');

            // Auto-hide after 5 seconds for success messages
            if (type === 'success') {
                setTimeout(() => {
                    statusDiv.classList.add('d-none');
                }, 5000);
            }
        }
    }

    // Show/hide loading indicator
    function showLoadingIndicator(show) {
        const indicator = document.getElementById('loadingIndicator');
        if (indicator) {
            if (show) {
                indicator.classList.remove('d-none');
            } else {
                indicator.classList.add('d-none');
            }
        }
    }

    // Show/hide no projects message
    function showNoProjectsMessage() {
        const message = document.getElementById('noProjectsMessage');
        if (message) {
            message.classList.remove('d-none');
        }
    }

    function hideNoProjectsMessage() {
        const message = document.getElementById('noProjectsMessage');
        if (message) {
            message.classList.add('d-none');
        }
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

})();