using FluentValidation;

namespace KRT.Application.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID da conta é obrigatório.");

        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("O nome do titular é obrigatório.")
            .MaximumLength(150).WithMessage("O nome do titular deve ter no máximo 150 caracteres.");
    }
}
