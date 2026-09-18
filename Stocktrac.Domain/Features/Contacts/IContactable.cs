using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Note Notes { get; }

    Maybe<Address> Address { get; }

    // ContactPhone and Email are immutable values; collection changes are owned by the aggregate.
    IReadOnlyList<ContactPhone> Phones { get; }

    IReadOnlyList<Email> Emails { get; }

    Result WithNotes(Note note);

    Result WithAddress(Address address);

    void WithoutAddress();

    Result<ContactPhone> AddPhone(ContactPhone phone);

    Result<ContactPhone> RemovePhone(ContactPhone phone);

    Result ReplacePhones(IReadOnlyList<ContactPhone> phones);

    Result<Email> AddEmail(Email email);

    Result<Email> RemoveEmail(Email email);

    Result ReplaceEmails(IReadOnlyList<Email> emails);

    bool HasPhoneNumber(string number);

    bool HasPrimaryPhone();

    bool HasEmailAddress(string address);

    bool HasPrimaryEmail();
}
