namespace KRT.Application.Accounts.Queries.GetAllAccounts;

public sealed record AccountDto(Guid Id, string HolderName, string Cpf, string Status);
