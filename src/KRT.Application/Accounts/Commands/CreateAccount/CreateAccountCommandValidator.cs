using FluentValidation;

namespace KRT.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("O nome do titular é obrigatório.")
            .MaximumLength(150).WithMessage("O nome do titular deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .Length(11, 14).WithMessage("CPF deve ter entre 11 e 14 caracteres (com ou sem máscara).");
    }
}
