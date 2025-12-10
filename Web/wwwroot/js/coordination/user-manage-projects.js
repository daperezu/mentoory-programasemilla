// User Manage Projects JavaScript
(function () {
    'use strict';

    let projectsTable;
    let addToProjectModal;
    let changeRoleModal;

    document.addEventListener('DOMContentLoaded', function () {
        initializeDataTable();
        initializeModals();
        initializeEventHandlers();
        loadIncubators();
    });

    function initializeDataTable() {
        projectsTable = $('#projectsTable').DataTable({
            ajax: {
                url: window.manageProjectsConfig.urls.listUserProjects,
                type: 'POST',
                data: function (d) {
                    return {
                        userId: window.manageProjectsConfig.userId
                    };
                },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            },
            columns: [
                {
                    data: 'projectName',
                    render: function (data, type, row) {
                        return `<strong>${data}</strong><br/><small class="text-muted">${row.projectKey}</small>`;
                    }
                },
                {
                    data: 'incubatorName',
                    render: function (data, type, row) {
                        return `${data}<br/><small class="text-muted">${row.incubatorKey}</small>`;
                    }
                },
                {
                    data: 'roleDisplayName',
                    render: function (data, type, row) {
                        return `<span class="badge bg-info">${data}</span>`;
                    }
                },
                {
                    data: 'isActive',
                    render: function (data, type, row) {
                        if (data) {
                            return '<span class="badge bg-success">Activo</span>';
                        } else {
                            return '<span class="badge bg-secondary">Inactivo</span>';
                        }
                    }
                },
                {
                    data: 'createdAtDisplay'
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        let actions = '';

                        if (window.manageProjectsConfig.canChangeRoles && row.isActive) {
                            actions += `<button type="button" class="btn btn-sm btn-warning me-1 btn-change-role"
                                        data-project-id="${row.projectId}"
                                        data-project-name="${row.projectName}"
                                        data-current-role="${row.roleDisplayName}"
                                        data-current-role-key="${row.role}"
                                        title="Cambiar Rol">
                                <i class="fas fa-exchange-alt"></i>
                            </button>`;
                        }

                        if (window.manageProjectsConfig.canRemoveFromProjects && row.isActive) {
                            actions += `<button type="button" class="btn btn-sm btn-danger btn-remove-from-project"
                                        data-project-id="${row.projectId}"
                                        data-project-name="${row.projectName}"
                                        title="Remover">
                                <i class="fas fa-trash"></i>
                            </button>`;
                        }

                        return actions || '<span class="text-muted">Sin acciones</span>';
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
            },
            order: [[4, 'desc']]
        });
    }

    function initializeModals() {
        addToProjectModal = new bootstrap.Modal(document.getElementById('addToProjectModal'));
        changeRoleModal = new bootstrap.Modal(document.getElementById('changeRoleModal'));

        // Cascading dropdown for incubator -> projects
        $('#add_incubatorId').on('change', function () {
            const incubatorId = $(this).val();
            if (incubatorId) {
                loadProjects(incubatorId);
            } else {
                $('#add_projectId').prop('disabled', true).html('<option value="">-- Primero seleccione una incubadora --</option>');
            }
        });
    }

    function initializeEventHandlers() {
        // Add to project button
        $('#btnAddToProject').on('click', function () {
            $('#addToProjectForm')[0].reset();
            $('#add_projectId').prop('disabled', true).html('<option value="">-- Primero seleccione una incubadora --</option>');
            addToProjectModal.show();
        });

        // Confirm add to project
        $('#btnConfirmAdd').on('click', function () {
            if (validateAddForm()) {
                addUserToProject();
            }
        });

        // Change role buttons (delegated)
        $('#projectsTable').on('click', '.btn-change-role', function () {
            const projectId = $(this).data('project-id');
            const projectName = $(this).data('project-name');
            const currentRole = $(this).data('current-role');
            const currentRoleKey = $(this).data('current-role-key');

            $('#change_projectId').val(projectId);
            $('#change_projectName').text(projectName);
            $('#change_currentRole').text(currentRole);
            $('#change_newRole').val('').find('option').prop('disabled', false);
            $('#change_newRole').find(`option[value="${currentRoleKey}"]`).prop('disabled', true);

            changeRoleModal.show();
        });

        // Confirm change role
        $('#btnConfirmChangeRole').on('click', function () {
            if (validateChangeRoleForm()) {
                changeUserRole();
            }
        });

        // Remove from project buttons (delegated)
        $('#projectsTable').on('click', '.btn-remove-from-project', function () {
            const projectId = $(this).data('project-id');
            const projectName = $(this).data('project-name');

            if (confirm(`¿Está seguro que desea remover a ${window.manageProjectsConfig.userFullName} del proyecto ${projectName}?`)) {
                removeUserFromProject(projectId, projectName);
            }
        });
    }

    function loadIncubators() {
        $.ajax({
            url: window.manageProjectsConfig.urls.getAllIncubators,
            type: 'GET',
            success: function (response) {
                if (response.success && response.incubators) {
                    const select = $('#add_incubatorId');
                    select.html('<option value="">-- Seleccione una incubadora --</option>');
                    response.incubators.forEach(function (incubator) {
                        select.append(`<option value="${incubator.id}">${incubator.name} (${incubator.key})</option>`);
                    });
                }
            },
            error: function () {
                showToast('error', 'Error al cargar las incubadoras');
            }
        });
    }

    function loadProjects(incubatorId) {
        $.ajax({
            url: window.manageProjectsConfig.urls.getProjectsByIncubator,
            type: 'GET',
            data: { incubatorId: incubatorId },
            success: function (response) {
                if (response.success && response.projects) {
                    const select = $('#add_projectId');
                    select.html('<option value="">-- Seleccione un proyecto --</option>');
                    response.projects.forEach(function (project) {
                        select.append(`<option value="${project.id}">${project.name} (${project.key})</option>`);
                    });
                    select.prop('disabled', false);
                } else {
                    $('#add_projectId').html('<option value="">No hay proyectos disponibles</option>').prop('disabled', true);
                }
            },
            error: function () {
                showToast('error', 'Error al cargar los proyectos');
            }
        });
    }

    function validateAddForm() {
        const incubatorId = $('#add_incubatorId').val();
        const projectId = $('#add_projectId').val();
        const role = $('#add_role').val();

        if (!incubatorId || !projectId || !role) {
            showToast('error', 'Por favor complete todos los campos requeridos');
            return false;
        }
        return true;
    }

    function validateChangeRoleForm() {
        const newRole = $('#change_newRole').val();
        if (!newRole) {
            showToast('error', 'Por favor seleccione un nuevo rol');
            return false;
        }
        return true;
    }

    function addUserToProject() {
        const data = {
            userId: $('#add_userId').val(),
            incubatorId: parseInt($('#add_incubatorId').val()),
            projectId: parseInt($('#add_projectId').val()),
            role: $('#add_role').val()
        };

        $.ajax({
            url: window.manageProjectsConfig.urls.addToProject,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showToast('success', response.message);
                    addToProjectModal.hide();
                    projectsTable.ajax.reload();
                } else {
                    showToast('error', response.message);
                }
            },
            error: function () {
                showToast('error', 'Error al agregar usuario al proyecto');
            }
        });
    }

    function changeUserRole() {
        const data = {
            userId: $('#change_userId').val(),
            projectId: parseInt($('#change_projectId').val()),
            projectName: $('#change_projectName').text(),
            currentRole: $('#change_currentRole').text(),
            newRole: $('#change_newRole').val()
        };

        $.ajax({
            url: window.manageProjectsConfig.urls.changeProjectRole,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showToast('success', response.message);
                    changeRoleModal.hide();
                    projectsTable.ajax.reload();
                } else {
                    showToast('error', response.message);
                }
            },
            error: function () {
                showToast('error', 'Error al cambiar el rol del usuario');
            }
        });
    }

    function removeUserFromProject(projectId, projectName) {
        $.ajax({
            url: window.manageProjectsConfig.urls.removeFromProject,
            type: 'POST',
            data: {
                userId: window.manageProjectsConfig.userId,
                projectId: projectId
            },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showToast('success', response.message);
                    projectsTable.ajax.reload();
                } else {
                    showToast('error', response.message);
                }
            },
            error: function () {
                showToast('error', 'Error al remover usuario del proyecto');
            }
        });
    }

    function showToast(type, message) {
        // Use LinaSys toast system
        if (typeof Toast !== 'undefined') {
            if (type === 'success') {
                Toast.success(message);
            } else {
                Toast.error(message);
            }
        } else {
            alert(message);
        }
    }
})();
