using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Note Notes { get; }

    Maybe<Address> Address { get; }

    // ContactPhone and ContactEmail are immutable values; collection changes are owned by the aggregate.
    IReadOnlyList<ContactPhone> Phones { get; }

    IReadOnlyList<ContactEmail> Emails { get; }

    Result WithNotes(Note note);

    Result WithAddress(Address? address);

    void WithoutAddress();

    Result<ContactPhone> AddPhone(ContactPhone? phone);

    Result<ContactPhone> RemovePhone(ContactPhone? phone);

    Result ReplacePhones(IReadOnlyList<ContactPhone>? phones);

    Result<ContactEmail> AddEmail(ContactEmail? email);

    Result<ContactEmail> RemoveEmail(ContactEmail? email);

    Result ReplaceEmails(IReadOnlyList<ContactEmail>? emails);

    bool HasPhoneNumber(string? number);

    bool HasPrimaryPhone();

    bool HasEmailAddress(string? address);

    bool HasPrimaryEmail();
}
