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
        IReadOnlyList<ContactPhone> phones)
    {
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(name is null, InvalidMessage))
            .Bind(() => ValidateContactCollections(phones, emails))
            .Map(contacts => new Business(name!, address, notes, contact, contacts));
    }

    public Result WithName(BusinessName name) =>
        name is null ? Result.Failure(InvalidMessage) : Result.Success().Tap(() => Name = name);

    public Result SetContact(Person contact) =>
        contact is null ? Result.Failure(InvalidMessage) : Result.Success().Tap(() => Contact = contact);

    public void ClearContact() => Contact = Maybe<Person>.None;

    // Code that pollutes our domain class (very minor impact in this case), but
    // is necessary for EntityFramework, makes our model <100% persistence ignorant.

    // EF requires a parameterless constructor
    private Business() =>
        Name = BusinessName.Create(
            "Business Name")
            .Value;
}
