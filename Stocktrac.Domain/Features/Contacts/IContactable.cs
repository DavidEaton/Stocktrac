using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Note Notes { get; }

    Maybe<Address> Address { get; }

    IReadOnlyList<Phone> Phones { get; }

    IReadOnlyList<Email> Emails { get; }

    Result<Note> SetNotes(Note note);

    Result SetAddress(Address address);

    Result ClearAddress();

    Result<Phone> AddPhone(Phone phone);

    Result<Phone> RemovePhone(Phone phone);

    Result ReplacePhones(IReadOnlyList<Phone> phones);

    Result<Email> AddEmail(Email email);

    Result<Email> RemoveEmail(Email email);

    Result ReplaceEmails(IReadOnlyList<Email> emails);

    bool HasPhone(Phone phone);

    bool HasPrimaryPhone();

    bool IsUniqueContactableEmail(Email email);

    bool HasPrimaryEmail();
}