using LinaSys.Shared.Application.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Shared.Application.Behaviors;

public partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        LogHandlingCommand(request.GetGenericTypeName(), request.GetType().FullName!);

        var response = await next(cancellationToken);

        LogCommandHandled(request.GetGenericTypeName(), request.GetType().FullName!);

        return response;
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "[Pipeline behavior] [Logging] Command handled: {CommandName} ({@CommandType})")]
    partial void LogCommandHandled(string commandName, string commandType);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "[Pipeline behavior] [Logging] Handling command: {CommandName} ({@CommandType})")]
    partial void LogHandlingCommand(string commandName, string commandType);
}
