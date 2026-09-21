using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record PhoneNumber
{
    public const string InvalidMessage = "Please enter a valid Number.";

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static Result<PhoneNumber> Create(string value) =>
        Result.Success(value)
            .Map(input => input?.Trim() ?? string.Empty)
            .Ensure(normalized => new PhoneAttribute().IsValid(normalized), InvalidMessage)
            .Ensure(
                normalized => Regex.IsMatch(normalized, @"^\+?[0-9\s().-]+$"),
                InvalidMessage)
            .Map(Normalize)
            .Ensure(
                normalized => Regex.IsMatch(normalized, @"^(?:[0-9]+|\+[1-9][0-9]{1,14})$"),
                InvalidMessage)
            .Map(normalized => new PhoneNumber(normalized));

    public string ToFormattedString() =>
        Value.StartsWith('+')
            ? Value
            : Value.Length switch
            {
                7 => $"{Value[..3]}-{Value[3..]}",
                10 => $"({Value[..3]}) {Value[3..6]}-{Value[6..]}",
                11 => $"{Value[..1]} ({Value[1..4]}) {Value[4..7]}-{Value[7..]}",
                _ => Value
            };

    public override string ToString() => ToFormattedString();

    private static string Normalize(string number)
    {
        var prefix = number.StartsWith('+') ? "+" : string.Empty;
        return prefix + string.Concat(number.Where(character => character is >= '0' and <= '9'));
    }
}
