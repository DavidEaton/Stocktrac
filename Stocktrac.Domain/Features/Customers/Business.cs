using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

public sealed class Business : Contactable
{
    public const string InvalidMessage = "Invalid business.";

    public BusinessName Name { get; private set; }
    public Maybe<Person> Contact { get; private set; }
    public override string ToString() => Name.ToString();

    private Business(
        BusinessName name,
        Maybe<Address> address,
        Note notes,
        Maybe<Person> contact,
        ValidatedContactCollections contacts)
        : base(notes, address, contacts)
    {
        Name = name;
        Contact = contact;
    }

    public static Result<Business> Create(
        BusinessName name,
        Maybe<Address> address,
        Note notes,
        Maybe<Person> contact,
        IReadOnlyList<ContactEmail> emails,
        IReadOnlyList<ContactPhone> phones) =>
        Result.Success(
            new Business(name, address, notes, contact, ValidateContactCollections(phones, emails).Value));

    public Result UpdateName(BusinessName name) =>
        Result.Success(
            new Business(name, Address, Notes, Contact, ValidateContactCollections(Phones, Emails).Value));

    public Result UpdateAddress(Address address) =>
        address is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => Address = address);

    public Result UpdateContact(Person contact) =>
        contact is null
            ? Result.Failure(InvalidMessage)
            : Result.Success().Tap(() => Contact = contact);

    public void RemoveContact() => Contact = Maybe<Person>.None;

    // EF requires a parameterless constructor
    private Business() =>
        Name = BusinessName.Create(
            NonEmptyString.Create(
                "Business Name").Value).Value;
}
