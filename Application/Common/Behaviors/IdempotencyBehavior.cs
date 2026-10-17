using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstraction.Mediator;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Common.Behaviors;

internal sealed class IdempotencyBehavior<TRequest, TResponse>(
    IIdempotencyRepository idempotencyRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly JsonSerializerOptions JsonOptions
        = new(JsonSerializerOptions.Web);

    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (request is not IIdempotentRequest idempotentRequest)
        {
            return await next();
        }

        var key = idempotentRequest.IdempotencyKey;

        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        var requestHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(requestJson)));

        var existing = await idempotencyRepository.GetByIdAsync(key, cancellationToken);

        if (existing is not null)
        {
            if (existing.ExpiresAt <= DateTime.UtcNow)
            {
                idempotencyRepository.Delete(existing);
            }
            else
            {
                if (!string.Equals
                        (existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "This Idempotency-Key has already been used for a different request.");
                }

                var cachedResponse =
                    JsonSerializer.Deserialize<TResponse>(
                        existing.ResponseJson,
                        JsonOptions);

                if (cachedResponse is null)
                {
                    throw new InvalidOperationException(
                        "The stored idempotency response could not be deserialized.");
                }

                return cachedResponse;
            }
        }

        var response = await next();

        var responseJson = JsonSerializer.Serialize(response, JsonOptions);

        var idempotencyRecord =
            new IdempotencyRecord
            {
                Id = key,
                ResponseJson = responseJson,
                RequestHash = requestHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

        idempotencyRepository.Add(idempotencyRecord);

        return response;
    }
}