using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Employees;

public sealed record EmploymentPeriod
{
    public const string DateRangeMessage = "Employment date(s) invalid.";

    public DateOnly Hired { get; }
    public Maybe<DateOnly> Exited { get; }
    public bool Active => Exited.HasNoValue;

    private EmploymentPeriod(DateOnly hired, Maybe<DateOnly> exited) =>
        (Hired, Exited) = (hired, exited);

    public static DateOnly StartDateMinimum(DateOnly date) =>
        date.Year > 50 ? date.AddYears(-50) : DateOnly.MinValue;

    public static DateOnly EndDateMaximum(DateOnly date) =>
        date.Year < 9999 ? date.AddYears(1) : DateOnly.MaxValue;

    public static Result<EmploymentPeriod> Create(
        DateOnly hired, DateOnly date, Maybe<DateOnly> exited = default) =>
        Result.Success(hired)
            .Ensure(value => IsWithinAllowedRange(value, date), DateRangeMessage)
            .Ensure(_ => exited.HasNoValue || IsWithinAllowedRange(exited.Value, date), DateRangeMessage)
            .Ensure(value => exited.HasNoValue || value <= exited.Value, DateRangeMessage)
            .Map(value => new EmploymentPeriod(value, exited));

    public Result<EmploymentPeriod> ReplaceHired(DateOnly hired, DateOnly date) =>
        Result.Success(hired)
            .Ensure(value => IsWithinAllowedRange(value, date), DateRangeMessage)
            .Ensure(value => Exited.HasNoValue || value <= Exited.Value, DateRangeMessage)
            .Map(value => new EmploymentPeriod(value, Exited));

    public Result<EmploymentPeriod> ReplaceExited(DateOnly exited, DateOnly date) =>
        Result.Success(exited)
            .Ensure(value => IsWithinAllowedRange(value, date), DateRangeMessage)
            .Ensure(value => value >= Hired, DateRangeMessage)
            .Map(value => new EmploymentPeriod(Hired, value));

    public EmploymentPeriod RemoveExited() => new(Hired, Maybe<DateOnly>.None);

    private static bool IsWithinAllowedRange(DateOnly value, DateOnly date) =>
        value >= StartDateMinimum(date) && value <= EndDateMaximum(date);
}
