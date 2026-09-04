using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Maybe<Address> Address { get; }
    Result SetAddress(Address address);
    Result ClearAddress();
    IReadOnlyList<Phone> Phones { get; }
    IReadOnlyList<Email> Emails { get; }
    string? Notes { get; }

    Result<Phone> AddPhone(Phone phone);
    Result<Phone> RemovePhone(Phone phone);
    Result ReplacePhones(IReadOnlyList<Phone> phones);
    Result<Email> AddEmail(Email email);
    Result<Email> RemoveEmail(Email email);
    Result ReplaceEmails(IReadOnlyList<Email> emails);
    bool HasPhone(Phone phone);
    bool IsUniqueContactableEmail(Email email);
    bool HasPrimaryPhone();
    bool HasPrimaryEmail();
}
