using FluentAssertions;
using KRT.Domain.Exceptions;
using KRT.Domain.ValueObjects;

namespace KRT.Domain.Tests.ValueObjects;

public sealed class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Create_ShouldSucceed_WhenCpfIsValid(string cpf)
    {
        var result = Cpf.Create(cpf);
        result.Value.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("12345678900")]
    [InlineData("")]
    [InlineData("123")]
    public void Create_ShouldThrow_WhenCpfIsInvalid(string cpf)
    {
        var act = () => Cpf.Create(cpf);
        act.Should().Throw<DomainException>().WithMessage("*CPF*");
    }

    [Fact]
    public void Equality_ShouldBeValueBased()
    {
        var cpf1 = Cpf.Create("52998224725");
        var cpf2 = Cpf.Create("529.982.247-25");

        cpf1.Should().Be(cpf2);
    }
}
