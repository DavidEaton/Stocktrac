using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record AddressLine
    {
        public const int MinimumLength = 2;
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Address must be between {MinimumLength} and {MaximumLength} characters.";
        public const string RequiredMessage = "Address Line is required.";
        public string Value { get; }

        private AddressLine(string value) => Value = value;

        public static Result<AddressLine> Create(string value) =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(
                    normalized => normalized.IsNonEmptyString(),
                    RequiredMessage)
                .Ensure(
                    normalized => normalized.Length.IsWithin(
                        MinimumLength,
                        MaximumLength),
                    InvalidLengthMessage)
                .Map(normalized => new AddressLine(normalized));

        public override string ToString() => Value;
    }
}
