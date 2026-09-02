using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public class Phone : Entity, IHasPrimary
{
    public static readonly string InvalidMessage = "Phone number and/or its format is invalid";
    public static readonly string EmptyMessage = "Phone number cannot be empty";
    public static readonly string PhoneTypeInvalidMessage = $"Please enter a valid Phone Type";

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
        if (!Enum.IsDefined(phoneType))
            return Result.Failure<Phone>(PhoneTypeInvalidMessage);

        number = (number ?? string.Empty).Trim();

        var phoneAttribute = new PhoneAttribute();

        if (!phoneAttribute.IsValid(number))
            return Result.Failure<Phone>(InvalidMessage);

        return Result.Success(new Phone(number, phoneType, isPrimary));
    }

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

    public Result<Phone> SetNumber(string number)
    {
        number = (number ?? string.Empty).Trim();

        var phoneAttribute = new PhoneAttribute();

        if (!phoneAttribute.IsValid(number))
            return Result.Failure<Phone>(InvalidMessage);

        return Result.Success(Copy(number: number));
    }

    public Result<Phone> SetPhoneType(PhoneType phoneType) =>
        !Enum.IsDefined(phoneType)
            ? Result.Failure<Phone>(PhoneTypeInvalidMessage)
            : Result.Success(Copy(phoneType: phoneType));

    public Result<Phone> SetIsPrimary(bool isPrimary) =>
        Result.Success(Copy(isPrimary: isPrimary));

    private Phone Copy(string? number = null, PhoneType? phoneType = null, bool? isPrimary = null) =>
        new(number ?? Number, phoneType ?? PhoneType, isPrimary ?? IsPrimary) { Id = Id };

    private static string RemoveNonNumericCharacters(string input) =>
        new(input.Where(char.IsDigit).ToArray());

    // EF requires a parameterless constructor
    protected Phone() { }
}
