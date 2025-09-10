using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Infrastructure.Persistence.Entities;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.BusinessIncubator.Infrastructure.Persistence;

public class BusinessIncubatorDbContext(DbContextOptions<BusinessIncubatorDbContext> options, IMediator mediator) : SharedAbstractDbContext(options, mediator)
{
    public virtual DbSet<Domain.Aggregates.BusinessIncubator.BusinessIncubator> BusinessIncubators { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectAnswerOption> ProjectAnswerOptions { get; set; }

    public virtual DbSet<ProjectBlock> ProjectBlocks { get; set; }

    public virtual DbSet<ProjectKnowledgeStructure> ProjectKnowledgeStructures { get; set; }

    public virtual DbSet<ProjectModule> ProjectModules { get; set; }

    public virtual DbSet<ProjectQuestion> ProjectQuestions { get; set; }

    public virtual DbSet<ProjectSubject> ProjectSubjects { get; set; }

    public virtual DbSet<ProjectSubjectResource> ProjectSubjectResources { get; set; }

    public virtual DbSet<ProjectTopic> ProjectTopics { get; set; }

    public virtual DbSet<ProjectInvitation> ProjectInvitations { get; set; }

    public virtual DbSet<BatchUserRegistration> BatchUserRegistrations { get; set; }

    public virtual DbSet<ProjectFormSubmission> ProjectFormSubmissions { get; set; }

    public virtual DbSet<ProjectFormReview> ProjectFormReviews { get; set; }

    public virtual DbSet<ProjectFormFeedback> ProjectFormFeedback { get; set; }

    public virtual DbSet<StarterProgressEntity> StarterProgress { get; set; }

    public virtual DbSet<StarterTaskEntity> StarterTasks { get; set; }

    public virtual DbSet<ProjectUser> ProjectUsers { get; set; }

    public virtual DbSet<ReportTemplate> ReportTemplates { get; set; }

    public virtual DbSet<ReportSchedule> ReportSchedules { get; set; }

    public virtual DbSet<ProjectStage> ProjectStages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Aggregates.BusinessIncubator.BusinessIncubator>(entity =>
        {
            entity.ToTable("BusinessIncubators", "businessincubators");

            entity.HasIndex(e => e.ExternalId, "IX_BusinessIncubators_ExternalId").IsUnique();

            entity.HasIndex(e => e.Key, "IX_BusinessIncubators_Key_Unique_Active")
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.Name, "IX_BusinessIncubators_Name_Unique_Active")
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.RestoredBy).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .HasDefaultValue(BusinessIncubatorStatus.Active)
                .IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects", "businessincubators");

            entity.HasIndex(e => e.BusinessIncubatorId, "IX_Projects_BusinessIncubatorId");

            entity.HasIndex(e => e.ExternalId, "IX_Projects_ExternalId").IsUnique();

            entity.HasIndex(e => e.Name, "IX_Projects_Name");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.RestoredBy).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .HasDefaultValue(ProjectStatus.Active)
                .IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.SourceFormId)
                .HasColumnName("SourceFormId")
                .IsRequired(false);

            entity.HasOne("BusinessIncubator").WithMany("Projects")
                .HasForeignKey("BusinessIncubatorId")
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Configure backing fields for collections
            entity.Navigation(e => e.ProjectBlocks)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectBlocks");

            entity.Navigation(e => e.ProjectInvitations)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectInvitations");

            entity.Navigation(e => e.BatchUserRegistrations)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_batchUserRegistrations");

            entity.Navigation(e => e.FormSubmissions)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_formSubmissions");

            entity.Navigation(e => e.ProjectUsers)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectUsers");
        });

        modelBuilder.Entity<ProjectUser>(entity =>
        {
            entity.ToTable("ProjectUsers", "businessincubators");

            entity.HasIndex(e => new { e.ProjectId, e.UserId, e.Role }, "UQ_ProjectUsers_Project_User_Role")
                .IsUnique();

            entity.HasIndex(e => e.ProjectId, "IX_ProjectUsers_ProjectId");
            entity.HasIndex(e => e.UserId, "IX_ProjectUsers_UserId");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2");

            entity.Property(e => e.LeftAt)
                .HasColumnType("datetime2");

            entity.Property(e => e.InvitedBy)
                .HasMaxLength(450);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(450);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectAnswerOption>(entity =>
        {
            entity.ToTable("ProjectAnswerOptions", "businessincubators");

            entity.HasIndex(e => e.ProjectQuestionId, "IX_ProjectAnswerOptions_Question");

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

            entity.HasOne("ProjectQuestion").WithMany("ProjectAnswerOptions")
                .HasForeignKey("ProjectQuestionId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectBlock>(entity =>
        {
            entity.ToTable("ProjectBlocks", "businessincubators");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne("Project").WithMany("ProjectBlocks")
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure backing fields for collections
            entity.Navigation(e => e.ProjectQuestions)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectQuestions");
        });

        modelBuilder.Entity<ProjectKnowledgeStructure>(entity =>
        {
            entity.ToTable("ProjectKnowledgeStructures", "businessincubators");

            entity.HasIndex(e => e.ProjectId, "IX_ProjectKnowledgeStructures_ProjectId").IsUnique();

            entity.HasIndex(e => e.ProjectId, "IX_ProjectKnowledgeStructures_ProjectId").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.LockedReason).HasMaxLength(250);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne("Project").WithOne("ProjectKnowledgeStructure")
                .HasForeignKey("ProjectKnowledgeStructure", "ProjectId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure backing fields for collections
            entity.Navigation(e => e.ProjectModules)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectModules");
        });

        modelBuilder.Entity<ProjectModule>(entity =>
        {
            entity.ToTable("ProjectModules", "businessincubators");

            entity.HasIndex(e => e.ProjectKnowledgeStructureId, "IX_ProjectModules_KnowledgeStructure");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne("ProjectKnowledgeStructure").WithMany("ProjectModules")
                .HasForeignKey("ProjectKnowledgeStructureId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure backing fields for collections
            entity.Navigation(e => e.ProjectTopics)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectTopics");
        });

        modelBuilder.Entity<ProjectQuestion>(entity =>
        {
            entity.ToTable("ProjectQuestions", "businessincubators");

            entity.HasIndex(e => e.ProjectTopicId, "IX_ProjectQuestions_ByTopic");

            entity.Property(e => e.Text).IsRequired();

            // Explicitly configure enum conversions to prevent Int64 to Int32 casting issues
            entity.Property(e => e.AnswerType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.AppliesToPhase)
                .HasConversion<int>()
                .IsRequired();

            entity.HasOne(d => d.ProjectBlock)
                .WithMany(p => p.ProjectQuestions)
                .HasForeignKey(d => d.ProjectBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ProjectTopic)
                .WithMany(p => p.ProjectQuestions)
                .HasForeignKey(d => d.ProjectTopicId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });

        modelBuilder.Entity<ProjectSubject>(entity =>
        {
            entity.ToTable("ProjectSubjects", "businessincubators");

            entity.HasIndex(e => e.ProjectTopicId, "IX_ProjectSubjects_Topic");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.HasOne("ProjectTopic").WithMany("ProjectSubjects")
                .HasForeignKey("ProjectTopicId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany("ProjectAnswerOptions").WithMany("ProjectSubjects")
                .UsingEntity(
                    "ProjectSubjectAnswerOption",
                    l => l.HasOne(typeof(ProjectAnswerOption)).WithMany().HasForeignKey("ProjectAnswerOptionId").OnDelete(DeleteBehavior.NoAction),
                    r => r.HasOne(typeof(ProjectSubject)).WithMany().HasForeignKey("ProjectSubjectId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.ToTable("ProjectSubjectAnswerOptions", "businessincubators");
                        j.HasKey("ProjectSubjectId", "ProjectAnswerOptionId");
                        j.HasIndex("ProjectAnswerOptionId").HasDatabaseName("IX_ProjectSubjectAnswerOptions_AnswerOption");
                    });

            // Configure backing fields for collections
            entity.Navigation(e => e.ProjectSubjectResources)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectSubjectResources");

            entity.Navigation(e => e.ProjectAnswerOptions)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectAnswerOptions");
        });

        modelBuilder.Entity<ProjectSubjectResource>(entity =>
        {
            entity.ToTable("ProjectSubjectResources", "businessincubators");

            entity.HasIndex(e => e.ProjectSubjectId, "IX_ProjectSubjectResources_ProjectSubjectId");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasOne("ProjectSubject").WithMany("ProjectSubjectResources")
                .HasForeignKey("ProjectSubjectId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectTopic>(entity =>
        {
            entity.ToTable("ProjectTopics", "businessincubators");

            entity.HasIndex(e => e.ProjectModuleId, "IX_ProjectTopics_Module");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne("ProjectModule").WithMany("ProjectTopics")
                .HasForeignKey("ProjectModuleId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure backing fields for collections
            entity.Navigation(e => e.ProjectQuestions)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectQuestions");

            entity.Navigation(e => e.ProjectSubjects)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_projectSubjects");
        });

        modelBuilder.Entity<ProjectInvitation>(entity =>
        {
            entity.ToTable("ProjectInvitations", "businessincubators");

            entity.HasIndex(e => e.ExternalId, "UQ_ProjectInvitations_ExternalId").IsUnique();
            entity.HasIndex(e => e.InvitationToken, "UQ_ProjectInvitations_Token").IsUnique();
            entity.HasIndex(e => e.ProjectId, "IX_ProjectInvitations_ProjectId")
                .HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => new { e.Email, e.Status }, "IX_ProjectInvitations_Email_Status")
                .HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.ExpiresAt, "IX_ProjectInvitations_ExpiresAt")
                .HasFilter("[IsDeleted] = 0 AND [Status] = 0");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.IdentificationNumber)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.InvitationToken)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);
            entity.Property(e => e.DeletedBy).HasMaxLength(450);

            entity.HasOne("Project").WithMany("ProjectInvitations")
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BatchUserRegistration>(entity =>
        {
            entity.ToTable("BatchUserRegistrations", "businessincubators");

            entity.HasIndex(e => e.ExternalId, "UQ_BatchUserRegistrations_ExternalId").IsUnique();
            entity.HasIndex(e => new { e.ProjectId, e.Status }, "IX_BatchUserRegistrations_ProjectId_Status")
                .HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.CreatedAt, "IX_BatchUserRegistrations_CreatedAt")
                .HasFilter("[IsDeleted] = 0");

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);
            entity.Property(e => e.DeletedBy).HasMaxLength(450);

            entity.HasOne("Project").WithMany("BatchUserRegistrations")
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectFormSubmission>(entity =>
        {
            entity.ToTable("ProjectFormSubmissions", "businessincubators");

            entity.HasIndex(e => new { e.ProjectId, e.Status }, "IX_ProjectFormSubmissions_ProjectStatus");
            entity.HasIndex(e => e.ParticipantUserId, "IX_ProjectFormSubmissions_ParticipantUser");
            entity.HasIndex(e => e.ExternalId, "IX_ProjectFormSubmissions_ExternalId")
                .IsUnique();

            entity.Property(e => e.ExternalId)
                .IsRequired()
                .HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Phase)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.DraftData)
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500);

            entity.HasOne("Project").WithMany("FormSubmissions")
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectFormReview>(entity =>
        {
            entity.ToTable("ProjectFormReviews", "businessincubators");

            entity.HasIndex(e => e.ExternalId, "IX_ProjectFormReviews_ExternalId")
                .IsUnique();
            entity.HasIndex(e => e.SubmissionId, "IX_ProjectFormReviews_SubmissionId");
            entity.HasIndex(e => e.ReviewerId, "IX_ProjectFormReviews_ReviewerId");

            entity.Property(e => e.ExternalId)
                .IsRequired();
            entity.Property(e => e.ReviewerId)
                .IsRequired()
                .HasMaxLength(450);
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.GeneralComments)
                .HasMaxLength(2000);

            entity.HasOne(e => e.Submission)
                .WithMany()
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the navigation property with backing field
            entity.Navigation(e => e.FeedbackItems)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasField("_feedbackItems");
        });

        modelBuilder.Entity<ProjectFormFeedback>(entity =>
        {
            entity.ToTable("ProjectFormFeedback", "businessincubators");

            entity.HasIndex(e => e.ReviewId, "IX_ProjectFormFeedback_ReviewId");
            entity.HasIndex(e => e.BlockId, "IX_ProjectFormFeedback_BlockId");
            entity.HasIndex(e => e.QuestionId, "IX_ProjectFormFeedback_QuestionId");
            entity.HasIndex(e => e.ParentFeedbackId, "IX_ProjectFormFeedback_ParentFeedbackId");
            entity.HasIndex(e => new { e.Status, e.ReviewId }, "IX_ProjectFormFeedback_Status_ReviewId");

            entity.Property(e => e.ExternalId)
                .IsRequired();
            entity.Property(e => e.FeedbackText)
                .IsRequired()
                .HasMaxLength(2000);
            entity.Property(e => e.FeedbackType)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(FeedbackStatus.ReviewNeeded);
            entity.Property(e => e.IsFromParticipant)
                .IsRequired()
                .HasDefaultValue(false);
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            entity.HasOne(e => e.Review)
                .WithMany(r => r.FeedbackItems)
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentFeedback)
                .WithMany(e => e.Replies)
                .HasForeignKey(e => e.ParentFeedbackId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<StarterProgressEntity>(entity =>
        {
            entity.ToTable("StarterProgress", "businessincubators");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ProjectId }).IsUnique();
            entity.HasIndex(e => e.UserId).IncludeProperties(e => new { e.ProjectId, e.CurrentPhase, e.OverallProgress, e.LastActivityDate });
            entity.HasIndex(e => e.ProjectId).IncludeProperties(e => new { e.UserId, e.OverallProgress });

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.CurrentPhase).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NextMilestoneName).HasMaxLength(200);
            entity.Property(e => e.OverallProgress).HasPrecision(5, 2);
            entity.Property(e => e.PhaseProgress).HasPrecision(5, 2);
            entity.Property(e => e.EngagementScore).HasPrecision(5, 2);
            entity.Property(e => e.PerformanceScore).HasPrecision(5, 2);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StarterTaskEntity>(entity =>
        {
            entity.ToTable("StarterTasks", "businessincubators");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Status, e.DueDate }).IncludeProperties(e => new { e.ProjectId, e.Title, e.Type, e.Priority, e.ActionUrl });
            entity.HasIndex(e => new { e.ProjectId, e.Status }).IncludeProperties(e => new { e.UserId, e.Title, e.DueDate });

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Phase).HasMaxLength(50);
            entity.Property(e => e.CompletedBy).HasMaxLength(450);
            entity.Property(e => e.CancelledBy).HasMaxLength(450);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            entity.Property(e => e.ActionText).HasMaxLength(100);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
            entity.Property(e => e.RelatedEntityId).HasMaxLength(100);
            entity.Property(e => e.RecurrenceRule).HasMaxLength(500);
            entity.Property(e => e.AutoCompleteCondition).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(450);
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentTask)
                .WithMany(e => e.SubTasks)
                .HasForeignKey(e => e.ParentTaskId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ReportTemplate>(entity =>
        {
            entity.ToTable("ReportTemplates", "businessincubators");

            entity.HasIndex(e => e.ExternalId, "IX_ReportTemplates_ExternalId").IsUnique();
            entity.HasIndex(e => new { e.ProjectId, e.Name }, "IX_ReportTemplates_Project_Name");
            entity.HasIndex(e => new { e.Type, e.IsGlobal }, "IX_ReportTemplates_Type_Global");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Type)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.ConfigurationJson)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Schedules)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportSchedule>(entity =>
        {
            entity.ToTable("ReportSchedules", "businessincubators");

            entity.HasIndex(e => e.ExternalId, "IX_ReportSchedules_ExternalId").IsUnique();
            entity.HasIndex(e => new { e.TemplateId, e.IsActive }, "IX_ReportSchedules_Template_Active");
            entity.HasIndex(e => new { e.IsActive, e.NextRunAt }, "IX_ReportSchedules_Active_NextRun");

            entity.Property(e => e.CronExpression)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Recipients)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<ProjectStage>(entity =>
        {
            entity.ToTable("ProjectStages", "businessincubators");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .UseIdentityColumn();

            entity.Property(e => e.ProjectId)
                .IsRequired();

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.EndDate)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            // Audit properties
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedAt);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.ProjectId)
                .HasDatabaseName("IX_ProjectStages_ProjectId");

            entity.HasIndex(e => e.Type)
                .HasDatabaseName("IX_ProjectStages_Type");

            entity.HasIndex(e => new { e.StartDate, e.EndDate })
                .HasDatabaseName("IX_ProjectStages_Dates");

            // Unique constraint
            entity.HasIndex(e => new { e.ProjectId, e.Type })
                .IsUnique()
                .HasDatabaseName("UQ_ProjectStages_ProjectId_Type");

            // Relationship
            entity.HasOne<Project>("Project")
                .WithMany("_projectStages")
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Project entity to ignore the public ProjectStages property
        modelBuilder.Entity<Project>()
            .Ignore(p => p.ProjectStages);

        modelBuilder.Entity<Domain.Aggregates.BusinessIncubator.BusinessIncubator>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<ProjectInvitation>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<BatchUserRegistration>().HasQueryFilter(f => !f.IsDeleted);
    }
}
