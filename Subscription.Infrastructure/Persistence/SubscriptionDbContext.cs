using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Subscription.Infrastructure.Persistence;

public class SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options, IMediator mediator)
    : SharedAbstractDbContext(options, mediator)
{
    public virtual DbSet<BusinessIncubatorPackage> BusinessIncubatorPackages { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PackageLimitOverride> PackageLimitOverrides { get; set; }

    public virtual DbSet<PackageVersion> PackageVersions { get; set; }

    public virtual DbSet<PackageVersionLimit> PackageVersionLimits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessIncubatorPackage>(entity =>
        {
            entity.ToTable("BusinessIncubatorPackages", "subscription");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasOne(d => d.PackageVersion).WithMany(p => p.BusinessIncubatorPackages)
                .HasForeignKey(d => d.PackageVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessIncubatorPackages_PackageVersions");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.ToTable("Packages", "subscription");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<PackageLimitOverride>(entity =>
        {
            entity.HasKey(e => new { e.BusinessIncubatorPackageId, e.Type, e.Quantity });

            entity.ToTable("PackageLimitOverrides", "subscription");

            entity.HasOne(d => d.BusinessIncubatorPackage).WithMany(p => p.PackageLimitOverrides)
                .HasForeignKey(d => d.BusinessIncubatorPackageId)
                .HasConstraintName("FK_PackageLimitOverrides_BusinessIncubatorPackages");
        });

        modelBuilder.Entity<PackageVersion>(entity =>
        {
            entity.ToTable("PackageVersions", "subscription");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasOne(d => d.Package).WithMany(p => p.PackageVersions)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK_PackageVersions_Packages");
        });

        modelBuilder.Entity<PackageVersionLimit>(entity =>
        {
            entity.HasKey(e => new { e.PackageVersionId, e.Type });

            entity.ToTable("PackageVersionLimits", "subscription");

            entity.HasOne(d => d.PackageVersion).WithMany(p => p.PackageVersionLimits)
                .HasForeignKey(d => d.PackageVersionId)
                .HasConstraintName("FK_PackageVersionLimits_PackageVersions");
        });
    }
}
