// Public Projects Geolocation Module
(function () {
    'use strict';

    // State management
    const state = {
        currentLocation: null,
        projects: [],
        searchRadius: 15,
        isLoading: false
    };

    // DOM elements
    const elements = {
        btnEnableLocation: document.getElementById('btnEnableLocation'),
        btnManualSearch: document.getElementById('btnManualSearch'),
        btnSearchLocation: document.getElementById('btnSearchLocation'),
        locationStatus: document.getElementById('locationStatus'),
        locationStatusText: document.getElementById('locationStatusText'),
        loadingIndicator: document.getElementById('loadingIndicator'),
        projectsSection: document.getElementById('projectsSection'),
        projectsGrid: document.getElementById('projectsGrid'),
        noProjectsMessage: document.getElementById('noProjectsMessage'),
        radiusSelect: document.getElementById('radiusSelect'),
        manualSearchModal: document.getElementById('manualSearchModal'),
        searchAddress: document.getElementById('searchAddress'),
        manualLat: document.getElementById('manualLat'),
        manualLon: document.getElementById('manualLon'),
        mapContainer: document.getElementById('mapContainer')
    };

    // Initialize
    document.addEventListener('DOMContentLoaded', function () {
        initializeEventListeners();
        checkGeolocationSupport();
    });

    // Event listeners
    function initializeEventListeners() {
        if (elements.btnEnableLocation) {
            elements.btnEnableLocation.addEventListener('click', requestUserLocation);
        }

        if (elements.btnManualSearch) {
            elements.btnManualSearch.addEventListener('click', showManualSearchModal);
        }

        if (elements.btnSearchLocation) {
            elements.btnSearchLocation.addEventListener('click', performManualSearch);
        }

        if (elements.radiusSelect) {
            elements.radiusSelect.addEventListener('change', function () {
                state.searchRadius = parseInt(this.value);
                if (state.currentLocation) {
                    searchNearbyProjects(state.currentLocation.latitude, state.currentLocation.longitude);
                }
            });
        }
    }

    // Check if geolocation is supported
    function checkGeolocationSupport() {
        if (!navigator.geolocation) {
            showLocationStatus('Tu navegador no soporta geolocalización. Por favor, usa la búsqueda manual.', 'warning');
            elements.btnEnableLocation.disabled = true;
        }
    }

    // Request user location
    function requestUserLocation() {
        if (!navigator.geolocation) {
            showLocationStatus('La geolocalización no está disponible en tu navegador.', 'danger');
            return;
        }

        showLocationStatus('Obteniendo tu ubicación...', 'info');
        showLoading(true);

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

    // Handle successful location retrieval
    function handleLocationSuccess(position) {
        const latitude = position.coords.latitude;
        const longitude = position.coords.longitude;
        const accuracy = position.coords.accuracy;

        state.currentLocation = {
            latitude: latitude,
            longitude: longitude,
            accuracy: accuracy
        };

        showLocationStatus(
            `Ubicación detectada (precisión: ${Math.round(accuracy)}m)`,
            'success'
        );

        // Update map placeholder
        updateMapPlaceholder(latitude, longitude);

        // Search for nearby projects
        searchNearbyProjects(latitude, longitude);
    }

    // Handle location error
    function handleLocationError(error) {
        showLoading(false);
        
        let message = '';
        switch (error.code) {
            case error.PERMISSION_DENIED:
                message = 'Permiso de ubicación denegado. Por favor, habilita la ubicación en tu navegador.';
                break;
            case error.POSITION_UNAVAILABLE:
                message = 'Información de ubicación no disponible.';
                break;
            case error.TIMEOUT:
                message = 'La solicitud de ubicación ha expirado. Intenta nuevamente.';
                break;
            default:
                message = 'Error desconocido al obtener la ubicación.';
        }
        
        showLocationStatus(message, 'danger');
    }

    // Search for nearby projects
    async function searchNearbyProjects(latitude, longitude) {
        showLoading(true);

        try {
            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            if (!token) {
                console.error('Anti-forgery token not found');
                showLocationStatus('Error de seguridad. Por favor, recarga la página.', 'danger');
                showLoading(false);
                return;
            }

            const response = await fetch('/Public/Projects/GetNearbyProjects', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    latitude: latitude,
                    longitude: longitude,
                    radiusKm: state.searchRadius,
                    maxResults: 20
                })
            });

            if (!response.ok) {
                throw new Error('Error al buscar proyectos');
            }

            const data = await response.json();
            state.projects = data.projects || [];
            
            displayProjects(state.projects);
            
            if (state.projects.length > 0) {
                showLocationStatus(
                    `Se encontraron ${state.projects.length} proyecto(s) en un radio de ${state.searchRadius}km`,
                    'success'
                );
            }
        } catch (error) {
            console.error('Error searching projects:', error);
            showLocationStatus('Error al buscar proyectos. Por favor, intenta nuevamente.', 'danger');
        } finally {
            showLoading(false);
        }
    }

    // Display projects in grid
    function displayProjects(projects) {
        elements.projectsSection.classList.remove('d-none');
        
        if (!projects || projects.length === 0) {
            elements.projectsGrid.classList.add('d-none');
            elements.noProjectsMessage.classList.remove('d-none');
            return;
        }

        elements.projectsGrid.classList.remove('d-none');
        elements.noProjectsMessage.classList.add('d-none');
        
        elements.projectsGrid.innerHTML = projects.map(project => createProjectCard(project)).join('');
        
        // Add click handlers to cards
        document.querySelectorAll('.project-card').forEach(card => {
            card.addEventListener('click', function () {
                const projectId = this.dataset.projectId;
                recordInterest(projectId, 'View');
                window.location.href = `/Public/Projects/Details/${projectId}`;
            });
        });
    }

    // Create project card HTML
    function createProjectCard(project) {
        const imageUrl = project.heroImageUrl || 'https://via.placeholder.com/800x400/6c757d/ffffff?text=Proyecto';
        const registrationInfo = project.registrationStartDate ?
            `<small class="text-muted d-block mb-2">
                <i class="fas fa-calendar-alt me-1"></i>
                Inscripciones: ${new Date(project.registrationStartDate).toLocaleDateString('es-CR')}
            </small>` : '';

        return `
            <div class="col-md-6 col-lg-4">
                <div class="card project-card h-100" data-project-id="${project.externalId}">
                    <img src="${imageUrl}" class="card-img-top" alt="${escapeHtml(project.name)}"
                         style="height: 200px; object-fit: cover;" loading="lazy"
                         onerror="this.onerror=null; this.src='https://via.placeholder.com/800x400/6c757d/ffffff?text=Proyecto'">
                    <div class="card-body position-relative">
                        <span class="distance-badge">
                            <i class="fas fa-map-marker-alt"></i> ${project.distanceKm} km
                        </span>
                        <h5 class="card-title">${escapeHtml(project.name)}</h5>
                        ${project.currentPhase ? `
                            <span class="badge bg-primary mb-2">${escapeHtml(project.currentPhase)}</span>
                        ` : ''}
                        <p class="card-text text-muted small mb-2">
                            <i class="fas fa-building me-1"></i> ${escapeHtml(project.businessIncubatorName || 'Incubadora')}
                        </p>
                        <p class="card-text">${escapeHtml(project.description || 'Sin descripción disponible')}</p>
                        ${project.locationName ? `
                            <p class="card-text small">
                                <i class="fas fa-location-dot me-1"></i> ${escapeHtml(project.locationName)}
                            </p>
                        ` : ''}
                        ${registrationInfo}
                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <small class="text-muted">
                                <i class="fas fa-users me-1"></i> ${project.activeParticipants || 0} participantes
                            </small>
                            <button class="btn btn-sm btn-outline-primary" onclick="event.stopPropagation(); recordInterest('${project.externalId}', 'Contact')">
                                <i class="fas fa-envelope me-1"></i> Contactar
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Show manual search modal
    function showManualSearchModal() {
        const modal = new bootstrap.Modal(elements.manualSearchModal);
        modal.show();
    }

    // Perform manual search
    function performManualSearch() {
        const lat = parseFloat(elements.manualLat.value);
        const lon = parseFloat(elements.manualLon.value);
        
        if (isNaN(lat) || isNaN(lon)) {
            alert('Por favor, ingresa coordenadas válidas.');
            return;
        }
        
        if (lat < -90 || lat > 90 || lon < -180 || lon > 180) {
            alert('Las coordenadas están fuera de rango válido.');
            return;
        }
        
        // Close modal
        bootstrap.Modal.getInstance(elements.manualSearchModal).hide();
        
        // Update state and search
        state.currentLocation = {
            latitude: lat,
            longitude: lon,
            accuracy: 0
        };
        
        updateMapPlaceholder(lat, lon);
        searchNearbyProjects(lat, lon);
    }

    // Record user interest in a project (redirects to registration if not authenticated)
    async function recordInterest(projectId, interestType) {
        // For public users, we just redirect to registration
        // The actual interest recording happens in the Observer area for authenticated users
        
        // Store the intent in session storage
        sessionStorage.setItem('pendingInterest', JSON.stringify({
            projectId: projectId,
            interestType: interestType,
            location: state.currentLocation
        }));
        
        // Redirect to registration page with Observer role and return URL
        // After registration, they'll be redirected to the Observer dashboard
        window.location.href = `/Identity/Account/Register?returnUrl=/Observer/Projects/Dashboard&role=Observer`;
    }

    // Update map placeholder with coordinates
    function updateMapPlaceholder(lat, lon) {
        if (elements.mapContainer) {
            elements.mapContainer.innerHTML = `
                <div class="text-center">
                    <i class="fas fa-map-marked-alt fa-3x mb-2"></i>
                    <p class="mb-0">Ubicación: ${lat.toFixed(6)}, ${lon.toFixed(6)}</p>
                    <small class="text-muted">Mapa interactivo próximamente</small>
                </div>
            `;
        }
    }

    // Show location status message
    function showLocationStatus(message, type) {
        elements.locationStatus.className = `alert alert-${type} m-3`;
        elements.locationStatus.classList.remove('d-none');
        elements.locationStatusText.textContent = message;
        
        // Auto-hide success messages after 5 seconds
        if (type === 'success') {
            setTimeout(() => {
                elements.locationStatus.classList.add('d-none');
            }, 5000);
        }
    }

    // Show/hide loading indicator
    function showLoading(show) {
        state.isLoading = show;
        if (show) {
            elements.loadingIndicator.classList.remove('d-none');
            elements.projectsSection.classList.add('d-none');
        } else {
            elements.loadingIndicator.classList.add('d-none');
        }
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        if (!text) return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }

    // Make recordInterest available globally for onclick handlers
    window.recordInterest = recordInterest;
})();