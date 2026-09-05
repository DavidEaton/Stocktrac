using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Address
{
    public static readonly string AddressRequiredMessage = $"Address is required";
    public static readonly string StateInvalidMessage = $"Please enter a valid State";
    public AddressLine AddressLine1 { get; }
    public Maybe<AddressLine> AddressLine2 { get; }
    public static Maybe<Address> Default => Maybe<Address>.None;
    public City City { get; }
    public State State { get; }
    public PostalCode PostalCode { get; }

    private Address(
        AddressLine addressLine1,
        City city,
        State state,
        PostalCode postalCode,
        Maybe<AddressLine> addressLine2)
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
        Maybe<AddressLine> addressLine2 = default) =>
        Result.Success((AddressLine: addressLine1, City: city, PostalCode: postalCode, State: state))
            .Ensure(
                values => values.AddressLine is not null && values.City is not null && values.PostalCode is not null,
                AddressRequiredMessage)
            .Ensure(
                values => Enum.IsDefined(values.State),
                StateInvalidMessage)
            .Map(values => new Address(values.AddressLine, values.City, values.State, values.PostalCode, addressLine2));

    public Result<Address> NewAddressLine1(AddressLine newAddressLine) =>
        Create(newAddressLine, City, State, PostalCode, AddressLine2);

    public Result<Address> NewCity(City newCity) =>
        Create(AddressLine1, newCity, State, PostalCode, AddressLine2);

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
        Create(AddressLine1, City, State, newPostalCode, AddressLine2);

    public Result<Address> NewAddressLine2(Maybe<AddressLine> newAddressLine2) =>
        Result.Success(
            new Address(AddressLine1, City, State, PostalCode, newAddressLine2));

    public override string ToString() =>
        AddressFull;

    public string AddressFull =>
        AddressLine2.HasValue
            ? $"{AddressLine1}, {AddressLine2}, {City}, {State} {PostalCode}"
            : $"{AddressLine1}, {City}, {State} {PostalCode}";
}
