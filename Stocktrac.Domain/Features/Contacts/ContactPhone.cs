using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record ContactPhone : IHasPrimary
{
    public const string PhoneTypeInvalidMessage = "Please enter a valid Type.";

    public PhoneNumber Number { get; private set; }
    public PhoneType PhoneType { get; private set; } = PhoneType.Unknown;
    public bool IsPrimary { get; private set; } = false;

    private ContactPhone(PhoneNumber number, PhoneType phoneType, bool isPrimary)
    {
        Number = number;
        PhoneType = phoneType;
        IsPrimary = isPrimary;
    }

    public static Result<ContactPhone> Create(string number, PhoneType phoneType, bool isPrimary)
    {
        var validNumber = PhoneNumber.Create(number);

        return Result.Combine(
                Environment.NewLine,
                validNumber,
                phoneType.AsValidPhoneType())
            .Map(() => new ContactPhone(validNumber.Value, phoneType, isPrimary));
    }

    public override string ToString() => Number.ToString();

    public Result<ContactPhone> WithNumber(string number) =>
        PhoneNumber.Create(number)
            .Map(validNumber => this with { Number = validNumber });

    public Result<ContactPhone> WithPhoneType(PhoneType phoneType) =>
        phoneType.AsValidPhoneType()
            .Map(validPhoneType => this with { PhoneType = validPhoneType });

    public ContactPhone WithIsPrimary(bool isPrimary) => this with { IsPrimary = isPrimary };
}
