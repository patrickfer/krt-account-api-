using FluentAssertions;
using KRT.Application.Accounts.Commands.DeleteAccount;
using KRT.Application.Common.Interfaces;
using KRT.Domain.Entities;
using KRT.Domain.Enums;
using KRT.Domain.Exceptions;
using KRT.Domain.Interfaces.Repositories;
using Moq;

namespace KRT.Application.Tests.Accounts.Commands;

public sealed class DeleteAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _handler = new DeleteAccountCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateAccount_WhenAccountExists()
    {
        var account = Account.Create("João da Silva", "52998224725");
        _repositoryMock.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);

        await _handler.Handle(new DeleteAccountCommand(account.Id), default);

        account.Status.Should().Be(AccountStatus.Inactive);
        _repositoryMock.Verify(r => r.Update(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(default), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAccountNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Account?)null);

        var act = async () => await _handler.Handle(new DeleteAccountCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*não encontrada*");
    }
}
