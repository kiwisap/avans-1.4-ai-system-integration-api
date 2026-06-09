using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace avans_1_4_ai_system_integration_api.Exceptions;

public sealed class TrashDetectionExceptionHandler(
    ILogger<TrashDetectionExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case NotFoundException:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource not found";
                problemDetails.Detail = exception.Message;
                break;

            case BadRequestException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad request";
                problemDetails.Detail = exception.Message;
                break;

            case UnauthorizedException:
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = exception.Message;
                break;

            case ValidationException validationException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails = new ValidationProblemDetails(validationException.Errors)
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = httpContext.Request.Path
                };
                break;

            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Server error";
                problemDetails.Detail = "Er is een onverwachte fout opgetreden.";
                break;
        }

        problemDetails.Status = httpContext.Response.StatusCode;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}