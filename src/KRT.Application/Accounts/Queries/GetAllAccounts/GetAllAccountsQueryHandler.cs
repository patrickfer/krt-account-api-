using KRT.Application.Accounts.Mappings;
using KRT.Application.Common.Interfaces;
using KRT.Domain.Interfaces.Repositories;
using MediatR;

namespace KRT.Application.Accounts.Queries.GetAllAccounts;

public sealed class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, IEnumerable<AccountDto>>
{
    private readonly IAccountRepository _repository;
    private readonly ICacheService _cacheService;
    private const int CacheDurationMinutes = 5;

    public GetAllAccountsQueryHandler(IAccountRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = request.Status.HasValue ? $"all_accounts_{request.Status}" : "all_accounts";

        var cached = await _cacheService.GetAsync<List<AccountDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var accounts = await _repository.GetAllAsync(request.Status, cancellationToken);
        var result = AccountMapper.ToDtoList(accounts).ToList();

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes), cancellationToken);

        return result;
    }
}
