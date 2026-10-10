using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons
{
    public sealed record DriversLicenseNumber
    {
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Drivers License Number must not exceed {MaximumLength} characters.";

        public NonEmptyString Number { get; }

        private DriversLicenseNumber(NonEmptyString number) =>
            Number = number;

        public static Result<DriversLicenseNumber> Create(NonEmptyString number) =>
            number.Value.Length <= MaximumLength
                ? Result.Success(new DriversLicenseNumber(number))
                : Result.Failure<DriversLicenseNumber>(InvalidLengthMessage);

        public static Result<DriversLicenseNumber> ReplaceNumber(NonEmptyString number) =>
            Create(number);

        public override string ToString() => Number.ToString();
    }
}
