using FluentAssertions;
using KRT.Domain.Entities;
using KRT.Domain.Enums;
using KRT.Domain.Exceptions;

namespace KRT.Domain.Tests.Entities;

public sealed class AccountTests
{
    private const string ValidCpf = "52998224725";
    private const string ValidName = "João da Silva";

    [Fact]
    public void Create_ShouldReturnAccount_WhenValidData()
    {
        var account = Account.Create(ValidName, ValidCpf);

        account.Id.Should().NotBeEmpty();
        account.HolderName.Should().Be(ValidName);
        account.Cpf.Value.Should().Be(ValidCpf);
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldThrow_WhenHolderNameIsEmpty(string? name)
    {
        var act = () => Account.Create(name!, ValidCpf);
        act.Should().Throw<DomainException>().WithMessage("*titular*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenCpfIsInvalid()
    {
        var act = () => Account.Create(ValidName, "00000000000");
        act.Should().Throw<DomainException>().WithMessage("*CPF*");
    }

    [Fact]
    public void Update_ShouldChangeName_WhenValidData()
    {
        var account = Account.Create(ValidName, ValidCpf);

        account.Update("Maria Souza");

        account.HolderName.Should().Be("Maria Souza");
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Deactivate_ShouldSetStatusInactive()
    {
        var account = Account.Create(ValidName, ValidCpf);

        account.Deactivate();

        account.Status.Should().Be(AccountStatus.Inactive);
    }
}
