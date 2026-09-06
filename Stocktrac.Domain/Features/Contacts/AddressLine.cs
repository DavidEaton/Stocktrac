using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record AddressLine
    {
        public const int MinimumLength = 2;
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
        public static readonly string RequiredMessage = "Address Line is required.";
        public string Value { get; }

        private AddressLine(string value) =>
            Value = value;

        public static Result<AddressLine> Create(string? value)
        {
            var normalized = value?.Trim() ?? string.Empty;
            return Result.Combine(
                    Environment.NewLine,
                    Result.FailureIf(string.IsNullOrWhiteSpace(normalized), RequiredMessage),
                    Result.FailureIf(!string.IsNullOrWhiteSpace(normalized) && normalized.Length is < MinimumLength or > MaximumLength, InvalidLengthMessage))
                .Map(() => new AddressLine(normalized));
        }

        public override string ToString() =>
            Value ?? string.Empty;
    }
}
