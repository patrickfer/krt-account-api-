using KRT.Application.Accounts.Queries.GetAllAccounts;
using KRT.Domain.Entities;

namespace KRT.Application.Accounts.Mappings;

public static class AccountMapper
{
    public static AccountDto ToDto(Account account) =>
        new(account.Id, account.HolderName, account.Cpf.Value, account.Status.ToString());

    public static IEnumerable<AccountDto> ToDtoList(IEnumerable<Account> accounts) =>
        accounts.Select(ToDto);
}
