using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contact;

public abstract class Contactable : Entity, IContactable
{
    // Targeting tests at the abstract base class binds them to the code’s implementation details.
    // Always test only concrete classes; don’t test abstract classes directly
    public static readonly int NoteMaximumLength = 10000;
    public static readonly string NoteMaximumLengthMessage = $"Notes cannot be over {NoteMaximumLength} characters in length.";
    public static readonly string RequiredMessage = "Please include all required items.";
    public static readonly string NonuniqueMessage = $"Item has already been entered; each item must be unique.";
    public static readonly string PrimaryExistsMessage = $"Primary has already been entered.";
    public static readonly string InvalidValueMessage = $"Value was invalid";
    public static readonly string NotFoundMessage = $"Item was not found";
    public string? Notes { get; private set; }
    public Address? Address { get; private set; }

    private readonly List<Phone> phones = [];
    public IReadOnlyList<Phone> Phones => [.. phones];
    private readonly List<Email> emails = [];
    public IReadOnlyList<Email> Emails => [.. emails];

    internal Contactable(string? notes, Address? address, IReadOnlyList<Phone>? phones, IReadOnlyList<Email>? emails)
    {
        Notes = notes?
            .Trim()
            .Truncate(NoteMaximumLength);

        if (address is not null)
            SetAddress(address);

        if (phones is not null)
            foreach (var phone in phones)
                AddPhone(phone);

        if (emails is not null)
            foreach (var email in emails)
                AddEmail(email);
    }

    public Result<Email> AddEmail(Email email)
    {
        if (email is null)
            return Result.Failure<Email>(RequiredMessage);

        if (HasEmail(email))
            return Result.Failure<Email>(NonuniqueMessage);

        if (HasPrimaryEmail() && email.IsPrimary)
            return Result.Failure<Email>(PrimaryExistsMessage);

        emails.Add(email);

        return Result.Success(email);
    }

    public Result<Email> RemoveEmail(Email email)
    {
        if (email is null)
            return Result.Failure<Email>(RequiredMessage);

        if (!emails.Contains(email))
            return Result.Failure<Email>(NotFoundMessage);

        emails.Remove(email);

        return Result.Success(email);
    }

    public Result<Phone> AddPhone(Phone phone)
    {
        if (phone is null)
            return Result.Failure<Phone>(RequiredMessage);

        if (HasPhone(phone))
            return Result.Failure<Phone>(NonuniqueMessage);

        if (HasPrimaryPhone() && phone.IsPrimary)
            return Result.Failure<Phone>(PrimaryExistsMessage);

        phones.Add(phone);

        return Result.Success(phone);
    }

    public Result<Phone> RemovePhone(Phone phone)
    {
        if (phone is null)
            return Result.Failure<Phone>(RequiredMessage);

        if (!phones.Contains(phone))
            return Result.Failure<Phone>(NotFoundMessage);

        phones.Remove(phone);

        return Result.Success(phone);
    }

    public Result<string> SetNotes(string note) =>
        Result.Success(Notes = note.Trim().Truncate(NoteMaximumLength));

    public Result SetAddress(Address address)
    {
        if (address is null)
            return Result.Failure<Address>(RequiredMessage);

        // Address (if present) is guaranteed to be valid;
        // it was validated on creation.
        return Result.Success(Address = address);
    }

    public Result ClearAddress() =>
        Result.Success(Address = null);

    public bool HasPhone(Phone phone) =>
        Phones.Any(existingPhone => existingPhone.Number == phone.Number);

    public bool HasPrimaryPhone() =>
        Phones.Any(existingPhone => existingPhone.IsPrimary);

    public bool HasEmail(Email email) =>
        Emails.Any(existingEmail => existingEmail.Address == email.Address);

    public bool HasPrimaryEmail() =>
        Emails.Any(email => email.IsPrimary);

    public void UpdateContactDetails(ContactDetails contactDetails)
    {
        UpdatePhones(contactDetails.Phones);
        UpdateEmails(contactDetails.Emails);
        Address = contactDetails.Address.GetValueOrDefault();
    }

    private void UpdatePhones(IReadOnlyList<Phone> requestedPhones)
    {
        ValidateContactDetails(requestedPhones, phone => phone.Id, phone => phone.Number, phone => phone.IsPrimary);

        var requestedPhonesById = requestedPhones
            .Where(phone => phone.Id != 0)
            .ToDictionary(phone => phone.Id);

        foreach (var existingPhone in phones.ToArray())
        {
            if (!requestedPhonesById.TryGetValue(existingPhone.Id, out var requestedPhone))
            {
                RemovePhone(existingPhone);
                continue;
            }

            UpdatePhone(existingPhone, requestedPhone);
        }

        foreach (var requestedPhone in requestedPhones.Where(phone => phone.Id == 0))
            AddPhoneOrThrow(requestedPhone);
    }

    private static void UpdatePhone(Phone existingPhone, Phone requestedPhone)
    {
        if (existingPhone.Number != requestedPhone.Number)
            existingPhone.SetNumber(requestedPhone.Number);

        if (existingPhone.PhoneType != requestedPhone.PhoneType)
            existingPhone.SetPhoneType(requestedPhone.PhoneType);

        if (existingPhone.IsPrimary != requestedPhone.IsPrimary)
            existingPhone.SetIsPrimary(requestedPhone.IsPrimary);
    }

    private void AddPhoneOrThrow(Phone phone)
    {
        var result = AddPhone(phone);
        if (result.IsFailure)
            throw new Exception(result.Error);
    }

    private void UpdateEmails(IReadOnlyList<Email> requestedEmails)
    {
        ValidateContactDetails(requestedEmails, email => email.Id, email => email.Address, email => email.IsPrimary);

        var requestedEmailsById = requestedEmails
            .Where(email => email.Id != 0)
            .ToDictionary(email => email.Id);

        foreach (var existingEmail in emails.ToArray())
        {
            if (!requestedEmailsById.TryGetValue(existingEmail.Id, out var requestedEmail))
            {
                RemoveEmail(existingEmail);
                continue;
            }

            UpdateEmail(existingEmail, requestedEmail);
        }

        foreach (var requestedEmail in requestedEmails.Where(email => email.Id == 0))
            AddEmailOrThrow(requestedEmail);
    }

    private static void UpdateEmail(Email existingEmail, Email requestedEmail)
    {
        if (existingEmail.Address != requestedEmail.Address)
            existingEmail.SetAddress(requestedEmail.Address);

        if (existingEmail.IsPrimary != requestedEmail.IsPrimary)
            existingEmail.SetIsPrimary(requestedEmail.IsPrimary);
    }

    private void AddEmailOrThrow(Email email)
    {
        Result result = AddEmail(email);
        if (result.IsFailure)
            throw new Exception(result.Error);
    }

    private static void ValidateContactDetails<TContact, TValue>(
        IReadOnlyList<TContact> contacts,
        Func<TContact, long> getId,
        Func<TContact, TValue> getValue,
        Func<TContact, bool> isPrimary)
        where TContact : class
    {
        if (contacts.Any(contact => contact is null))
            throw new Exception(RequiredMessage);

        if (contacts.Where(contact => getId(contact) != 0).GroupBy(getId).Any(group => group.Count() > 1))
            throw new Exception(NonuniqueMessage);

        if (contacts.GroupBy(getValue).Any(group => group.Count() > 1))
            throw new Exception(NonuniqueMessage);

        if (contacts.Count(isPrimary) > 1)
            throw new Exception(PrimaryExistsMessage);
    }

    // EF requires a parameterless constructor
    protected Contactable()
    {
        phones = [];
        emails = [];
    }
}
