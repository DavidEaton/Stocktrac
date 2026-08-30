using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Domain.Features.Persons;

public class Person : Contactable, ICustomerEntity
{
    public PersonName Name { get; private set; }

    public Birthday? Birthday { get; private set; }

    public DriversLicense? DriversLicense { get; private set; }

    public EntityType EntityType => EntityType.Person;

    internal Person(
        PersonName name,
        string notes,
        Maybe<Address> address,
        IReadOnlyList<Email>? emails,
        IReadOnlyList<Phone>? phones,
        DriversLicense? driversLicense,
        Birthday? birthday)
        : base(notes, address, phones, emails)
    {
        Name = name;
        Birthday = birthday;
        DriversLicense = driversLicense;
    }

    public static Result<Person> Create(
        PersonName name,
        string? notes,
        Birthday? birthday = null,
        IReadOnlyList<Email>? emails = null,
        IReadOnlyList<Phone>? phones = null,
        Maybe<Address> address = default,
        DriversLicense? driversLicense = null)
    {
        if (name is null)
            return Result.Failure<Person>(RequiredMessage);

        notes = (notes ?? string.Empty)
            .Trim()
            .Truncate(NoteMaximumLength);

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

    public void SetBirthday(Birthday birthday) =>
        Birthday = birthday;

    public void RemoveBirthday() =>
        Birthday = null;

    public Result<DriversLicense> SetDriversLicense(
        DriversLicense driversLicense) =>
        driversLicense switch
        {
            null => Result.Failure<DriversLicense>(InvalidValueMessage),
            _ => Result.Success(DriversLicense = driversLicense)
        };

    public override string ToString() =>
        Name.ToString();

    // EF requires a parameterless constructor.
    private Person()
    {
        Name = PersonName.Create(string.Empty, string.Empty).Value;
        Birthday = null;
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