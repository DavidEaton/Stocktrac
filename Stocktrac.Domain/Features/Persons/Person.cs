using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contact;

namespace Stocktrac.Domain.Features.Persons;

public class Person : Contactable, ICustomerEntity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public PersonName Name { get; private set; }
    public DateTime? Birthday { get; private set; }
    public DriversLicense DriversLicense { get; private set; }
    public override string ToString() => Name.ToString();
    public string DisplayName => Name.ToString();
    public EntityType EntityType => EntityType.Person;

    internal Person(
        PersonName name,
        string notes,
        Address address,
        IReadOnlyList<Email> emails,
        IReadOnlyList<Phone> phones,
        DateTime? birthday = null,
        DriversLicense driversLicense = null)
        : base(notes, address, phones, emails)
    {
        Name = name;
        Birthday = birthday;
        DriversLicense = driversLicense;
    }

    public static Result<Person> Create(
        PersonName name,
        string notes,
        DateTime? birthday = null,
        IReadOnlyList<Email> emails = null,
        IReadOnlyList<Phone> phones = null,
        Address address = null,
        DriversLicense driversLicense = null)
    {
        if (name is null)
            return Result.Failure<Person>(RequiredMessage);

        notes = (notes ?? string.Empty).Trim().Truncate(NoteMaximumLength);

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > NoteMaximumLength)
            return Result.Failure<Person>(NoteMaximumLengthMessage);

        if (birthday.HasValue)
            if (!IsValidAgeOn(birthday))
                return Result.Failure<Person>(InvalidValueMessage);

        return Result.Success(new Person(name, notes, address, emails, phones, birthday, driversLicense));
    }

    public Result<PersonName> SetName(PersonName name) =>
        name is null
            ? Result.Failure<PersonName>(RequiredMessage)
            : Result.Success(Name = name);

    public Result<DateTime?> SetBirthday(DateTime? birthday) => !IsValidAgeOn(birthday)
            ? Result.Failure<DateTime?>(InvalidValueMessage)
            : Result.Success(Birthday = birthday);

    public Result<DriversLicense> SetDriversLicense(DriversLicense driversLicense) =>
        driversLicense switch
        {
            null => Result.Failure<DriversLicense>(InvalidValueMessage),
            _ => Result.Success(DriversLicense = driversLicense)
        };

    public static bool IsValidAgeOn(DateTime? birthDate)
    {
        if (birthDate is null)
            return false;

        if (!birthDate.HasValue)
            return false;

        if (birthDate >= DateTime.Today)
            return false;

        int thisYear = DateTime.Today.Year;
        int birthYear = birthDate.Value.Year;

        if (birthYear <= thisYear && birthYear > (thisYear - 120))
            return true;

        return false;
    }

    // EF requires a parameterless constructor
    protected Person() { }
}
