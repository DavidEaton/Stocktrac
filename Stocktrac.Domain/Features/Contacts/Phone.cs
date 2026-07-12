using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features.Contacts;

public class Phone : Entity, IHasPrimary
{
    public static readonly string InvalidMessage = "Phone number and/or its format is invalid";
    public static readonly string EmptyMessage = "Phone number cannot be empty";
    public static readonly string PhoneTypeInvalidMessage = $"Please enter a valid Phone Type";

    public string Number { get; private set; }
    public PhoneType PhoneType { get; private set; }
    public bool IsPrimary { get; private set; }

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

    public Result<string> SetNumber(string number)
    {
        number = (number ?? string.Empty).Trim();

        var phoneAttribute = new PhoneAttribute();

        if (!phoneAttribute.IsValid(number))
            return Result.Failure<string>(InvalidMessage);

        return Result.Success(Number = number);
    }

    public Result<PhoneType> SetPhoneType(PhoneType phoneType) =>
        !Enum.IsDefined(phoneType)
            ? Result.Failure<PhoneType>(InvalidMessage)
            : Result.Success(PhoneType = phoneType);

    public Result<bool> SetIsPrimary(bool isPrimary) =>
        Result.Success(IsPrimary = isPrimary);

    private static string RemoveNonNumericCharacters(string input) =>
        new(input.Where(char.IsDigit).ToArray());

    // EF requires a parameterless constructor
    protected Phone() { }
}
