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
            value.Value.Length <= MaximumLength
                ? Result.Success(new AddressLine(value))
                : Result.Failure<AddressLine>(InvalidLengthMessage);

        public override string ToString() => Value.ToString();
    }
}
