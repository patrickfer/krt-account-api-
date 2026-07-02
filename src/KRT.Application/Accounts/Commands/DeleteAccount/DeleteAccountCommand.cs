using MediatR;

namespace KRT.Application.Accounts.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(Guid Id) : IRequest;
