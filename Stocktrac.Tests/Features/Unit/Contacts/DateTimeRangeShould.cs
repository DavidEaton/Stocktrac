using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class DateTimeRangeShould
{
    private static readonly DateTime Start = new(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void PreserveDates_On_Create_WhenEndFollowsStart()
    {
        var end = Start.AddHours(2);
        var result = DateTimeRange.Create(Start, end);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Start.ShouldBe(Start);
        result.Value.End.ShouldBe(end);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_Create_WhenEndDoesNotFollowStart(int minuteOffset) =>
        DateTimeRange.Create(Start, Start.AddMinutes(minuteOffset)).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void CalculateEnd_On_Create_WhenDurationIsPositive() =>
        DateTimeRange.Create(Start, TimeSpan.FromMinutes(90)).Value.End.ShouldBe(Start.AddMinutes(90));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_Create_WhenDurationIsNotPositive(int minutes) =>
        DateTimeRange.Create(Start, TimeSpan.FromMinutes(minutes)).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnCalculationError_On_Create_WhenDurationOverflows() =>
        DateTimeRange.Create(DateTime.MaxValue, TimeSpan.FromTicks(1)).Error.ShouldBe(DateTimeRange.DateCalculationMessage);

    [Fact]
    public void ReturnWholeMinutes_On_DurationInMinutes_WhenDurationHasPartialMinute()
    {
        var range = DateTimeRange.Create(Start, Start.AddSeconds(119)).Value;

        range.DurationInMinutes().Value.ShouldBe(1);
    }

    [Fact]
    public void ReturnCalculationError_On_DurationInMinutes_WhenDurationExceedsIntegerRange()
    {
        var range = DateTimeRange.Create(DateTime.MinValue, DateTime.MaxValue).Value;

        range.DurationInMinutes().Error.ShouldBe(DateTimeRange.DateCalculationMessage);
    }

    [Fact]
    public void ReturnCopy_On_NewStart_WhenNewStartPrecedesEnd()
    {
        var original = DateTimeRange.Create(Start, Start.AddHours(2)).Value;
        var updated = original.NewStart(Start.AddHours(1)).Value;

        updated.Start.ShouldBe(Start.AddHours(1));
        updated.End.ShouldBe(original.End);
        original.Start.ShouldBe(Start);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void ReturnRequiredError_On_NewStart_WhenNewStartDoesNotPrecedeEnd(int hours) =>
        DateTimeRange.Create(Start, Start.AddHours(2)).Value.NewStart(Start.AddHours(hours)).Error
            .ShouldBe(DateTimeRange.RequiredMessage);

    [Fact]
    public void ReturnCopy_On_NewEnd_WhenNewEndFollowsStart() =>
        DateTimeRange.Create(Start, Start.AddHours(2)).Value.NewEnd(Start.AddHours(3)).Value.End
            .ShouldBe(Start.AddHours(3));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnRequiredError_On_NewEnd_WhenNewEndDoesNotFollowStart(int hours) =>
        DateTimeRange.Create(Start, Start.AddHours(2)).Value.NewEnd(Start.AddHours(hours)).Error
            .ShouldBe(DateTimeRange.RequiredMessage);

    [Fact]
    public void SetMaximumEnd_On_ClearEnd_WhenRangeIsFinite()
    {
        var original = DateTimeRange.Create(Start, Start.AddHours(2)).Value;
        var updated = original.ClearEnd().Value;

        updated.Start.ShouldBe(Start);
        updated.End.ShouldBe(DateTime.MaxValue);
        original.End.ShouldBe(Start.AddHours(2));
    }

    [Fact]
    public void ReplaceDuration_On_NewDuration_WhenDurationIsValid() =>
        DateTimeRange.Create(Start, Start.AddHours(2)).Value.NewDuration(TimeSpan.FromDays(1)).Value.End
            .ShouldBe(Start.AddDays(1));

    [Fact]
    public void CalculateEnd_On_CreateDaysRange_WhenDayCountIsValid() =>
        DateTimeRange.CreateDaysRange(Start, 2).Value.End.ShouldBe(Start.AddDays(2));

    [Fact]
    public void CalculateEnd_On_CreateWeeksRange_WhenWeekCountIsValid() =>
        DateTimeRange.CreateWeeksRange(Start, 2).Value.End.ShouldBe(Start.AddDays(14));

    [Fact]
    public void CalculateEnd_On_CreateMonthsRange_WhenMonthCountIsValid() =>
        DateTimeRange.CreateMonthsRange(Start, 2).Value.End.ShouldBe(Start.AddMonths(2));

    [Fact]
    public void ReturnCalculationError_On_RangeFactories_WhenCalculationExceedsDateLimits()
    {
        DateTimeRange.CreateDaysRange(DateTime.MaxValue, 1).Error.ShouldBe(DateTimeRange.DateCalculationMessage);
        DateTimeRange.CreateWeeksRange(DateTime.MaxValue, int.MaxValue).Error.ShouldBe(DateTimeRange.DateCalculationMessage);
        DateTimeRange.CreateMonthsRange(DateTime.MaxValue, 1).Error.ShouldBe(DateTimeRange.DateCalculationMessage);
    }

    [Fact]
    public void ReturnEndBeforeStartError_On_RangeFactories_WhenCountIsZero()
    {
        DateTimeRange.CreateDaysRange(Start, 0).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);
        DateTimeRange.CreateWeeksRange(Start, 0).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);
        DateTimeRange.CreateMonthsRange(Start, 0).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenDatesAreEqual()
    {
        var first = DateTimeRange.Create(Start, Start.AddDays(1)).Value;
        var second = DateTimeRange.Create(Start, Start.AddDays(1)).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(DateTimeRange.Create(Start, Start.AddDays(2)).Value);
    }
}