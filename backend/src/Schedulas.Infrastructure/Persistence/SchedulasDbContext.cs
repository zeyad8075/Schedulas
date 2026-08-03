using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Common;
using Schedulas.Domain.Entities;
using Program = Schedulas.Domain.Entities.Program; // disambiguate from top-level Program.cs

namespace Schedulas.Infrastructure.Persistence;

public class SchedulasDbContext : DbContext, IApplicationDbContext, IApplicationDbContextEventSource
{
    private readonly ITenantService _tenantService;
    private readonly DbContextOptions _options;

    public SchedulasDbContext(DbContextOptions<SchedulasDbContext> options, ITenantService tenantService) : base(options)
    {
        _options = options;
        _tenantService = tenantService;
    }

    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Program> Programs => Set<Program>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Class> Classes => Set<Class>();

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<ParentStudentLink> ParentStudentLinks => Set<ParentStudentLink>();
    public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();
    public DbSet<ClassTeacher> ClassTeachers => Set<ClassTeacher>();

    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<RuleDefinition> RuleDefinitions => Set<RuleDefinition>();
    public DbSet<RuleEvaluationLog> RuleEvaluationLogs => Set<RuleEvaluationLog>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Schedulas.Domain.Common.DomainEvent>();
        
        // Applies every IEntityTypeConfiguration<T> in Persistence/Configurations
        // (one file per entity, per Constitution §7 folder-structure rule).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulasDbContext).Assembly);

        // Apply global query filters (Soft Delete & Multi-Tenancy) dynamically
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var method = typeof(SchedulasDbContext).GetMethod(nameof(ApplyFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);
                
            method.Invoke(this, new object[] { modelBuilder });
        }

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Dispatches buffered domain events after a successful commit. Called
    /// by a MediatR pipeline behavior in the Application layer (built in a
    /// later pass) — kept here as the mechanism for collecting them.
    /// </summary>
    public IReadOnlyList<Domain.Common.DomainEvent> CollectAndClearDomainEvents()
    {
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var events = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        return events;
    }

    private void ApplyFilter<T>(ModelBuilder builder) where T : class
    {
        bool isSoftDeletable = typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(T));
        bool isMustHaveTenant = typeof(IMustHaveTenant).IsAssignableFrom(typeof(T));
        bool isMayHaveTenant = typeof(IMayHaveTenant).IsAssignableFrom(typeof(T));

        if (!isSoftDeletable && !isMustHaveTenant && !isMayHaveTenant)
            return;

        builder.Entity<T>().HasQueryFilter(e => 
            (!isSoftDeletable || ((ISoftDeletableEntity)e).DeletedAt == null) &&
            (!isMustHaveTenant || !_tenantService.IsTenantEnforced || ((IMustHaveTenant)e).InstitutionId == _tenantService.TenantId) &&
            (!isMayHaveTenant || !_tenantService.IsTenantEnforced || ((IMayHaveTenant)e).InstitutionId == _tenantService.TenantId)
        );
    }
}
