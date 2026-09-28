using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record PhoneNumber
{
    public const string InvalidMessage = "Please enter a valid Number.";

    public NonEmptyString Value { get; }

    private PhoneNumber(NonEmptyString value) => Value = value;

    public static Result<PhoneNumber> Create(NonEmptyString value) =>
        Result.Success(value)
            .Ensure(input => new PhoneAttribute().IsValid(input.Value), InvalidMessage)
            .Ensure(
                input => Regex.IsMatch(input.Value, @"^\+?[0-9\s().-]+$"),
                InvalidMessage)
            .Ensure(
                input => Regex.IsMatch(Normalize(input.Value), @"^(?:[0-9]+|\+[1-9][0-9]{1,14})$"),
                InvalidMessage)
            .Map(input => NonEmptyString.Create(Normalize(input.Value)).Value)
            .Map(normalized => new PhoneNumber(normalized));

    public string ToFormattedString() =>
        Value.Value.StartsWith('+')
            ? Value.Value
            : Value.Value.Length switch
            {
                7 => $"{Value.Value[..3]}-{Value.Value[3..]}",
                10 => $"({Value.Value[..3]}) {Value.Value[3..6]}-{Value.Value[6..]}",
                11 => $"{Value.Value[..1]} ({Value.Value[1..4]}) {Value.Value[4..7]}-{Value.Value[7..]}",
                _ => Value.Value
            };

    public override string ToString() => ToFormattedString();

    private static string Normalize(string number)
    {
        var prefix = number.StartsWith('+') ? "+" : string.Empty;
        return prefix + string.Concat(number.Where(character => character is >= '0' and <= '9'));
    }
}
