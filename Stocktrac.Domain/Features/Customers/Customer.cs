using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features.Customers;

public class Customer : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static readonly string DuplicateItemMessagePrefix = $"Customer already has this ";
    public static readonly string UnknownEntityTypeMessage = $"Unknown entity type.";
    public static readonly string UnknownCustomerTypeMessage = $"Unknown type.";
    public static readonly string RequiredMessage = "Please include all required items.";
    public static readonly string UnsupportedEntityTypeMessage = "Unsupported customer entity type.";

    public CustomerType CustomerType { get; private set; }
    public CustomerCode? Code { get; private set; }
    public ContactPreferences ContactPreferences { get; private set; }
    public ICustomerEntity CustomerEntity { get; private set; }
    public EntityType EntityType => CustomerEntity.EntityType;
    public string? Name => CustomerEntity?.ToString();
    public string? Notes => CustomerEntity?.Notes;
    public Address? Address => CustomerEntity.Address;
    private readonly List<Vehicle> vehicles = [];
    public IReadOnlyList<Vehicle> Vehicles => [.. vehicles];
    public IReadOnlyList<Phone> Phones => CustomerEntity.Phones;
    public IReadOnlyList<Email> Emails => CustomerEntity.Emails;

    private Customer(
        ICustomerEntity entity,
        CustomerType customerType,
        CustomerCode? code,
        ContactPreferences contactPreferences)
    {
        CustomerEntity = entity;
        CustomerType = customerType;
        Code = code;
        ContactPreferences = contactPreferences;
    }

    public static Result<Customer> Create(ICustomerEntity entity, CustomerType customerType, CustomerCode? code)
    {
        if (entity is null)
            return Result.Failure<Customer>(RequiredMessage);

        if (!Enum.IsDefined(customerType))
            return Result.Failure<Customer>(UnknownCustomerTypeMessage);

        return Result.Success(
            new Customer(
                entity: entity,
                customerType: customerType,
                code: code,
                contactPreferences: ContactPreferences.Create(
                    true, true, true).Value
        ));
    }

    public Result SetAddress(Address address) =>
        CustomerEntity switch
        {
            Person person => person.SetAddress(address),
            Business business => business.SetAddress(address),
            _ => Result.Failure(UnsupportedEntityTypeMessage),
        };

    public void ClearAddress()
    {
        switch (CustomerEntity)
        {
            case Person person:
                person.ClearAddress();
                break;

            case Business business:
                business.ClearAddress();
                break;

            default:
                throw new InvalidOperationException(UnsupportedEntityTypeMessage);
        }
    }

    public Result SetCustomerType(CustomerType customerType)
    {
        if (Enum.IsDefined(customerType))
        {
            CustomerType = customerType;
            return Result.Success();
        }

        return Result.Failure(RequiredMessage);
    }

    public Result<Phone> AddPhone(Phone phone) =>
        CustomerEntity switch
        {
            Person person => person.AddPhone(phone),
            Business business => business.AddPhone(phone),
            _ => Result.Failure<Phone>(UnsupportedEntityTypeMessage),
        };

    public Result<Phone> RemovePhone(Phone phone) =>
        CustomerEntity switch
        {
            Person person => person.RemovePhone(phone),
            Business business => business.RemovePhone(phone),
            _ => Result.Failure<Phone>(UnsupportedEntityTypeMessage),
        };

    public Result<Email> AddEmail(Email email) =>
        CustomerEntity switch
        {
            Person person => person.AddEmail(email),
            Business business => business.AddEmail(email),
            _ => Result.Failure<Email>(UnsupportedEntityTypeMessage),
        };

    public Result<Email> RemoveEmail(Email email) =>
        CustomerEntity switch
        {
            Person person => person.RemoveEmail(email),
            Business business => business.RemoveEmail(email),
            _ => Result.Failure<Email>(UnsupportedEntityTypeMessage),
        };

    public Result<Vehicle> AddVehicle(Vehicle vehicle)
    {
        if (vehicle is null)
            return Result.Failure<Vehicle>(RequiredMessage);

        if (CustomerHasVehicle(vehicle))
            return Result.Failure<Vehicle>($"{DuplicateItemMessagePrefix} Vehicle: {vehicle}, VIN: {vehicle.VIN}");

        vehicles.Add(vehicle);
        return Result.Success(vehicle);
    }

    public Result<Vehicle> RemoveVehicle(Vehicle vehicle)
    {
        if (vehicle is null)
            return Result.Failure<Vehicle>(RequiredMessage);

        vehicles.Remove(vehicle);
        return Result.Success(vehicle);
    }

    private bool CustomerHasVehicle(Vehicle vehicle) =>
        Vehicles.Any(existingVehicle => existingVehicle == vehicle);

    public Result<CustomerCode> SetCode(CustomerCode code) =>
        Result.Success(Code = code);

    public Result SetCustomerEntity(ICustomerEntity entity)
    {
        if (entity is null)
            return Result.Failure<ICustomerEntity>(RequiredMessage);

        switch (entity.EntityType)
        {
            case EntityType.Person or EntityType.Business:
                CustomerEntity = entity;
                break;

            default:
                return Result.Failure(UnsupportedEntityTypeMessage);
        }

        return Result.Success();
    }

    // EF requires a parameterless constructor
    private Customer()
    {
        vehicles = [];
        CustomerEntity = Person.Create(
            PersonName.Create(
                lastName: "First",
                firstName: "Last").Value,
            notes: null,
            birthday: null,
            emails: null,
            phones: null,
            address: null).Value;
        ContactPreferences = ContactPreferences.Create(
            allowMail: true,
            allowEmail: true,
            allowSms: true).Value;
        CustomerType = CustomerType.Retail;
    }
}
