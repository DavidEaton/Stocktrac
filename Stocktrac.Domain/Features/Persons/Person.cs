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
            .Ensure(validName => validName is not null, NameRequiredMessage)
            .Map(validName => new Person(
                validName,
                notes,
                address,
                emails,
                phones,
                ToMaybe(driversLicense),
                ToMaybe(birthday)));
    }

    private static Maybe<Birthday> ToMaybe(Birthday? birthday)
    {
        if (birthday is null)
            return Maybe<Birthday>.None;

        return birthday;
    }

    private static Maybe<DriversLicense> ToMaybe(DriversLicense? driversLicense)
    {
        if (driversLicense is null)
            return Maybe<DriversLicense>.None;

        return driversLicense;
    }

    public Result<PersonName> SetName(PersonName name) =>
        name is null
            ? Result.Failure<PersonName>(NameRequiredMessage)
            : Result.Success(Name = name);

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
