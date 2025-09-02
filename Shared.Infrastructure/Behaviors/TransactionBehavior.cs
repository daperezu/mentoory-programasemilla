using LinaSys.Shared.Infrastructure.Extensions;
using LinaSys.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinaSys.Shared.Infrastructure.Behaviors;

public partial class TransactionBehavior<TRequest, TResponse>(IDbContextFactory dbContextFactory, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const string LogPrefix = "[Pipeline behavior] [Transaction] ";

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        var requestType = request.GetGenericTypeName();
        var requestName = request.GetType().FullName;

        try
        {
            if (!dbContextFactory.TryGetDbContextForRequest<TRequest>(out var dbContext))
            {
                LogSkippedTransaction(requestType, requestName!);
                return await next(cancellationToken);
            }

            if (dbContext.HasActiveTransaction)
            {
                return await next(cancellationToken);
            }

            var strategy = dbContext.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.TryBeginTransactionAsync(cancellationToken);

                using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                {
                    LogBeginTransaction(transaction.TransactionId, requestType, requestName!);

                    response = await next(cancellationToken);

                    LogCommitTransaction(transaction.TransactionId, requestType, requestName!);

                    await dbContext.CommitTransactionAsync(transaction, cancellationToken);
                }

                //// await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
            });

            return response!;
        }
        catch (Exception)
        {
            LogTransactionError(requestType, requestName!);
            throw;
            //// TODO: Handle return a Result object instead of throwing an exception
        }
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = LogPrefix + "Begin transaction {TransactionId} for {CommandName} ({CommandType})")]
    partial void LogBeginTransaction(Guid transactionId, string commandName, string commandType);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = LogPrefix + "Commit transaction {TransactionId} for {CommandName} ({CommandType})")]
    partial void LogCommitTransaction(Guid transactionId, string commandName, string commandType);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Error, Message = LogPrefix + "Error handling transaction for for {CommandName} ({CommandType})")]
    partial void LogTransactionError(string commandName, string commandType);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Warning, Message = LogPrefix + "This command doesn't have a DbContext registered. Pipeline skipped. {CommandName} ({CommandType})")]
    partial void LogSkippedTransaction(string commandName, string commandType);
}
