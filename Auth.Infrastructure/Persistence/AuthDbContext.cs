using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.AggregatesModel.User;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Auth.Infrastructure.Persistence;

public partial class AuthDbContext(DbContextOptions<AuthDbContext> options, IMediator mediator)
    : IdentityDbContext<User>(options)
{
    public virtual DbSet<UserContextPreferences> UserContextPreferences { get; set; }

    // Read models for access control (synchronized via integration events)
    public virtual DbSet<UserProjectAccess> UserProjectAccesses { get; set; }
    public virtual DbSet<UserIncubatorAccess> UserIncubatorAccesses { get; set; }
    public virtual DbSet<UserMentorshipAccess> UserMentorshipAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserContextPreferences>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.LastRole)
                .HasMaxLength(256);

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2");

            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<UserContextPreferences>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure read model entities
        builder.Entity<UserProjectAccess>(entity =>
        {
            entity.ToTable("UserProjectAccess", "dbo");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.LastSyncedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasIndex(e => new { e.ProjectId, e.IsActive });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserIncubatorAccess>(entity =>
        {
            entity.ToTable("UserIncubatorAccess", "dbo");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.LastSyncedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasIndex(e => new { e.IncubatorId, e.IsActive });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserMentorshipAccess>(entity =>
        {
            entity.ToTable("UserMentorshipAccess", "dbo");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.MentorUserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.StarterUserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.AssignedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastSyncedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.MentorUserId, e.IsActive });
            entity.HasIndex(e => new { e.StarterUserId, e.IsActive });
            entity.HasIndex(e => new { e.ProjectId, e.IsActive });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.MentorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.StarterUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
