using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public readonly record struct Address
{
    public static readonly string AddressRequiredMessage = $"Address is required";
    public static readonly string StateInvalidMessage = $"Please enter a valid State";
    public static Maybe<Address> Default => Maybe<Address>.None;
    public AddressLine AddressLine1 { get; }
    public AddressLine? AddressLine2 { get; }
    public City City { get; }
    public State State { get; }
    public PostalCode PostalCode { get; }

    private Address(
        AddressLine addressLine1,
        City city,
        State state,
        PostalCode postalCode,
        AddressLine? addressLine2 = null)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public static Result<Address> Create(
        AddressLine addressLine1,
        City city,
        State state,
        PostalCode postalCode,
        AddressLine? addressLine2 = null) =>
        Result.Success(state)
            .Ensure(
                value => Enum.IsDefined(value),
                StateInvalidMessage)
            .Map(state => new Address(addressLine1, city, state, postalCode, addressLine2));

    public Result<Address> NewAddressLine1(AddressLine newAddressLine) =>
        Result.Success(
            new Address(newAddressLine, City, State, PostalCode, AddressLine2));

    public Result<Address> NewCity(City newCity) =>
        Result.Success(
            new Address(AddressLine1, newCity, State, PostalCode, AddressLine2));

    public Result<Address> NewState(State newState)
    {
        var current = this;

        return Result.Success(newState)
            .Ensure(
                static value => Enum.IsDefined(value),
                StateInvalidMessage)
            .Map(state => new Address(
                current.AddressLine1,
                current.City,
                state,
                current.PostalCode,
                current.AddressLine2));
    }

    public Result<Address> NewPostalCode(PostalCode newPostalCode) =>
        Result.Success(
            new Address(AddressLine1, City, State, newPostalCode, AddressLine2));

    public Result<Address> NewAddressLine2(AddressLine? newAddressLine2) =>
        Result.Success(
            new Address(AddressLine1, City, State, PostalCode, newAddressLine2));

    public override string ToString() =>
        AddressFull;

    public string AddressFull =>
        AddressLine2.HasValue
            ? $"{AddressLine1}, {City}, {State} {PostalCode}"
            : $"{AddressLine1}, {AddressLine2}, {City}, {State} {PostalCode}";
}
