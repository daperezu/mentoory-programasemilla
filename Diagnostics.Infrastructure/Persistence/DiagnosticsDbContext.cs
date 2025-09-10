using LinaSys.Diagnostics.Domain.Aggregates.Block;
using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Diagnostics.Infrastructure.Persistence;

public class DiagnosticsDbContext(DbContextOptions<DiagnosticsDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Block> Blocks { get; set; }

    // Removed old DiagnosisAnswers DbSet - now using aggregate's DiagnosisAnswer
    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<FormQuestion> FormQuestions { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    // Note: UserProjectDiagnosis is not exposed as a DbSet since it's an aggregate root
    // It will be accessed through the repository pattern only
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.ToTable("AnswerOptions", "diagnostics");

            entity.Property(e => e.Foda)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasConversion(Converters.FodaTypeConverter)
                .IsFixedLength();
            entity.Property(e => e.FodaExplanation).IsRequired();
            entity.Property(e => e.Odsr)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasConversion(Converters.OdsrTypeConverter)
                .IsFixedLength();
            entity.Property(e => e.OdsrExplanation).IsRequired();
            entity.Property(e => e.Text).IsRequired();

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Block>(entity =>
        {
            entity.ToTable("Blocks", "diagnostics");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
        });

        // Removed old Domain.Entities.DiagnosisAnswer configuration - now using aggregate's DiagnosisAnswer
        modelBuilder.Entity<Form>(entity =>
        {
            entity.ToTable("Forms", "diagnostics");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Configure the backing field for FormQuestions collection
            entity.Navigation(e => e.FormQuestions)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_formQuestions");
        });

        modelBuilder.Entity<FormQuestion>(entity =>
        {
            entity.HasKey(e => new { e.FormId, e.QuestionId });

            entity.ToTable("FormQuestions", "diagnostics");

            entity.HasOne(d => d.Block).WithMany("FormQuestions")
                .HasForeignKey(d => d.BlockId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Form).WithMany(p => p.FormQuestions)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Question).WithMany("FormQuestions")
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions", "diagnostics");

            entity.Property(e => e.Text).IsRequired();

            // Configure the backing field for AnswerOptions collection
            entity.Navigation(e => e.AnswerOptions)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_answerOptions");
        });

        // UserProjectDiagnosis aggregate configuration
        modelBuilder.Entity<UserProjectDiagnosis>(entity =>
        {
            entity.ToTable("UserProjectDiagnoses", "diagnostics");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.ProjectId, x.UserId })
                .IsUnique();

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            // Configure owned collection of phase summaries
            entity.OwnsMany(x => x.PhaseSummaries, summaries =>
            {
                summaries.ToTable("DiagnosisPhaseSummaries", "diagnostics");
                summaries.Property<long>("Id");  // Explicitly configure Id as long to match database
                summaries.HasKey("Id");
                summaries.Property(s => s.Phase).HasConversion<int>();
                summaries.Property(s => s.CompletedAt).IsRequired();
                summaries.Property(s => s.AnswerCount).IsRequired();
            });

            // Configure the backing field for answers collection
            entity.Navigation(e => e.Answers)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_answers");
        });

        // Configure DiagnosisAnswer as part of UserProjectDiagnosis aggregate
        modelBuilder.Entity<DiagnosisAnswer>(entity =>
        {
            // This entity will have its own table but is part of the aggregate
            entity.ToTable("DiagnosisAnswers", "diagnostics");

            entity.HasKey(e => e.Id);

            // Add foreign key to UserProjectDiagnosis (for migration phase)
            entity.Property<long?>("UserProjectDiagnosisId");

            entity.HasOne<UserProjectDiagnosis>()
                .WithMany(e => e.Answers)
                .HasForeignKey("UserProjectDiagnosisId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure all properties
            entity.Property(e => e.ProjectId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.BlockId).IsRequired();
            entity.Property(e => e.BlockName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.QuestionId).IsRequired();
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.AnswerOptionId).IsRequired();
            entity.Property(e => e.AnswerOptionText).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.Phase).HasConversion<int>().IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.SubmittedAt).IsRequired();

            entity.Property(e => e.TopicName).HasMaxLength(200);
            entity.Property(e => e.ModuleName).HasMaxLength(200);

            entity.Property(e => e.Foda)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasConversion(Converters.FodaTypeConverter)
                .IsFixedLength();

            entity.Property(e => e.FodaExplanation).IsRequired();

            entity.Property(e => e.Odsr)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasConversion(Converters.OdsrTypeConverter)
                .IsFixedLength();

            entity.Property(e => e.OdsrExplanation).IsRequired();

            // Create indexes for performance
            entity.HasIndex(e => new { e.ProjectId, e.UserId });
            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => e.Phase);
        });
    }
}
