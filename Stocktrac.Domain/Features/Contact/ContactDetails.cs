using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contact;

public class ContactDetails : ValueObject
{
    public IReadOnlyList<Phone> Phones { get; } = [];
    public IReadOnlyList<Email> Emails { get; } = [];
    public Maybe<Address> Address { get; } = Maybe<Address>.None;

    private ContactDetails(IReadOnlyList<Phone> phones, IReadOnlyList<Email> emails, Maybe<Address> address)
    {
        Phones = phones;
        Emails = emails;
        Address = address;
    }

    public static Result<ContactDetails> Create(IReadOnlyList<Phone> phones, IReadOnlyList<Email> emails, Maybe<Address> address)
    {
        phones ??= [];
        emails ??= [];

        if (phones?.Count(phone => phone.IsPrimary) > 1)
            return Result.Failure<ContactDetails>(Contactable.PrimaryExistsMessage);

        if (emails?.Count(email => email.IsPrimary) > 1)
            return Result.Failure<ContactDetails>(Contactable.PrimaryExistsMessage);

        return Result.Success(new ContactDetails(phones, emails, address));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var phone in Phones)
            yield return phone;

        foreach (var email in Emails)
            yield return email;

        yield return Address;
    }

    // EF requires a parameterless constructor
    protected ContactDetails() { }
}
