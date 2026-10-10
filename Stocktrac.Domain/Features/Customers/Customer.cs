using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

// The union selects the customer form; copies share the encapsulated aggregate
// state. Keep the returned value when replacing the person or business case.
public readonly union Customer(PersonCustomer, BusinessCustomer) : IEquatable<Customer>
{
    public const string DuplicateItemMessagePrefix = "Customer already has this ";
    public const string UnknownCustomerTypeMessage = "Unknown type.";
    public const string RequiredMessage = "Please include all required items.";

    private CustomerState State => this switch
    {
        PersonCustomer customer => customer.State,
        BusinessCustomer customer => customer.State,
        null => throw new InvalidOperationException("An empty Customer has no aggregate state.")
    };

    private Contactable Contactable => this switch
    {
        PersonCustomer customer => customer.Person,
        BusinessCustomer customer => customer.Business,
        null => throw new InvalidOperationException("An empty Customer has no contact details.")
    };

    public long Id => State.Id;
    public CustomerType CustomerType => State.CustomerType;
    public Maybe<CustomerCode> Code => State.Code;
    public ContactPreferences ContactPreferences => State.ContactPreferences;
    public string Name => this switch
    {
        PersonCustomer customer => customer.Person.ToString(),
        BusinessCustomer customer => customer.Business.ToString(),
        null => throw new InvalidOperationException("An empty Customer has no name.")
    };
    public Note Notes => Contactable.Notes;
    public Maybe<Address> Address => Contactable.Address;
    public IReadOnlyList<Vehicle> Vehicles => State.Vehicles;
    public IReadOnlyList<ContactPhone> Phones => Contactable.Phones;
    public IReadOnlyList<ContactEmail> Emails => Contactable.Emails;

    public static Result<Customer> Create(Person person, CustomerType customerType, Maybe<CustomerCode> code) =>
        ValidateCreation(person, customerType)
            .Map(() => (Customer)new PersonCustomer(person, new CustomerState(customerType, code)));

    public static Result<Customer> Create(Business business, CustomerType customerType, Maybe<CustomerCode> code) =>
        ValidateCreation(business, customerType)
            .Map(() => (Customer)new BusinessCustomer(business, new CustomerState(customerType, code)));

    private static Result ValidateCreation(Contactable entity, CustomerType customerType) =>
        Result.Combine(
            Environment.NewLine,
            Result.FailureIf(entity is null, RequiredMessage),
            Result.FailureIf(!Enum.IsDefined(customerType), UnknownCustomerTypeMessage));

    // Native unions are structs: boundaries must reject default/null-case values.
    public Result Validate() => Result.FailureIf(Value is null, RequiredMessage);

    public Result<Customer> ReplacePerson(Person person) =>
        Value is null || person is null
            ? Result.Failure<Customer>(RequiredMessage)
            : Result.Success<Customer>(new PersonCustomer(person, State));

    public Result<Customer> ReplaceBusiness(Business business) =>
        Value is null || business is null
            ? Result.Failure<Customer>(RequiredMessage)
            : Result.Success<Customer>(new BusinessCustomer(business, State));

    public Result UpdateAddress(Address address) =>
        Value is null ? Result.Failure(RequiredMessage) : Contactable.ReplaceAddress(address);

    public void RemoveAddress() => Contactable.RemoveAddress();

    public Result UpdateCustomerType(CustomerType customerType) =>
        Value is null ? Result.Failure(RequiredMessage) : State.UpdateCustomerType(customerType);

    public Result<ContactPhone> AddPhone(ContactPhone phone) =>
        Value is null ? Result.Failure<ContactPhone>(RequiredMessage) : Contactable.AddPhone(phone);

    public Result<ContactPhone> RemovePhone(ContactPhone phone) =>
        Value is null ? Result.Failure<ContactPhone>(RequiredMessage) : Contactable.RemovePhone(phone);

    public Result<ContactEmail> AddEmail(ContactEmail email) =>
        Value is null ? Result.Failure<ContactEmail>(RequiredMessage) : Contactable.AddEmail(email);

    public Result<ContactEmail> RemoveEmail(ContactEmail email) =>
        Value is null ? Result.Failure<ContactEmail>(RequiredMessage) : Contactable.RemoveEmail(email);

    public Result<Vehicle> AddVehicle(Vehicle vehicle) =>
        Value is null ? Result.Failure<Vehicle>(RequiredMessage) : State.AddVehicle(vehicle);

    public Result<Vehicle> RemoveVehicle(Vehicle vehicle) =>
        Value is null ? Result.Failure<Vehicle>(RequiredMessage) : State.RemoveVehicle(vehicle);

    public Result UpdateCode(CustomerCode code) =>
        Value is null ? Result.Failure(RequiredMessage) : State.UpdateCode(code);

    public void RemoveCode() => State.RemoveCode();

    // Replacing a case preserves customer identity, independently of its subject.
    public bool Equals(Customer other) =>
        Value is null
            ? other.Value is null
            : other.Value is not null && State.Equals(other.State);

    public override bool Equals(object? obj) => obj is Customer other && Equals(other);
    public override int GetHashCode() => Value is null ? 0 : State.GetHashCode();
    public static bool operator ==(Customer left, Customer right) => left.Equals(right);
    public static bool operator !=(Customer left, Customer right) => !left.Equals(right);
}
