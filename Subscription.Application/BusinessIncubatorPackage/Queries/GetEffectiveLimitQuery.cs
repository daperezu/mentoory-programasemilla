using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Queries;

public record GetEffectiveLimitQuery(long BusinessIncubatorId, int Type) : IBaseRequest<int>;

public class GetEffectiveLimitQueryValidator : AbstractValidator<GetEffectiveLimitQuery>
{
    public GetEffectiveLimitQueryValidator()
    {
        RuleFor(x => x.BusinessIncubatorId).GreaterThan(0);
        RuleFor(x => x.Type)
            .Must(type => Enum.IsDefined(typeof(PackageLimitType), type))
            .WithMessage("Invalid limit type.");
    }
}

public partial class GetEffectiveLimitQueryHandler(ILogger<GetEffectiveLimitQueryHandler> logger, IBusinessIncubatorPackageRepository repository)
    : BaseCommandHandler<GetEffectiveLimitQuery, int>
{
    public override async Task<Result<int>> Handle(GetEffectiveLimitQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await repository.GetWithVersionAndLimitsByIncubatorIdAsync(request.BusinessIncubatorId, cancellationToken);
            if (entity is null)
            {
                return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound);
            }

            var type = (PackageLimitType)request.Type;
            var limit = entity.GetEffectiveLimit(type);

            return Result.Success(limit);
        }
        catch (Exception ex)
        {
            LogQueryFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_GetEffectiveLimitFailed, (nameof(GetEffectiveLimitQuery), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2205, Level = LogLevel.Error, Message = nameof(GetEffectiveLimitQueryHandler) + " failed: {ErrorMessage}")]
    partial void LogQueryFailed(string errorMessage);
}
