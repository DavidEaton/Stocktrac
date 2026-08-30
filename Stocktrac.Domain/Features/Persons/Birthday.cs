using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons
{
    public readonly record struct Birthday
    {
        public static readonly DateTime MinimumDate = new(1900, 1, 1);
        public static readonly DateTime MaximumDate = DateTime.Today;
        public static readonly string InvalidValueMessage = $"Birthday must be between {MinimumDate:d} and {MaximumDate:d}";
        public static readonly Birthday None = new();
        private readonly DateTime _date;

        private Birthday(DateTime date) =>
            _date = date;

        public static Result<Birthday> Create(DateTime date) =>
            IsValidAgeOn(date)
                ? Result.Success(new Birthday(date))
                : Result.Failure<Birthday>(InvalidValueMessage);

        public static bool IsValidAgeOn(DateTime? date) =>
            !date.HasValue || (date.Value >= MinimumDate && date.Value <= MaximumDate);

        public static implicit operator DateTime(Birthday birthday) => birthday._date;

        public static implicit operator Birthday(DateTime date) => new(date);

        public override string ToString() => _date.ToShortDateString();

        // EF requires a parameterless constructor
        public Birthday() => _date = None;
    }
}