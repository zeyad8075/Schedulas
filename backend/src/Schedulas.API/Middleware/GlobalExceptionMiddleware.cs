using System.Net;
using FluentValidation;
using Schedulas.API.Common;
using Schedulas.Domain.Exceptions;
using Schedulas.Infrastructure.Identity;

namespace Schedulas.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, response) = Map(exception);

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Path}", context.Request.Path);
        else
            _logger.LogWarning(exception, "Handled exception ({Status}) processing {Path}", status, context.Request.Path);

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    private static (HttpStatusCode Status, ApiResponse<object>) Map(Exception exception) => exception switch
    {
        ValidationException validationEx => (
            HttpStatusCode.BadRequest,
            ApiResponse<object>.Fail(
                ArabicMessages.Resolve("VALIDATION_FAILED"),
                validationEx.Errors.Select(e => new FieldError(e.PropertyName, e.ErrorMessage)).ToList())),

        RuleViolationException ruleEx => (
            HttpStatusCode.UnprocessableEntity,
            ApiResponse<object>.Fail(ArabicMessages.Resolve(ruleEx.ReasonCode))),

        InvalidStateTransitionException stateEx => (
            HttpStatusCode.UnprocessableEntity,
            ApiResponse<object>.Fail(ArabicMessages.Resolve(stateEx.ReasonCode))),

        EntityNotFoundException => (
            HttpStatusCode.NotFound,
            ApiResponse<object>.Fail(ArabicMessages.Resolve("ENTITY_NOT_FOUND"))),

        UnauthorizedAccessException unauthorizedEx => (
            HttpStatusCode.Forbidden,
            ApiResponse<object>.Fail(ArabicMessages.Resolve(unauthorizedEx.Message))),

        SupabaseAuthException supabaseEx => (
            HttpStatusCode.BadRequest,
            ApiResponse<object>.Fail(supabaseEx.Message)),

        _ => (
            HttpStatusCode.InternalServerError,
            ApiResponse<object>.Fail($"DEBUG_ERROR: {exception.Message} \n {exception.StackTrace}"))
    };
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionMiddleware>();
}
