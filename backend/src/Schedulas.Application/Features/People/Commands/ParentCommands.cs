using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.People.Commands;

public sealed record ParentDto(Guid Id, Guid ProfileId, string FullName, string Email, bool IsActive);

public sealed record CreateParentCommand(Guid ProfileId) : IRequest<ParentDto>;

public sealed class CreateParentCommandValidator : AbstractValidator<CreateParentCommand>
{
    public CreateParentCommandValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
    }
}

public sealed class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, ParentDto>
{
    private readonly IApplicationDbContext _db;

    public CreateParentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ParentDto> Handle(CreateParentCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([request.ProfileId], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", request.ProfileId);

        if (profile.Role != UserRole.Parent)
            throw new InvalidStateTransitionException("INVALID_ROLE", "Profile does not have the Parent role.");

        var existing = await _db.Parents.FirstOrDefaultAsync(p => p.ProfileId == request.ProfileId, cancellationToken);
        if (existing != null)
            throw new InvalidStateTransitionException("ALREADY_EXISTS", "A Parent record already exists for this Profile.");

        var parent = new Parent(request.ProfileId);
        _db.Parents.Add(parent);
        await _db.SaveChangesAsync(cancellationToken);
        
        return new ParentDto(parent.Id, parent.ProfileId, profile.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record DeleteParentCommand(Guid ParentId) : IRequest<Unit>;

public sealed class DeleteParentCommandHandler : IRequestHandler<DeleteParentCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteParentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteParentCommand request, CancellationToken cancellationToken)
    {
        var parent = await _db.Parents.FindAsync([request.ParentId], cancellationToken)
            ?? throw new EntityNotFoundException("Parent", request.ParentId);

        _db.Parents.Remove(parent);
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}

public sealed record RestoreParentCommand(Guid ParentId) : IRequest<Unit>;

public sealed class RestoreParentCommandHandler : IRequestHandler<RestoreParentCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public RestoreParentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(RestoreParentCommand request, CancellationToken cancellationToken)
    {
        var parent = await _db.Parents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.ParentId, cancellationToken)
            ?? throw new EntityNotFoundException("Parent", request.ParentId);

        var profile = await _db.Profiles.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == parent.ProfileId, cancellationToken);
        if (profile == null || profile.DeletedAt != null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore parent because the underlying profile is deleted.");

        parent.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
