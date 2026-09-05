using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public readonly record struct DriversLicenseNumber
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
        public static readonly string RequiredMessage = $"Drivers License Number is required.";

        public string Number { get; }

        private DriversLicenseNumber(string number) =>
            Number = number;

        public static Result<DriversLicenseNumber> Create(string? number) =>
            Result.Success(number?.Trim() ?? string.Empty)
                .Ensure(value => !string.IsNullOrWhiteSpace(value), RequiredMessage)
                .Ensure(
                    value => value.Length is >= MinimumLength and <= MaximumLength,
                    InvalidLengthMessage)
                .Map(value => new DriversLicenseNumber(value));

        public static Result<DriversLicenseNumber> NewNumber(string newNumber) =>
            Create(newNumber);
    }
}
