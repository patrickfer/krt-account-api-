using KRT.Domain.Exceptions;
using KRT.Domain.Enums;
using KRT.Domain.ValueObjects;

namespace KRT.Domain.Entities;

public sealed class Account
{
    public Guid Id { get; private set; }
    public string HolderName { get; private set; } = string.Empty;
    public Cpf Cpf { get; private set; } = null!;
    public AccountStatus Status { get; private set; }

    private Account() { }

    public static Account Create(string holderName, string cpf)
    {
        if (string.IsNullOrWhiteSpace(holderName))
            throw new DomainException("O nome do titular é obrigatório.");

        return new Account
        {
            Id = Guid.NewGuid(),
            HolderName = holderName.Trim(),
            Cpf = Cpf.Create(cpf),
            Status = AccountStatus.Active
        };
    }

    public void Update(string holderName)
    {
        if (string.IsNullOrWhiteSpace(holderName))
            throw new DomainException("O nome do titular é obrigatório.");

        HolderName = holderName.Trim();
    }

    public void Deactivate() => Status = AccountStatus.Inactive;
}
