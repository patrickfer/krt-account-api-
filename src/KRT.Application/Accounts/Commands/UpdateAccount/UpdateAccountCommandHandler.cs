using KRT.Application.Common.Interfaces;
using KRT.Domain.Exceptions;
using KRT.Domain.Interfaces.Repositories;
using MediatR;

namespace KRT.Application.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateAccountCommandHandler(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Conta com ID {request.Id} não encontrada.");

        account.Update(request.HolderName);

        _repository.Update(account);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _cacheService.RemoveAsync($"account_{request.Id}", cancellationToken);
        await _cacheService.RemoveAsync("all_accounts", cancellationToken);
    }
}
