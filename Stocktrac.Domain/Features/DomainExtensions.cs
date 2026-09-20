using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features;

public static class DomainExtensions
{
    extension(string value)
    {
        /// <summary>
        /// Validates a string to ensure it is not null or whitespace.
        /// </summary>
        public Result<string> AsNonEmptyString() =>
            string.IsNullOrWhiteSpace(value)
            ? Result.Failure<string>("Value cannot be empty")
            : Result.Success(value);

        /// <summary>
        /// Validates a string to ensure it is not an empty string or only white space.
        /// </summary>
        public bool IsNonEmptyString() => !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Validates a string to ensure it is a valid required person name part.
        /// </summary>
        internal Result<string> AsValidRequiredPersonNamePart() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), PersonName.RequiredMessage)
                .Ensure(
                    normalized => normalized.Length.IsWithin(PersonName.MinimumLength, PersonName.MaximumLength),
                    PersonName.InvalidLengthMessage);

        /// <summary>
        /// Validates a string to ensure it is a valid optional person name part.
        /// </summary>
        internal Result<string> AsValidOptionalPersonNamePart() =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(
                    normalized => normalized.Length.IsWithin(PersonName.MinimumLength, PersonName.MaximumLength),
                    PersonName.InvalidLengthMessage);
    }

    extension(int value)
    {
        /// <summary>
        /// Validates an integer to ensure it is within a specified range.
        /// </summary>
        public bool IsWithin(int minimum, int maximum) =>
            value >= minimum && value <= maximum;
    }

    extension(PhoneType phoneType)
    {
        /// <summary>
        /// Validates a PhoneType to ensure it is a valid enum value.
        /// </summary>
        internal Result<PhoneType> AsValidPhoneType() =>
            Enum.IsDefined(phoneType)
                ? Result.Success(phoneType)
                : Result.Failure<PhoneType>(ContactPhone.PhoneTypeInvalidMessage);
    }

    extension(AddressLine line)
    {
        //
        // Summary: 
        // Validates the AddressLine, City, State, and PostalCode for an Address.
        internal Result<AddressLine> AsValidLine() =>
            line is null
                ? Result.Failure<AddressLine>(Address.AddressRequiredMessage)
                : Result.Success(line);
    }

    extension(City city)
    {
        /// <summary>
        /// Validates the City for an Address.
        /// </summary>
        internal Result<City> AsValidCity() =>
            city is null
                ? Result.Failure<City>(Address.CityRequiredMessage)
                : Result.Success(city);
    }

    extension(PostalCode postalCode)
    {
        /// <summary>
        /// Validates the PostalCode for an Address.
        /// </summary>
        internal Result<PostalCode> AsValidPostalCode() =>
            postalCode is null
                ? Result.Failure<PostalCode>(Address.PostalCodeRequiredMessage)
                : Result.Success(postalCode);
    }

    extension(State state)
    {
        /// <summary>
        /// Validates the State for an Address.
        /// </summary>
        internal Result<State> AsValidState() =>
            !Enum.IsDefined(state)
                ? Result.Failure<State>(Address.StateInvalidMessage)
                : Result.Success(state);

        /// <summary>
        /// Validates the State for a Drivers License.
        /// </summary>
        internal Result<State> AsValidDriversLicenseState() =>
            !Enum.IsDefined(state)
                ? Result.Failure<State>(DriversLicense.StateInvalidMessage)
                : Result.Success(state);
    }

    extension(DriversLicenseNumber number)
    {
        /// <summary>
        /// Validates a DriversLicenseNumber is not null.
        /// </summary>
        internal Result<DriversLicenseNumber> AsValidDriversLicenseNumber() =>
            number is null
                ? Result.Failure<DriversLicenseNumber>(DriversLicense.RequiredMessage)
                : Result.Success(number);
    }

    extension(DateTimeRange dateRange)
    {
        /// <summary>
        /// Validates a driver's license date range.
        /// </summary>
        internal Result<DateTimeRange> AsValidDriversLicenseDateTimeRange(
            DateOnly today)
        {
            if (dateRange is null)
                return Result.Failure<DateTimeRange>(DriversLicense.RequiredMessage);

            return Result.Combine(
                    Environment.NewLine,
                    Result.FailureIf(
                        dateRange.Start >= dateRange.End,
                        DriversLicense.DateOrderInvalidMessage),
                    Result.FailureIf(
                        dateRange.Start < DriversLicense.MinimumValidDate,
                        DriversLicense.StartDateTooEarlyMessage),
                    Result.FailureIf(
                        dateRange.Start > today,
                        DriversLicense.StartDateInFutureMessage),
                    Result.FailureIf(
                        ExceedsMaximumDriversLicenseValidity(dateRange.Start, dateRange.End),
                        DriversLicense.DateRangeTooLongMessage))
                .Map(() => dateRange);
        }

        private static bool ExceedsMaximumDriversLicenseValidity(
            DateOnly start,
            DateOnly end) =>
            start <= DateOnly.MaxValue.AddYears(
                -DriversLicense.MaximumValidityYears)
            && end > start.AddYears(
                DriversLicense.MaximumValidityYears);
    }
}
