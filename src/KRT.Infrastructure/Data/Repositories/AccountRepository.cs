using KRT.Domain.Entities;
using KRT.Domain.Enums;
using KRT.Domain.Interfaces.Repositories;
using KRT.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Infrastructure.Data.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IEnumerable<Account>> GetAllAsync(AccountStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Accounts.AsNoTracking();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await _context.Accounts.AnyAsync(a => a.Cpf.Value == cpf, cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        => await _context.Accounts.AddAsync(account, cancellationToken);

    public void Update(Account account)
        => _context.Accounts.Update(account);
}
