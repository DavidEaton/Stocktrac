using Shouldly;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class BirthdayShould
{
    private static readonly DateOnly Today = new(2025, 6, 15);

    [Fact]
    public void ContainDateOnlyValue_On_Create_WhenDateIsValid()
    {
        var date = new DateOnly(1990, 6, 15);

        var result = Birthday.Create(date, Today);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(date);
        result.Value.Value.GetType().ShouldBe(typeof(DateOnly));
    }

    [Theory]
    [MemberData(nameof(ValidBoundaryDates))]
    public void ReturnSuccess_On_Create_WhenDateIsOnBoundary(DateOnly date)
    {
        var result = Birthday.Create(date, Today);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(date);
    }

    [Theory]
    [MemberData(nameof(InvalidDates))]
    public void ReturnFailure_On_Create_WhenDateIsOutsideAllowedRange(DateOnly date)
    {
        var result = Birthday.Create(date, Today);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(
            $"Birthday must be between {Birthday.MinimumDate:d} and {Today:d}.");
    }

    [Fact]
    public void ReturnShortDate_On_ToString()
    {
        var birthday = Birthday.Create(new DateOnly(1990, 6, 15), Today).Value;

        birthday.ToString().ShouldBe(birthday.Value.ToShortDateString());
    }

    [Theory]
    [InlineData(2025, 12, 31, 2026, 1, 1)]
    [InlineData(2024, 2, 28, 2024, 2, 29)]
    public void UseEvaluationDate_On_Create_WhenDateBecomesValidTheNextDay(
        int previousYear, int previousMonth, int previousDay,
        int year, int month, int day)
    {
        var previousDate = new DateOnly(previousYear, previousMonth, previousDay);
        var date = new DateOnly(year, month, day);

        Birthday.Create(date, previousDate).Error.ShouldBe(
            $"Birthday must be between {Birthday.MinimumDate:d} and {previousDate:d}.");
        Birthday.Create(date, date).Value.Value.ShouldBe(date);
        Birthday.Create(date, previousDate).Error.ShouldBe(
            $"Birthday must be between {Birthday.MinimumDate:d} and {previousDate:d}.");
    }

    [Fact]
    public void RejectMinimumBirthday_On_Create_WhenEvaluationDateIsBeforeMinimum()
    {
        var today = Birthday.MinimumDate.AddDays(-1);

        var result = Birthday.Create(Birthday.MinimumDate, today);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe($"Birthday must be between {Birthday.MinimumDate:d} and {today:d}.");
    }

    [Fact]
    public void AcceptMaximumBirthday_On_Create_WhenEvaluationDateIsDateOnlyMaximum()
    {
        var result = Birthday.Create(DateOnly.MaxValue, DateOnly.MaxValue);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(DateOnly.MaxValue);
    }

    [Fact]
    public void CompareEqual_On_Create_WhenBirthdayMatchesAndEvaluationDatesDiffer()
    {
        var date = new DateOnly(1990, 6, 15);
        var earlier = Birthday.Create(date, new DateOnly(2020, 1, 1)).Value;
        var later = Birthday.Create(date, new DateOnly(2030, 1, 1)).Value;

        earlier.ShouldBe(later);
    }

    public static TheoryData<DateOnly> ValidBoundaryDates =>
        new()
        {
            Birthday.MinimumDate,
            Today
        };

    public static TheoryData<DateOnly> InvalidDates =>
        new()
        {
            Birthday.MinimumDate.AddDays(-1),
            Today.AddDays(1)
        };
}
