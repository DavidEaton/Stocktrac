using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stocktrac.Domain.Features;

public static class DomainExtensions
{
    extension(string value)
    {
        //
        // Summary: 
        //     Validates a string to ensure it is not null or whitespace.
        public Result<string> AsNonEmptyString() =>
            string.IsNullOrWhiteSpace(value)
            ? Result.Failure<string>("Value cannot be empty")
            : Result.Success(value);
    }

    extension(string value)
    {
        //
        // Summary: 
        //     Validates a string to ensure it is not an empty string or only white space.
        public bool IsNonEmptyString() => !string.IsNullOrWhiteSpace(value);
    }

    extension(int value)
    {
        //
        // Summary: 
        //     Validates an integer to ensure it is within a specified range.
        public bool IsWithin(int minimum, int maximum) =>
            value >= minimum && value <= maximum;
    }

    extension(string value)
    {
        //
        // Summary: 
        //     Validates a string to ensure it is a valid email domain address.
        internal Result<string> AsValidEmailAddress() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), Email.EmptyMessage)
                .Ensure(normalized => normalized.Length >= Email.MinimumLength, Email.MinimumLengthMessage)
                .Ensure(normalized => normalized.Length <= Email.MaximumLength, Email.MaximumLengthMessage)
                .Ensure(normalized => new EmailAddressAttribute().IsValid(normalized), Email.InvalidMessage);

        //
        // Summary: 
        //     Validates a string and converts it to a canonical phone number. An
        //     international number retains its leading '+' while formatting characters
        //     are removed from all numbers.
        internal Result<string> AsValidPhoneNumber() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => new PhoneAttribute().IsValid(normalized), Phone.InvalidMessage)
                .Ensure(
                    normalized => Regex.IsMatch(normalized, @"^\+?[0-9\s().-]+$"),
                    Phone.InvalidMessage)
                .Map(CanonicalizePhoneNumber)
                .Ensure(
                    normalized => Regex.IsMatch(normalized, @"^(?:[0-9]+|\+[1-9][0-9]{1,14})$"),
                    Phone.InvalidMessage);
    }

    private static string CanonicalizePhoneNumber(string number)
    {
        var prefix = number.StartsWith('+') ? "+" : string.Empty;
        return prefix + string.Concat(number.Where(character => character is >= '0' and <= '9'));
    }

    extension(PhoneType phoneType)
    {
        //
        // Summary: 
        //     Validates a PhoneType to ensure it is a valid enum value.
        internal Result<PhoneType> AsValidPhoneType() =>
            Enum.IsDefined(phoneType)
                ? Result.Success(phoneType)
                : Result.Failure<PhoneType>(Phone.PhoneTypeInvalidMessage);
    }

    extension(AddressLine line)
    {
        //
        // Summary: 
        //     Validates the AddressLine, City, State, and PostalCode for an Address.
        internal Result<AddressLine> AsValidLine() =>
            line is null
                ? Result.Failure<AddressLine>(Address.AddressRequiredMessage)
                : Result.Success(line);
    }

    extension(City city)
    {
        //
        // Summary: 
        //     Validates the City for an Address.
        internal Result<City> AsValidCity() =>
            city is null
                ? Result.Failure<City>(Address.CityRequiredMessage)
                : Result.Success(city);
    }

    extension(PostalCode postalCode)
    {
        //
        // Summary: 
        //     Validates the PostalCode for an Address.
        internal Result<PostalCode> AsValidPostalCode() =>
            postalCode is null
                ? Result.Failure<PostalCode>(Address.PostalCodeRequiredMessage)
                : Result.Success(postalCode);
    }

    extension(State state)
    {
        //
        // Summary: 
        //     Validates the State for an Address.
        internal Result<State> AsValidState() =>
            !Enum.IsDefined(state)
                ? Result.Failure<State>(Address.StateInvalidMessage)
                : Result.Success(state);
    }

    extension(DriversLicenseNumber number)
    {
        //
        // Summary: 
        //     Validates a Drivers License number and date range.
        internal Result<DriversLicenseNumber> AsValidDriversLicenseNumber(DateTimeRange validRange) =>
            number is null || validRange is null
                ? Result.Failure<DriversLicenseNumber>(DriversLicense.RequiredMessage)
                : Result.Success(number);
    }

    extension(State state)
    {
        //
        // Summary: 
        //     Validates the State for a Drivers License.
        internal Result<State> AsValidDriversLicenseState() =>
            !Enum.IsDefined(state)
                ? Result.Failure<State>(DriversLicense.StateInvalidMessage)
                : Result.Success(state);
    }

    extension(string value)
    {
        //
        // Summary: 
        //     Validates a string to ensure it is a valid required person name part.
        internal Result<string> AsValidRequiredPersonNamePart() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), PersonName.RequiredMessage)
                .Ensure(
                    normalized => normalized.Length.IsWithin(PersonName.MinimumLength, PersonName.MaximumLength),
                    PersonName.InvalidLengthMessage);

        //
        // Summary: 
        //     Validates a string to ensure it is a valid optional person name part.
        internal Result<string> AsValidOptionalPersonNamePart() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(
                    normalized => normalized.Length.IsWithin(PersonName.MinimumLength, PersonName.MaximumLength),
                    PersonName.InvalidLengthMessage);
    }

}
