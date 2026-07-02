using KRT.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace KRT.Domain.ValueObjects;

public sealed class Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Cpf Create(string cpf)
    {
        var digits = Regex.Replace(cpf ?? string.Empty, @"\D", "");

        if (digits.Length != 11 || digits.Distinct().Count() == 1)
            throw new DomainException("CPF inválido.");

        if (!ValidateDigits(digits))
            throw new DomainException("CPF inválido.");

        return new Cpf(digits);
    }

    private static bool ValidateDigits(string digits)
    {
        int[] weights1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weights2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var sum1 = digits.Take(9).Select((d, i) => int.Parse(d.ToString()) * weights1[i]).Sum();
        var remainder1 = sum1 % 11;
        var digit1 = remainder1 < 2 ? 0 : 11 - remainder1;

        var sum2 = digits.Take(10).Select((d, i) => int.Parse(d.ToString()) * weights2[i]).Sum();
        var remainder2 = sum2 % 11;
        var digit2 = remainder2 < 2 ? 0 : 11 - remainder2;

        return digits[9] == digit1.ToString()[0] && digits[10] == digit2.ToString()[0];
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Cpf other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
