using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.OrgHierarchy.Commands;
using Schedulas.Application.Features.OrgHierarchy.Queries;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/classes")]
[Authorize(Roles = "InstitutionAdmin,DepartmentAdmin")]
public sealed class ClassesController : ControllerBase
{
    private readonly ISender _mediator;
    public ClassesController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// The classes the AUTHENTICATED caller teaches — resolved entirely
    /// from the JWT via GetMyTaughtClassesQuery, never from a client-
    /// supplied id. Deliberately overrides this controller's class-level
    /// InstitutionAdmin/DepartmentAdmin restriction with a narrower
    /// Teacher-only one: this route answers "what are MY classes," which
    /// only makes sense for a Teacher caller, not an admin.
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyTaughtClassesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<MyClassDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Create), new { id = result.Id },
            ApiResponse<ClassDto>.Ok(result, "تم إنشاء الفصل بنجاح"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateClassCommand body, CancellationToken ct)
    {
        var command = body with { ClassId = id };
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<ClassDto>.Ok(result, "تم تحديث الفصل بنجاح"));
    }

    public sealed record StudentIdBody(Guid StudentId);
    public sealed record TeacherIdBody(Guid TeacherId);

    [HttpGet("{id:guid}/students")]
    public async Task<IActionResult> GetClassStudents(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClassStudentsQuery(id), ct);
        return Ok(ApiResponse<IReadOnlyList<Schedulas.Application.Features.People.Commands.StudentDto>>.Ok(result));
    }

    [HttpGet("{id:guid}/teachers")]
    public async Task<IActionResult> GetClassTeachers(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClassTeachersQuery(id), ct);
        return Ok(ApiResponse<IReadOnlyList<Schedulas.Application.Features.People.Commands.TeacherDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/students")]
    public async Task<IActionResult> EnrollStudent(Guid id, StudentIdBody body, CancellationToken ct)
    {
        await _mediator.Send(new EnrollStudentCommand(id, body.StudentId), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تسجيل الطالب في الفصل"));
    }

    [HttpDelete("{id:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> UnenrollStudent(Guid id, Guid studentId, CancellationToken ct)
    {
        await _mediator.Send(new UnenrollStudentCommand(id, studentId), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إلغاء تسجيل الطالب من الفصل"));
    }

    [HttpPost("{id:guid}/teachers")]
    public async Task<IActionResult> AssignTeacher(Guid id, TeacherIdBody body, CancellationToken ct)
    {
        await _mediator.Send(new AssignTeacherCommand(id, body.TeacherId), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تعيين المعلم للفصل"));
    }

    [HttpDelete("{id:guid}/teachers/{teacherId:guid}")]
    public async Task<IActionResult> UnassignTeacher(Guid id, Guid teacherId, CancellationToken ct)
    {
        await _mediator.Send(new UnassignTeacherCommand(id, teacherId), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إلغاء تعيين المعلم من الفصل"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id), ct);
        return Ok(ApiResponse<ClassDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteClassCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف الفصل بنجاح"));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreClassCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة الفصل بنجاح"));
    }
}
