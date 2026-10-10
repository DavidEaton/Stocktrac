using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record City
    {
        public const int MaximumLength = 100;
        public static readonly string InvalidLengthMessage = $"City must not exceed {MaximumLength} characters.";
        public NonEmptyString Value { get; }

        private City(NonEmptyString value) => Value = value;

        public static Result<City> Create(NonEmptyString value) =>
            value.Value.Length <= MaximumLength
                ? Result.Success(new City(value))
                : Result.Failure<City>(InvalidLengthMessage);

        public override string ToString() => Value.ToString();
    }
}
