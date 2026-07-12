using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public interface IContactable
{
    Address? Address { get; }
    Result SetAddress(Address address);
    Result ClearAddress();
    IReadOnlyList<Phone> Phones { get; }
    IReadOnlyList<Email> Emails { get; }
    string? Notes { get; }

    Result<Phone> AddPhone(Phone phone);
    Result<Phone> RemovePhone(Phone phone);
    Result<Email> AddEmail(Email email);
    Result<Email> RemoveEmail(Email email);
    bool HasPhone(Phone phone);
    bool HasEmail(Email email);
    bool HasPrimaryPhone();
    bool HasPrimaryEmail();
    void UpdateContactDetails(ContactDetails contactDetails);
}