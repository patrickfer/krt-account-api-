using KRT.Application.Common.Interfaces;
using KRT.Domain.Entities;
using KRT.Domain.Exceptions;
using KRT.Domain.Interfaces.Repositories;
using MediatR;

namespace KRT.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public CreateAccountCommandHandler(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var cpfDigits = System.Text.RegularExpressions.Regex.Replace(request.Cpf, @"\D", "");

        if (await _repository.ExistsByCpfAsync(cpfDigits, cancellationToken))
            throw new DomainException("Já existe uma conta cadastrada com este CPF.");

        var account = Account.Create(request.HolderName, request.Cpf);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _cacheService.RemoveAsync("all_accounts", cancellationToken);

        return account.Id;
    }
}
