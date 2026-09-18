using System.Net;
using System.Text.Json;
using AppointmentScheduler.Application.Exceptions;
using AppointmentScheduler.Domain.Exceptions;

namespace AppointmentScheduler.Api.Middleware;

/// <summary>
/// Translates exceptions raised by the domain/application layers into the
/// right HTTP status codes, so controllers can stay free of try/catch
/// boilerplate:
///   - NotFoundException            -> 404
///   - SchedulingConflictException  -> 409 (a DomainException, checked first)
///   - DomainException              -> 400
///   - anything else                -> 500
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                NotFoundException => (HttpStatusCode.NotFound, "Not Found"),
                SchedulingConflictException => (HttpStatusCode.Conflict, "Scheduling Conflict"),
                DomainException => (HttpStatusCode.BadRequest, "Invalid Request"),
                _ => (HttpStatusCode.InternalServerError, "Unexpected Error")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogInformation("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problem = new
            {
                type = $"https://httpstatuses.io/{(int)statusCode}",
                title,
                status = (int)statusCode,
                detail = exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
