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
            Result.Success(date)
                .Ensure(value => value >= MinimumDate && value <= today,
                    $"Birthday must be between {MinimumDate:d} and {today:d}.")
                .Map(value => new Birthday(value));

        public override string ToString() =>
            Value.ToShortDateString();
    }
}
