using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Customers;

// Shared aggregate identity and state survive replacement of the customer case.
internal sealed class CustomerState : Entity
{
    internal CustomerType CustomerType { get; private set; }
    internal Maybe<CustomerCode> Code { get; private set; }
    internal ContactPreferences ContactPreferences { get; } = ContactPreferences.Create(true, true, true);
    private readonly List<Vehicle> vehicles = [];
    internal IReadOnlyList<Vehicle> Vehicles => [.. vehicles];

    internal CustomerState(CustomerType customerType, Maybe<CustomerCode> code) =>
        (CustomerType, Code) = (customerType, code);

    internal Result UpdateCustomerType(CustomerType customerType) =>
        !Enum.IsDefined(customerType)
            ? Result.Failure(Customer.UnknownCustomerTypeMessage)
            : Result.Success(CustomerType = customerType);

    internal Result<Vehicle> AddVehicle(Vehicle vehicle) =>
        vehicle is null
            ? Result.Failure<Vehicle>(Customer.RequiredMessage)
            : vehicles.Any(existingVehicle => existingVehicle == vehicle)
                ? Result.Failure<Vehicle>($"{Customer.DuplicateItemMessagePrefix} Vehicle: {vehicle}, VIN: {vehicle.VIN}.")
                : Result.Success(vehicle).Tap(() => vehicles.Add(vehicle));

    internal Result<Vehicle> RemoveVehicle(Vehicle vehicle) =>
        vehicle is null
            ? Result.Failure<Vehicle>(Customer.RequiredMessage)
            : vehicles.Remove(vehicle)
                ? Result.Success(vehicle)
                : Result.Failure<Vehicle>(Contactable.NotFoundMessage);

    internal Result UpdateCode(CustomerCode code) =>
        code is null
            ? Result.Failure(Customer.RequiredMessage)
            : Result.Success(Code = code);

    internal void RemoveCode() => Code = Maybe<CustomerCode>.None;
}
