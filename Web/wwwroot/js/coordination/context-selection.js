// Context Selection Management with Role-Based Visibility
(function () {
    'use strict';

    // Role constants for clarity
    const ROLES = {
        GLOBAL_ADMIN: 'Global Administrator',
        ADMIN: 'Administrator'
    };

    // DOM elements
    let roleSelect, incubatorSelect, projectSelect;
    let incubatorSection, projectSection;
    let saveBtn;
    let contextForm;

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        initializeElements();
        loadSavedContext();
        setupEventListeners();
    });

    function initializeElements() {
        roleSelect = document.getElementById('roleSelect');
        incubatorSelect = document.getElementById('incubatorSelect');
        projectSelect = document.getElementById('projectSelect');
        incubatorSection = document.getElementById('incubatorSection');
        projectSection = document.getElementById('projectSection');
        saveBtn = document.getElementById('saveContextBtn');
        contextForm = document.getElementById('contextSelectionForm');
    }

    function setupEventListeners() {
        if (roleSelect) {
            roleSelect.addEventListener('change', onRoleChange);
        }

        if (incubatorSelect) {
            incubatorSelect.addEventListener('change', onIncubatorChange);
        }

        if (contextForm) {
            contextForm.addEventListener('submit', onFormSubmit);
        }
    }

    // Add role-based visibility rules
    function applyRoleVisibilityRules(role) {
        const isGlobalAdmin = role === ROLES.GLOBAL_ADMIN;
        const isAdmin = role === ROLES.ADMIN;
        
        // Get UI elements for required/optional indicators
        const globalAdminMessage = document.getElementById('globalAdminMessage');
        const incubatorRequired = document.getElementById('incubatorRequired');
        const incubatorOptional = document.getElementById('incubatorOptional');
        const projectRequired = document.getElementById('projectRequired');
        const projectOptional = document.getElementById('projectOptional');
        const incubatorHelpText = document.getElementById('incubatorHelpText');
        const projectHelpText = document.getElementById('projectHelpText');
        
        if (isGlobalAdmin) {
            // Global Admin: show incubator and project as optional
            showSection(incubatorSection);
            showSection(projectSection);
            
            // Show global admin message
            if (globalAdminMessage) globalAdminMessage.style.display = 'block';
            
            // Mark fields as optional
            if (incubatorRequired) incubatorRequired.style.display = 'none';
            if (incubatorOptional) incubatorOptional.style.display = 'inline-block';
            if (projectRequired) projectRequired.style.display = 'none';
            if (projectOptional) projectOptional.style.display = 'inline-block';
            
            // Update help text
            if (incubatorHelpText) incubatorHelpText.textContent = 'Opcional: Seleccione una incubadora para trabajar en un contexto específico';
            if (projectHelpText) projectHelpText.textContent = 'Opcional: Seleccione un proyecto para trabajar en un contexto específico';
            
            // Remove required attribute
            if (incubatorSelect) incubatorSelect.removeAttribute('required');
            if (projectSelect) projectSelect.removeAttribute('required');
            
        } else if (isAdmin) {
            // Administrator: show incubator (required), hide project
            showSection(incubatorSection);
            hideSection(projectSection);
            
            // Hide global admin message
            if (globalAdminMessage) globalAdminMessage.style.display = 'none';
            
            // Mark incubator as required
            if (incubatorRequired) incubatorRequired.style.display = 'inline';
            if (incubatorOptional) incubatorOptional.style.display = 'none';
            if (incubatorHelpText) incubatorHelpText.textContent = 'Seleccione la incubadora en la que desea trabajar';
            
            // Set required attribute
            if (incubatorSelect) incubatorSelect.setAttribute('required', 'required');
            
            // Clear project selection
            if (projectSelect) {
                projectSelect.value = '';
                projectSelect.removeAttribute('required');
            }
        } else if (role) {
            // Other roles: show all as required
            showSection(incubatorSection);
            showSection(projectSection);
            
            // Hide global admin message
            if (globalAdminMessage) globalAdminMessage.style.display = 'none';
            
            // Mark both as required
            if (incubatorRequired) incubatorRequired.style.display = 'inline';
            if (incubatorOptional) incubatorOptional.style.display = 'none';
            if (projectRequired) projectRequired.style.display = 'inline';
            if (projectOptional) projectOptional.style.display = 'none';
            
            // Update help text
            if (incubatorHelpText) incubatorHelpText.textContent = 'Seleccione la incubadora en la que desea trabajar';
            if (projectHelpText) projectHelpText.textContent = 'Seleccione el proyecto en el que desea trabajar';
            
            // Set required attributes
            if (incubatorSelect) incubatorSelect.setAttribute('required', 'required');
            if (projectSelect) projectSelect.setAttribute('required', 'required');
        } else {
            // No role selected: hide everything
            hideSection(incubatorSection);
            hideSection(projectSection);
            if (globalAdminMessage) globalAdminMessage.style.display = 'none';
        }
    }

    function loadSavedContext() {
        // Check if server already populated the dropdowns
        const serverSelectedRole = roleSelect?.value;
        
        if (serverSelectedRole) {
            // Apply visibility rules based on role
            applyRoleVisibilityRules(serverSelectedRole);
            
            // Check if incubator and project are also populated
            const serverSelectedIncubator = incubatorSelect?.value;
            const serverSelectedProject = projectSelect?.value;
            
            // If we have selections from server, we're done
            if (serverSelectedIncubator || serverSelectedProject) {
                return;
            }
        } else if (window.contextSelectionData?.urls) {
            // No server selections, try to load from server
            loadContextFromServer();
        }
    }

    async function loadContextFromServer() {
        try {
            const response = await fetch(window.contextSelectionData.urls.loadContext, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            // Check if response is ok before trying to parse JSON
            if (!response.ok) {
                console.error('Failed to load context, status:', response.status);
                
                // If unauthorized, might need to redirect to login
                if (response.status === 401) {
                    console.error('Unauthorized - user may need to login');
                }
                return;
            }

            // Check content type to ensure it's JSON
            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                console.error('Response is not JSON, content-type:', contentType);
                const text = await response.text();
                console.error('Response text:', text.substring(0, 200)); // Log first 200 chars
                return;
            }

            const data = await response.json();
            if (data.success && data.context) {
                applyContextToUI(data.context);
            }
        } catch (error) {
            console.error('Error loading context:', error);
        }
    }

    // Update role change handler
    async function onRoleChange() {
        const role = roleSelect.value;
        
        if (!role) {
            hideSection(incubatorSection);
            hideSection(projectSection);
            return;
        }
        
        // Apply visibility rules immediately
        applyRoleVisibilityRules(role);
        
        // Clear dependent selections
        const isGlobalAdmin = role === ROLES.GLOBAL_ADMIN;
        incubatorSelect.innerHTML = isGlobalAdmin ? 
            '<option value="">-- Seleccione una incubadora (opcional) --</option>' :
            '<option value="">-- Seleccione una incubadora --</option>';
        projectSelect.innerHTML = isGlobalAdmin ? 
            '<option value="">-- Seleccione un proyecto (opcional) --</option>' :
            '<option value="">-- Seleccione un proyecto --</option>';
        
        // Load incubators for all roles (Global Admin can optionally select)
        await loadIncubators(role);
    }

    // Update incubator change handler
    async function onIncubatorChange() {
        const incubatorId = incubatorSelect.value;
        const role = roleSelect.value;
        const isGlobalAdmin = role === ROLES.GLOBAL_ADMIN;
        const isAdmin = role === ROLES.ADMIN;
        
        if (!incubatorId) {
            // Clear project selection
            projectSelect.innerHTML = isGlobalAdmin ? 
                '<option value="">-- Seleccione un proyecto (opcional) --</option>' :
                '<option value="">-- Seleccione un proyecto --</option>';
            if (isAdmin) {
                hideSection(projectSection);
            }
            return;
        }
        
        // Clear project selection
        projectSelect.innerHTML = isGlobalAdmin ? 
            '<option value="">-- Seleccione un proyecto (opcional) --</option>' :
            '<option value="">-- Seleccione un proyecto --</option>';
        
        // Show project section for Global Admin and other non-admin roles (but not regular Admin)
        if (!isAdmin) {
            showSection(projectSection);
            await loadProjects(role, incubatorId);
        } else {
            hideSection(projectSection);
        }
    }

    async function loadIncubators(role) {
        try {
            const url = `${window.contextSelectionData.urls.getIncubators}?role=${encodeURIComponent(role)}`;
            const response = await fetch(url);
            const data = await response.json();

            if (data.success && data.incubators) {
                const isGlobalAdmin = role === ROLES.GLOBAL_ADMIN;
                incubatorSelect.innerHTML = isGlobalAdmin ? 
                    '<option value="">-- Seleccione una incubadora (opcional) --</option>' :
                    '<option value="">-- Seleccione una incubadora --</option>';
                data.incubators.forEach(incubator => {
                    const option = document.createElement('option');
                    option.value = incubator.id;
                    option.textContent = incubator.name;
                    incubatorSelect.appendChild(option);
                });

                // Check if server pre-selected an incubator
                const serverSelectedIncubator = window.contextSelectionData?.selectedIncubatorId;
                if (serverSelectedIncubator) {
                    incubatorSelect.value = serverSelectedIncubator;
                    if (incubatorSelect.value == serverSelectedIncubator) {
                        await onIncubatorChange();
                    }
                }
            }
        } catch (error) {
            console.error('Error loading incubators:', error);
            showToast('Error al cargar las incubadoras', 'danger');
        }
    }

    async function loadProjects(role, incubatorId) {
        try {
            const url = `${window.contextSelectionData.urls.getProjects}?role=${encodeURIComponent(role)}&incubatorId=${incubatorId}`;
            const response = await fetch(url);
            const data = await response.json();

            if (data.success && data.projects) {
                const isGlobalAdmin = role === ROLES.GLOBAL_ADMIN;
                projectSelect.innerHTML = isGlobalAdmin ? 
                    '<option value="">-- Seleccione un proyecto (opcional) --</option>' :
                    '<option value="">-- Seleccione un proyecto --</option>';
                data.projects.forEach(project => {
                    const option = document.createElement('option');
                    option.value = project.id;
                    option.textContent = project.name;
                    if (project.role) {
                        option.textContent += ` (${project.role})`;
                        option.setAttribute('data-role', project.role);
                    }
                    projectSelect.appendChild(option);
                });

                // Check if server pre-selected a project
                const serverSelectedProject = window.contextSelectionData?.selectedProjectId;
                if (serverSelectedProject) {
                    projectSelect.value = serverSelectedProject;
                }
            }
        } catch (error) {
            console.error('Error loading projects:', error);
            showToast('Error al cargar los proyectos', 'danger');
        }
    }

    // Add validation function
    function validateContext() {
        const role = roleSelect.value;
        const incubatorId = incubatorSelect.value;
        const projectId = projectSelect.value;
        
        if (!role) {
            showToast('Debe seleccionar un rol', 'warning');
            roleSelect.focus();
            return false;
        }
        
        const isGlobalAdmin = role === ROLES.GLOBAL_ADMIN;
        const isAdmin = role === ROLES.ADMIN;
        
        if (isGlobalAdmin) {
            // Global Admin needs only role
            return true;
        } else if (isAdmin) {
            // Administrator needs role + incubator
            if (!incubatorId) {
                showToast('Debe seleccionar una incubadora', 'warning');
                incubatorSelect.focus();
                return false;
            }
            return true;
        } else {
            // Other roles need all three
            if (!incubatorId) {
                showToast('Debe seleccionar una incubadora', 'warning');
                incubatorSelect.focus();
                return false;
            }
            if (!projectId) {
                showToast('Debe seleccionar un proyecto', 'warning');
                projectSelect.focus();
                return false;
            }
            return true;
        }
    }

    // Update form submission
    async function onFormSubmit(e) {
        e.preventDefault();
        
        // Validate based on role
        if (!validateContext()) {
            return;
        }
        
        const role = roleSelect.value;
        const incubatorId = incubatorSelect.value;
        const projectId = projectSelect.value;
        
        // Build request based on role requirements
        const requestData = {
            role: role || null,
            incubatorId: incubatorId ? parseInt(incubatorId) : null,
            projectId: projectId ? parseInt(projectId) : null
        };
        
        // Show loading state
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Guardando...';
        
        try {
            const response = await fetch(window.contextSelectionData.urls.selectContext, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(requestData)
            });
            
            const data = await response.json();
            if (data.success) {
                showToast('Contexto guardado exitosamente', 'success');
                
                // Redirect after short delay
                setTimeout(() => {
                    window.location.href = data.redirectUrl || '/AuthRedirect';
                }, 500);
            } else {
                showToast(data.message || 'Error al guardar el contexto', 'danger');
                // Reset button
                saveBtn.disabled = false;
                saveBtn.innerHTML = '<i class="fas fa-arrow-right"></i> Continuar';
            }
        } catch (error) {
            console.error('Error saving context:', error);
            showToast('Error al guardar el contexto', 'danger');
            // Reset button
            saveBtn.disabled = false;
            saveBtn.innerHTML = '<i class="fas fa-arrow-right"></i> Continuar';
        }
    }


    function clearUI() {
        if (roleSelect) roleSelect.value = '';
        if (incubatorSelect) {
            incubatorSelect.innerHTML = '<option value="">-- Seleccione una incubadora --</option>';
        }
        if (projectSelect) {
            projectSelect.innerHTML = '<option value="">-- Seleccione un proyecto --</option>';
        }
        hideSection(incubatorSection);
        hideSection(projectSection);
    }

    function applyContextToUI(context) {
        if (context.role && roleSelect) {
            roleSelect.value = context.role;
            applyRoleVisibilityRules(context.role);
            onRoleChange().then(() => {
                if (context.incubatorId && incubatorSelect) {
                    incubatorSelect.value = context.incubatorId;
                    onIncubatorChange().then(() => {
                        if (context.projectId && projectSelect) {
                            projectSelect.value = context.projectId;
                        }
                    });
                }
            });
        }
    }

    function showSection(section) {
        if (section) {
            section.style.display = 'block';
        }
    }

    function hideSection(section) {
        if (section) {
            section.style.display = 'none';
        }
    }

    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

})();