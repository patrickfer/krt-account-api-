using MediatR;

namespace KRT.Application.Accounts.Commands.UpdateAccount;

public sealed record UpdateAccountCommand(Guid Id, string HolderName) : IRequest;
