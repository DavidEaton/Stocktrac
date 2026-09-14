using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public static class DateTimeRangeExtensions
{
    public static Result<int> DurationInMinutes(this DateTimeRange range) =>
        Result.Success((range.End - range.Start).TotalMinutes)
            .Ensure(minutes => minutes is >= int.MinValue and <= int.MaxValue, DateTimeRange.DateCalculationMessage)
            .Map(minutes => (int)minutes);

    public static Result<DateTimeRange> WithStart(this DateTimeRange range, DateTime newStart) =>
        Result.Success(newStart)
            .Ensure(start => start < range.End, DateTimeRange.RequiredMessage)
            .Bind(start => DateTimeRange.Create(start, range.End));

    public static Result<DateTimeRange> WithEnd(this DateTimeRange range, DateTime newEnd) =>
        Result.Success(newEnd)
            .Ensure(end => range.Start < end, DateTimeRange.RequiredMessage)
            .Bind(end => DateTimeRange.Create(range.Start, end));

    public static Result<DateTimeRange> WithoutEnd(this DateTimeRange range) =>
        DateTimeRange.Create(range.Start, DateTime.MaxValue);

    public static Result<DateTimeRange> WithDuration(this DateTimeRange range, TimeSpan newDuration) =>
        DateTimeRange.Create(range.Start, newDuration);
}
