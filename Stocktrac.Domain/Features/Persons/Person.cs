using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Domain.Features.Persons;

public class Person : Contactable, ICustomerEntity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public PersonName Name { get; private set; }
    public Maybe<Birthday> Birthday { get; private set; }
    public DriversLicense? DriversLicense { get; private set; }
    public override string ToString() => Name.ToString();
    public EntityType EntityType => EntityType.Person;

    internal Person(
        PersonName name,
        string notes,
        Maybe<Address> address,
        IReadOnlyList<Email>? emails,
        IReadOnlyList<Phone>? phones,
        DriversLicense? driversLicense,
        Maybe<Birthday> birthday)
        : base(notes, address, phones, emails)
    {
        Name = name;
        Birthday = birthday;
        DriversLicense = driversLicense;
    }

    public static Result<Person> Create(
        PersonName name,
        string? notes,
        Maybe<Birthday> birthday = default,
        IReadOnlyList<Email>? emails = null,
        IReadOnlyList<Phone>? phones = null,
        Maybe<Address> address = default,
        DriversLicense? driversLicense = null)
    {
        if (name is null)
            return Result.Failure<Person>(RequiredMessage);

        notes = (notes ?? string.Empty).Trim().Truncate(NoteMaximumLength);

        if (birthday.HasValue)
            return Result.Failure<Person>(InvalidValueMessage);

        return Result.Success(new Person(
            name: name,
            notes: notes,
            address: address,
            emails: emails,
            phones: phones,
            birthday: birthday,
            driversLicense: driversLicense));
    }

    public Result<PersonName> SetName(PersonName name) =>
        name is null
            ? Result.Failure<PersonName>(RequiredMessage)
            : Result.Success(Name = name);

    public Maybe<Birthday> SetBirthday(Birthday date) =>
        Birthday = date;

    public Result<DriversLicense> SetDriversLicense(DriversLicense driversLicense) =>
        driversLicense switch
        {
            null => Result.Failure<DriversLicense>(InvalidValueMessage),
            _ => Result.Success(DriversLicense = driversLicense)
        };

    // EF requires a parameterless constructor
    private Person()
    {
        Name = PersonName.Create(string.Empty, string.Empty).Value;
        Birthday = default;
        DriversLicense = DriversLicense.Create(
            string.Empty,
            State.MI,
            DateTimeRange.Create(
                DateTime.MinValue,
                DateTime.MinValue.AddYears(1))
            .Value)
        .Value;
    }
}
