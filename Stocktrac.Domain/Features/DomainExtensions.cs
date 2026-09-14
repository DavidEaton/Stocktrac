using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using System.ComponentModel.DataAnnotations;

namespace Stocktrac.Domain.Features;

public static class DomainExtensions
{
    extension(string value)
    {
        public Result<string> AsNonEmptyString() =>
            string.IsNullOrWhiteSpace(value)
            ? Result.Failure<string>("Value cannot be empty")
            : Result.Success(value);
    }

    extension(string value)
    {
        public bool IsNonEmptyString() => !string.IsNullOrWhiteSpace(value);
    }

    extension(int value)
    {
        public bool IsWithin(int minimum, int maximum) =>
            value >= minimum && value <= maximum;
    }

    extension(string value)
    {
        internal Result<string> AsValidEmailAddress() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), Email.EmptyMessage)
                .Ensure(normalized => normalized.Length >= Email.MinimumLength, Email.MinimumLengthMessage)
                .Ensure(normalized => normalized.Length <= Email.MaximumLength, Email.MaximumLengthMessage)
                .Ensure(normalized => new EmailAddressAttribute().IsValid(normalized), Email.InvalidMessage);

        internal Result<string> AsValidPhoneNumber() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => new PhoneAttribute().IsValid(normalized), Phone.InvalidMessage);
    }

    extension(PhoneType phoneType)
    {
        internal Result<PhoneType> AsValidPhoneType() =>
            Enum.IsDefined(phoneType)
                ? Result.Success(phoneType)
                : Result.Failure<PhoneType>(Phone.PhoneTypeInvalidMessage);
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

    extension(DriversLicenseNumber number)
    {
        internal Result<DriversLicenseNumber> AsValidDriversLicenseNumber(DateTimeRange validRange) =>
            number is null || validRange is null
                ? Result.Failure<DriversLicenseNumber>(DriversLicense.RequiredMessage)
                : Result.Success(number);
    }

    extension(State state)
    {
        internal Result<State> AsValidDriversLicenseState() =>
            !Enum.IsDefined(state)
                ? Result.Failure<State>(DriversLicense.StateInvalidMessage)
                : Result.Success(state);
    }

}
