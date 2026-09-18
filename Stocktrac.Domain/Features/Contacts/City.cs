using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record City
    {
        public const int MinimumLength = 1;
        public const int MaximumLength = 100;
        public static readonly string InvalidLengthMessage = $"City must be between {MinimumLength} and {MaximumLength} characters.";
        public const string RequiredMessage = "City is required.";
        public string Value { get; }

        private City(string value) => Value = value;

        public static Result<City> Create(string value) =>
            Result.Success(value)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), RequiredMessage)
                .Ensure(normalized => normalized.Length.IsWithin(MinimumLength, MaximumLength), InvalidLengthMessage)
                .Map(normalized => new City(normalized));

        public static implicit operator string(City city) => city.Value;

        public static explicit operator City(string value) => new(value);

        public override string ToString() => Value;
    }
}
