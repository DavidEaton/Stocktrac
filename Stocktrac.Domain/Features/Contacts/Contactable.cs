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

    private readonly ContactCollection<ContactPhone, PhoneNumber> phoneCollection;

    private readonly ContactCollection<ContactEmail, EmailAddress> emailCollection;

    public Note Notes { get; private set; }

    public Maybe<Address> Address { get; private set; }

    public IReadOnlyList<ContactPhone> Phones => phoneCollection.Items;

    public IReadOnlyList<ContactEmail> Emails => emailCollection.Items;

    protected Contactable(
        Note notes,
        Maybe<Address> address,
        ValidatedContactCollections contacts)
    {
        Notes = notes;
        Address = address;
        phones = [.. contacts.Phones];
        emails = [.. contacts.Emails];
        phoneCollection = new(phones, phone => phone.Number);
        emailCollection = new(emails, email => email.Address);
    }

    public Result<ContactPhone> AddPhone(ContactPhone phone) => phoneCollection.Add(phone);

    public Result<ContactPhone> RemovePhone(ContactPhone phone) => phoneCollection.Remove(phone);

    public Result ReplacePhones(IReadOnlyList<ContactPhone> requestedPhones) =>
        phoneCollection.Replace(requestedPhones);

    public Result<ContactEmail> AddEmail(ContactEmail email) => emailCollection.Add(email);

    public Result<ContactEmail> RemoveEmail(ContactEmail email) => emailCollection.Remove(email);

    public Result ReplaceEmails(IReadOnlyList<ContactEmail> requestedEmails) =>
        emailCollection.Replace(requestedEmails);

    public Result WithNotes(Note note) => Result.Success().Tap(() => Notes = note);

    public Result WithAddress(Address address) =>
        address is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => Address = address);

    public void WithoutAddress() => Address = Maybe<Address>.None;

    public bool HasPhoneNumber(string number) =>
        PhoneNumber.Create(number)
            .Match(phoneCollection.Contains, otherwise => false);

    public bool HasPrimaryPhone() => phoneCollection.HasPrimary;

    public bool HasEmailAddress(string address) =>
        EmailAddress.Create(address)
            .Match(HasEmailAddress, otherwise => false);

    private bool HasEmailAddress(EmailAddress address) =>
        emailCollection.Contains(address);

    public bool HasPrimaryEmail() => emailCollection.HasPrimary;

    protected static Result<ValidatedContactCollections> ValidateContactCollections(
            IReadOnlyList<ContactPhone> phones,
            IReadOnlyList<ContactEmail> emails) =>
        ContactCollection<ContactPhone, PhoneNumber>.Validate(
                phones,
                phone => phone.Number)
            .Bind(validPhones =>
                ContactCollection<ContactEmail, EmailAddress>.Validate(
                        emails,
                        email => email.Address)
                    .Map(validEmails => new ValidatedContactCollections(
                        validPhones,
                        validEmails)));

    // Required by Entity Framework.
    protected Contactable()
    {
        phoneCollection = new(phones, phone => phone.Number);
        emailCollection = new(emails, email => email.Address);
    }
}
