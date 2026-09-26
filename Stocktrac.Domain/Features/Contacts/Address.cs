using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Address
{
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
        Result.Combine(
                Environment.NewLine,
                addressLine1.AsRequired(),
                city.AsRequired(),
                state.AsValidState(),
                postalCode.AsRequired())
            .Map(() => new Address(
                addressLine1,
                city,
                state,
                postalCode,
                addressLine2));

    public Result<Address> ReplaceAddressLine1(AddressLine addressLine) =>
        addressLine.AsRequired()
            .Map(validLine => new Address(
                validLine,
                City,
                State,
                PostalCode,
                AddressLine2));

    public Result<Address> ReplaceCity(City city) =>
        city.AsRequired()
            .Map(validCity => new Address(
                AddressLine1,
                validCity,
                State,
                PostalCode,
                AddressLine2));

    public Result<Address> ReplaceState(State state) =>
        state.AsValidState()
            .Map(validState => new Address(
                AddressLine1,
                City,
                validState,
                PostalCode,
                AddressLine2));

    public Result<Address> ReplacePostalCode(PostalCode postalCode) =>
        postalCode.AsRequired()
            .Map(validPostalCode => new Address(
                AddressLine1,
                City,
                State,
                validPostalCode,
                AddressLine2));

    public Result<Address> AddOrReplaceAddressLine2(AddressLine addressLine2) =>
        addressLine2.AsRequired()
            .Map(validLine2 => new Address(
                AddressLine1,
                City,
                State,
                PostalCode,
                validLine2));

    public Address RemoveAddressLine2() =>
        new(
            AddressLine1,
            City,
            State,
            PostalCode,
            Maybe<AddressLine>.None);

    public override string ToString() =>
        AddressFull;

    public string AddressFull =>
        AddressLine2.HasValue
            ? $"{AddressLine1}, {AddressLine2.Value}, {City}, {State} {PostalCode}"
            : $"{AddressLine1}, {City}, {State} {PostalCode}";
}