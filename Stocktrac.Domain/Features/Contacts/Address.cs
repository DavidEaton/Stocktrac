using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Address
{
    public AddressLine AddressLine1 { get; }
    public Maybe<AddressLine> AddressLine2 { get; }
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
        state.AsValidState()
            .Map(validState => new Address(addressLine1, city, validState, postalCode, addressLine2));

    public Result<Address> ReplaceAddressLine1(AddressLine addressLine) =>
        Result.Success(
            new Address(addressLine, City, State, PostalCode, AddressLine2));

    public Result<Address> ReplaceCity(City city) =>
        Result.Success(
            new Address(AddressLine1, city, State, PostalCode, AddressLine2));

    public Result<Address> ReplacePostalCode(PostalCode postalCode) =>
        Result.Success(
            new Address(AddressLine1, City, State, postalCode, AddressLine2));

    public Result<Address> AddOrReplaceAddressLine2(AddressLine addressLine2) =>
        Result.Success(
            new Address(AddressLine1, City, State, PostalCode, addressLine2));

    public Result<Address> RemoveAddressLine2() =>
        Result.Success(
            new Address(AddressLine1, City, State, PostalCode, Maybe<AddressLine>.None));

    public Result<Address> ReplaceState(State state) =>
        state.AsValidState()
            .Map(validState =>
                new Address(AddressLine1, City, validState, PostalCode, AddressLine2));

    public override string ToString() => AddressFull;

    public string AddressFull =>
        AddressLine2.HasValue
            ? $"{AddressLine1}, {AddressLine2.Value}, {City}, {State} {PostalCode}"
            : $"{AddressLine1}, {City}, {State} {PostalCode}";
}