using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public class ContactDetails : ValueObject
{
    public IReadOnlyList<Phone> Phones { get; } = [];
    public IReadOnlyList<Email> Emails { get; } = [];
    public Maybe<Address> Address { get; } = Maybe<Address>.None;

    private ContactDetails(
        IReadOnlyList<Phone> phones,
        IReadOnlyList<Email> emails,
        Maybe<Address> address)
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
        var normalizedPhones = phones?.ToArray() ?? [];
        var normalizedEmails = emails?.ToArray() ?? [];

        if (normalizedPhones.Any(phone => phone is null) ||
            normalizedEmails.Any(email => email is null))
            return Result.Failure<ContactDetails>(Contactable.RequiredMessage);

        if (normalizedPhones.Count(phone => phone.IsPrimary) > 1)
            return Result.Failure<ContactDetails>(Contactable.PrimaryExistsMessage);

        if (normalizedEmails.Count(email => email.IsPrimary) > 1)
            return Result.Failure<ContactDetails>(Contactable.PrimaryExistsMessage);

        if (normalizedPhones.GroupBy(phone => phone.Number).Any(group => group.Count() > 1) ||
            normalizedEmails.GroupBy(email => email.Address).Any(group => group.Count() > 1))
            return Result.Failure<ContactDetails>(Contactable.NonuniqueMessage);

        return Result.Success(
            new ContactDetails(
                phones: normalizedPhones,
                emails: normalizedEmails,
                address: address ?? Maybe<Address>.None));
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
