using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Commands;

public sealed record CreateFormCommand(string Name)
    : IBaseRequest<long>;

public class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");
    }
}

public sealed class CreateCommandHandler(IFormRepository repository)
    : BaseCommandHandler<CreateFormCommand, long>
{
    public override async Task<Result<long>> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            return Failure(ResultErrorCodes.DiagnosisForm_NameAlreadyExists, (nameof(request.Name), "A diagnosis form with the same name already exists."));
        }

        var form = new Domain.Aggregates.Form.Form(request.Name);
        repository.Add(form);

        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Success(form.Id);
    }
}
