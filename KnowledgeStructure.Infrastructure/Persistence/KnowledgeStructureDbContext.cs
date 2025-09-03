using LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;
using LinaSys.KnowledgeStructure.Domain.Aggregates.Module;
using LinaSys.KnowledgeStructure.Domain.Aggregates.Subject;
using LinaSys.KnowledgeStructure.Domain.Aggregates.Topic;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.KnowledgeStructure.Infrastructure.Persistence;

public class KnowledgeStructureDbContext(DbContextOptions<KnowledgeStructureDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public virtual DbSet<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure> KnowledgeStructures { get; set; }

    public virtual DbSet<KnowledgeStructureModule> KnowledgeStructureModules { get; set; }

    public virtual DbSet<KnowledgeStructureTopic> KnowledgeStructureTopics { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SubjectResource> SubjectResources { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Aggregates.KnowledgeStructure.KnowledgeStructure>(entity =>
        {
            entity.ToTable("KnowledgeStructures", "knowledgestructure");

            entity.HasIndex(e => e.IsActive, "IX_KnowledgeStructures_IsActive");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Configure backing field for KnowledgeStructureModules collection
            entity.Navigation(e => e.KnowledgeStructureModules)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_knowledgeStructureModules");
        });

        modelBuilder.Entity<KnowledgeStructureModule>(entity =>
        {
            entity.ToTable("KnowledgeStructureModules", "knowledgestructure");

            entity.HasIndex(e => e.ModuleId, "IX_KnowledgeStructureModules_Module");

            entity.HasIndex(e => e.KnowledgeStructureId, "IX_KnowledgeStructureModules_Structure");

            entity.HasOne(d => d.KnowledgeStructure).WithMany(p => p.KnowledgeStructureModules)
                .HasForeignKey(d => d.KnowledgeStructureId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Module).WithMany(p => p.KnowledgeStructureModules)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Configure backing field for KnowledgeStructureTopics collection
            entity.Navigation(e => e.KnowledgeStructureTopics)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_knowledgeStructureTopics");
        });

        modelBuilder.Entity<KnowledgeStructureTopic>(entity =>
        {
            entity.ToTable("KnowledgeStructureTopics", "knowledgestructure");

            entity.HasIndex(e => e.KnowledgeStructureModuleId, "IX_KnowledgeStructureTopics_Module");

            entity.HasIndex(e => e.TopicId, "IX_KnowledgeStructureTopics_Topic");

            entity.HasOne(d => d.KnowledgeStructureModule).WithMany(p => p.KnowledgeStructureTopics)
                .HasForeignKey(d => d.KnowledgeStructureModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Topic).WithMany(p => p.KnowledgeStructureTopics)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Configure SubjectReference as owned type collection
            entity.OwnsMany(t => t.SubjectReferences, sr =>
            {
                sr.ToTable("KnowledgeStructureTopicSubjectReferences", "knowledgestructure");
                sr.WithOwner().HasForeignKey("KnowledgeStructureTopicId");
                sr.Property(s => s.SubjectId).HasColumnName("SubjectId");
                sr.Property(s => s.Order).HasColumnName("Order");
                sr.HasKey("KnowledgeStructureTopicId", "SubjectId");
            });

            // Configure backing field for SubjectReferences collection
            entity.Navigation(e => e.SubjectReferences)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_subjectReferences");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("Modules", "knowledgestructure");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Configure backing field for KnowledgeStructureModules collection
            entity.Navigation(e => e.KnowledgeStructureModules)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_knowledgeStructureModules");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subjects", "knowledgestructure");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);

            // Configure backing field for SubjectResources collection
            entity.Navigation(e => e.SubjectResources)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_subjectResources");
        });

        modelBuilder.Entity<SubjectResource>(entity =>
        {
            entity.ToTable("SubjectResources", "knowledgestructure");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(1000);

            // Configure the relationship and shadow property
            entity.Property<long>("SubjectId");

            entity.HasOne<Subject>()
                .WithMany("SubjectResources")
                .HasForeignKey("SubjectId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex("SubjectId").HasDatabaseName("IX_SubjectResources_SubjectId");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics", "knowledgestructure");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            // Configure backing field for KnowledgeStructureTopics collection
            entity.Navigation(e => e.KnowledgeStructureTopics)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_knowledgeStructureTopics");
        });
    }
}
