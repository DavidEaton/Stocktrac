using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public static class DateTimeRangeExtensions
{
    public static Result<int> DurationInMinutes(this DateTimeRange range) =>
        range is null
            ? Result.Failure<int>(DateTimeRange.EndBeforeStartMessage)
            : Result.Success((range.End - range.Start).TotalMinutes)
            .Ensure(minutes => minutes is >= int.MinValue and <= int.MaxValue, DateTimeRange.DateCalculationMessage)
            .Map(minutes => (int)minutes);

    public static Result<DateTimeRange> WithStart(this DateTimeRange range, DateTime newStart) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : Result.Success(newStart)
            .Ensure(start => start < range.End, DateTimeRange.EndBeforeStartMessage)
            .Bind(start => DateTimeRange.Create(start, range.End));

    public static Result<DateTimeRange> WithEnd(this DateTimeRange range, DateTime newEnd) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : Result.Success(newEnd)
            .Ensure(end => range.Start < end, DateTimeRange.EndBeforeStartMessage)
            .Bind(end => DateTimeRange.Create(range.Start, end));

    public static Result<DateTimeRange> WithoutEnd(this DateTimeRange range) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : DateTimeRange.Create(range.Start, DateTime.MaxValue);

    public static Result<DateTimeRange> WithDuration(this DateTimeRange range, TimeSpan newDuration) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : DateTimeRange.Create(range.Start, newDuration);

    internal static Result<DateTimeRange> CalculateEnd(
        DateTime start,
        Func<DateTime> calculation) =>
        Result.Try(
                calculation,
                _ => DateTimeRange.DateCalculationMessage)
            .Bind(end => DateTimeRange.Create(start, end));
}
