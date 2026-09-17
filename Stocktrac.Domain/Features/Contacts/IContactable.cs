using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Note Notes { get; }

    Maybe<Address> Address { get; }

    // Phone and Email are immutable values; collection changes are owned by the aggregate.
    IReadOnlyList<Phone> Phones { get; }

    IReadOnlyList<Email> Emails { get; }

    Result WithNotes(Note note);

    Result WithAddress(Address address);

    void WithoutAddress();

    Result<Phone> AddPhone(Phone phone);

    Result<Phone> RemovePhone(Phone phone);

    Result ReplacePhones(IReadOnlyList<Phone> phones);

    Result<Email> AddEmail(Email email);

    Result<Email> RemoveEmail(Email email);

    Result ReplaceEmails(IReadOnlyList<Email> emails);

    bool HasPhoneNumber(string number);

    bool HasPrimaryPhone();

    bool HasEmailAddress(string address);

    bool HasPrimaryEmail();
}
