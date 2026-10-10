using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public sealed record Note
    {
        public const int MaximumLength = 10000;
        public static readonly string MaximumLengthMessage = $"Notes must be {MaximumLength} or fewer characters in length.";
        public NonEmptyString Value { get; }

        public static Result<Note> Create(NonEmptyString notes) =>
            notes.Value.Length <= MaximumLength
                ? Result.Success(new Note(notes))
                : Result.Failure<Note>(MaximumLengthMessage);

        private Note(NonEmptyString note) => Value = note;

        public override string ToString() => Value.ToString();
    }
}
