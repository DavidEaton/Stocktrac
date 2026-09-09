using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public abstract class Contactable : Entity, IContactable
{
    public const string NonuniqueMessage = "Duplicate entry; each must be unique.";
    public const string PrimaryExistsMessage = "A primary contact already exists.";
    public const string MultiplePrimariesMessage = "Only one contact may be primary.";
    public const string NotFoundMessage = "Entry not found.";

    private readonly List<Phone> phones = [];

    private readonly List<Email> emails = [];

    public Note Notes { get; private set; }

    public Maybe<Address> Address { get; private set; }

    public IReadOnlyList<Phone> Phones => [.. phones];

    public IReadOnlyList<Email> Emails => [.. emails];

    protected Contactable(
        Note notes,
        Maybe<Address> address,
        ValidatedContactCollections contacts)
    {
        Notes = notes;
        Address = address;
        phones = [.. contacts.Phones];
        emails = [.. contacts.Emails];
    }

    public Result<Phone> AddPhone(Phone phone) =>
        Result.Success(phone)
            .Ensure(
                requestedPhone => !HasPhoneNumber(requestedPhone.Number),
                NonuniqueMessage)
            .Ensure(
                requestedPhone =>
                    !requestedPhone.IsPrimary || !HasPrimaryPhone(),
                PrimaryExistsMessage)
            .Tap(phones.Add);

    public Result<Phone> RemovePhone(Phone phone) =>
        Result.Success(phone)
            .Ensure(requestedPhone => HasPhoneNumber(requestedPhone.Number), NotFoundMessage)
            .Tap(requestedPhone =>
                phones.RemoveAll(existingPhone => existingPhone.Number == requestedPhone.Number));

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
        Result.Success(email)
            .Ensure(
                requestedEmail => !HasEmailAddress(requestedEmail.Address),
                NonuniqueMessage)
            .Ensure(
                requestedEmail =>
                    !requestedEmail.IsPrimary || !HasPrimaryEmail(),
                PrimaryExistsMessage)
            .Tap(emails.Add);

    public Result<Email> RemoveEmail(Email email) =>
        Result.Success(email)
            .Ensure(requestedEmail => HasEmailAddress(requestedEmail.Address), NotFoundMessage)
            .Tap(requestedEmail =>
                emails.RemoveAll(existingEmail => existingEmail.Address == requestedEmail.Address));

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

    public void SetNotes(Note note) =>
        Notes = note;

    public void SetAddress(Address address) =>
        Address = address;

    public void ClearAddress() =>
        Address = Maybe<Address>.None;

    public bool HasPhoneNumber(string number) =>
        phones.Any(existingPhone =>
            existingPhone.Number == number);

    public bool HasPrimaryPhone() =>
        phones.Any(phone => phone.IsPrimary);

    public bool HasEmailAddress(string address) =>
        emails.Any(existingEmail =>
            existingEmail.Address == address);

    public bool HasPrimaryEmail() =>
        emails.Any(email => email.IsPrimary);

    protected static Result<ValidatedContactCollections> ValidateContactCollections(
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
                    .Map(validEmails => new ValidatedContactCollections(
                        validPhones,
                        validEmails)));

    private static Result<IReadOnlyList<TContact>>
        ValidateContactList<TContact, TValue>(
            IReadOnlyList<TContact> contacts,
            Func<TContact, TValue> getValue,
            Func<TContact, bool> isPrimary)
        where TContact : class =>
        Result.Success(contacts)
            .Ensure(
                contactList =>
                    contactList
                        .GroupBy(getValue)
                        .All(group => group.Count() == 1),
                NonuniqueMessage)
            .Ensure(
                contactList =>
                    contactList.Count(isPrimary) <= 1,
                MultiplePrimariesMessage);

    protected sealed class ValidatedContactCollections
    {
        public IReadOnlyList<Phone> Phones { get; }
        public IReadOnlyList<Email> Emails { get; }

        private ValidatedContactCollections(
            IReadOnlyList<Phone> phones,
            IReadOnlyList<Email> emails) =>
            (Phones, Emails) = (phones, emails);
    }

    // Required by Entity Framework.
    protected Contactable() { }
}
