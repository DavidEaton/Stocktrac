using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record DriversLicenseNumber
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
        public const string RequiredMessage = "Drivers License Number is required.";

        public string Number { get; }

        private DriversLicenseNumber(string number) =>
            Number = number;

        public static Result<DriversLicenseNumber> Create(string number) =>
            Result.Success(number)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), RequiredMessage)
                .Ensure(normalized => normalized.Length.IsWithin(MinimumLength, MaximumLength), InvalidLengthMessage)
                .Map(normalized => new DriversLicenseNumber(normalized));

        public static Result<DriversLicenseNumber> NewNumber(string newNumber) =>
            Create(newNumber);

        public static implicit operator string(DriversLicenseNumber driversLicenseNumber) =>
            driversLicenseNumber.Number;

        public static explicit operator DriversLicenseNumber(string number) => new(number);
    }
}
