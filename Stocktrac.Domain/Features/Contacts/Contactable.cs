using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public abstract partial class Contactable : Entity, IContactable
{
    public const string NonuniqueMessage = "Duplicate entry; each must be unique.";
    public const string PrimaryExistsMessage = "A primary contact already exists.";
    public const string MultiplePrimariesMessage = "Only one contact may be primary.";
    public const string NotFoundMessage = "Entry not found.";
    public const string RequiredMessage = "Please include all required items.";

    private readonly List<ContactPhone> phones = [];

    private readonly List<ContactEmail> emails = [];

    public Note Notes { get; private set; }

    public Maybe<Address> Address { get; private set; }

    public IReadOnlyList<ContactPhone> Phones => [.. phones];

    public IReadOnlyList<ContactEmail> Emails => [.. emails];

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

    public Result<ContactPhone> AddPhone(ContactPhone phone) =>
        phone is null
            ? Result.Failure<ContactPhone>(RequiredMessage)
            : Result.Success(phone)
            .Ensure(
                requestedPhone => !phones.Any(existingPhone => existingPhone.Number == requestedPhone.Number),
                NonuniqueMessage)
            .Ensure(
                requestedPhone =>
                    !requestedPhone.IsPrimary || !HasPrimaryPhone(),
                PrimaryExistsMessage)
            .Tap(phones.Add);

    public Result<ContactPhone> RemovePhone(ContactPhone phone) =>
        phone is null
            ? Result.Failure<ContactPhone>(RequiredMessage)
            : Result.Success(phone)
            .Ensure(
                requestedPhone => phones.Any(existingPhone => existingPhone.Number == requestedPhone.Number),
                NotFoundMessage)
            .Tap(requestedPhone =>
                phones.RemoveAll(existingPhone => existingPhone.Number == requestedPhone.Number));

    public Result ReplacePhones(IReadOnlyList<ContactPhone> requestedPhones) =>
        ValidateContactList(
                requestedPhones,
                phone => phone.Number)
            .Bind(validPhones =>
            {
                phones.Clear();
                phones.AddRange(validPhones);

                return Result.Success();
            });

    public Result<ContactEmail> AddEmail(ContactEmail email) =>
        email is null
            ? Result.Failure<ContactEmail>(RequiredMessage)
            : Result.Success(email)
            .Ensure(
                requestedEmail => !HasEmailAddress(requestedEmail.Address),
                NonuniqueMessage)
            .Ensure(
                requestedEmail =>
                    !requestedEmail.IsPrimary || !HasPrimaryEmail(),
                PrimaryExistsMessage)
            .Tap(emails.Add);

    public Result<ContactEmail> RemoveEmail(ContactEmail email) =>
        email is null
            ? Result.Failure<ContactEmail>(RequiredMessage)
            : Result.Success(email)
            .Ensure(requestedEmail => HasEmailAddress(requestedEmail.Address), NotFoundMessage)
            .Tap(requestedEmail =>
                emails.RemoveAll(existingEmail => existingEmail.Address == requestedEmail.Address));

    public Result ReplaceEmails(IReadOnlyList<ContactEmail> requestedEmails) =>
        ValidateContactList(
                requestedEmails,
                email => email.Address)
            .Bind(validEmails =>
            {
                emails.Clear();
                emails.AddRange(validEmails);

                return Result.Success();
            });

    public Result WithNotes(Note note) => Result.Success().Tap(() => Notes = note);

    public Result WithAddress(Address address) =>
        address is null ? Result.Failure(RequiredMessage) : Result.Success().Tap(() => Address = address);

    public void WithoutAddress() => Address = Maybe<Address>.None;

    public bool HasPhoneNumber(string number)
    {
        var validNumber = PhoneNumber.Create(number);

        return validNumber.IsSuccess &&
            phones.Any(existingPhone => existingPhone.Number == validNumber.Value);
    }

    public bool HasPrimaryPhone() => phones.Any(phone => phone.IsPrimary);

    public bool HasEmailAddress(string address)
    {
        var validAddress = EmailAddress.Create(address);

        return validAddress.IsSuccess && HasEmailAddress(validAddress.Value);
    }

    private bool HasEmailAddress(EmailAddress address) =>
        emails.Any(existingEmail => existingEmail.Address == address);

    public bool HasPrimaryEmail() => emails.Any(email => email.IsPrimary);

    protected static Result<ValidatedContactCollections> ValidateContactCollections(
            IReadOnlyList<ContactPhone> phones,
            IReadOnlyList<ContactEmail> emails) =>
        ValidateContactList(
                phones,
                phone => phone.Number)
            .Bind(validPhones =>
                ValidateContactList(
                        emails,
                        email => email.Address)
                    .Map(validEmails => new ValidatedContactCollections(
                        validPhones,
                        validEmails)));

    private static Result<IReadOnlyList<TContact>>
        ValidateContactList<TContact, TValue>(
            IReadOnlyList<TContact> contacts,
            Func<TContact, TValue> getValue)
        where TContact : class, IHasPrimary =>
        contacts is null
            ? Result.Failure<IReadOnlyList<TContact>>(RequiredMessage)
            : contacts.Any(contact => contact is null)
                ? Result.Failure<IReadOnlyList<TContact>>(RequiredMessage)
                : Result.Success(contacts)
            .Ensure(
                contactList =>
                    contactList
                        .GroupBy(getValue)
                        .All(group => group.Count() == 1),
                NonuniqueMessage)
            .Ensure(
                contactList =>
                    contactList.Count(contact => contact.IsPrimary) <= 1,
                MultiplePrimariesMessage);

    // Required by Entity Framework.
    protected Contactable() { }
}
