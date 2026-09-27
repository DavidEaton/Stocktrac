using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record ContactPhone : IHasPrimary
{
    public const string PhoneTypeInvalidMessage = "Please enter a valid Type.";

    public PhoneNumber Number { get; }
    public PhoneType PhoneType { get; } = PhoneType.Unknown;
    public bool IsPrimary { get; } = false;

    private ContactPhone(PhoneNumber number, PhoneType phoneType, bool isPrimary)
    {
        Number = number;
        PhoneType = phoneType;
        IsPrimary = isPrimary;
    }

    public static Result<ContactPhone> Create(PhoneNumber number, PhoneType phoneType, bool isPrimary)
    {
        return Result.Combine(
                Environment.NewLine,
                number.AsRequired(),
                phoneType.AsValidPhoneType())
            .Map(() => new ContactPhone(number, phoneType, isPrimary));
    }

    public override string ToString() => Number.ToString();

    public Result<ContactPhone> ReplaceNumber(PhoneNumber number) =>
        number.AsRequired()
            .Map(validLine => new ContactPhone(number, PhoneType, IsPrimary));

    public Result<ContactPhone> ReplacePhoneType(PhoneType phoneType) =>
        phoneType.AsValidPhoneType()
            .Map(validPhoneType => new ContactPhone(Number, validPhoneType, IsPrimary));

    public ContactPhone ReplaceIsPrimary(bool isPrimary) => new(Number, PhoneType, isPrimary);
}
