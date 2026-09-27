using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features
{
    public record struct NonEmptyString
    {
        public const string RequiredMessage = "Value is required.";
        private NonEmptyString(string value) => Value = value;

        public string Value { get; }

        public static Result<NonEmptyString> Create(string value) =>
            Result.Success(value)
                .Ensure(value => !string.IsNullOrWhiteSpace(value), RequiredMessage)
                .Map(value => new NonEmptyString(value));

        public override readonly string ToString() => Value;
    }
}