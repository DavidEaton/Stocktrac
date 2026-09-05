using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public readonly record struct City
    {
        public const int MinimumLength = 1;
        public const int MaximumLength = 100;
        public static readonly string InvalidLengthMessage = $"City must be between {MinimumLength} and {MaximumLength} characters.";
        public static readonly string RequiredMessage = "City is required.";
        public string Value { get; }

        private City(string value) =>
            Value = value;

        public static Result<City> Create(string? value) =>
            Result.Success(value?.Trim() ?? string.Empty)
                .Ensure(value => !string.IsNullOrWhiteSpace(value), RequiredMessage)
                .Ensure(
                    value => value.Length is >= MinimumLength and <= MaximumLength,
                    InvalidLengthMessage)
                .Map(value => new City(value));

        public override string ToString() =>
            Value ?? string.Empty;
    }
}
