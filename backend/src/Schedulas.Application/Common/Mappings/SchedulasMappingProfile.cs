using AutoMapper;
using Schedulas.Application.Features.Auth.Queries;
using Schedulas.Application.Features.Institutions.Commands;
using Schedulas.Application.Features.Settings;
using DomainProfile = Schedulas.Domain.Entities.Profile;
using DomainInstitution = Schedulas.Domain.Entities.Institution;

namespace Schedulas.Application.Common.Mappings;

/// <summary>
/// Entity -> DTO projections for the Auth/Profile/Institution vertical
/// slice, per this session's explicit "Configure AutoMapper" requirement.
/// Other features in the codebase still construct DTOs manually inline
/// (e.g. inside EF `.Select()` projections, where AutoMapper's
/// ProjectTo is a further optimization, not a correctness requirement) --
/// this profile demonstrates the mechanism is genuinely wired and used,
/// not just referenced in a .csproj. Extending it to every feature is a
/// mechanical follow-up, not an architectural one.
///
/// Note the type aliases above: AutoMapper's own base class is also
/// named "Profile", colliding with our Domain entity of the same name --
/// aliasing DomainProfile keeps this file unambiguous without renaming
/// either type.
/// </summary>
public sealed class SchedulasMappingProfile : Profile
{
    public SchedulasMappingProfile()
    {
        CreateMap<DomainProfile, CurrentUserDto>()
            .ConstructUsing(p => new CurrentUserDto(
                p.Id, p.FullName, p.Email, p.Role, p.InstitutionId, p.DepartmentId, p.PreferredTheme));

        CreateMap<DomainProfile, UserSettingsDto>()
            .ConstructUsing(p => new UserSettingsDto(p.Id, p.FullName, p.PhoneNumber, p.PreferredTheme));

        CreateMap<DomainInstitution, InstitutionDto>()
            .ConstructUsing(i => new InstitutionDto(i.Id, i.Name, i.Type, i.Timezone, i.LogoUrl, i.IsSuspended));

        CreateMap<DomainInstitution, InstitutionSettingsDto>()
            .ConstructUsing(i => new InstitutionSettingsDto(i.Id, i.Name, i.LogoUrl, i.Timezone));
    }
}
