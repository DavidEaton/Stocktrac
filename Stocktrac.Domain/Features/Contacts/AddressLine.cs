using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public readonly record struct AddressLine
    {
        public const int MinimumLength = 2;
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
        public static readonly string RequiredMessage = "Address Line is required.";
        public string Value { get; }

        private AddressLine(string value) =>
            Value = value;

        public static Result<AddressLine> Create(string? value) =>
            Result.Success(value?.Trim() ?? string.Empty)
                .Ensure(value => !string.IsNullOrWhiteSpace(value), RequiredMessage)
                .Ensure(
                    value => value.Length is >= MinimumLength and <= MaximumLength,
                    InvalidLengthMessage)
                .Map(value => new AddressLine(value));

        public override string ToString() =>
            Value ?? string.Empty;
    }
}
