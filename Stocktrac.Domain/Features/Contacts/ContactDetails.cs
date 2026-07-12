using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public class ContactDetails : ValueObject
{
    public IReadOnlyList<Phone>? Phones { get; } = [];
    public IReadOnlyList<Email>? Emails { get; } = [];
    public Maybe<Address>? Address { get; } = Maybe<Address>.None;

    private ContactDetails(
        IReadOnlyList<Phone>? phones,
        IReadOnlyList<Email>? emails,
        Maybe<Address>? address)
    {
        Phones = phones;
        Emails = emails;
        Address = address;
    }

    public static Result<ContactDetails> Create(
        IReadOnlyList<Phone>? phones,
        IReadOnlyList<Email>? emails,
        Maybe<Address>? address)
    {
        phones ??= [];
        emails ??= [];

        if (phones?
            .Count(phone => phone.IsPrimary) > 1)
            return Result.Failure<ContactDetails>(Contactable.PrimaryExistsMessage);

        if (emails?
            .Count(email => email.IsPrimary) > 1)
            return Result.Failure<ContactDetails>(Contactable.PrimaryExistsMessage);

        return Result.Success(
            new ContactDetails(
                phones: phones,
                emails: emails,
                address: address));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        if (Phones is not null)
            foreach (var phone in Phones)
                yield return phone;

        if (Emails is not null)
            foreach (var email in Emails)
                yield return email;

        if (Address is not null)
            yield return Address;
    }

    // EF requires a parameterless constructor
    protected ContactDetails() { }
}
