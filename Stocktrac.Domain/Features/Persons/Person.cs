using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Domain.Features.Persons;

public class Person : Contactable, ICustomerEntity
{
    public PersonName Name { get; private set; }

    public Maybe<Birthday> Birthday { get; private set; }

    public Maybe<DriversLicense> DriversLicense { get; private set; }

    public EntityType EntityType => EntityType.Person;

    internal Person(
        PersonName name,
        string? notes,
        Maybe<Address> address,
        IReadOnlyList<Email>? emails,
        IReadOnlyList<Phone>? phones,
        Maybe<DriversLicense> driversLicense,
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
        Birthday? birthday = null,
        IReadOnlyList<Email>? emails = null,
        IReadOnlyList<Phone>? phones = null,
        Maybe<Address> address = default,
        DriversLicense? driversLicense = null)
    {
        return Result.Success(name)
            .Map(validName => new Person(
                validName,
                notes,
                address,
                emails,
                phones,
                ToMaybe(driversLicense),
                ToMaybe(birthday)));
    }

    private static Maybe<Birthday> ToMaybe(Birthday? birthday) =>
        birthday.HasValue ? birthday.Value : Maybe<Birthday>.None;

    private static Maybe<DriversLicense> ToMaybe(DriversLicense? driversLicense) =>
        driversLicense.HasValue ? driversLicense.Value : Maybe<DriversLicense>.None;

    public Result<PersonName> SetName(PersonName name) =>
        Result.Success(Name = name);

    public Result<Maybe<Birthday>> SetBirthday(Birthday birthday) =>
        Result.Success(Birthday = birthday);

    public void RemoveBirthday() =>
        Birthday = Maybe<Birthday>.None;

    public void RemoveDriversLicense() =>
        DriversLicense = Maybe<DriversLicense>.None;

    public Result<Maybe<DriversLicense>> SetDriversLicense(DriversLicense driversLicense) =>
        Result.Success(DriversLicense = driversLicense);

    public override string ToString() =>
        Name.ToString();
}
