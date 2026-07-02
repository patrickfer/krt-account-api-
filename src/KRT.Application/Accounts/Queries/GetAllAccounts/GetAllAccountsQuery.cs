using KRT.Domain.Enums;
using MediatR;

namespace KRT.Application.Accounts.Queries.GetAllAccounts;

public sealed record GetAllAccountsQuery(AccountStatus? Status = null) : IRequest<IEnumerable<AccountDto>>;
