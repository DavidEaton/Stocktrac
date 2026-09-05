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
        var normalizedNotes = (notes ?? string.Empty)
            .Trim()
            .Truncate(NoteMaximumLength);

        return Result.Success(name)
            .Ensure(value => value is not null, RequiredMessage)
            .Map(validName => new Person(
                name: validName,
                notes: normalizedNotes,
                address: address,
                emails: emails,
                phones: phones,
                birthday: birthday,
                driversLicense: driversLicense));
    }

    public Result<PersonName> SetName(PersonName name) =>
        Result.Success(Name = name);

    public Result<Birthday?> SetBirthday(Birthday birthday) =>
        Result.Success(Birthday = birthday);

    public void RemoveBirthday() =>
        Birthday = null;

    public void RemoveDriversLicense() =>
        DriversLicense = null;

    public Result<DriversLicense?> SetDriversLicense(DriversLicense driversLicense) =>
        Result.Success(DriversLicense = driversLicense);

    public override string ToString() =>
        Name.ToString();
}
