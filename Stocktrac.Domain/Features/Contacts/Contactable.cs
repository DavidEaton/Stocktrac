using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public abstract class Contactable : Entity, IContactable
{
    public static readonly string RequiredMessage =
        "Please complete all required entries.";

    public static readonly string NonuniqueMessage =
        "Duplicate entry; each must be unique.";

    public static readonly string PrimaryExistsMessage =
        "Primary has already been entered.";

    public static readonly string InvalidValueMessage =
        "Invalid value.";

    public static readonly string NotFoundMessage =
        "Entry not found.";

    private readonly List<Phone> phones = [];

    private readonly List<Email> emails = [];

    public Note Notes { get; private set; }

    public Maybe<Address> Address { get; private set; }

    public IReadOnlyList<Phone> Phones => [.. phones];

    public IReadOnlyList<Email> Emails => [.. emails];

    protected Contactable(
        Note notes,
        Maybe<Address> address,
        IReadOnlyList<Phone> phones,
        IReadOnlyList<Email> emails)
    {
        Notes = notes;
        Address = address;
        this.phones = [.. phones];
        this.emails = [.. emails];
    }

    public Result<Phone> AddPhone(Phone phone) =>
        Maybe.From(phone)
            .ToResult(RequiredMessage)
            .Ensure(
                requestedPhone => !HasPhone(requestedPhone),
                NonuniqueMessage)
            .Ensure(
                requestedPhone =>
                    !requestedPhone.IsPrimary || !HasPrimaryPhone(),
                PrimaryExistsMessage)
            .Tap(phones.Add);

    public Result<Phone> RemovePhone(Phone phone) =>
        Maybe.From(phone)
            .ToResult(RequiredMessage)
            .Ensure(
                phones.Contains,
                NotFoundMessage)
            .Tap(requestedPhone => phones.Remove(requestedPhone));

    public Result ReplacePhones(IReadOnlyList<Phone> requestedPhones) =>
        ValidateContactList(
                requestedPhones,
                phone => phone.Number,
                phone => phone.IsPrimary)
            .Bind(validPhones =>
            {
                phones.Clear();
                phones.AddRange(validPhones);

                return Result.Success();
            });

    public Result<Email> AddEmail(Email email) =>
        Maybe.From(email)
            .ToResult(RequiredMessage)
            .Ensure(
                IsUniqueContactableEmail,
                NonuniqueMessage)
            .Ensure(
                requestedEmail =>
                    !requestedEmail.IsPrimary || !HasPrimaryEmail(),
                PrimaryExistsMessage)
            .Tap(emails.Add);

    public Result<Email> RemoveEmail(Email email) =>
        Maybe.From(email)
            .ToResult(RequiredMessage)
            .Ensure(
                emails.Contains,
                NotFoundMessage)
            .Tap(requestedEmail => emails.Remove(requestedEmail));

    public Result ReplaceEmails(IReadOnlyList<Email> requestedEmails) =>
        ValidateContactList(
                requestedEmails,
                email => email.Address,
                email => email.IsPrimary)
            .Bind(validEmails =>
            {
                emails.Clear();
                emails.AddRange(validEmails);

                return Result.Success();
            });

    public Result<Note> SetNotes(Note note) =>
        Result.Success(Notes = note);

    public Result SetAddress(Address address) =>
        Result.Success()
            .Tap(() => Address = address);

    public Result ClearAddress() =>
        Result.Success()
            .Tap(() => Address = Maybe<Address>.None);

    public bool HasPhone(Phone phone) =>
        phones.Any(existingPhone =>
            existingPhone.Number == phone.Number);

    public bool HasPrimaryPhone() =>
        phones.Any(phone => phone.IsPrimary);

    public bool IsUniqueContactableEmail(Email email) =>
        !emails.Any(existingEmail =>
            existingEmail.Address == email.Address);

    public bool HasPrimaryEmail() =>
        emails.Any(email => email.IsPrimary);

    protected static Result<(
        IReadOnlyList<Phone> Phones,
        IReadOnlyList<Email> Emails)> ValidateContactCollections(
            IReadOnlyList<Phone> phones,
            IReadOnlyList<Email> emails) =>
        ValidateContactList(
                phones,
                phone => phone.Number,
                phone => phone.IsPrimary)
            .Bind(validPhones =>
                ValidateContactList(
                        emails,
                        email => email.Address,
                        email => email.IsPrimary)
                    .Map(validEmails => (
                        Phones: validPhones,
                        Emails: validEmails)));

    private static Result<IReadOnlyList<TContact>>
        ValidateContactList<TContact, TValue>(
            IReadOnlyList<TContact> contacts,
            Func<TContact, TValue> getValue,
            Func<TContact, bool> isPrimary)
        where TContact : class =>
        Maybe.From(contacts)
            .ToResult(RequiredMessage)
            .Ensure(
                contactList =>
                    contactList.All(contact => contact is not null),
                RequiredMessage)
            .Ensure(
                contactList =>
                    contactList
                        .GroupBy(getValue)
                        .All(group => group.Count() == 1),
                NonuniqueMessage)
            .Ensure(
                contactList =>
                    contactList.Count(isPrimary) <= 1,
                PrimaryExistsMessage);

    // Required by Entity Framework.
    protected Contactable() { }
}
