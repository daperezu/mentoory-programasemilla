using FluentValidation;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries;

public sealed record GetUserIdByUsernameQuery(string Username) : IBaseRequest<string>;

public sealed class GetUserIdByUsernameQueryValidator : AbstractValidator<GetUserIdByUsernameQuery>
{
    public GetUserIdByUsernameQueryValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
    }
}

public sealed class GetUserIdByUsernameQueryHandler(
    IAuthRepository authRepository)
    : BaseCommandHandler<GetUserIdByUsernameQuery, string>
{
    public override async Task<Result<string>> Handle(GetUserIdByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await authRepository.FindUserByNameAsync(request.Username, cancellationToken);

        return user is null
            ? Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(request.Username), "User not found."))
            : Success(user.Id);
    }
}
