using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync
    (HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "An unhandled exception occurred.");

        int statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            OperationCanceledException when context.RequestAborted.IsCancellationRequested
                => 499,
            _ => StatusCodes.Status500InternalServerError
        };

        // The request was cancelled; don't attempt to write an error body.
        if (statusCode == 499)
        {
            return true;
        }

        string message = statusCode switch
        {
            400 => "The request is invalid.",
            404 => "The requested resource was not found.",
            409 => "The request conflicts with an existing resource or operation.",
            401 => "You are not authorized to perform this action.",
            _ => "An unexpected error occurred."
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = environment.IsDevelopment()
                ? exception.Message
                : null,
            Instance = context.Request.Path
        };

        await context.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}