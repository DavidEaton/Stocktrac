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
            Result.Success(date)
                .Ensure(
                    value => value >= MinimumDate && value <= MaximumDate,
                    $"Birthday must be between {MinimumDate:d} and {MaximumDate:d}")
                .Map(value => new Birthday(value));

        public static implicit operator DateTime(Birthday birthday) =>
            birthday._date;

        public override string ToString() =>
            _date.ToShortDateString();
    }
}
