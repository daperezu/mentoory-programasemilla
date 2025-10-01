using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.UserManagement.Infrastructure.Persistence;

public class UserManagementDbContext(DbContextOptions<UserManagementDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(builder =>
        {
            builder.ToTable("UserProfiles", "usermanagement");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).UseIdentityColumn();

            builder.Property(u => u.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(u => u.UserId)
                .IsUnique()
                .HasDatabaseName("UX_UserProfiles_UserId");

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasField("_firstName");

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasField("_lastName");

            builder.Property(u => u.Identification)
                .IsRequired()
                .HasMaxLength(50)
                .HasField("_identification");

            builder.HasIndex(u => u.Identification)
                .IsUnique()
                .HasDatabaseName("UX_UserProfiles_Identification");

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500)
                .HasField("_avatarUrl");

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasField("_isActive");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_UserProfiles_IsActive")
                .HasFilter("[IsActive] = 1");

            // Configure Location value object
            builder.OwnsOne(u => u.Location, location =>
            {
                location.Property(l => l.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(100);

                location.Property(l => l.Province)
                    .HasColumnName("Province")
                    .HasMaxLength(100);

                location.Property(l => l.Canton)
                    .HasColumnName("Canton")
                    .HasMaxLength(100);

                location.Property(l => l.District)
                    .HasColumnName("District")
                    .HasMaxLength(100);

                location.Property(l => l.FullAddress)
                    .HasColumnName("FullAddress")
                    .HasMaxLength(500);
            });

            // Configure Preferences collection
            builder.HasMany(u => u.Preferences)
                .WithOne()
                .HasForeignKey("UserProfileId")
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            builder.Ignore(u => u.FullName);
            builder.Ignore(u => u.DomainEvents);
        });

        // Configure UserPreferences entity
        modelBuilder.Entity<UserPreferences>(builder =>
        {
            builder.ToTable("UserPreferences", "usermanagement");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).UseIdentityColumn();

            builder.Property("UserProfileId")
                .IsRequired();

            builder.Property(p => p.Key)
                .IsRequired()
                .HasMaxLength(100)
                .HasField("_key");

            builder.Property(p => p.Value)
                .IsRequired()
                .HasField("_value");

            builder.HasIndex("UserProfileId", "Key")
                .IsUnique()
                .HasDatabaseName("UX_UserPreferences_UserProfileId_Key");
        });
    }
}
