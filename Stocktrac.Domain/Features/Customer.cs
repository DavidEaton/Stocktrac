using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contact;

namespace Stocktrac.Domain.Features;

public class Customer : Entity
{
    public static readonly int MaximumCodeLength = 20;
    public static readonly string DuplicateItemMessagePrefix = $"Customer already has this ";
    public static readonly string UnknownEntityTypeMessage = $"Unknown entity type.";
    public static readonly string UnknownCustomerTypeMessage = $"Unknown type.";
    public static readonly string RequiredMessage = "Please include all required items.";
    public static readonly string InvalidCodeLengthMessage = $"Code must be {MaximumCodeLength} characters or less.";
    public static readonly string UnsupportedEntityTypeMessage = "Unsupported customer entity type.";

    public CustomerType CustomerType { get; private set; }
    public string Code { get; private set; } //optional
    public ContactPreferences ContactPreferences { get; private set; }
    public ICustomerEntity CustomerEntity { get; private set; }
    public EntityType EntityType => CustomerEntity.EntityType;
    public string DisplayName => CustomerEntity.DisplayName;
    public string Notes => CustomerEntity.Notes;
    public Address Address => CustomerEntity.Address;
    public string? Name => CustomerEntity?.ToString();
    private readonly List<Vehicle> vehicles = [];
    public IReadOnlyList<Vehicle> Vehicles => [.. vehicles];
    public IReadOnlyList<Phone> Phones => CustomerEntity.Phones;
    public IReadOnlyList<Email> Emails => CustomerEntity.Emails;

    private Customer(ICustomerEntity entity, CustomerType customerType, string code)
    {
        CustomerEntity = entity;
        CustomerType = customerType;
        Code = code;
    }

    public static Result<Customer> Create(ICustomerEntity entity, CustomerType customerType, string code)
    {
        if (entity is null)
            return Result.Failure<Customer>(RequiredMessage);

        if (!Enum.IsDefined(customerType))
            return Result.Failure<Customer>(UnknownCustomerTypeMessage);

        code = code?.Trim() ?? string.Empty;
        return code.Length > MaximumCodeLength
            ? Result.Failure<Customer>(InvalidCodeLengthMessage)
            : Result.Success(new Customer(entity, customerType, code));
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

    public Result<string> SetCode(string code)
    {
        code = code?.Trim() ?? string.Empty;
        return code.Length <= MaximumCodeLength
            ? Result.Success(Code = code)
            : Result.Failure<string>(InvalidCodeLengthMessage);
    }

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
    protected Customer() =>
        vehicles = [];
}