using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record Note
    {
        public const int MaximumLength = 10000;
        public static readonly string MaximumLengthMessage = $"Notes must be {MaximumLength} or fewer characters in length.";
        public NonEmptyString Value { get; }

        public static Result<Note> Create(NonEmptyString notes) =>
            Result.Success(notes)
                .Ensure(
                    value => value.Value.Length <= MaximumLength,
                    MaximumLengthMessage)
                .Map(value => new Note(value));

        private Note(NonEmptyString note) => Value = note;

        public override string ToString() => Value.ToString();
    }
}
