using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record DriversLicenseNumber
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
        public static readonly string RequiredMessage = $"Drivers License Number is required.";

        public string Number { get; }

        private DriversLicenseNumber(string number) =>
            Number = number;

        public static Result<DriversLicenseNumber> Create(string? number)
        {
            var normalized = number?.Trim() ?? string.Empty;
            return Result.Combine(
                    Environment.NewLine,
                    Result.FailureIf(string.IsNullOrWhiteSpace(normalized), RequiredMessage),
                    Result.FailureIf(!string.IsNullOrWhiteSpace(normalized) && normalized.Length is < MinimumLength or > MaximumLength, InvalidLengthMessage))
                .Map(() => new DriversLicenseNumber(normalized));
        }

        public static Result<DriversLicenseNumber> NewNumber(string newNumber) =>
            Create(newNumber);
    }
}
