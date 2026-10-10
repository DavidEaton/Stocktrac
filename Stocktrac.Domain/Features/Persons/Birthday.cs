using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons
{
    public sealed record Birthday
    {
        public static readonly DateOnly MinimumDate = new(1900, 1, 1);

        public DateOnly Value { get; }

        private Birthday(DateOnly date) =>
            Value = date;

        public static Result<Birthday> Create(DateOnly date, DateOnly today) =>
            date >= MinimumDate && date <= today
                ? Result.Success(new Birthday(date))
                : Result.Failure<Birthday>($"Birthday must be between {MinimumDate:d} and {today:d}.");

        public override string ToString() =>
            Value.ToShortDateString();
    }
}
