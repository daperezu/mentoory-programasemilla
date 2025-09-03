using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Starter.Services;

public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

public class TaskGenerationService(
    IStarterRepository starterRepository,
    ITimeProvider timeProvider,
    ILogger<TaskGenerationService> logger) : ITaskGenerationService
{

    // Task templates per phase
    private readonly Dictionary<string, List<TaskTemplate>> _phaseTemplates = new()
    {
        ["diagnosis"] =
        [
            new("Completar evaluación inicial", "Complete el formulario de evaluación diagnóstica de su emprendimiento", "form", TaskPriority.High, 7),
            new("Cargar plan de negocio", "Suba su plan de negocio actualizado en formato PDF", "document", TaskPriority.Normal, 14),
            new("Agendar primera mentoría", "Programe su sesión inicial con el mentor asignado", "meeting", TaskPriority.High, 3),
            new("Completar análisis FODA", "Realice el análisis de Fortalezas, Oportunidades, Debilidades y Amenazas", "form", TaskPriority.Normal, 10),
            new("Definir modelo de negocio", "Desarrolle su modelo de negocio usando la metodología Canvas", "document", TaskPriority.Normal, 21),
            new("Investigación de mercado", "Realice un estudio básico de su mercado objetivo", "custom", TaskPriority.Normal, 14),
            new("Identificar competencia", "Identifique y analice a sus principales competidores", "form", TaskPriority.Normal, 10),
            new("Definir propuesta de valor", "Articule claramente su propuesta de valor única", "document", TaskPriority.High, 7)
        ],
        ["development"] =
        [
            new("Desarrollar MVP", "Cree la versión mínima viable de su producto", "milestone", TaskPriority.High, 30),
            new("Crear plan financiero", "Desarrolle proyecciones financieras a 3 años", "document", TaskPriority.High, 14),
            new("Validar hipótesis de mercado", "Realice entrevistas con clientes potenciales", "form", TaskPriority.Normal, 21),
            new("Establecer métricas KPI", "Defina los indicadores clave de rendimiento", "document", TaskPriority.Normal, 7),
            new("Diseñar arquitectura técnica", "Diseñe la arquitectura de su solución", "document", TaskPriority.High, 10),
            new("Crear mockups", "Diseñe los prototipos de interfaz de usuario", "custom", TaskPriority.Normal, 14),
            new("Definir estrategia de precios", "Establezca su modelo de precios", "document", TaskPriority.High, 7),
            new("Crear roadmap de producto", "Desarrolle el plan de evolución del producto", "document", TaskPriority.Normal, 10)
        ],
        ["validation"] =
        [
            new("Realizar pruebas con usuarios", "Ejecute pruebas con un grupo de usuarios beta", "form", TaskPriority.High, 14),
            new("Ajustar producto según feedback", "Implemente mejoras basadas en retroalimentación", "milestone", TaskPriority.High, 21),
            new("Preparar pitch deck", "Cree su presentación para inversores", "document", TaskPriority.Normal, 7),
            new("Validar modelo de precios", "Confirme su estrategia de precios con el mercado", "form", TaskPriority.Normal, 7),
            new("Crear landing page", "Desarrolle una página de aterrizaje para su producto", "custom", TaskPriority.High, 10),
            new("Definir estrategia de marketing", "Establezca su plan de marketing digital", "document", TaskPriority.Normal, 14),
            new("Análisis de métricas", "Analice las métricas de su producto", "form", TaskPriority.Normal, 7)
        ],
        ["implementation"] =
        [
            new("Lanzamiento al mercado", "Lance oficialmente su producto al mercado", "milestone", TaskPriority.High, 30),
            new("Configurar analytics", "Implemente herramientas de análisis y seguimiento", "custom", TaskPriority.High, 7),
            new("Establecer soporte al cliente", "Configure sistema de atención al cliente", "custom", TaskPriority.High, 10),
            new("Plan de ventas Q1", "Desarrolle su estrategia de ventas para el primer trimestre", "document", TaskPriority.Normal, 14),
            new("Configurar pasarela de pagos", "Integre sistema de pagos en línea", "custom", TaskPriority.High, 7),
            new("Crear términos y condiciones", "Redacte los términos legales de su servicio", "document", TaskPriority.Normal, 10),
            new("Implementar SEO", "Optimice su presencia en motores de búsqueda", "custom", TaskPriority.Normal, 14)
        ],
        ["growth"] =
        [
            new("Expansión de mercado", "Planifique la expansión a nuevos mercados", "milestone", TaskPriority.Normal, 60),
            new("Búsqueda de inversión", "Prepare documentación para ronda de inversión", "document", TaskPriority.Normal, 30),
            new("Optimización de procesos", "Mejore la eficiencia operativa", "custom", TaskPriority.Normal, 21),
            new("Contratar equipo clave", "Reclute talento esencial para el crecimiento", "custom", TaskPriority.High, 30),
            new("Establecer partnerships", "Cree alianzas estratégicas con socios clave", "meeting", TaskPriority.Normal, 21),
            new("Escalar infraestructura", "Prepare su infraestructura para el crecimiento", "custom", TaskPriority.High, 14),
            new("Internacionalización", "Prepare su producto para mercados internacionales", "milestone", TaskPriority.Normal, 90)
        ]
    };

    public async Task GenerateTasksForPhaseAsync(string userId, long projectId, string phase)
    {
        if (!_phaseTemplates.ContainsKey(phase.ToLower()))
        {
            logger.LogWarning("No templates found for phase: {Phase}", phase);
            return;
        }

        var templates = _phaseTemplates[phase.ToLower()];
        var existingTasks = await starterRepository.GetStarterTasksAsync(userId, projectId);

        foreach (var template in templates)
        {
            // Check if task already exists
            if (existingTasks.Any(t => t.Title.Equals(template.Title, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var now = timeProvider.UtcNow;
            var task = new StarterTask(
                projectId,
                userId,
                template.Title,
                template.Description,
                now,
                template.Type,
                template.Priority.ToString().ToLower(),
                now.AddDays(template.DueDays),
                "system",
                phase);

            // Save task to database
            await SaveTaskAsync(task);

            logger.LogInformation("Generated task: {Title} for user {UserId} in phase {Phase}",
                template.Title, userId, phase);
        }
    }

    public async Task CheckAndGenerateOverdueTasksAsync(string userId, long projectId)
    {
        var tasks = await starterRepository.GetStarterTasksAsync(userId, projectId);
        var overdueTasks = tasks.Where(t => t.IsOverdue()).ToList();

        foreach (var task in overdueTasks)
        {
            logger.LogWarning("Task {TaskId} '{Title}' is overdue for user {UserId}",
                task.Id, task.Title, userId);

            // Update task status to overdue
            await UpdateTaskStatusAsync(task.Id, "overdue", "Task marked as overdue by system");

            // Generate notification for overdue task
            await GenerateOverdueNotificationAsync(userId, task);
        }
    }

    public async Task GenerateTaskFromTemplateAsync(string userId, long projectId, string templateCode)
    {
        // This would look up a specific template and create a task from it
        logger.LogInformation("Generating task from template {TemplateCode} for user {UserId}",
            templateCode, userId);

        await Task.CompletedTask;
    }

    public async Task<List<StarterTask>> GetPendingTasksAsync(string userId, long projectId)
    {
        var tasks = await starterRepository.GetStarterTasksAsync(userId, projectId);
        return tasks.Where(t => t.Status == Domain.Aggregates.Starter.TaskStatus.Pending).ToList();
    }

    public async Task UpdateTaskStatusAsync(long taskId, string status, string? notes = null)
    {
        await starterRepository.UpdateTaskStatusAsync(taskId, status);
        logger.LogInformation("Updated task {TaskId} status to {Status}", taskId, status);
    }

    private async Task SaveTaskAsync(StarterTask task)
    {
        await starterRepository.AddTaskAsync(task);
    }

    private async Task GenerateOverdueNotificationAsync(string userId, StarterTask task)
    {
        // TODO: Implement notification creation through a proper notification service
        // For now, log the notification
        logger.LogWarning("Task {TaskId} '{TaskTitle}' is overdue for user {UserId}",
            task.Id, task.Title, userId);
        await Task.CompletedTask;
    }
}

public class TaskTemplate(string title, string description, string type, TaskPriority priority, int dueDays)
{
    public string Title { get; set; } = title;

    public string Description { get; set; } = description;

    public string Type { get; set; } = type;

    public TaskPriority Priority { get; set; } = priority;

    public int DueDays { get; set; } = dueDays;
}