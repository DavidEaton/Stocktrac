using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

public sealed class Business : Contactable
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
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
            Result.Combine(
                Environment.NewLine,
                Result.FailureIf(name is null, InvalidMessage))
            .Bind(() => ValidateContactCollections(phones, emails))
            .Map(contactCollections => new Business(name!, address, notes, contact, contactCollections));

    public Result UpdateName(BusinessName name) =>
    Result.Success(
            new Business(name, Address, Notes, Contact, ValidateContactCollections(Phones, Emails).Value));

    public Result UpdateAddress (Address address) =>
        address is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => Address = address);
            
    public Result UpdateContact(Person contact) =>
        contact is null ? Result.Failure(InvalidMessage) : Result.Success().Tap(() => Contact = contact);

    public void RemoveContact() => Contact = Maybe<Person>.None;

    // Code that pollutes our domain class (very minor impact in this case), but
    // is necessary for EntityFramework, makes our model <100% persistence ignorant.

    // EF requires a parameterless constructor
    private Business() =>
        Name = BusinessName.Create(
            NonEmptyString.Create(
                "Business Name").Value).Value;
}
