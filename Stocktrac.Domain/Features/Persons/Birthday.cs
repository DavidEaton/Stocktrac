using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons
{
    public readonly record struct Birthday
    {
        public static readonly DateTime MinimumDate = new(1900, 1, 1);
        public static DateTime MaximumDate => DateTime.Today;

        private readonly DateTime _date;

        private Birthday(DateTime date) =>
            _date = date;

        public static Result<Birthday> Create(DateTime date) =>
            date >= MinimumDate && date <= MaximumDate
                ? Result.Success(new Birthday(date))
                : Result.Failure<Birthday>(
                    $"Birthday must be between {MinimumDate:d} and {MaximumDate:d}");

        public static implicit operator DateTime(Birthday birthday) =>
            birthday._date;

        public override string ToString() =>
            _date.ToShortDateString();
    }
}