using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features
{
    public sealed record NonEmptyString
    {
        public const string RequiredMessage = "Value is required.";

        private NonEmptyString(string value) => Value = value;

        public string Value { get; }

        public static Result<NonEmptyString> Create(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? Result.Failure<NonEmptyString>(RequiredMessage)
                : Result.Success(new NonEmptyString(value.Trim()));

        public override string ToString() => Value;
    }
}
