using Application.Abstraction.Data;
using Application.Abstraction.Mediator;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

internal sealed class TransactionBehavior<TRequest, TResponse>(
    IAppDbContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (request is not ITransactionalRequest)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        if (dbContext.Database.CurrentTransaction is not null)
        {
            logger.LogInformation("Using existing transaction for {RequestName}", requestName);
            return await next();
        }

        await using IDbContextTransaction transaction
            = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            logger.LogInformation("Starting transaction for {RequestName}", requestName);

            TResponse response = await next();

            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Committed transaction for {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rolling back transaction for {RequestName}", requestName);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}