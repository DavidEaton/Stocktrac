using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

public class Business : Contactable, ICustomerEntity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const string InvalidMessage = "Invalid business.";

    public BusinessName Name { get; private set; }
    public Maybe<Person> Contact { get; private set; }
    public override string ToString() => Name.Name;
    public EntityType EntityType => EntityType.Business;

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
        IReadOnlyList<Email> emails,
        IReadOnlyList<Phone> phones)
    {
        return ValidateContactCollections(phones, emails)
            .Map(contacts => new Business(
                name, address, notes, contact, contacts));
    }

    public void WithName(BusinessName name) => Name = name;

    public void SetContact(Person contact) => Contact = contact;

    public void ClearContact() => Contact = Maybe<Person>.None;

    // Code that pollutes our domain class (very minor impact in this case), but
    // is necessary for EntityFramework, makes our model <100% persistence ignorant.

    // EF requires a parameterless constructor
    private Business() =>
        Name = BusinessName.Create(
            "Business Name")
            .Value;
}
