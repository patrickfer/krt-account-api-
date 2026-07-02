using KRT.Application.Common.Models;
using KRT.Domain.Enums;
using MediatR;

namespace KRT.Application.Accounts.Queries.GetAllAccounts;

public sealed record GetAllAccountsQuery(AccountStatus? Status = null, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<AccountDto>>;
