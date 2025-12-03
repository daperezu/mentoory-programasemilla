#region usings section
using System.Reflection;
using LinaSys.Auth.Application;
using LinaSys.Auth.Infrastructure;
using LinaSys.BusinessIncubator.Application;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.BusinessIncubator.Infrastructure;
using LinaSys.BusinessIncubator.Infrastructure.Persistence.Repositories;
using LinaSys.Core.Application;
using LinaSys.Core.Infrastructure;
using LinaSys.Diagnostics.Application;
using LinaSys.Diagnostics.Infrastructure;
using LinaSys.KnowledgeStructure.Application;
using LinaSys.KnowledgeStructure.Infrastructure;
using LinaSys.Notification.Application;
using LinaSys.Notification.Infrastructure;
using LinaSys.Orchestration.Application;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Auth;
using LinaSys.Shared.Application.Behaviors;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Shared.Infrastructure;
using LinaSys.Shared.Infrastructure.Behaviors;
using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.Subscription.Application;
using LinaSys.Subscription.Infrastructure;
using LinaSys.UserManagement.Application;
using LinaSys.UserManagement.Infrastructure;
using LinaSys.Web.Auth;
using LinaSys.Web.Hubs;
using LinaSys.Web.Infrastructure.Persistence;
using LinaSys.Web.Infrastructure.Services;
using LinaSys.Web.Filters;
using LinaSys.Web.ModelBinders;
using LinaSys.Web.Services;
using OfficeOpenXml;
#endregion

var builder = WebApplication.CreateBuilder(args);

//// Aspire extension
builder.AddServiceDefaults();

builder.AddAzureBlobServiceClient("blobs");

ExcelPackage.License.SetNonCommercialPersonal("LinaSys");

var mediatRLicenseKey = builder.Configuration.GetValue<string>("MediatR:LicenseKey");
builder.Services.AddMediatR(cfg =>
{
    cfg.LicenseKey = mediatRLicenseKey;

    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());

    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();

builder.Services.AddSingleton<ITimeProvider, DefaultSystemTimeProvider>();
builder.Services.AddScoped<ICurrentUserService, CurrentHttpUserService>();
builder.Services.AddScoped<IAuditContext>(provider =>
{
    var timeProvider = provider.GetRequiredService<ITimeProvider>();
    var currentUserService = provider.GetRequiredService<ICurrentUserService>();

    return new AuditContext(timeProvider.UtcNow, currentUserService.UserName);
});

builder.Services.AddScoped<IAccessChecker, AccessChecker>();
builder.Services.AddScoped<IAuthScopeProvider, AuthScopeProvider>();
builder.Services.AddScoped<MediatorExecutor>();

builder.Services.AddSingleton<IVersionProvider, VersionProvider>();

// Progress tracking service for bulk operations
builder.Services.AddSingleton<IProgressTrackingService, ProgressTrackingService>();

// Register ApplicationUrlService for generating URLs in the application layer
builder.Services.AddScoped<IApplicationUrlService, ApplicationUrlService>();

// Register Google Analytics service for web analytics tracking
builder.Services.AddSingleton<IGoogleAnalyticsService, GoogleAnalyticsService>();

// Register Google Maps configuration options
builder.Services.Configure<LinaSys.Web.Options.GoogleMapsOptions>(
    builder.Configuration.GetSection(LinaSys.Web.Options.GoogleMapsOptions.SectionName));

#region Modules dependencies registration

// Register shared application services (including IIntegrationEventService)
builder.Services.AddSharedApplication();

// Register shared infrastructure services (including IFileStorageService)
builder.AddSharedInfrastructureServices();

// Register image rendering service (uses IFileStorageService)
builder.Services.AddScoped<ImageRenderingService>();

//// Auth Domain
builder.AddAuthInfrastructure();
builder.Services.AddAuthApplication();

//// BusinessIncubator Domain
builder.AddBusinessIncubatorInfrastructure();
builder.Services.AddBusinessIncubatorApplication();

// Add Core Infrastructure (Dashboard system)
builder.Services.AddCoreInfrastructure(builder.Configuration);
builder.Services.AddCoreApplication();

// Register Starter repository
builder.Services.AddScoped<IStarterRepository, StarterRepository>(); //// ??????????????????????

//// Diagnostics Domain
builder.AddDiagnosticsInfrastructure();
builder.Services.AddDiagnosticsApplication();

//// KnowledgeStructure Domain
builder.AddKnowledgeStructureInfrastructure();
builder.Services.AddKnowledgeStructureApplication();

//// Notification Domain
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddNotificationApplication();

//// Orchestration Module
builder.Services.AddOrchestrationApplication();

//// Subscription Domain
builder.AddSubscriptionInfrastructure();
builder.Services.AddSubscriptionApplication();

//// UserManagement Domain
builder.AddUserManagementInfrastructure();
builder.Services.AddUserManagementApplication();

#endregion

//// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new AnswerViewModelListModelBinderProvider());
    options.ModelBinderProviders.Insert(0, new DataTableRequestModelBinderProvider());

    // Add the password change enforcement filter globally
    options.Filters.Add<RequirePasswordChangeFilter>();
});

//// [SignalR hub] SignalR for real-time notifications
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

//// [SignalR hub] Register notification service
builder.Services.AddScoped<IReviewNotificationService, ReviewNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    //// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

//// [SignalR hub] Map SignalR hubs
app.MapHub<ReviewNotificationHub>("/hubs/review-notifications");
app.MapHub<UserManagementHub>("/hubs/user-management");

app.MapDefaultEndpoints();

app.MapFallbackToController("PageNotFound", "Home");

await app.RunAsync();
