using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public abstract class Contactable : Entity, IContactable
{
    // Targeting tests at the abstract base class binds them to the code’s implementation details.
    // Always test only concrete classes; don’t test abstract classes directly
    public const int NoteMaximumLength = 10000;
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
        Maybe<Address> address,
        IReadOnlyList<Phone>? phones,
        IReadOnlyList<Email>? emails)
    {
        Notes = notes?
            .Trim()
            .Truncate(NoteMaximumLength);

        Address = address;

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

    public Result ReplacePhones(IReadOnlyList<Phone> requestedPhones)
    {
        var validation = ValidateContacts(
            requestedPhones,
            phone => phone.Number,
            phone => phone.IsPrimary);

        if (validation.IsFailure)
            return validation;

        phones.Clear();
        phones.AddRange(requestedPhones);
        return Result.Success();
    }

    public Result ReplaceEmails(IReadOnlyList<Email> requestedEmails)
    {
        var validation = ValidateContacts(
            requestedEmails,
            email => email.Address,
            email => email.IsPrimary);

        if (validation.IsFailure)
            return validation;

        emails.Clear();
        emails.AddRange(requestedEmails);
        return Result.Success();
    }

    public Result<string> SetNotes(string note) =>
        Result.Success(Notes = note.Trim().Truncate(NoteMaximumLength));

    public Result SetAddress(Address address) =>
        Result.Success(Address = address);

    public Result ClearAddress() =>
        Result.Success(Address = Maybe<Address>.None);

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

    private static Result ValidateContacts<TContact, TValue>(
        IReadOnlyList<TContact>? contacts,
        Func<TContact, TValue> getValue,
        Func<TContact, bool> isPrimary)
        where TContact : class
    {
        if (contacts is null)
            return Result.Failure(RequiredMessage);

        if (contacts.Any(contact => contact is null))
            return Result.Failure(RequiredMessage);

        if (contacts.GroupBy(getValue).Any(group => group.Count() > 1))
            return Result.Failure(NonuniqueMessage);

        if (contacts.Count(isPrimary) > 1)
            return Result.Failure(PrimaryExistsMessage);

        return Result.Success();
    }

    // EF requires a parameterless constructor
    protected Contactable()
    {
        phones = [];
        emails = [];
    }
}
