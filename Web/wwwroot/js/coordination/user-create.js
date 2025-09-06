document.addEventListener('DOMContentLoaded', function() {
    // Configuration
    const roleDefinitions = {
        'GlobalAdministrator': {
            displayName: 'Administrador Global',
            description: 'Acceso completo al sistema en todas las incubadoras',
            icon: 'fas fa-crown',
            color: 'danger',
            requiresIncubator: false,
            requiresProject: false
        },
        'Administrator': {
            displayName: 'Administrador',
            description: 'Gestiona la configuración del sistema y los usuarios de la incubadora',
            icon: 'fas fa-user-shield',
            color: 'warning',
            requiresIncubator: true,
            requiresProject: false
        },
        'Coordinator': {
            displayName: 'Coordinador',
            description: 'Gestiona proyectos y participantes de la incubadora',
            icon: 'fas fa-users-cog',
            color: 'primary',
            requiresIncubator: true,
            requiresProject: true // Required for coordinators
        },
        'Liaison': {
            displayName: 'Enlace',
            description: 'Actúa como conexión entre diferentes partes interesadas',
            icon: 'fas fa-link',
            color: 'info',
            requiresIncubator: true,
            requiresProject: false
        },
        'Mentor': {
            displayName: 'Mentor',
            description: 'Proporciona orientación y apoyo a los participantes',
            icon: 'fas fa-chalkboard-teacher',
            color: 'success',
            requiresIncubator: true,
            requiresProject: true
        },
        'Guide': {
            displayName: 'Guía',
            description: 'Asiste a los participantes en aspectos específicos',
            icon: 'fas fa-compass',
            color: 'teal',
            requiresIncubator: true,
            requiresProject: true
        },
        'Facilitator': {
            displayName: 'Facilitador',
            description: 'Facilita talleres y sesiones de formación',
            icon: 'fas fa-hands-helping',
            color: 'purple',
            requiresIncubator: true,
            requiresProject: true
        },
        'Starter': {
            displayName: 'Emprendedor',
            description: 'Participante del programa de incubación',
            icon: 'fas fa-rocket',
            color: 'secondary',
            requiresIncubator: true,
            requiresProject: true
        }
    };

    // State management
    let currentStep = 1;
    const totalSteps = 3;
    let selectedRole = null;
    let incubators = [];
    let projects = [];

    // DOM Elements
    const form = document.getElementById('createUserForm');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');
    const stepIndicators = document.querySelectorAll('.step');
    const stepContents = document.querySelectorAll('.step-content');
    const roleCardsContainer = document.getElementById('roleCards');
    const selectedRoleInput = document.getElementById('selectedRole');
    const contextSection = document.getElementById('contextSection');
    const incubatorSection = document.getElementById('incubatorSection');
    const projectSection = document.getElementById('projectSection');
    const incubatorSelect = document.getElementById('incubatorSelect');
    const projectSelect = document.getElementById('projectSelect');
    const generateTempPasswordCheckbox = document.getElementById('generateTempPassword');
    const passwordField = document.getElementById('passwordField');
    const confirmPasswordField = document.getElementById('confirmPasswordField');

    // Initialize
    initializeRoles();
    loadIncubators();
    setupEventListeners();

    // Initialize role cards
    function initializeRoles() {
        roleCardsContainer.innerHTML = '';
        
        Object.entries(roleDefinitions).forEach(([key, role]) => {
            const col = document.createElement('div');
            col.className = 'col-md-6 col-lg-4';
            
            col.innerHTML = `
                <div class="card role-card h-100" data-role="${key}">
                    <div class="card-body">
                        <div class="d-flex align-items-start">
                            <div class="flex-shrink-0">
                                <div class="avatar avatar-xl">
                                    <div class="avatar-name rounded-circle bg-${role.color} bg-opacity-10">
                                        <span class="${role.icon} text-${role.color}"></span>
                                    </div>
                                </div>
                            </div>
                            <div class="flex-grow-1 ms-3">
                                <h6 class="mb-1">${role.displayName}</h6>
                                <p class="text-muted small mb-0">${role.description}</p>
                                ${role.requiresIncubator && role.requiresProject ? 
                                    '<span class="badge bg-info bg-opacity-10 text-info mt-2">Requiere Incubadora y Proyecto</span>' :
                                    role.requiresIncubator ? 
                                    '<span class="badge bg-warning bg-opacity-10 text-warning mt-2">Requiere Incubadora</span>' :
                                    '<span class="badge bg-success bg-opacity-10 text-success mt-2">Sin restricciones</span>'}
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            roleCardsContainer.appendChild(col);
        });
    }

    // Load incubators from server
    async function loadIncubators() {
        try {
            const response = await fetch('/Coordination/UserManagement/GetAllIncubators');
            const data = await response.json();
            
            if (data.success) {
                incubators = data.incubators;
                populateIncubatorSelect();
            }
        } catch (error) {
            console.error('Error loading incubators:', error);
            // Use empty list if endpoint doesn't exist yet
            incubators = [];
        }
    }

    // Populate incubator dropdown
    function populateIncubatorSelect() {
        incubatorSelect.innerHTML = '<option value="">-- Seleccione una incubadora --</option>';
        
        incubators.forEach(incubator => {
            const option = document.createElement('option');
            option.value = incubator.id;
            option.textContent = incubator.name;
            incubatorSelect.appendChild(option);
        });
    }

    // Load projects for selected incubator
    async function loadProjects(incubatorId) {
        if (!incubatorId) {
            projects = [];
            projectSelect.innerHTML = '<option value="">-- Primero seleccione una incubadora --</option>';
            projectSelect.disabled = true;
            return;
        }

        try {
            const response = await fetch(`/Coordination/UserManagement/GetProjectsByIncubator?incubatorId=${incubatorId}`);
            const data = await response.json();
            
            if (data.success) {
                projects = data.projects;
                populateProjectSelect();
                projectSelect.disabled = false;
            }
        } catch (error) {
            console.error('Error loading projects:', error);
            projects = [];
            projectSelect.disabled = true;
        }
    }

    // Populate project dropdown
    function populateProjectSelect() {
        projectSelect.innerHTML = '<option value="">-- Seleccione un proyecto --</option>';
        
        projects.forEach(project => {
            const option = document.createElement('option');
            option.value = project.id;
            option.textContent = project.name;
            option.dataset.externalId = project.externalId || ''; // Store external ID if needed
            projectSelect.appendChild(option);
        });
    }

    // Setup event listeners
    function setupEventListeners() {
        // Navigation buttons
        nextBtn.addEventListener('click', () => navigateStep(1));
        prevBtn.addEventListener('click', () => navigateStep(-1));

        // Role selection
        roleCardsContainer.addEventListener('click', function(e) {
            const card = e.target.closest('.role-card');
            if (card) {
                selectRole(card.dataset.role);
            }
        });

        // Incubator change
        incubatorSelect.addEventListener('change', function() {
            loadProjects(this.value);
            updateContextSummary();
        });

        // Project change
        projectSelect.addEventListener('change', updateContextSummary);

        // Password generation toggle
        generateTempPasswordCheckbox.addEventListener('change', function() {
            const isChecked = this.checked;
            const passwordInput = passwordField.querySelector('input');
            const confirmPasswordInput = confirmPasswordField.querySelector('input');
            
            if (isChecked) {
                passwordField.style.display = 'none';
                confirmPasswordField.style.display = 'none';
                passwordInput.removeAttribute('required');
                confirmPasswordInput.removeAttribute('required');
                passwordInput.value = '';
                confirmPasswordInput.value = '';
            } else {
                passwordField.style.display = 'block';
                confirmPasswordField.style.display = 'block';
                passwordInput.setAttribute('required', 'required');
                confirmPasswordInput.setAttribute('required', 'required');
            }
        });

        // Email preference buttons
        document.getElementById('selectAllPreferences').addEventListener('click', function() {
            document.querySelectorAll('[id^="EmailPreferences_"]').forEach(checkbox => {
                checkbox.checked = true;
            });
        });

        document.getElementById('deselectAllPreferences').addEventListener('click', function() {
            document.querySelectorAll('[id^="EmailPreferences_"]').forEach(checkbox => {
                checkbox.checked = false;
            });
        });
    }

    // Select a role
    function selectRole(roleKey) {
        // Update visual selection
        document.querySelectorAll('.role-card').forEach(card => {
            card.classList.remove('selected');
        });
        document.querySelector(`[data-role="${roleKey}"]`).classList.add('selected');
        
        // Update hidden input
        selectedRole = roleKey;
        selectedRoleInput.value = roleKey;
        
        // Update context requirements
        const role = roleDefinitions[roleKey];
        updateContextRequirements(role);
    }

    // Update context requirements based on role
    function updateContextRequirements(role) {
        const roleContextMessage = document.getElementById('roleContextMessage');
        
        // Show/hide context section
        if (role.requiresIncubator || role.requiresProject) {
            contextSection.style.display = 'block';
            
            // Update message
            if (role.requiresIncubator && role.requiresProject) {
                roleContextMessage.textContent = `El rol ${role.displayName} requiere seleccionar una incubadora y un proyecto.`;
            } else if (role.requiresIncubator) {
                roleContextMessage.textContent = `El rol ${role.displayName} requiere seleccionar una incubadora.`;
            }
            
            // Show/hide incubator section
            if (role.requiresIncubator) {
                incubatorSection.style.display = 'block';
                incubatorSelect.setAttribute('required', 'required');
            } else {
                incubatorSection.style.display = 'none';
                incubatorSelect.removeAttribute('required');
            }
            
            // Show/hide project section
            if (role.requiresProject) {
                projectSection.style.display = 'block';
                projectSelect.setAttribute('required', 'required');
            } else {
                projectSection.style.display = 'none';
                projectSelect.removeAttribute('required');
            }
        } else {
            contextSection.style.display = 'none';
            incubatorSection.style.display = 'none';
            projectSection.style.display = 'none';
            incubatorSelect.removeAttribute('required');
            projectSelect.removeAttribute('required');
        }
        
        updateContextSummary();
    }

    // Update context summary
    function updateContextSummary() {
        const contextSummary = document.getElementById('contextSummary');
        const contextSummaryContent = document.getElementById('contextSummaryContent');
        
        if (!selectedRole) {
            contextSummary.style.display = 'none';
            return;
        }
        
        const role = roleDefinitions[selectedRole];
        const selectedIncubator = incubators.find(i => i.id == incubatorSelect.value);
        const selectedProject = projects.find(p => p.id == projectSelect.value);
        
        let summaryHtml = `<p><strong>Rol:</strong> ${role.displayName}</p>`;
        
        if (selectedIncubator) {
            summaryHtml += `<p><strong>Incubadora:</strong> ${selectedIncubator.name}</p>`;
        }
        
        if (selectedProject) {
            summaryHtml += `<p><strong>Proyecto:</strong> ${selectedProject.name}</p>`;
        }
        
        if (selectedIncubator || selectedProject) {
            contextSummaryContent.innerHTML = summaryHtml;
            contextSummary.style.display = 'block';
            contextSummary.classList.add('has-selection');
        } else {
            contextSummary.style.display = 'none';
        }
    }

    // Navigate between steps
    function navigateStep(direction) {
        // Validate current step before moving forward
        if (direction > 0 && !validateCurrentStep()) {
            return;
        }
        
        // Calculate new step
        const newStep = currentStep + direction;
        
        if (newStep < 1 || newStep > totalSteps) {
            return;
        }
        
        // Update step
        showStep(newStep);
    }

    // Show specific step
    function showStep(stepNumber) {
        currentStep = stepNumber;
        
        // Update step content visibility
        stepContents.forEach((content, index) => {
            content.style.display = index === stepNumber - 1 ? 'block' : 'none';
        });
        
        // Update step indicators
        stepIndicators.forEach((indicator, index) => {
            indicator.classList.remove('active', 'completed');
            if (index < stepNumber - 1) {
                indicator.classList.add('completed');
            } else if (index === stepNumber - 1) {
                indicator.classList.add('active');
            }
        });
        
        // Update navigation buttons
        prevBtn.style.display = stepNumber === 1 ? 'none' : 'inline-block';
        nextBtn.style.display = stepNumber === totalSteps ? 'none' : 'inline-block';
        submitBtn.style.display = stepNumber === totalSteps ? 'inline-block' : 'none';
    }

    // Validate current step
    function validateCurrentStep() {
        let isValid = true;
        
        switch (currentStep) {
            case 1:
                // Validate personal information
                const requiredFields = ['FirstName', 'LastName', 'Email', 'Identification'];
                requiredFields.forEach(fieldName => {
                    const field = document.getElementById(fieldName);
                    if (!field.value.trim()) {
                        field.classList.add('is-invalid');
                        isValid = false;
                    } else {
                        field.classList.remove('is-invalid');
                    }
                });
                
                // Validate email format
                const emailField = document.getElementById('Email');
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (emailField.value && !emailRegex.test(emailField.value)) {
                    emailField.classList.add('is-invalid');
                    isValid = false;
                }
                break;
                
            case 2:
                // Validate role selection
                if (!selectedRole) {
                    showToast('Por favor seleccione un rol', 'danger');
                    isValid = false;
                }
                
                // Validate context requirements
                if (selectedRole) {
                    const role = roleDefinitions[selectedRole];
                    
                    if (role.requiresIncubator && !incubatorSelect.value) {
                        incubatorSelect.classList.add('is-invalid');
                        isValid = false;
                    } else {
                        incubatorSelect.classList.remove('is-invalid');
                    }
                    
                    if (role.requiresProject && !projectSelect.value) {
                        projectSelect.classList.add('is-invalid');
                        isValid = false;
                    } else {
                        projectSelect.classList.remove('is-invalid');
                    }
                }
                break;
                
            case 3:
                // Validate password if not generating temporary
                if (!generateTempPasswordCheckbox.checked) {
                    const passwordInput = passwordField.querySelector('input');
                    const confirmPasswordInput = confirmPasswordField.querySelector('input');
                    
                    if (!passwordInput.value) {
                        passwordInput.classList.add('is-invalid');
                        isValid = false;
                    }
                    
                    if (!confirmPasswordInput.value) {
                        confirmPasswordInput.classList.add('is-invalid');
                        isValid = false;
                    }
                    
                    if (passwordInput.value && confirmPasswordInput.value && 
                        passwordInput.value !== confirmPasswordInput.value) {
                        confirmPasswordInput.classList.add('is-invalid');
                        showToast('Las contraseñas no coinciden', 'danger');
                        isValid = false;
                    }
                }
                break;
        }
        
        if (!isValid) {
            showToast('Por favor complete todos los campos requeridos', 'danger');
        }
        
        return isValid;
    }
});