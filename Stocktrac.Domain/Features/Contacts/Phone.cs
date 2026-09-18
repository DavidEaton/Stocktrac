using CSharpFunctionalExtensions;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Phone : IHasPrimary
{
    public const string InvalidMessage = "Please enter a valid Number.";
    public const string PhoneTypeInvalidMessage = "Please enter a valid Type.";

    public string Number { get; private set; } = string.Empty;
    public PhoneType PhoneType { get; private set; } = PhoneType.Unknown;
    public bool IsPrimary { get; private set; } = false;

    private Phone(string number, PhoneType phoneType, bool isPrimary)
    {
        Number = number;
        PhoneType = phoneType;
        IsPrimary = isPrimary;
    }

    public static Result<Phone> Create(string number, PhoneType phoneType, bool isPrimary)
    {
        var validNumber = number.AsValidPhoneNumber();

        return Result.Combine(
                Environment.NewLine,
                validNumber,
                phoneType.AsValidPhoneType())
            .Map(() => new Phone(validNumber.Value, phoneType, isPrimary));
    }

    public override string ToString()
    {
        if (Number.StartsWith('+'))
        {
            return Number;
        }

        var numericNumber = OnlyDigitsFrom(Number);

        return numericNumber.Length switch
        {
            7 => Regex.Replace(numericNumber, @"(\d{3})(\d{4})", "$1-$2"),
            10 => Regex.Replace(numericNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3"),
            _ => numericNumber,
        };
    }

    public Result<Phone> WithNumber(string number) =>
        number.AsValidPhoneNumber()
            .Map(validNumber => this with { Number = validNumber });

    public Result<Phone> WithPhoneType(PhoneType phoneType) =>
        phoneType.AsValidPhoneType()
            .Map(validPhoneType => this with { PhoneType = validPhoneType });

    public Phone WithIsPrimary(bool isPrimary) => this with { IsPrimary = isPrimary };

    private static string OnlyDigitsFrom(string input) =>
        new([.. input.Where(char.IsDigit)]);
}
