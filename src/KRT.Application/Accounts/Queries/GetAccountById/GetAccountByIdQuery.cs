using KRT.Application.Accounts.Queries.GetAllAccounts;
using MediatR;

namespace KRT.Application.Accounts.Queries.GetAccountById;

public sealed record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto?>;
