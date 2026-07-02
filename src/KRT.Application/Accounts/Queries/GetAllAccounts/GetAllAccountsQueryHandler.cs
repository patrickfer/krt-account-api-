using KRT.Application.Accounts.Mappings;
using KRT.Application.Common.Interfaces;
using KRT.Application.Common.Models;
using KRT.Domain.Interfaces.Repositories;
using MediatR;

namespace KRT.Application.Accounts.Queries.GetAllAccounts;

public sealed class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, PagedResult<AccountDto>>
{
    private readonly IAccountRepository _repository;
    private readonly ICacheService _cacheService;
    private const int CacheDurationMinutes = 5;

    public GetAllAccountsQueryHandler(IAccountRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var cacheKey = request.Status.HasValue
            ? $"all_accounts_{request.Status}_page{pageNumber}_size{pageSize}"
            : $"all_accounts_page{pageNumber}_size{pageSize}";

        var cached = await _cacheService.GetAsync<PagedResult<AccountDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var (accounts, totalCount) = await _repository.GetAllAsync(request.Status, pageNumber, pageSize, cancellationToken);
        var items = AccountMapper.ToDtoList(accounts).ToList();
        var result = new PagedResult<AccountDto>(items, pageNumber, pageSize, totalCount);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes), cancellationToken);

        return result;
    }
}
