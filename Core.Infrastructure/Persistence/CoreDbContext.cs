using LinaSys.Core.Domain.Aggregates.Activity;
using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Aggregates.Navigation;
using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using LinaSys.Core.Infrastructure.Persistence.EntityConfigurations;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LinaSys.Core.Infrastructure.Persistence;

public class CoreDbContext : SharedAbstractDbContext
{
    private readonly IAuditContext? _auditContext;

    public CoreDbContext(DbContextOptions<CoreDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    public CoreDbContext(DbContextOptions<CoreDbContext> options, IMediator mediator, IAuditContext auditContext)
        : base(options, mediator)
    {
        _auditContext = auditContext;
    }

    public DbSet<UserDashboard> UserDashboards { get; set; }
    public DbSet<DashboardWidget> DashboardWidgets { get; set; }
    public DbSet<UserWidgetConfiguration> UserWidgetConfigurations { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<RoleDashboardTemplate> RoleDashboardTemplates { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<NavigationMenuItem> NavigationMenuItems { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserDashboard
        modelBuilder.Entity<UserDashboard>(entity =>
        {
            entity.ToTable("UserDashboards", "core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Theme).HasMaxLength(50).HasDefaultValue("light");
            entity.Property(e => e.Language).HasMaxLength(10).HasDefaultValue("es");
            entity.HasIndex(e => new { e.UserId, e.Role }).IsUnique();

            // Configure DashboardPreferences as owned type
            entity.OwnsOne(e => e.Preferences, p =>
            {
                p.Property(x => x.Theme).HasColumnName("PreferencesTheme").HasMaxLength(50);
                p.Property(x => x.Language).HasColumnName("PreferencesLanguage").HasMaxLength(10);
                p.Property(x => x.RefreshInterval).HasColumnName("PreferencesRefreshInterval");
                p.Property(x => x.ShowNotifications).HasColumnName("PreferencesShowNotifications");
                p.Property(x => x.PlayNotificationSound).HasColumnName("PreferencesPlayNotificationSound");
                p.Property(x => x.ShowTaskReminders).HasColumnName("PreferencesShowTaskReminders");
                p.Property(x => x.AutoRefreshEnabled).HasColumnName("PreferencesAutoRefreshEnabled");
                p.Property(x => x.CompactView).HasColumnName("PreferencesCompactView");
                p.Property(x => x.ShowWidgetHeaders).HasColumnName("PreferencesShowWidgetHeaders");
                p.Property(x => x.EnableAnimations).HasColumnName("PreferencesEnableAnimations");
                p.Property(x => x.DateFormat).HasColumnName("PreferencesDateFormat").HasMaxLength(20);
                p.Property(x => x.TimeFormat).HasColumnName("PreferencesTimeFormat").HasMaxLength(20);
                p.Property(x => x.Timezone).HasColumnName("PreferencesTimezone").HasMaxLength(50);
                p.Property(x => x.WidgetLayout).HasColumnName("PreferencesWidgetLayout");
            });

            // Ignore navigation properties from base class
            entity.Ignore(e => e.Widgets);
            entity.Ignore(e => e.Notifications);

            // Map audit shadow properties
            entity.Property<DateTime>("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property<string>("CreatedBy").HasMaxLength(450);
            entity.Property<DateTime?>("UpdatedAt");
            entity.Property<string>("UpdatedBy").HasMaxLength(450);
        });

        // Configure DashboardWidget
        modelBuilder.Entity<DashboardWidget>(entity =>
        {
            entity.ToTable("DashboardWidgets", "core");
            entity.HasKey(e => e.Id);

            // Map properties correctly - Name and Code are the same column
            entity.Property(e => e.Name).HasColumnName("Name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Component).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsActive).IsRequired();

            // Map other domain properties to database columns
            entity.Property(e => e.Roles).HasColumnName("Roles"); // Roles maps to Roles column
            entity.Property(e => e.Configuration).HasColumnName("DefaultConfig"); // Configuration maps to DefaultConfig
            entity.Property(e => e.DefaultPosition).HasColumnName("SortOrder"); // DefaultPosition maps to SortOrder
            entity.Property(e => e.RefreshInterval).HasColumnName("RefreshIntervalSeconds"); // RefreshInterval maps to RefreshIntervalSeconds

            // Properties that don't exist in the database - ignore them
            entity.Ignore(e => e.WidgetId);
            entity.Ignore(e => e.GridRow);
            entity.Ignore(e => e.GridColumn);
            entity.Ignore(e => e.Width);
            entity.Ignore(e => e.Height);
            entity.Ignore(e => e.Size);
            entity.Ignore(e => e.IsVisible);
            entity.Ignore(e => e.IsCollapsed);
            entity.Ignore(e => e.LastRefreshedAt);

            // Shadow properties for database columns not in domain
            entity.Property<string>("DisplayName").HasMaxLength(200).IsRequired();
            entity.Property<string>("Description").HasMaxLength(500);
            entity.Property<string>("IconClass").HasMaxLength(100);
            entity.Property<string>("MinSize").HasMaxLength(20).HasDefaultValue("small");
            entity.Property<string>("MaxSize").HasMaxLength(20).HasDefaultValue("full");
            entity.Property<bool>("IsResizable").HasDefaultValue(true);
            entity.Property<bool>("IsDraggable").HasDefaultValue(true);
            entity.Property<bool>("Refreshable").HasDefaultValue(true);
            entity.Property<DateTime>("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property<DateTime?>("UpdatedAt");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure UserWidgetConfiguration
        modelBuilder.Entity<UserWidgetConfiguration>(entity =>
        {
            entity.ToTable("UserWidgetConfigurations", "core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.UserId, e.WidgetId }).IsUnique();
        });

        // Configure UserNotification
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications", "core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();

            // Configure enum conversions
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Store enum as string in database

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Store enum as string in database

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<int>(); // Store enum as int in database (matches DB schema)

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            entity.Property(e => e.ActionText).HasMaxLength(100);
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.IsRead).IsRequired();
            entity.Property(e => e.IsDismissed).IsRequired();
            entity.Property(e => e.IsActionTaken).IsRequired();
            entity.Property(e => e.ExpiresAt);
            entity.Property(e => e.ReadAt);
            entity.Property(e => e.DismissedAt);
            entity.Property(e => e.ActionTakenAt);

            // Shadow property for Data column (JSON)
            entity.Property<string>("Data").HasColumnName("Data");

            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure RoleDashboardTemplate
        modelBuilder.Entity<RoleDashboardTemplate>(entity =>
        {
            entity.ToTable("RoleDashboardTemplates", "core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DefaultLayout).HasColumnName("DefaultLayout");
            entity.Property(e => e.DefaultTheme).HasMaxLength(50).HasDefaultValue("light");
            entity.Property(e => e.DefaultLanguage).HasMaxLength(10).HasDefaultValue("es");
            entity.Property(e => e.DefaultRefreshInterval).HasDefaultValue(300);
            entity.Property(e => e.WidgetCodes).HasColumnName("WidgetCodes");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).HasColumnName("CreatedDate");
            entity.Property(e => e.ModifiedDate).HasColumnName("ModifiedDate");
            entity.HasIndex(e => e.Role).IsUnique();
        });

        // Configure UserActivity
        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.ToTable("UserActivities", "core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.EntityId);
            entity.Property(e => e.Metadata);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // Configure NavigationMenuItem
        modelBuilder.Entity<NavigationMenuItem>(entity =>
        {
            entity.ToTable("NavigationMenuItems", "core");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("UQ_NavigationMenuItems_Code");

            entity.Property(e => e.DisplayText)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Icon)
                .HasMaxLength(50);

            entity.Property(e => e.CssClass)
                .HasMaxLength(100);

            entity.Property(e => e.Url)
                .HasMaxLength(500);

            entity.Property(e => e.AllowedRoles)
                .HasMaxLength(500);

            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0);

            entity.Property(e => e.Level)
                .HasDefaultValue(0);

            entity.Property(e => e.IsSection)
                .HasDefaultValue(false);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.RequiresAuthentication)
                .HasDefaultValue(true);

            entity.Property(e => e.RequiresIncubator)
                .HasDefaultValue(false);

            entity.Property(e => e.RequiresProject)
                .HasDefaultValue(false);

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Apply AuditLog configuration
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }

    private void UpdateAuditFields()
    {
        if (_auditContext == null)
        {
            return;
        }

        var entries = ChangeTracker.Entries<UserDashboard>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = _auditContext.UtcNow;
                entry.Property("CreatedBy").CurrentValue = _auditContext.User ?? "system";
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = _auditContext.UtcNow;
                entry.Property("UpdatedBy").CurrentValue = _auditContext.User ?? "system";
            }
        }
    }
}
