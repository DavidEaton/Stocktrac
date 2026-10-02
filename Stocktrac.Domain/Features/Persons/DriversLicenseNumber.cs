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
            Result.Success(number)
                .Ensure(value => value.Value.Length <= MaximumLength, InvalidLengthMessage)
                .Map(value => new DriversLicenseNumber(value));

        public static Result<DriversLicenseNumber> ReplaceNumber(NonEmptyString number) =>
            Create(number);

        public override string ToString() => Number.ToString();
    }
}
