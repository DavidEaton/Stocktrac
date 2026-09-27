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
            Result.Success(value)
                .Ensure(value => value.Value.Length <= MaximumLength, InvalidLengthMessage)
                .Map(value => new City(value));

        public override string ToString() => Value.ToString();
    }
}
