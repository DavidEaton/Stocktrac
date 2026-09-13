using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Address
{
    public const string AddressRequiredMessage = "Address line 1 is required.";
    public const string CityRequiredMessage = "A valid city is required.";
    public const string StateInvalidMessage = "A valid State is required.";
    public const string PostalCodeRequiredMessage = "A valid postal code is required.";
    public AddressLine AddressLine1 { get; private set; }
    public Maybe<AddressLine> AddressLine2 { get; private set; } = Maybe<AddressLine>.None;
    public static Maybe<Address> Default => Maybe<Address>.None;
    public City City { get; private set; }
    public State State { get; private set; }
    public PostalCode PostalCode { get; private set; }

    private Address(AddressLine addressLine1, City city, State state, PostalCode postalCode, Maybe<AddressLine> addressLine2)
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
                addressLine1.AsValidLine(),
                city.AsValidCity(),
                state.AsValidState(),
                postalCode.AsValidPostalCode())
            .Map(() => new Address(addressLine1!, city!, state, postalCode!, addressLine2));

    public Result<Address> NewAddressLine1(AddressLine newAddressLine) =>
       newAddressLine
           .AsValidLine()
           .Map(validLine => this with { AddressLine1 = validLine });

    public Result<Address> NewCity(City newCity) =>
        newCity
            .AsValidCity()
            .Map(validCity => this with { City = validCity });

    public Result<Address> NewState(State newState) =>
        newState
            .AsValidState()
            .Map(validState => this with { State = validState });

    public Result<Address> NewPostalCode(PostalCode newPostalCode) =>
        newPostalCode
            .AsValidPostalCode()
            .Map(validPostalCode => this with { PostalCode = validPostalCode });

    public Result<Address> NewAddressLine2(AddressLine newAddressLine2) =>
        newAddressLine2
            .AsValidLine()
            .Map(validLine => this with { AddressLine2 = validLine });

    public Address ClearAddressLine2() =>
        new(AddressLine1, City, State, PostalCode, Maybe<AddressLine>.None);

    public override string ToString() =>
        AddressFull;

    public string AddressFull =>
        AddressLine2.HasValue
            ? $"{AddressLine1}, {AddressLine2.Value}, {City}, {State} {PostalCode}"
            : $"{AddressLine1}, {City}, {State} {PostalCode}";
}
