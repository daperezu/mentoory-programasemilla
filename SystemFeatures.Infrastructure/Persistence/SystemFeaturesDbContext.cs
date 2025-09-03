using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.SystemFeatures.Domain.AggregatesModel.WebFeatureAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.SystemFeatures.Infrastructure.Persistence;

public class SystemFeaturesDbContext(DbContextOptions<SystemFeaturesDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public virtual DbSet<WebFeature> WebFeatures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebFeature>(entity =>
        {
            entity.ToTable("WebFeatures", "systemfeatures");

            entity.HasIndex(e => new { e.Area, e.Controller, e.Action }, "IX_WebFeatures_AreaControllerAction");

            entity.HasIndex(e => new { e.IsMenu, e.ParentId }, "IX_WebFeatures_IsMenu_Parent");

            entity.HasIndex(e => e.IsPublic, "IX_WebFeatures_IsPublic");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.Controller).HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.IsPublic).HasDefaultValue(false);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasForeignKey(d => d.ParentId);
        });
    }
}
