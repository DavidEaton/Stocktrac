using CSharpFunctionalExtensions;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public class Phone : Entity, IHasPrimary
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

    public static Result<Phone> Create(
        string number,
        PhoneType phoneType,
        bool isPrimary) =>
        Result.Combine(
                Environment.NewLine,
                number.AsValidPhoneNumber(),
                phoneType.AsValidPhoneType())
            .Map(() => new Phone(number.Trim(), phoneType, isPrimary));

    public override string ToString()
    {
        var numericNumber = RemoveNonNumericCharacters(Number);

        return numericNumber.Length switch
        {
            7 => Regex.Replace(numericNumber, @"(\d{3})(\d{4})", "$1-$2"),
            10 => Regex.Replace(numericNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3"),
            _ => numericNumber,
        };
    }

    public Result<Phone> WithNumber(string number) =>
        number.AsValidPhoneNumber()
            .Map(validNumber => Copy(number: validNumber));

    public Result<Phone> WithPhoneType(PhoneType phoneType) =>
        phoneType.AsValidPhoneType()
            .Map(validPhoneType => Copy(phoneType: validPhoneType));

    public Phone WithIsPrimary(bool isPrimary) => Copy(isPrimary: isPrimary);

    private Phone Copy(string? number = null, PhoneType? phoneType = null, bool? isPrimary = null) =>
        new(number ?? Number, phoneType ?? PhoneType, isPrimary ?? IsPrimary) { Id = Id };

    private static string RemoveNonNumericCharacters(string input) =>
        new(input.Where(char.IsDigit).ToArray());

    // EF requires a parameterless constructor
    protected Phone() { }
}
