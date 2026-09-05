using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

public class Business : Contactable, ICustomerEntity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static readonly string InvalidMessage = $"Invalid business.";

    public BusinessName Name { get; private set; }
    public Maybe<Person> Contact { get; private set; }
    public override string ToString() => Name.Name;
    public EntityType EntityType => EntityType.Business;

    private Business(
        BusinessName name,
        Maybe<Address> address,
        string? notes = null,
        Maybe<Person> contact = default,
        IReadOnlyList<Phone>? phones = null,
        IReadOnlyList<Email>? emails = null)
        : base(notes, address, phones, emails)
    {
        Name = name;
        Contact = contact;
    }

    public static Result<Business> Create(
        BusinessName name,
        Maybe<Address> address,
        string? notes = null,
        Person? contact = null,
        IReadOnlyList<Email>? emails = null,
        IReadOnlyList<Phone>? phones = null)
    {
        // ValueObject parameters are already validated by BusinessValidator,
        // which runs within the asp.net request pipeline, invoking each
        // ValueObject's contract validator. For example, AddressValidator :
        // AbstractValidator<AddressRequest>
        // Only the primitive type (vs. ValueObject type) Notes property is
        // transformed and validated (parsed) here in the domain class that
        // creates it.
        return Result.Success(name)
            .Map(validName => new Business(
                validName, address, notes, ToMaybe(contact), phones, emails));
    }

    private static Maybe<Person> ToMaybe(Person? contact) =>
        contact is null ? Maybe<Person>.None : contact;

    // BusinessName has already been validated; no need to validate
    public void SetName(BusinessName name) =>
        Name = name;

    // Person has already been validated; no need to validate
    public void SetContact(Person contact) =>
        Contact = contact;

    // Code that pollutes our domain class (very minor impact in this case), but
    // is necessary for EntityFramework, makes our model <100% persistence ignorant.

    // EF requires a parameterless constructor
    private Business() =>
        Name = BusinessName.Create(
            "Business Name")
            .Value;
}
