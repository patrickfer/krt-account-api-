using KRT.Application.Accounts.Mappings;
using KRT.Application.Accounts.Queries.GetAllAccounts;
using KRT.Application.Common.Interfaces;
using KRT.Domain.Interfaces.Repositories;
using MediatR;

namespace KRT.Application.Accounts.Queries.GetAccountById;

public sealed class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly IAccountRepository _repository;
    private readonly ICacheService _cacheService;
    private const int CacheDurationMinutes = 5;

    public GetAccountByIdQueryHandler(IAccountRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"account_{request.Id}";

        var cached = await _cacheService.GetAsync<AccountDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return null;

        var result = AccountMapper.ToDto(account);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes), cancellationToken);

        return result;
    }
}
