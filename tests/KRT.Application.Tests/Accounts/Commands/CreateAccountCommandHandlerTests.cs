using FluentAssertions;
using KRT.Application.Accounts.Commands.CreateAccount;
using KRT.Application.Common.Interfaces;
using KRT.Domain.Entities;
using KRT.Domain.Exceptions;
using KRT.Domain.Interfaces.Repositories;
using Moq;

namespace KRT.Application.Tests.Accounts.Commands;

public sealed class CreateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CreateAccountCommandHandler _handler;

    private const string ValidCpf = "52998224725";

    public CreateAccountCommandHandlerTests()
    {
        _handler = new CreateAccountCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnId_WhenDataIsValid()
    {
        _repositoryMock
            .Setup(r => r.ExistsByCpfAsync(ValidCpf, default))
            .ReturnsAsync(false);

        var command = new CreateAccountCommand("João da Silva", ValidCpf);
        var result = await _handler.Handle(command, default);

        result.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(default), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("all_accounts", default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCpfAlreadyExists()
    {
        _repositoryMock
            .Setup(r => r.ExistsByCpfAsync(ValidCpf, default))
            .ReturnsAsync(true);

        var command = new CreateAccountCommand("João da Silva", ValidCpf);
        var act = async () => await _handler.Handle(command, default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*CPF*");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>(), default), Times.Never);
    }
}
