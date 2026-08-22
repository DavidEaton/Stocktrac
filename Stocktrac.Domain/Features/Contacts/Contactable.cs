using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public abstract class Contactable : Entity, IContactable
{
    // Targeting tests at the abstract base class binds them to the code’s implementation details.
    // Always test only concrete classes; don’t test abstract classes directly
    public static readonly int NoteMaximumLength = 10000;
    public static readonly string NoteMaximumLengthMessage = $"Notes must be {NoteMaximumLength} or fewer characters in length.";
    public static readonly string RequiredMessage = "Please complete all required entries.";
    public static readonly string NonuniqueMessage = "Duplicate entry; each must be unique.";
    public static readonly string PrimaryExistsMessage = "Primary has already been entered.";
    public static readonly string InvalidValueMessage = "Invalid value";
    public static readonly string NotFoundMessage = "Entry not found";
    public string? Notes { get; private set; }
    public Maybe<Address> Address { get; private set; }

    private readonly List<Phone> phones = [];
    public IReadOnlyList<Phone> Phones => [.. phones];
    private readonly List<Email> emails = [];
    public IReadOnlyList<Email> Emails => [.. emails];

    internal Contactable(
        string? notes,
        Address? address,
        IReadOnlyList<Phone>? phones,
        IReadOnlyList<Email>? emails)
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

        if (!IsUniqueContactableEmail(email))
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

    public Result SetAddress(Address address) =>
        address is null
            ? Result.Failure<Address>(RequiredMessage)
            : Result.Success(Address = address);

    public Result ClearAddress() =>
        Result.Success(Address = null);

    public bool HasPhone(Phone phone) =>
        Phones.Any(existingPhone =>
            existingPhone.Number == phone.Number);

    public bool HasPrimaryPhone() =>
        Phones.Any(existingPhone =>
            existingPhone.IsPrimary);

    public bool IsUniqueContactableEmail(Email email) =>
        !Emails.Any(existingEmail =>
            existingEmail.Address == email.Address);

    public bool HasPrimaryEmail() =>
        Emails.Any(email =>
            email.IsPrimary);

    public void UpdateContactDetails(ContactDetails contactDetails)
    {
        UpdatePhones(contactDetails?.Phones);
        UpdateEmails(contactDetails?.Emails);
        UpdateAddress(contactDetails?.Address);
    }

    private void UpdateAddress(Maybe<Address>? address) =>
        Address = address ?? Maybe<Address>.None;

    private void UpdatePhones(IReadOnlyList<Phone>? requestedPhones)
    {
        if (requestedPhones is null || requestedPhones.Count < 1)
            return;

        ValidateContactDetails(
            contacts: requestedPhones,
            getId: phone => phone.Id,
            getValue: phone => phone.Number,
            isPrimary: phone => phone.IsPrimary);

        if (requestedPhones is null || requestedPhones.Count < 1)
            return;

        var toAdd = requestedPhones
            .Where(phone => phone.Id == 0);

        toAdd.ToList()
            .ForEach(phone =>
                AddPhoneOrThrow(phone));

        var toUpdate = requestedPhones
            .Where(phone =>
                phones.Any(callerPhone =>
                    callerPhone.Id == phone.Id));

        toUpdate.ToList()
            .ForEach(phone =>
                UpdatePhone(
                    existingPhone: phones.First(callerPhone =>
                        callerPhone.Id == phone.Id),
                    requestedPhone: phone));
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

    private void UpdateEmails(IReadOnlyList<Email>? requestedEmails)
    {
        if (requestedEmails is null || requestedEmails.Count < 1)
            return;

        ValidateContactDetails(
            contacts: requestedEmails,
            getId: email => email.Id,
            getValue: email => email.Address,
            isPrimary: email => email.IsPrimary);

        if (requestedEmails is null || requestedEmails.Count < 1)
            return;

        var toAdd = requestedEmails
            .Where(email => email.Id == 0);
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
        var result = AddEmail(email);
        if (result.IsFailure)
            throw new Exception(result.Error);
    }

    private static void ValidateContactDetails<TContact, TValue>(
        IReadOnlyList<TContact>? contacts,
        Func<TContact, long> getId,
        Func<TContact, TValue> getValue,
        Func<TContact, bool> isPrimary)
        where TContact : class
    {
        if (contacts is null)
        {
            // Log warning: "Contacts list is null. No validation performed."
            return;
        }

        if (contacts.Any(contact => contact is null))
            throw new Exception(RequiredMessage);

        if (contacts.Where(contact =>
            getId(contact) != 0)
            .GroupBy(getId)
            .Any(group => group.Count() > 1))
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
