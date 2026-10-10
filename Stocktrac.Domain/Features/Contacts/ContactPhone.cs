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
        var validPhoneType = phoneType.AsValidPhoneType();

        return validPhoneType.IsSuccess
            ? Result.Success(new ContactPhone(number, validPhoneType.Value, isPrimary))
            : Result.Failure<ContactPhone>(validPhoneType.Error);
    }

    public override string ToString() => Number.ToString();

    public Result<ContactPhone> ReplaceNumber(PhoneNumber number) =>
        Result.Success(new ContactPhone(number, PhoneType, IsPrimary));

    public Result<ContactPhone> ReplacePhoneType(PhoneType phoneType) =>
        phoneType.AsValidPhoneType()
            .Map(validPhoneType => new ContactPhone(Number, validPhoneType, IsPrimary));

    public Result<ContactPhone> ReplaceIsPrimary(bool isPrimary) =>
        Result.Success(new ContactPhone(Number, PhoneType, isPrimary));
}
