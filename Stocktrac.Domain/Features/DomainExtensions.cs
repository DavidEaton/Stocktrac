using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;
using Stocktrac.Domain.Features.Persons;
using static Stocktrac.Domain.Features.NonEmptyStringConstruction;

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

    extension(PersonName name)
    {
        /// <summary>
        /// Validates that a PersonName value object was supplied.
        /// The PersonName itself is responsible for validating its own invariants.
        /// </summary>
        internal Result<PersonName> AsRequired() =>
            name is null
                ? Result.Failure<PersonName>(RequiredMessage)
                : Result.Success(name);
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
