using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Shipping.Api.Middleware;

public class ProblemDetailsExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsExceptionMiddleware> _logger;

    public ProblemDetailsExceptionMiddleware(RequestDelegate next, ILogger<ProblemDetailsExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred during request processing.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            ValidationException valEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation failures occurred.",
                Type = "https://example.com/errors/validation-error",
                Extensions = { ["errors"] = valEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) }
            },
            KeyNotFoundException notFoundEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Resource Not Found",
                Detail = notFoundEx.Message,
                Type = "https://example.com/errors/not-found"
            },
            InvalidOperationException invEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Invalid Domain Operation",
                Detail = invEx.Message,
                Type = "https://example.com/errors/invalid-operation"
            },
            ArgumentException argEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Invalid Argument",
                Detail = argEx.Message,
                Type = "https://example.com/errors/invalid-argument"
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = $"An unexpected error occurred on the server. {exception.Message}",
                Type = "https://example.com/errors/internal-server-error"
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
