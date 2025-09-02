using MediatR;

namespace LinaSys.Shared.Application.MediatR;

public interface IBaseRequest : IRequest<Result>
{
}

public interface IBaseRequest<T> : IRequest<Result<T>>
{
}

public abstract class BaseCommandHandler<TCommand, TResult>
    : IRequestHandler<TCommand, Result<TResult>>
    where TCommand : IRequest<Result<TResult>>
{
    public abstract Task<Result<TResult>> Handle(TCommand request, CancellationToken cancellationToken);

    protected static Result<TResult> Failure(ResultErrorCodes code, params (string Context, string Message)[] messages) => Result<TResult>.Failure(code, messages);

    protected static Result<TResult> Failure(TResult value, ResultErrorCodes code, params (string Context, string Message)[] messages) => Result<TResult>.Failure(value, code, messages);

    protected static Result<TResult> Success(TResult value) => Result.Success(value);
}

public abstract class BaseCommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : IRequest<Result>
{
    public abstract Task<Result> Handle(TCommand request, CancellationToken cancellationToken);

    protected static Result Failure(ResultErrorCodes code, params (string Context, string Message)[] messages) => Result.Failure(code, messages);

    protected static Result Success() => Result.Success();
}
