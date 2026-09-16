using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Customers;

namespace Stocktrac.Domain.Features.Persons;

public class Person : Contactable, ICustomerEntity
{
    public const string NameRequiredMessage = "Person name is required.";
    public PersonName Name { get; private set; }
    public Maybe<Birthday> Birthday { get; private set; }
    public Maybe<DriversLicense> DriversLicense { get; private set; }
    public EntityType EntityType => EntityType.Person;

    private Person(
        PersonName name,
        Note notes,
        Maybe<Address> address,
        ValidatedContactCollections contacts,
        Maybe<DriversLicense> driversLicense,
        Maybe<Birthday> birthday)
        : base(notes, address, contacts)
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
        Result.Combine(
                Environment.NewLine,
                Result.FailureIf(name is null, NameRequiredMessage))
            .Bind(() => ValidateContactCollections(phones, emails))
            .Map(contacts => new Person(
                name!, notes, address, contacts, driversLicense, birthday));

    public Result WithName(PersonName name) =>
        name is null
            ? Result.Failure(NameRequiredMessage)
            : Result.Success().Tap(() => Name = name);

    public Result WithBirthday(Birthday birthday) =>
        birthday is null
            ? Result.Failure(Contactable.RequiredMessage)
            : Result.Success().Tap(() => Birthday = birthday);

    public void RemoveBirthday() => Birthday = Maybe<Birthday>.None;

    public void RemoveDriversLicense() => DriversLicense = Maybe<DriversLicense>.None;

    public Result WithDriversLicense(DriversLicense driversLicense) =>
        driversLicense is null
            ? Result.Failure(Contactable.RequiredMessage)
            : Result.Success().Tap(() => DriversLicense = driversLicense);

    public override string ToString() =>
        Name.ToString();
}
