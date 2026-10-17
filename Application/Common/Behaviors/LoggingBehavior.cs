using System.Diagnostics;
using System.Reflection;
using Application.Abstraction.Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly HashSet<string> SensitiveProperties
        = new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "PasswordHash",
            "Token",
            "AccessToken",
            "RefreshToken",
            "Secret",
            "ApiKey"
        };

    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var requestName = typeof(TRequest).Name;

        var requestProperties = GetRequestProperties(request);

        logger.LogInformation(
            "Logger: Handling request {RequestName}. Request data: {@RequestProperties}",
            requestName,
            requestProperties);

        var stopWatch = Stopwatch.StartNew();


        var response = await next();

        stopWatch.Stop();

        logger.LogInformation(
            "Logger: Request {RequestName} succeeded in {ElapsedMilliseconds}ms",
            requestName,
            stopWatch.ElapsedMilliseconds);

        return response;
    }

    private static Dictionary<string, object?> GetRequestProperties
        (TRequest request)
    {
        var properties = new Dictionary<string, object?>();
        foreach (PropertyInfo property in typeof(TRequest).GetProperties())
        {
            if (SensitiveProperties.Contains(property.Name))
            {
                continue;
            }

            properties[property.Name] = property.GetValue(request, null);
        }

        return properties;
    }
}