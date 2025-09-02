/**
 * Navigation Helper Module
 * Handles active state highlighting and menu expansion for the current page
 */
(function () {
    'use strict';

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        highlightActiveMenuItem();
        expandActiveMenuParent();
    });

    /**
     * Highlights the current active menu item based on the current URL
     */
    function highlightActiveMenuItem() {
        const currentPath = window.location.pathname.toLowerCase();
        const currentArea = getAreaFromPath(currentPath);
        const currentController = getControllerFromPath(currentPath);
        
        // Find all navigation links
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
        
        navLinks.forEach(link => {
            if (link.classList.contains('dropdown-indicator')) {
                return; // Skip dropdown toggles
            }
            
            const href = link.getAttribute('href');
            if (!href) return;
            
            const linkPath = href.toLowerCase();
            const linkArea = getAreaFromPath(linkPath);
            const linkController = getControllerFromPath(linkPath);
            
            // Check if this is the active link
            if (isActiveLink(currentArea, currentController, linkArea, linkController, currentPath, linkPath)) {
                link.classList.add('active');
                
                // Also highlight the parent menu if in a submenu
                const parentCollapse = link.closest('.collapse.parent');
                if (parentCollapse) {
                    const parentToggle = document.querySelector(`[href="#${parentCollapse.id}"]`);
                    if (parentToggle) {
                        parentToggle.classList.add('active-parent');
                    }
                }
            }
        });
    }

    /**
     * Expands the parent menu if the current page is within that section
     */
    function expandActiveMenuParent() {
        const activeLink = document.querySelector('.navbar-nav .nav-link.active');
        if (!activeLink) return;
        
        const parentCollapse = activeLink.closest('.collapse.parent');
        if (!parentCollapse) return;
        
        // Expand the parent menu
        const bsCollapse = new bootstrap.Collapse(parentCollapse, {
            toggle: false
        });
        bsCollapse.show();
        
        // Update the dropdown indicator
        const parentToggle = document.querySelector(`[href="#${parentCollapse.id}"]`);
        if (parentToggle) {
            parentToggle.setAttribute('aria-expanded', 'true');
            const indicator = parentToggle.querySelector('.dropdown-indicator-icon');
            if (indicator) {
                indicator.classList.add('rotate-90');
            }
        }
    }

    /**
     * Extracts the area name from a URL path
     */
    function getAreaFromPath(path) {
        const match = path.match(/\/([^\/]+)\/([^\/]+)/);
        return match ? match[1] : '';
    }

    /**
     * Extracts the controller name from a URL path
     */
    function getControllerFromPath(path) {
        const match = path.match(/\/([^\/]+)\/([^\/]+)/);
        return match ? match[2] : '';
    }

    /**
     * Determines if a link should be marked as active
     */
    function isActiveLink(currentArea, currentController, linkArea, linkController, currentPath, linkPath) {
        // Exact match
        if (currentPath === linkPath) {
            return true;
        }
        
        // Area and controller match
        if (currentArea === linkArea && currentController === linkController) {
            return true;
        }
        
        // Special cases for coordination area
        if (currentArea === 'coordination') {
            // Dashboard is default for coordination
            if (linkController === 'dashboard' && currentController === 'coordination') {
                return true;
            }
            
            // Participants and bulk invite are related
            if (linkController === 'participants' && 
                (currentController === 'participants' || currentPath.includes('bulkinvite'))) {
                return true;
            }
        }
        
        return false;
    }

    // Export for use in other modules if needed
    window.NavigationHelper = {
        highlightActiveMenuItem: highlightActiveMenuItem,
        expandActiveMenuParent: expandActiveMenuParent
    };
})();