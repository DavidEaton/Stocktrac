using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Domain.Features.Persons;

public class Person : Contactable, ICustomerEntity
{
    public const string NameRequiredMessage = "Person name is required";
    public PersonName Name { get; private set; }
    public Maybe<Birthday> Birthday { get; private set; }
    public Maybe<DriversLicense> DriversLicense { get; private set; }
    public EntityType EntityType => EntityType.Person;

    internal Person(
        PersonName name,
        Note notes,
        Maybe<Address> address,
        IReadOnlyList<Email> emails,
        IReadOnlyList<Phone> phones,
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
        Note notes,
        IReadOnlyList<Email> emails,
        IReadOnlyList<Phone> phones,
        Maybe<Birthday> birthday,
        Maybe<Address> address,
        Maybe<DriversLicense> driversLicense) =>
            Result.Success(new Person(name, notes, address, emails, phones, driversLicense, birthday));

    public Result<PersonName> SetName(PersonName name) =>
        Result.Success(Name = name);

    public Result<Maybe<Birthday>> SetBirthday(Birthday birthday) =>
        Result.Success(Birthday = birthday);

    public void RemoveBirthday() => Birthday = Maybe<Birthday>.None;

    public void RemoveDriversLicense() => DriversLicense = Maybe<DriversLicense>.None;

    public Result<Maybe<DriversLicense>> SetDriversLicense(DriversLicense driversLicense) =>
        Result.Success(DriversLicense = driversLicense);

    public override string ToString() =>
        Name.ToString();
}
