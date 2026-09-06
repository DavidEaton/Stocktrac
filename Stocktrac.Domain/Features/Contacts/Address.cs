using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Address
{
    public static readonly string AddressRequiredMessage = $"Address line 1 is required";
    public static readonly string CityRequiredMessage = $"A valid city is required";
    public static readonly string StateInvalidMessage = $"Please enter a valid State";
    public static readonly string PostalCodeRequiredMessage = $"A valid postal code is required";
    public AddressLine AddressLine1 { get; }
    public Maybe<AddressLine> AddressLine2 { get; }
    public static Maybe<Address> Default => Maybe<Address>.None;
    public City City { get; }
    public State State { get; }
    public PostalCode PostalCode { get; }

    private Address(AddressLine addressLine1, City city, State state, PostalCode postalCode, Maybe<AddressLine> addressLine2)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public static Result<Address> Create(AddressLine addressLine1, City city, State state, PostalCode postalCode, Maybe<AddressLine> addressLine2 = default) =>
        Result.Combine(
            Environment.NewLine,
            Result.FailureIf(
                addressLine1 is null,
                AddressRequiredMessage),
            Result.FailureIf(
                city is null,
                CityRequiredMessage),
            Result.FailureIf(
                !Enum.IsDefined(state),
                StateInvalidMessage),
            Result.FailureIf(
                postalCode is null,
                PostalCodeRequiredMessage))
        .Map(() => new Address(addressLine1!, city!, state, postalCode!, addressLine2));

    public Result<Address> NewAddressLine1(AddressLine newAddressLine) =>
        Create(newAddressLine, City, State, PostalCode, AddressLine2);

    public Result<Address> NewCity(City newCity) =>
        Create(AddressLine1, newCity, State, PostalCode, AddressLine2);

    public Result<Address> NewState(State newState) =>
        Create(AddressLine1, City, newState, PostalCode, AddressLine2);

    public Result<Address> NewPostalCode(PostalCode newPostalCode) =>
        Create(AddressLine1, City, State, newPostalCode, AddressLine2);

    public Result<Address> NewAddressLine2(AddressLine newAddressLine2) =>
        Create(AddressLine1, City, State, PostalCode, newAddressLine2);

    public override string ToString() =>
        AddressFull;

    public string AddressFull =>
        AddressLine2.HasValue
            ? $"{AddressLine1}, {AddressLine2}, {City}, {State} {PostalCode}"
            : $"{AddressLine1}, {City}, {State} {PostalCode}";
}
