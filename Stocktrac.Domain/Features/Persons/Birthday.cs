using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons
{
    public sealed record Birthday
    {
        public static readonly DateOnly MinimumDate = new(1900, 1, 1);
        public static DateOnly MaximumDate => DateOnly.FromDateTime(DateTime.Today);

        public DateOnly Value { get; }

        private Birthday(DateOnly date) =>
            Value = date;

        public static Result<Birthday> Create(DateOnly date) =>
            Result.Combine(
                    Environment.NewLine,
                    Result.FailureIf(
                        date < MinimumDate || date > MaximumDate,
                        $"Birthday must be between {MinimumDate:d} and {MaximumDate:d}."))
                .Map(() => new Birthday(date));

        public override string ToString() =>
            Value.ToShortDateString();
    }
}
