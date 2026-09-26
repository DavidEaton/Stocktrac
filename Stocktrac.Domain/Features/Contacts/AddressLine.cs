using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record AddressLine
    {
        public const int MaximumLength = 255;
        public static readonly string InvalidLengthMessage = $"Address must not exceed {MaximumLength} characters.";
        public NonEmptyString Value { get; }

        private AddressLine(NonEmptyString value) => Value = value;

        public static Result<AddressLine> Create(NonEmptyString value) =>
            Result.Success(value)
                .Ensure(value => value.Value.Length <= MaximumLength, InvalidLengthMessage)
                .Map(value => new AddressLine(value));

        public override string ToString() => Value.ToString();
    }
}
