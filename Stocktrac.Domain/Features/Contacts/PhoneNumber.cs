using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record PhoneNumber
{
    public const string InvalidMessage = "Please enter a valid Number.";

    public string Value { get; }

    private PhoneNumber(string value) =>
        Value = value;

    public static Result<PhoneNumber> Create(string? value) =>
        Result.Success(value)
            .Map(input => input?.Trim() ?? string.Empty)
            .Ensure(normalized => new PhoneAttribute().IsValid(normalized), InvalidMessage)
            .Ensure(
                normalized => Regex.IsMatch(normalized, @"^\+?[0-9\s().-]+$"),
                InvalidMessage)
            .Map(Canonicalize)
            .Ensure(
                normalized => Regex.IsMatch(normalized, @"^(?:[0-9]+|\+[1-9][0-9]{1,14})$"),
                InvalidMessage)
            .Map(normalized => new PhoneNumber(normalized));

    public override string ToString()
    {
        if (Value.StartsWith('+'))
        {
            return Value;
        }

        return Value.Length switch
        {
            7 => Regex.Replace(Value, @"(\d{3})(\d{4})", "$1-$2"),
            10 => Regex.Replace(Value, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3"),
            _ => Value,
        };
    }

    private static string Canonicalize(string number)
    {
        var prefix = number.StartsWith('+') ? "+" : string.Empty;
        return prefix + string.Concat(number.Where(character => character is >= '0' and <= '9'));
    }
}
