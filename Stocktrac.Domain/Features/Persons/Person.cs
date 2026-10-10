using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Persons;

public sealed class Person : Contactable
{
    public const string NameRequiredMessage = "Person name is required.";
    public PersonName Name { get; private set; }
    public Maybe<Birthday> Birthday { get; private set; }
    public Maybe<DriversLicense> DriversLicense { get; private set; }

    private Person(
        PersonName name,
        Maybe<Birthday> birthday,
        Maybe<DriversLicense> driversLicense,
        Note notes,
        Maybe<Address> address,
        ValidatedContactCollections contacts)
        : base(notes, address, contacts)
    {
        Name = name;
        Birthday = birthday;
        DriversLicense = driversLicense;
    }

    public static Result<Person> Create(
        PersonName name,
        Note notes,
        IReadOnlyList<ContactEmail> emails,
        IReadOnlyList<ContactPhone> phones,
        Maybe<Birthday> birthday = default,
        Maybe<DriversLicense> driversLicense = default,
        Maybe<Address> address = default) =>
        name.AsRequired()
            .Bind(validName => ValidateContactCollections(phones, emails)
                .Map(contacts => new Person(
                    validName,
                    birthday,
                    driversLicense,
                    notes,
                    address,
                    contacts)));

    public Result UpdateName(PersonName name) =>
        name is null
            ? Result.Failure(NameRequiredMessage)
            : Result.Success().Tap(() => Name = name);

    public Result UpdateBirthday(Birthday birthday) =>
        birthday is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => Birthday = birthday);

    public Result UpdateDriversLicense(DriversLicense driversLicense) =>
        driversLicense is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => DriversLicense = driversLicense);

    public Result UpdateAddress (Address address) =>
        address is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => Address = address);

    public void RemoveBirthday() => Birthday = Maybe<Birthday>.None;

    public void RemoveDriversLicense() =>
        DriversLicense = Maybe<DriversLicense>.None;

    public override string ToString() => Name.ToString();
}
