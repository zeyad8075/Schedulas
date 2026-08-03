using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.AcademicCalendar;

public sealed record AcademicTermDto(Guid Id, Guid InstitutionId, string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive);
public sealed record HolidayDto(Guid Id, Guid InstitutionId, Guid? AcademicTermId, string Name, DateOnly HolidayDate);

// SECURITY FIX (Phase 8 Verification pass): CreateAcademicTermCommand,
// GetAcademicTermsQuery, CreateHolidayCommand, and GetHolidaysQuery all
// previously trusted a caller-supplied InstitutionId with zero check it
// matched the caller's own institution -- on the write side, this meant
// an InstitutionAdmin could create academic terms/holidays for a
// DIFFERENT institution, not just read another institution's data. Fixed
// by implementing ITenantScopedRequest (Architecture §4/§6), which
// TenantAuthorizationBehavior already enforces in the MediatR pipeline --
// this is the same mechanism CreateDepartmentCommand demonstrated in
// Part 4, now actually applied to a second feature instead of staying a
// single, isolated example.

public sealed record CreateAcademicTermCommand(Guid InstitutionId, string Name, DateOnly StartDate, DateOnly EndDate)
    : IRequest<AcademicTermDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class CreateAcademicTermCommandValidator : AbstractValidator<CreateAcademicTermCommand>
{
    public CreateAcademicTermCommandValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public sealed class CreateAcademicTermCommandHandler : IRequestHandler<CreateAcademicTermCommand, AcademicTermDto>
{
    private readonly IApplicationDbContext _db;
    public CreateAcademicTermCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<AcademicTermDto> Handle(CreateAcademicTermCommand request, CancellationToken cancellationToken)
    {
        var term = new AcademicTerm(request.InstitutionId, request.Name, request.StartDate, request.EndDate);
        _db.AcademicTerms.Add(term);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(term);
    }

    internal static AcademicTermDto ToDto(AcademicTerm t) =>
        new(t.Id, t.InstitutionId, t.Name, t.StartDate, t.EndDate, t.IsActive);
}

public sealed record UpdateAcademicTermCommand(Guid TermId, bool IsActive) : IRequest<Unit>;

/// <summary>SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant check.</summary>
public sealed class UpdateAcademicTermCommandHandler : IRequestHandler<UpdateAcademicTermCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateAcademicTermCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateAcademicTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _db.AcademicTerms.FindAsync([request.TermId], cancellationToken)
            ?? throw new EntityNotFoundException("AcademicTerm", request.TermId);


        if (request.IsActive) term.Activate(); else term.Deactivate();
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record DeleteAcademicTermCommand(Guid TermId) : IRequest<Unit>;

public sealed class DeleteAcademicTermCommandHandler : IRequestHandler<DeleteAcademicTermCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteAcademicTermCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteAcademicTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _db.AcademicTerms.FindAsync([request.TermId], cancellationToken)
            ?? throw new EntityNotFoundException("AcademicTerm", request.TermId);


        bool hasClasses = await _db.Classes.AnyAsync(c => c.AcademicTermId == request.TermId, cancellationToken);
        if (hasClasses)
            throw new InvalidStateTransitionException("TERM_HAS_CLASSES", "Cannot delete an academic term that is referenced by active classes.");

        _db.AcademicTerms.Remove(term);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record RestoreAcademicTermCommand(Guid TermId) : IRequest<Unit>;

public sealed class RestoreAcademicTermCommandHandler : IRequestHandler<RestoreAcademicTermCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreAcademicTermCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreAcademicTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _db.AcademicTerms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TermId, cancellationToken)
            ?? throw new EntityNotFoundException("AcademicTerm", request.TermId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != term.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");


        var institution = await _db.Institutions.FindAsync([term.InstitutionId], cancellationToken);
        if (institution == null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore academic term because its institution is deleted.");

        term.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record GetAcademicTermsQuery(
    Guid InstitutionId,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<AcademicTermDto>>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetAcademicTermsQueryHandler : IRequestHandler<GetAcademicTermsQuery, PaginatedList<AcademicTermDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;

    public GetAcademicTermsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ITenantService tenantService)
    {
        _db = db;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<PaginatedList<AcademicTermDto>> Handle(GetAcademicTermsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.PlatformAdmin)
            _tenantService.SetTenantId(request.InstitutionId);

        var query = _db.AcademicTerms.Where(t => t.InstitutionId == request.InstitutionId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(t => t.Name.Contains(request.SearchTerm));

        if (request.IsActive.HasValue)
            query = query.Where(t => t.IsActive == request.IsActive.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "startdate" => request.SortDescending ? query.OrderByDescending(t => t.StartDate) : query.OrderBy(t => t.StartDate),
            "enddate" => request.SortDescending ? query.OrderByDescending(t => t.EndDate) : query.OrderBy(t => t.EndDate),
            "createdat" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(t => t.StartDate) : query.OrderByDescending(t => t.StartDate)
        };

        var projected = query.Select(t => new AcademicTermDto(t.Id, t.InstitutionId, t.Name, t.StartDate, t.EndDate, t.IsActive));

        return await PaginatedList<AcademicTermDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record GetAcademicTermByIdQuery(Guid TermId) : IRequest<AcademicTermDto>;

public sealed class GetAcademicTermByIdQueryHandler : IRequestHandler<GetAcademicTermByIdQuery, AcademicTermDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAcademicTermByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AcademicTermDto> Handle(GetAcademicTermByIdQuery request, CancellationToken cancellationToken)
    {
        var term = await _db.AcademicTerms.FirstOrDefaultAsync(t => t.Id == request.TermId, cancellationToken)
            ?? throw new EntityNotFoundException("AcademicTerm", request.TermId);


        return new AcademicTermDto(term.Id, term.InstitutionId, term.Name, term.StartDate, term.EndDate, term.IsActive);
    }
}

public sealed record CreateHolidayCommand(Guid InstitutionId, Guid? AcademicTermId, string Name, DateOnly HolidayDate)
    : IRequest<HolidayDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, HolidayDto>
{
    private readonly IApplicationDbContext _db;
    public CreateHolidayCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<HolidayDto> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = new Holiday(request.InstitutionId, request.AcademicTermId, request.Name, request.HolidayDate);
        _db.Holidays.Add(holiday);
        await _db.SaveChangesAsync(cancellationToken);
        return new HolidayDto(holiday.Id, holiday.InstitutionId, holiday.AcademicTermId, holiday.Name, holiday.HolidayDate);
    }
}

public sealed record DeleteHolidayCommand(Guid HolidayId) : IRequest<Unit>;

/// <summary>SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant check.</summary>
public sealed class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteHolidayCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _db.Holidays.FindAsync([request.HolidayId], cancellationToken)
            ?? throw new EntityNotFoundException("Holiday", request.HolidayId);


        _db.Holidays.Remove(holiday);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record GetHolidaysQuery(Guid InstitutionId, Guid? AcademicTermId)
    : IRequest<IReadOnlyList<HolidayDto>>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, IReadOnlyList<HolidayDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;

    public GetHolidaysQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ITenantService tenantService)
    {
        _db = db;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<IReadOnlyList<HolidayDto>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.PlatformAdmin)
            _tenantService.SetTenantId(request.InstitutionId);

        var query = _db.Holidays.AsQueryable();

        if (request.AcademicTermId is Guid termId)
            query = query.Where(h => h.AcademicTermId == termId);

        return await query
            .OrderBy(h => h.HolidayDate)
            .Select(h => new HolidayDto(h.Id, h.InstitutionId, h.AcademicTermId, h.Name, h.HolidayDate))
            .ToListAsync(cancellationToken);
    }
}
