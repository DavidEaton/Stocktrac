using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class DateTimeRangeShould
{
    private static readonly DateOnly Start = new(2025, 1, 15);

    [Fact]
    public void PreserveDates_On_Create_WhenEndFollowsStart()
    {
        var end = Start.AddDays(2);
        var result = DateTimeRange.Create(Start, end);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Start.ShouldBe(Start);
        result.Value.End.ShouldBe(end);
    }

    [Fact]
    public void ExposeDateOnlyBoundaries_On_StartAndEnd()
    {
        typeof(DateTimeRange).GetProperty(nameof(DateTimeRange.Start))!.PropertyType.ShouldBe(typeof(DateOnly));
        typeof(DateTimeRange).GetProperty(nameof(DateTimeRange.End))!.PropertyType.ShouldBe(typeof(DateOnly));
    }

    [Fact]
    public void ExposeImmutableBoundaries_On_StartAndEnd()
    {
        typeof(DateTimeRange).GetProperty(nameof(DateTimeRange.Start))!.SetMethod.ShouldBeNull();
        typeof(DateTimeRange).GetProperty(nameof(DateTimeRange.End))!.SetMethod.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_Create_WhenEndDoesNotFollowStart(int dayOffset)
    {
        var result = DateTimeRange.Create(Start, Start.AddDays(dayOffset));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);
    }

    [Fact]
    public void ReturnDayCount_On_DurationInDays_WhenRangeIsValid() =>
        DateTimeRange.Create(Start, Start.AddDays(10)).Value.DurationInDays().Value.ShouldBe(10);

    [Fact]
    public void ReturnFailure_On_DurationInDays_WhenRangeIsNull() =>
        DateTimeRangeExtensions.DurationInDays(null!).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnCopy_On_WithStart_WhenStartPrecedesEnd()
    {
        var original = DateTimeRange.Create(Start, Start.AddDays(2)).Value;
        var updated = original.WithStart(Start.AddDays(1)).Value;

        updated.Start.ShouldBe(Start.AddDays(1));
        updated.End.ShouldBe(original.End);
        original.Start.ShouldBe(Start);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void ReturnEndBeforeStartError_On_WithStart_WhenStartDoesNotPrecedeEnd(int days) =>
        DateTimeRange.Create(Start, Start.AddDays(2)).Value.WithStart(Start.AddDays(days)).Error
            .ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnFailure_On_WithStart_WhenRangeIsNull() =>
        DateTimeRangeExtensions.WithStart(null!, Start).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnCopy_On_WithEnd_WhenEndFollowsStart() =>
        DateTimeRange.Create(Start, Start.AddDays(2)).Value.WithEnd(Start.AddDays(3)).Value.End
            .ShouldBe(Start.AddDays(3));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_WithEnd_WhenEndDoesNotFollowStart(int days) =>
        DateTimeRange.Create(Start, Start.AddDays(2)).Value.WithEnd(Start.AddDays(days)).Error
            .ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnFailure_On_WithEnd_WhenRangeIsNull() =>
        DateTimeRangeExtensions.WithEnd(null!, Start).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Fact]
    public void SetMaximumEnd_On_WithoutEnd_WhenRangeIsFinite()
    {
        var original = DateTimeRange.Create(Start, Start.AddDays(2)).Value;
        var updated = original.WithoutEnd().Value;

        updated.Start.ShouldBe(Start);
        updated.End.ShouldBe(DateOnly.MaxValue);
        original.End.ShouldBe(Start.AddDays(2));
    }

    [Fact]
    public void ReturnFailure_On_WithoutEnd_WhenRangeIsNull() =>
        DateTimeRangeExtensions.WithoutEnd(null!).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

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
        DateTimeRange.CreateDaysRange(DateOnly.MaxValue, 1).Error.ShouldBe(DateTimeRange.DateCalculationMessage);
        DateTimeRange.CreateWeeksRange(DateOnly.MaxValue, int.MaxValue).Error.ShouldBe(DateTimeRange.DateCalculationMessage);
        DateTimeRange.CreateMonthsRange(DateOnly.MaxValue, 1).Error.ShouldBe(DateTimeRange.DateCalculationMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_CreateDaysRange_WhenDayCountIsNotPositive(int days) =>
        DateTimeRange.CreateDaysRange(Start, days).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_CreateWeeksRange_WhenWeekCountIsNotPositive(int weeks) =>
        DateTimeRange.CreateWeeksRange(Start, weeks).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_CreateMonthsRange_WhenMonthCountIsNotPositive(int months) =>
        DateTimeRange.CreateMonthsRange(Start, months).Error.ShouldBe(DateTimeRange.EndBeforeStartMessage);

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
