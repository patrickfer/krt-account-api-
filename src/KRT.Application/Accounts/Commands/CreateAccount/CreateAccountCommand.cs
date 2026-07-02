using MediatR;

namespace KRT.Application.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(string HolderName, string Cpf) : IRequest<Guid>;
