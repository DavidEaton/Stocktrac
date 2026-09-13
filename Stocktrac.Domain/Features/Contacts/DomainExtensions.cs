using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public static class DomainExtensions
{
    extension(string value)
    {
        public Result<string> AsNonEmptyString() =>
            string.IsNullOrWhiteSpace(value)
            ? Result.Failure<string>("Value cannot be empty")
            : Result.Success(value);
    }

    extension(AddressLine line)
    {
        internal Result<AddressLine> AsValidLine() =>
            line is null
                ? Result.Failure<AddressLine>(Address.AddressRequiredMessage)
                : Result.Success(line);
    }

    extension(City city)
    {
        internal Result<City> AsValidCity() =>
            city is null
                ? Result.Failure<City>(Address.CityRequiredMessage)
                : Result.Success(city);
    }

    extension(PostalCode postalCode)
    {
        internal Result<PostalCode> AsValidPostalCode() =>
            postalCode is null
                ? Result.Failure<PostalCode>(Address.PostalCodeRequiredMessage)
                : Result.Success(postalCode);
    }

    extension(State state)
    {
        internal Result<State> AsValidState() =>
            !Enum.IsDefined(state)
                ? Result.Failure<State>(Address.StateInvalidMessage)
                : Result.Success(state);
    }

}