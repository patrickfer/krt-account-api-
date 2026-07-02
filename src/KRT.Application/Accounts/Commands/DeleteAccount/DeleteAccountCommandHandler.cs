using KRT.Application.Common.Interfaces;
using KRT.Domain.Exceptions;
using KRT.Domain.Interfaces.Repositories;
using MediatR;

namespace KRT.Application.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteAccountCommandHandler(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Conta com ID {request.Id} não encontrada.");

        account.Deactivate();

        _repository.Update(account);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _cacheService.RemoveAsync($"account_{request.Id}", cancellationToken);
        await _cacheService.RemoveAsync("all_accounts", cancellationToken);
    }
}
