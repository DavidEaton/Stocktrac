using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts
{
    public readonly record struct Note
    {
        public const int MaximumLength = 10000;
        public static readonly string MaximumLengthMessage = $"Notes must be {MaximumLength} or fewer characters in length.";
        public string Value { get; }

        public static Result<Note> Create(string? notes) =>
            Result.Success(Normalize(notes))
                .Ensure(
                    value => value.Length <= MaximumLength,
                    MaximumLengthMessage)
                .Map(value => new Note(value));

        private Note(string note) => Value = note;

        private static string Normalize(string? notes) =>
            notes?.Trim() ?? string.Empty;
    }
}
