using KRT.Domain.Entities;
using KRT.Domain.Enums;

namespace KRT.Domain.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetAllAsync(AccountStatus? status = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    void Update(Account account);
}
