using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Domain.Features;

public static class DomainExtensions
{
    extension(int value)
    {
        /// <summary>
        /// Determines whether an integer is within a specified range.
        /// </summary>
        public bool IsWithin(int minimum, int maximum) =>
            value >= minimum && value <= maximum;
    }

    // extension(NonEmptyString nonEmptyString)
    // {
    //     /// <summary>
    //     /// Validates that a NonEmptyString value object was supplied.
    //     /// </summary>
    //     // internal Result<NonEmptyString> AsRequired() =>
    //     //     nonEmptyString is null
    //     //         ? Result.Failure<NonEmptyString>(NonEmptyString.RequiredMessage)
    //     //         : Result.Success(nonEmptyString);
    // }


    extension(PhoneType phoneType)
    {
        /// <summary>
        /// Validates a PhoneType to ensure it is a valid enum value.
        /// </summary>
        internal Result<PhoneType> AsValidPhoneType() =>
            Enum.IsDefined(phoneType)
                ? Result.Success(phoneType)
                : Result.Failure<PhoneType>(
                    ContactPhone.PhoneTypeInvalidMessage);
    }

    extension(PhoneNumber phoneNumber)
    {
        /// <summary>
        /// Validates that an PhoneNumber value object was supplied.
        /// </summary>
        internal Result<PhoneNumber> AsRequired() =>
            phoneNumber is null
                ? Result.Failure<PhoneNumber>(NonEmptyString.RequiredMessage)
                : Result.Success(phoneNumber);
    }

    extension(AddressLine line)
    {
        /// <summary>
        /// Validates that an AddressLine value object was supplied.
        /// The AddressLine itself is responsible for validating its own invariants.
        /// </summary>
        internal Result<AddressLine> AsRequired() =>
            line is null
                ? Result.Failure<AddressLine>(NonEmptyString.RequiredMessage)
                : Result.Success(line);
    }
    extension(PersonName name)
    {
        /// <summary>
        /// Validates that a PersonName value object was supplied.
        /// The PersonName itself is responsible for validating its own invariants.
        /// </summary>
        internal Result<PersonName> AsRequired() =>
            name is null
                ? Result.Failure<PersonName>(NonEmptyString.RequiredMessage)
                : Result.Success(name);
    }

    extension(City city)
    {
        /// <summary>
        /// Validates that a City value object was supplied.
        /// The City itself is responsible for validating its own invariants.
        /// </summary>
        internal Result<City> AsRequired() =>
            city is null
                ? Result.Failure<City>(NonEmptyString.RequiredMessage)
                : Result.Success(city);
    }

    extension(PostalCode postalCode)
    {
        /// <summary>
        /// Validates that a PostalCode value object was supplied.
        /// The PostalCode itself is responsible for validating its own invariants.
        /// </summary>
        internal Result<PostalCode> AsRequired() =>
            postalCode is null
                ? Result.Failure<PostalCode>(NonEmptyString.RequiredMessage)
                : Result.Success(postalCode);
    }

    extension(State state)
    {
        /// <summary>
        /// Validates that a State enum value is defined.
        /// </summary>
        internal Result<State> AsValidState() =>
            Enum.IsDefined(state)
                ? Result.Success(state)
                : Result.Failure<State>("A valid State is required.");
    }

    extension(DriversLicenseNumber number)
    {
        /// <summary>
        /// Validates that a DriversLicenseNumber was supplied.
        /// </summary>
        internal Result<DriversLicenseNumber> AsValidDriversLicenseNumber() =>
            number is null
                ? Result.Failure<DriversLicenseNumber>(
                    DriversLicense.RequiredMessage)
                : Result.Success(number);
    }

    extension(DateRange dateRange)
    {
        /// <summary>
        /// Validates a driver's license date range.
        /// </summary>
        internal Result<DateRange> AsValidDriversLicenseDateRange(
            DateOnly today)
        {
            if (dateRange is null)
                return Result.Failure<DateRange>(
                    DriversLicense.RequiredMessage);

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
                        ExceedsMaximumDriversLicenseValidity(
                            dateRange.Start,
                            dateRange.End),
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