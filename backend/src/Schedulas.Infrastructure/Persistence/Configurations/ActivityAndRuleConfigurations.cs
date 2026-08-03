using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedulas.Domain.Entities;

namespace Schedulas.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> b)
    {
        b.ToTable("activities");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ClassId).HasColumnName("class_id").IsRequired();
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired(); // denormalized, Database Design §6
        b.Property(x => x.ActivityType).HasColumnName("activity_type").HasConversion<string>().IsRequired();
        b.Property(x => x.Title).HasColumnName("title").IsRequired();
        b.Property(x => x.Description).HasColumnName("description");
        b.Property(x => x.ScheduledDate).HasColumnName("scheduled_date").HasColumnType("date").IsRequired();
        b.Property(x => x.ScheduledTime).HasColumnName("scheduled_time").HasColumnType("time");
        b.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time");
        b.Property(x => x.Duration).HasColumnName("duration");
        b.Property(x => x.Priority).HasColumnName("priority").IsRequired().HasDefaultValue(1);
        b.Property(x => x.EstimatedWeight).HasColumnName("estimated_weight").HasColumnType("numeric(5,2)");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        b.HasIndex(x => x.ClassId).HasDatabaseName("ix_activities_class_id");
        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_activities_institution_id");
        b.HasIndex(x => x.ScheduledDate).HasDatabaseName("ix_activities_scheduled_date");
        b.HasIndex(x => x.ActivityType).HasDatabaseName("ix_activities_activity_type");

        b.HasOne<Class>().WithMany().HasForeignKey(x => x.ClassId);

        AuditColumns.Apply(b);
    }
}

public class RuleDefinitionConfiguration : IEntityTypeConfiguration<RuleDefinition>
{
    public void Configure(EntityTypeBuilder<RuleDefinition> b)
    {
        b.ToTable("rule_definitions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.ScopeLevel).HasColumnName("scope_level").HasConversion<string>().IsRequired();
        b.Property(x => x.ScopeId).HasColumnName("scope_id").IsRequired();
        b.Property(x => x.RuleType).HasColumnName("rule_type").HasConversion<string>().IsRequired();
        b.Property(x => x.ParametersJson).HasColumnName("parameters").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Priority).HasColumnName("priority").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active");

        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_rule_definitions_institution_id");
        b.HasIndex(x => new { x.ScopeLevel, x.ScopeId }).HasDatabaseName("ix_rule_definitions_scope");
        b.HasIndex(x => x.RuleType).HasDatabaseName("ix_rule_definitions_rule_type");

        AuditColumns.Apply(b);
    }
}

public class RuleEvaluationLogConfiguration : IEntityTypeConfiguration<RuleEvaluationLog>
{
    public void Configure(EntityTypeBuilder<RuleEvaluationLog> b)
    {
        b.ToTable("rule_evaluation_logs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ActivityId).HasColumnName("activity_id");
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.Outcome).HasColumnName("outcome").HasConversion<string>().IsRequired();
        b.Property(x => x.TriggeredRuleId).HasColumnName("triggered_rule_id");
        b.Property(x => x.ReasonCode).HasColumnName("reason_code");
        b.Property(x => x.OverrideNote).HasColumnName("override_note");
        b.Property(x => x.EvaluatedBy).HasColumnName("evaluated_by");
        b.Property(x => x.EvaluatedAt).HasColumnName("evaluated_at").IsRequired();

        b.HasIndex(x => x.ActivityId).HasDatabaseName("ix_rule_evaluation_logs_activity_id");
        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_rule_evaluation_logs_institution_id");
        b.HasIndex(x => x.EvaluatedAt).HasDatabaseName("ix_rule_evaluation_logs_evaluated_at");

        b.HasOne<Activity>().WithMany().HasForeignKey(x => x.ActivityId);
        b.HasOne<RuleDefinition>().WithMany().HasForeignKey(x => x.TriggeredRuleId);

        // No CreatedAt/UpdatedAt/DeletedAt/CreatedBy/UpdatedBy here by
        // design -- this is an ImmutableRecordEntity, not a BaseEntity;
        // EvaluatedAt + EvaluatedBy above are its only timestamp/actor
        // fields, and there is nothing to ignore because nothing else
        // exists on the CLR type (see Domain/Entities/RuleDefinition.cs
        // and Domain/Common/BaseEntity.cs's ImmutableRecordEntity).
    }
}
