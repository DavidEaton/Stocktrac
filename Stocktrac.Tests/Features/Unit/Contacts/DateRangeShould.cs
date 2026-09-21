using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class DateRangeShould
{
    private static readonly DateOnly Start = new(2025, 1, 15);

    [Fact]
    public void PreserveDates_On_Create_WhenEndFollowsStart()
    {
        var end = Start.AddDays(2);
        var result = DateRange.Create(Start, end);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Start.ShouldBe(Start);
        result.Value.End.ShouldBe(end);
    }

    [Fact]
    public void ExposeDateOnlyBoundaries_On_StartAndEnd()
    {
        typeof(DateRange).GetProperty(nameof(DateRange.Start))!.PropertyType.ShouldBe(typeof(DateOnly));
        typeof(DateRange).GetProperty(nameof(DateRange.End))!.PropertyType.ShouldBe(typeof(DateOnly));
    }

    [Fact]
    public void ExposeImmutableBoundaries_On_StartAndEnd()
    {
        typeof(DateRange).GetProperty(nameof(DateRange.Start))!.SetMethod.ShouldBeNull();
        typeof(DateRange).GetProperty(nameof(DateRange.End))!.SetMethod.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_Create_WhenEndDoesNotFollowStart(int dayOffset)
    {
        var result = DateRange.Create(Start, Start.AddDays(dayOffset));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DateRange.EndBeforeStartMessage);
    }

    [Fact]
    public void ReturnDayCount_On_DurationInDays_WhenRangeIsValid() =>
        DateRange.Create(Start, Start.AddDays(10)).Value.DurationInDays().Value.ShouldBe(10);

    [Fact]
    public void ReturnFailure_On_DurationInDays_WhenRangeIsNull() =>
        DateRangeExtensions.DurationInDays(null!).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnCopy_On_WithStart_WhenStartPrecedesEnd()
    {
        var original = DateRange.Create(Start, Start.AddDays(2)).Value;
        var updated = original.WithStart(Start.AddDays(1)).Value;

        updated.Start.ShouldBe(Start.AddDays(1));
        updated.End.ShouldBe(original.End);
        original.Start.ShouldBe(Start);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void ReturnEndBeforeStartError_On_WithStart_WhenStartDoesNotPrecedeEnd(int days) =>
        DateRange.Create(Start, Start.AddDays(2)).Value.WithStart(Start.AddDays(days)).Error
            .ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnFailure_On_WithStart_WhenRangeIsNull() =>
        DateRangeExtensions.WithStart(null!, Start).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnCopy_On_WithEnd_WhenEndFollowsStart() =>
        DateRange.Create(Start, Start.AddDays(2)).Value.WithEnd(Start.AddDays(3)).Value.End
            .ShouldBe(Start.AddDays(3));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_WithEnd_WhenEndDoesNotFollowStart(int days) =>
        DateRange.Create(Start, Start.AddDays(2)).Value.WithEnd(Start.AddDays(days)).Error
            .ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void ReturnFailure_On_WithEnd_WhenRangeIsNull() =>
        DateRangeExtensions.WithEnd(null!, Start).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void SetMaximumEnd_On_WithoutEnd_WhenRangeIsFinite()
    {
        var original = DateRange.Create(Start, Start.AddDays(2)).Value;
        var updated = original.WithoutEnd().Value;

        updated.Start.ShouldBe(Start);
        updated.End.ShouldBe(DateOnly.MaxValue);
        original.End.ShouldBe(Start.AddDays(2));
    }

    [Fact]
    public void ReturnFailure_On_WithoutEnd_WhenRangeIsNull() =>
        DateRangeExtensions.WithoutEnd(null!).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void CalculateEnd_On_CreateDaysRange_WhenDayCountIsValid() =>
        DateRange.CreateDaysRange(Start, 2).Value.End.ShouldBe(Start.AddDays(2));

    [Fact]
    public void CalculateEnd_On_CreateWeeksRange_WhenWeekCountIsValid() =>
        DateRange.CreateWeeksRange(Start, 2).Value.End.ShouldBe(Start.AddDays(14));

    [Fact]
    public void CalculateEnd_On_CreateMonthsRange_WhenMonthCountIsValid() =>
        DateRange.CreateMonthsRange(Start, 2).Value.End.ShouldBe(Start.AddMonths(2));

    [Fact]
    public void ReturnCalculationError_On_RangeFactories_WhenCalculationExceedsDateLimits()
    {
        DateRange.CreateDaysRange(DateOnly.MaxValue, 1).Error.ShouldBe(DateRange.DateCalculationMessage);
        DateRange.CreateWeeksRange(DateOnly.MaxValue, int.MaxValue).Error.ShouldBe(DateRange.DateCalculationMessage);
        DateRange.CreateMonthsRange(DateOnly.MaxValue, 1).Error.ShouldBe(DateRange.DateCalculationMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_CreateDaysRange_WhenDayCountIsNotPositive(int days) =>
        DateRange.CreateDaysRange(Start, days).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_CreateWeeksRange_WhenWeekCountIsNotPositive(int weeks) =>
        DateRange.CreateWeeksRange(Start, weeks).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnEndBeforeStartError_On_CreateMonthsRange_WhenMonthCountIsNotPositive(int months) =>
        DateRange.CreateMonthsRange(Start, months).Error.ShouldBe(DateRange.EndBeforeStartMessage);

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenDatesAreEqual()
    {
        var first = DateRange.Create(Start, Start.AddDays(1)).Value;
        var second = DateRange.Create(Start, Start.AddDays(1)).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(DateRange.Create(Start, Start.AddDays(2)).Value);
    }
}
