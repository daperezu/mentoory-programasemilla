using LinaSys.Permissions.Domain.Aggregates.ProtectedResource;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Permissions.Infrastructure.Persistence;

public class PermissionsDbContext(DbContextOptions<PermissionsDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public virtual DbSet<ProtectedResource> ProtectedResources { get; set; }

    public virtual DbSet<RoleProtectedResourcePermission> RoleProtectedResourcePermissions { get; set; }

    public virtual DbSet<UserProtectedResourcePermission> UserProtectedResourcePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ProtectedResource>(entity =>
        {
            entity.ToTable("ProtectedResources", "permissions");

            entity.HasIndex(e => e.ExternalId, "IX_ProtectedResources_ExternalId").IsUnique();

            entity.HasIndex(e => e.ResourceType, "IX_ProtectedResources_ResourceType");

            entity.Property(e => e.ExternalId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
        });

        builder.Entity<RoleProtectedResourcePermission>(entity =>
        {
            entity.ToTable("RoleProtectedResourcePermissions", "permissions");

            entity.HasIndex(e => new { e.Role, e.ProtectedResourceId }, "IX_RoleProtectedResourcePermissions_ProtectedResourceId");

            entity.Property(e => e.Role).IsRequired().HasMaxLength(256);

            entity.HasOne(d => d.ProtectedResource).WithMany(p => p.RoleProtectedResourcePermissions).HasForeignKey(d => d.ProtectedResourceId);
        });

        builder.Entity<UserProtectedResourcePermission>(entity =>
        {
            entity.ToTable("UserProtectedResourcePermissions", "permissions");

            entity.HasIndex(e => new { e.ProtectedResourceId, e.UserId }, "IX_UserProtectedResourcePermissions_ProtectedResourceIdUser");

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.ProtectedResource).WithMany(p => p.UserProtectedResourcePermissions).HasForeignKey(d => d.ProtectedResourceId);
        });
    }
}
