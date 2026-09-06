using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record City
    {
        public const int MinimumLength = 1;
        public const int MaximumLength = 100;
        public static readonly string InvalidLengthMessage = $"City must be between {MinimumLength} and {MaximumLength} characters.";
        public static readonly string RequiredMessage = "City is required.";
        public string Value { get; }

        private City(string value) =>
            Value = value;

        public static Result<City> Create(string? value)
        {
            var normalized = value?.Trim() ?? string.Empty;

            return Result.Combine(
                    Environment.NewLine,
                    Result.FailureIf(string.IsNullOrWhiteSpace(normalized), RequiredMessage),
                    Result.FailureIf(
                        !string.IsNullOrWhiteSpace(normalized) && normalized.Length is < MinimumLength or > MaximumLength,
                        InvalidLengthMessage))
                .Map(() => new City(normalized));
        }

        public override string ToString() =>
            Value;
    }
}
