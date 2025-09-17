/**
 * Google Maps integration for Project Details page
 * Handles map initialization, marker placement, and interactive features
 */

window.ProjectDetailsMap = (function () {
    'use strict';

    let map = null;
    let marker = null;
    let infoWindow = null;
    let geocoder = null;

    /**
     * Initialize Google Maps with project location
     * @param {number} latitude - Project latitude
     * @param {number} longitude - Project longitude
     * @param {string} projectName - Name of the project
     * @param {string} locationName - Display name of the location
     * @param {string} description - Project description for info window
     */
    function initializeMap(latitude, longitude, projectName, locationName, description) {
        // Validate coordinates
        if (!latitude || !longitude) {
            console.warn('Coordenadas no válidas para el mapa');
            showMapError('Ubicación no disponible');
            return;
        }

        const projectLocation = {
            lat: parseFloat(latitude),
            lng: parseFloat(longitude)
        };

        // Map configuration with Phoenix theme colors
        const mapOptions = {
            zoom: 15,
            center: projectLocation,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            mapTypeControl: true,
            mapTypeControlOptions: {
                style: google.maps.MapTypeControlStyle.DROPDOWN_MENU,
                position: google.maps.ControlPosition.TOP_RIGHT
            },
            zoomControl: true,
            zoomControlOptions: {
                position: google.maps.ControlPosition.RIGHT_CENTER
            },
            scaleControl: true,
            streetViewControl: true,
            streetViewControlOptions: {
                position: google.maps.ControlPosition.RIGHT_BOTTOM
            },
            fullscreenControl: true,
            styles: getMapStyles()
        };

        // Initialize map
        const mapElement = document.getElementById('projectLocationMap');
        if (!mapElement) {
            console.error('Elemento del mapa no encontrado');
            return;
        }

        map = new google.maps.Map(mapElement, mapOptions);

        // Create custom marker
        marker = new google.maps.Marker({
            position: projectLocation,
            map: map,
            title: projectName,
            animation: google.maps.Animation.DROP,
            icon: {
                url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(getMarkerIcon()),
                scaledSize: new google.maps.Size(40, 50),
                anchor: new google.maps.Point(20, 50)
            }
        });

        // Create info window with project details
        const infoContent = `
            <div class="map-info-window" style="max-width: 300px;">
                <h5 class="mb-2">${projectName}</h5>
                <p class="mb-2 text-muted">
                    <i class="fas fa-map-marker-alt me-1"></i>${locationName || 'Ubicación del proyecto'}
                </p>
                ${description ? `<p class="small">${description.substring(0, 150)}${description.length > 150 ? '...' : ''}</p>` : ''}
                <div class="mt-3">
                    <a href="https://www.google.com/maps/dir/?api=1&destination=${latitude},${longitude}"
                       target="_blank"
                       class="btn btn-sm btn-primary">
                        <i class="fas fa-directions me-1"></i>Cómo llegar
                    </a>
                </div>
            </div>
        `;

        infoWindow = new google.maps.InfoWindow({
            content: infoContent,
            maxWidth: 350
        });

        // Marker click event
        marker.addListener('click', function () {
            infoWindow.open(map, marker);
        });

        // Add bounce animation on hover
        marker.addListener('mouseover', function () {
            marker.setAnimation(google.maps.Animation.BOUNCE);
        });

        marker.addListener('mouseout', function () {
            marker.setAnimation(null);
        });

        // Initialize geocoder for additional features
        geocoder = new google.maps.Geocoder();

        // Add nearby places search
        addNearbyPlacesControl();

        // Add custom controls
        addCustomControls();

        // Open info window by default
        setTimeout(() => {
            infoWindow.open(map, marker);
        }, 1000);
    }

    /**
     * Get custom map styles matching Phoenix theme
     */
    function getMapStyles() {
        return [
            {
                featureType: 'all',
                elementType: 'geometry',
                stylers: [{ color: '#f5f7fa' }]
            },
            {
                featureType: 'water',
                elementType: 'geometry',
                stylers: [{ color: '#c9d7e8' }]
            },
            {
                featureType: 'road',
                elementType: 'geometry.fill',
                stylers: [{ color: '#ffffff' }]
            },
            {
                featureType: 'road',
                elementType: 'geometry.stroke',
                stylers: [{ color: '#e9ecef' }]
            },
            {
                featureType: 'poi',
                elementType: 'geometry',
                stylers: [{ color: '#e5e7eb' }]
            },
            {
                featureType: 'poi.park',
                elementType: 'geometry',
                stylers: [{ color: '#d4f3d4' }]
            }
        ];
    }

    /**
     * Get custom SVG marker icon
     */
    function getMarkerIcon() {
        return `
            <svg width="40" height="50" viewBox="0 0 40 50" xmlns="http://www.w3.org/2000/svg">
                <path d="M20 0C8.95 0 0 8.95 0 20c0 15 20 30 20 30s20-15 20-30c0-11.05-8.95-20-20-20z"
                      fill="#667eea" stroke="#5a67d8" stroke-width="1"/>
                <circle cx="20" cy="20" r="8" fill="white"/>
                <circle cx="20" cy="20" r="4" fill="#667eea"/>
            </svg>
        `;
    }

    /**
     * Add nearby places search control
     */
    function addNearbyPlacesControl() {
        const controlDiv = document.createElement('div');
        controlDiv.className = 'map-control-nearby';
        controlDiv.style.margin = '10px';

        const controlUI = document.createElement('div');
        controlUI.style.backgroundColor = '#fff';
        controlUI.style.border = '2px solid #fff';
        controlUI.style.borderRadius = '3px';
        controlUI.style.boxShadow = '0 2px 6px rgba(0,0,0,.3)';
        controlUI.style.cursor = 'pointer';
        controlUI.style.marginBottom = '22px';
        controlUI.style.textAlign = 'center';
        controlUI.title = 'Buscar lugares cercanos';
        controlDiv.appendChild(controlUI);

        const controlText = document.createElement('div');
        controlText.style.color = 'rgb(25,25,25)';
        controlText.style.fontFamily = 'Roboto,Arial,sans-serif';
        controlText.style.fontSize = '14px';
        controlText.style.lineHeight = '38px';
        controlText.style.paddingLeft = '10px';
        controlText.style.paddingRight = '10px';
        controlText.innerHTML = '<i class="fas fa-search me-1"></i>Lugares cercanos';
        controlUI.appendChild(controlText);

        controlUI.addEventListener('click', function () {
            showNearbyPlacesModal();
        });

        map.controls[google.maps.ControlPosition.TOP_LEFT].push(controlDiv);
    }

    /**
     * Add custom controls to the map
     */
    function addCustomControls() {
        // Reset view button
        const resetControlDiv = document.createElement('div');
        resetControlDiv.className = 'map-control-reset';
        resetControlDiv.style.margin = '10px';

        const resetControlUI = document.createElement('div');
        resetControlUI.style.backgroundColor = '#fff';
        resetControlUI.style.border = '2px solid #fff';
        resetControlUI.style.borderRadius = '3px';
        resetControlUI.style.boxShadow = '0 2px 6px rgba(0,0,0,.3)';
        resetControlUI.style.cursor = 'pointer';
        resetControlUI.style.textAlign = 'center';
        resetControlUI.title = 'Centrar mapa en el proyecto';
        resetControlDiv.appendChild(resetControlUI);

        const resetControlText = document.createElement('div');
        resetControlText.style.color = 'rgb(25,25,25)';
        resetControlText.style.fontFamily = 'Roboto,Arial,sans-serif';
        resetControlText.style.fontSize = '14px';
        resetControlText.style.lineHeight = '38px';
        resetControlText.style.paddingLeft = '10px';
        resetControlText.style.paddingRight = '10px';
        resetControlText.innerHTML = '<i class="fas fa-crosshairs me-1"></i>Centrar';
        resetControlUI.appendChild(resetControlText);

        resetControlUI.addEventListener('click', function () {
            if (marker) {
                map.setCenter(marker.getPosition());
                map.setZoom(15);
                marker.setAnimation(google.maps.Animation.BOUNCE);
                setTimeout(() => marker.setAnimation(null), 1000);
            }
        });

        map.controls[google.maps.ControlPosition.TOP_LEFT].push(resetControlDiv);
    }

    /**
     * Show modal for nearby places search
     */
    function showNearbyPlacesModal() {
        // Using Bootstrap modal instead of SweetAlert
        const modalHtml = `
            <div class="modal fade" id="nearbyPlacesModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Buscar lugares cercanos</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p>Selecciona el tipo de lugar que deseas buscar cerca del proyecto:</p>
                            <div class="d-grid gap-2">
                                <button class="btn btn-outline-primary nearby-search" data-type="restaurant">
                                    <i class="fas fa-utensils me-2"></i>Restaurantes
                                </button>
                                <button class="btn btn-outline-primary nearby-search" data-type="bank">
                                    <i class="fas fa-university me-2"></i>Bancos
                                </button>
                                <button class="btn btn-outline-primary nearby-search" data-type="parking">
                                    <i class="fas fa-parking me-2"></i>Estacionamientos
                                </button>
                                <button class="btn btn-outline-primary nearby-search" data-type="transit_station">
                                    <i class="fas fa-bus me-2"></i>Transporte público
                                </button>
                                <button class="btn btn-outline-primary nearby-search" data-type="cafe">
                                    <i class="fas fa-coffee me-2"></i>Cafeterías
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        const existingModal = document.getElementById('nearbyPlacesModal');
        if (existingModal) {
            existingModal.remove();
        }

        document.body.insertAdjacentHTML('beforeend', modalHtml);
        const modal = new bootstrap.Modal(document.getElementById('nearbyPlacesModal'));

        // Add click handlers for search buttons
        document.querySelectorAll('.nearby-search').forEach(button => {
            button.addEventListener('click', function () {
                const placeType = this.dataset.type;
                searchNearbyPlaces(placeType);
                modal.hide();
            });
        });

        modal.show();
    }

    /**
     * Search for nearby places of a specific type
     */
    function searchNearbyPlaces(type) {
        if (!map || !marker) return;

        const request = {
            location: marker.getPosition(),
            radius: 1500, // 1.5km radius
            type: [type]
        };

        const service = new google.maps.places.PlacesService(map);
        service.nearbySearch(request, (results, status) => {
            if (status === google.maps.places.PlacesServiceStatus.OK) {
                // Clear existing place markers
                clearPlaceMarkers();

                // Add markers for found places
                results.slice(0, 10).forEach(place => {
                    createPlaceMarker(place);
                });

                // Adjust map bounds to show all markers
                const bounds = new google.maps.LatLngBounds();
                bounds.extend(marker.getPosition());
                results.slice(0, 10).forEach(place => {
                    bounds.extend(place.geometry.location);
                });
                map.fitBounds(bounds);

                showToast(`Se encontraron ${Math.min(results.length, 10)} lugares cercanos`, 'success');
            } else {
                showToast('No se encontraron lugares de este tipo cerca del proyecto', 'info');
            }
        });
    }

    let placeMarkers = [];

    /**
     * Clear all place markers from the map
     */
    function clearPlaceMarkers() {
        placeMarkers.forEach(marker => {
            marker.setMap(null);
        });
        placeMarkers = [];
    }

    /**
     * Create a marker for a place
     */
    function createPlaceMarker(place) {
        const placeMarker = new google.maps.Marker({
            map: map,
            position: place.geometry.location,
            title: place.name,
            icon: {
                url: place.icon,
                size: new google.maps.Size(25, 25),
                scaledSize: new google.maps.Size(25, 25)
            }
        });

        const placeInfoWindow = new google.maps.InfoWindow({
            content: `
                <div style="min-width: 200px;">
                    <strong>${place.name}</strong><br>
                    ${place.vicinity || ''}<br>
                    ${place.rating ? `<small>⭐ ${place.rating}/5</small>` : ''}
                </div>
            `
        });

        placeMarker.addListener('click', function () {
            placeInfoWindow.open(map, placeMarker);
        });

        placeMarkers.push(placeMarker);
    }

    /**
     * Show error message when map cannot be loaded
     */
    function showMapError(message) {
        const mapElement = document.getElementById('projectLocationMap');
        if (mapElement) {
            mapElement.innerHTML = `
                <div class="d-flex flex-column align-items-center justify-content-center h-100 text-muted">
                    <i class="fas fa-map-marked-alt fa-3x mb-3"></i>
                    <p>${message}</p>
                </div>
            `;
        }
    }

    /**
     * Show toast notification
     */
    function showToast(message, type) {
        // Use the global showToast function if available
        if (window.showToast) {
            window.showToast(message, type);
        } else {
            console.log(`[${type}] ${message}`);
        }
    }

    /**
     * Handle Google Maps API loading error
     */
    function handleMapError() {
        console.error('Error al cargar Google Maps API');
        showMapError('Error al cargar el mapa. Por favor, intenta más tarde.');
    }

    /**
     * Check if Google Maps API is loaded
     */
    function isGoogleMapsLoaded() {
        return typeof google !== 'undefined' && typeof google.maps !== 'undefined';
    }

    // Public API
    return {
        initialize: initializeMap,
        handleError: handleMapError,
        isLoaded: isGoogleMapsLoaded,
        resetView: function () {
            if (map && marker) {
                map.setCenter(marker.getPosition());
                map.setZoom(15);
            }
        },
        clearPlaces: clearPlaceMarkers
    };
})();

// Global callback for Google Maps API
window.initProjectMap = function() {
    // This will be called from the view when ready
    console.log('Google Maps API loaded and ready');
};