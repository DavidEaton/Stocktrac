using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public readonly record struct Note
    {
        public const int MaximumLength = 10000;
        public static readonly string MaximumLengthMessage = $"Notes must be {MaximumLength} or fewer characters in length.";
        // A nullable backing field is required because default(Note) cannot invoke a
        // struct constructor. The public domain value is nevertheless always non-null.
        private readonly string? value;
        public string Value => value ?? string.Empty;

        public static Result<Note> Create(string notes) =>
            Result.Success(Normalize(notes))
                .Ensure(
                    value => value.Length <= MaximumLength,
                    MaximumLengthMessage)
                .Map(value => new Note(value));

        private Note(string note) => value = note;

        public static implicit operator string(Note note) => note.Value;

        public static explicit operator Note(string value) => new(value);

        private static string Normalize(string? notes) =>
            notes?.Trim() ?? string.Empty;
    }
}
