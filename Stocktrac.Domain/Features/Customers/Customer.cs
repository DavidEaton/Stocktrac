using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Customers;

public sealed class Customer : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const string DuplicateItemMessagePrefix = "Customer already has this ";
    public const string UnknownCustomerTypeMessage = "Unknown type.";
    public const string RequiredMessage = "Please include all required items.";

    public CustomerType CustomerType { get; private set; }
    public Maybe<CustomerCode> Code { get; private set; }
    public ContactPreferences ContactPreferences { get; private set; }
    public CustomerEntity CustomerEntity { get; private set; }
    public string Name => CustomerEntity.Name;
    public Note Notes => CustomerEntity.Contactable.Notes;
    public Maybe<Address> Address => CustomerEntity.Contactable.Address;
    private readonly List<Vehicle> vehicles = [];
    public IReadOnlyList<Vehicle> Vehicles => [.. vehicles];
    public IReadOnlyList<ContactPhone> Phones => CustomerEntity.Contactable.Phones;
    public IReadOnlyList<ContactEmail> Emails => CustomerEntity.Contactable.Emails;

    private Customer(
        CustomerEntity entity,
        CustomerType customerType,
        Maybe<CustomerCode> code,
        ContactPreferences contactPreferences)
    {
        CustomerEntity = entity;
        CustomerType = customerType;
        Code = code;
        ContactPreferences = contactPreferences;
    }

    public static Result<Customer> Create(CustomerEntity entity, CustomerType customerType, Maybe<CustomerCode> code) =>
        Result.Combine(
                Environment.NewLine,
                Result.FailureIf(entity.Value is null, RequiredMessage),
                Result.FailureIf(!Enum.IsDefined(customerType), UnknownCustomerTypeMessage))
            .Map(() => new Customer(
                entity,
                customerType,
                code,
                ContactPreferences.Create(true, true, true)));

    public Result UpdateAddress(Address address) =>
        CustomerEntity.Contactable.ReplaceAddress(address);

    public void RemoveAddress() => CustomerEntity.Contactable.RemoveAddress();

    public Result UpdateCustomerType(CustomerType customerType)
    {
        if (Enum.IsDefined(customerType))
        {
            CustomerType = customerType;
            return Result.Success();
        }

        return Result.Failure(RequiredMessage);
    }

    public Result<ContactPhone> AddPhone(ContactPhone phone) =>
        CustomerEntity.Contactable.AddPhone(phone);

    public Result<ContactPhone> RemovePhone(ContactPhone phone) =>
        CustomerEntity.Contactable.RemovePhone(phone);

    public Result<ContactEmail> AddEmail(ContactEmail email) =>
        CustomerEntity.Contactable.AddEmail(email);

    public Result<ContactEmail> RemoveEmail(ContactEmail email) =>
        CustomerEntity.Contactable.RemoveEmail(email);

    public Result<Vehicle> AddVehicle(Vehicle vehicle)
    {
        if (vehicle is null)
            return Result.Failure<Vehicle>(RequiredMessage);

        if (CustomerHasVehicle(vehicle))
            return Result.Failure<Vehicle>($"{DuplicateItemMessagePrefix} Vehicle: {vehicle}, VIN: {vehicle.VIN}.");

        vehicles.Add(vehicle);
        return Result.Success(vehicle);
    }

    public Result<Vehicle> RemoveVehicle(Vehicle vehicle)
    {
        if (vehicle is null)
            return Result.Failure<Vehicle>(RequiredMessage);

        return vehicles.Remove(vehicle)
            ? Result.Success(vehicle)
            : Result.Failure<Vehicle>(Contactable.NotFoundMessage);
    }

    private bool CustomerHasVehicle(Vehicle vehicle) =>
        Vehicles.Any(existingVehicle => existingVehicle == vehicle);

    public Result UpdateCode(CustomerCode code) =>
        code is null ? Result.Failure(RequiredMessage) : Result.Success().Tap(() => Code = code);

    public void RemoveCode() => Code = Maybe<CustomerCode>.None;

    public Result UpdateCustomerEntity(CustomerEntity entity) =>
        entity.Value is null
            ? Result.Failure(RequiredMessage)
            : Result.Success().Tap(() => CustomerEntity = entity);
}
