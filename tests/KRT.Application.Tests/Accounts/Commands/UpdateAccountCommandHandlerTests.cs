using FluentAssertions;
using KRT.Application.Accounts.Commands.UpdateAccount;
using KRT.Application.Common.Interfaces;
using KRT.Domain.Entities;
using KRT.Domain.Enums;
using KRT.Domain.Exceptions;
using KRT.Domain.Interfaces.Repositories;
using Moq;

namespace KRT.Application.Tests.Accounts.Commands;

public sealed class UpdateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly UpdateAccountCommandHandler _handler;

    public UpdateAccountCommandHandlerTests()
    {
        _handler = new UpdateAccountCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateName_WhenAccountExists()
    {
        var account = Account.Create("Original Name", "52998224725");
        _repositoryMock.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);

        var command = new UpdateAccountCommand(account.Id, "New Name");
        await _handler.Handle(command, default);

        account.HolderName.Should().Be("New Name");
        account.Status.Should().Be(AccountStatus.Active);
        _unitOfWorkMock.Verify(u => u.CommitAsync(default), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAccountNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Account?)null);

        var command = new UpdateAccountCommand(Guid.NewGuid(), "New Name");
        var act = async () => await _handler.Handle(command, default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*não encontrada*");
    }
}
