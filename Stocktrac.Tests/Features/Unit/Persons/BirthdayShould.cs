using Shouldly;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class BirthdayShould
{
    [Fact]
    public void ContainDateOnlyValue_On_Create_WhenDateIsValid()
    {
        var date = new DateOnly(1990, 6, 15);

        var result = Birthday.Create(date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(date);
        result.Value.Value.GetType().ShouldBe(typeof(DateOnly));
    }

    [Theory]
    [MemberData(nameof(ValidBoundaryDates))]
    public void ReturnSuccess_On_Create_WhenDateIsOnBoundary(DateOnly date)
    {
        var result = Birthday.Create(date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(date);
    }

    [Theory]
    [MemberData(nameof(InvalidDates))]
    public void ReturnFailure_On_Create_WhenDateIsOutsideAllowedRange(DateOnly date)
    {
        var result = Birthday.Create(date);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(
            $"Birthday must be between {Birthday.MinimumDate:d} and {Birthday.MaximumDate:d}.");
    }

    [Fact]
    public void ReturnShortDate_On_ToString()
    {
        var birthday = Birthday.Create(new DateOnly(1990, 6, 15)).Value;

        birthday.ToString().ShouldBe(birthday.Value.ToShortDateString());
    }

    public static TheoryData<DateOnly> ValidBoundaryDates =>
        new()
        {
            Birthday.MinimumDate,
            Birthday.MaximumDate
        };

    public static TheoryData<DateOnly> InvalidDates =>
        new()
        {
            Birthday.MinimumDate.AddDays(-1),
            Birthday.MaximumDate.AddDays(1)
        };
}
