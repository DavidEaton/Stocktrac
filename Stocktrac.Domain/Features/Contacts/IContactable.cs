using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Note Notes { get; }

    Maybe<Address> Address { get; }

    IReadOnlyList<Phone> Phones { get; }

    IReadOnlyList<Email> Emails { get; }

    void WithNotes(Note note);

    void WithAddress(Address address);

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
