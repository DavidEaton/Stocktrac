using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public static class DateTimeRangeExtensions
{
    public static Result<int> DurationInMinutes(this DateTimeRange range) =>
        range is null
            ? Result.Failure<int>(DateTimeRange.RequiredMessage)
            : Result.Success((range.End - range.Start).TotalMinutes)
            .Ensure(minutes => minutes is >= int.MinValue and <= int.MaxValue, DateTimeRange.DateCalculationMessage)
            .Map(minutes => (int)minutes);

    public static Result<DateTimeRange> WithStart(this DateTimeRange range, DateTime newStart) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.RequiredMessage)
            : Result.Success(newStart)
            .Ensure(start => start < range.End, DateTimeRange.RequiredMessage)
            .Map(range.WithStartValue);

    public static Result<DateTimeRange> WithEnd(this DateTimeRange range, DateTime newEnd) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.RequiredMessage)
            : Result.Success(newEnd)
            .Ensure(end => range.Start < end, DateTimeRange.RequiredMessage)
            .Map(range.WithEndValue);

    public static Result<DateTimeRange> WithoutEnd(this DateTimeRange range) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.RequiredMessage)
            : Result.Success(range.WithEndValue(DateTime.MaxValue));

    public static Result<DateTimeRange> WithDuration(this DateTimeRange range, TimeSpan newDuration) =>
        range is null
            ? Result.Failure<DateTimeRange>(DateTimeRange.RequiredMessage)
            : DateTimeRange.Create(range.Start, newDuration)
                .Map(updated => range.WithEndValue(updated.End));
}
