using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public static class DateTimeRangeExtensions
{
    public static Result<int> DurationInDays(this DateTimeRange range) =>
        range is null
            ? Result.Failure<int>(DateTimeRange.EndBeforeStartMessage)
            : Result.Success(range.End.DayNumber - range.Start.DayNumber);

    public static Result<DateTimeRange> WithStart(this DateTimeRange range, DateOnly newStart) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : DateTimeRange.Create(newStart, range.End);

    public static Result<DateTimeRange> WithEnd(this DateTimeRange range, DateOnly newEnd) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : DateTimeRange.Create(range.Start, newEnd);

    public static Result<DateTimeRange> WithoutEnd(this DateTimeRange range) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.EndBeforeStartMessage)
            : DateTimeRange.Create(range.Start, DateOnly.MaxValue);
}
