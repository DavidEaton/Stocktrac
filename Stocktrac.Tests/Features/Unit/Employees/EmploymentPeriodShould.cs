using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Employees;

namespace Stocktrac.Tests.Features.Unit.Employees;

public class EmploymentPeriodShould
{
    private static readonly DateOnly Date = new(2025, 1, 15);

    [Fact]
    public void PreserveHireDateAndAbsence_On_Create_WhenExitIsAbsent()
    {
        var result = EmploymentPeriod.Create(Date, Date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Hired.ShouldBe(Date);
        result.Value.Exited.HasNoValue.ShouldBeTrue();
        result.Value.Active.ShouldBeTrue();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void ValidateOrdering_On_Create_WhenExitIsPresent(int offset, bool success)
    {
        var result = EmploymentPeriod.Create(Date, Date, Date.AddDays(offset));

        result.IsSuccess.ShouldBe(success);
        if (success)
        {
            result.Value.Exited.Value.ShouldBe(Date.AddDays(offset));
            result.Value.Active.ShouldBeFalse();
        }
        else
            result.Error.ShouldBe(EmploymentPeriod.DateRangeMessage);
    }

    [Theory]
    [InlineData(-50, -1, false)]
    [InlineData(-50, 0, true)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    public void ValidateHireBounds_On_Create_UsingSuppliedDate(int years, int days, bool success)
    {
        EmploymentPeriod.Create(Date.AddYears(years).AddDays(days), Date)
            .IsSuccess.ShouldBe(success);
    }

    [Theory]
    [InlineData(-50, -1, false)]
    [InlineData(-50, 0, true)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    public void ValidateExitBounds_On_Create_UsingSuppliedDate(int years, int days, bool success)
    {
        EmploymentPeriod.Create(Date.AddYears(-50), Date, Date.AddYears(years).AddDays(days))
            .IsSuccess.ShouldBe(success);
    }

    [Fact]
    public void ReturnNewValueAndPreserveOriginal_On_ReplaceHired_WhenDateIsValid()
    {
        var original = EmploymentPeriod.Create(Date, Date, Date.AddDays(2)).Value;

        var result = original.ReplaceHired(Date.AddDays(1), Date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Hired.ShouldBe(Date.AddDays(1));
        result.Value.Exited.ShouldBe(original.Exited);
        original.Hired.ShouldBe(Date);
    }

    [Fact]
    public void ReturnFailureAndPreserveOriginal_On_ReplaceHired_WhenHireFollowsExit()
    {
        var original = EmploymentPeriod.Create(Date, Date, Date).Value;

        var result = original.ReplaceHired(Date.AddDays(1), Date);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmploymentPeriod.DateRangeMessage);
        original.Hired.ShouldBe(Date);
        original.Exited.Value.ShouldBe(Date);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(366)]
    public void ReturnFailureAndPreserveOriginal_On_ReplaceExited_WhenDateIsInvalid(int days)
    {
        var original = EmploymentPeriod.Create(Date, Date, Date).Value;

        var result = original.ReplaceExited(Date.AddDays(days), Date);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmploymentPeriod.DateRangeMessage);
        original.Exited.Value.ShouldBe(Date);
    }

    [Fact]
    public void ReturnNewValueAndPreserveOriginal_On_ReplaceExited_WhenDateIsValid()
    {
        var original = EmploymentPeriod.Create(Date, Date).Value;

        var result = original.ReplaceExited(Date.AddDays(1), Date);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Exited.Value.ShouldBe(Date.AddDays(1));
        result.Value.Hired.ShouldBe(Date);
        original.Exited.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ReturnActiveValueAndPreserveOriginal_On_RemoveExited_WhenExitExists()
    {
        var original = EmploymentPeriod.Create(Date, Date, Date).Value;

        var result = original.RemoveExited();

        result.Hired.ShouldBe(Date);
        result.Exited.ShouldBe(Maybe<DateOnly>.None);
        result.Active.ShouldBeTrue();
        original.Exited.Value.ShouldBe(Date);
    }

    [Fact]
    public void RetainHistoricalHire_On_ReplaceExited_WhenHireHasAgedOutsideCurrentBounds()
    {
        var hired = Date.AddYears(-50);
        var original = EmploymentPeriod.Create(hired, Date).Value;

        var result = original.ReplaceExited(Date.AddDays(1), Date.AddDays(1));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Hired.ShouldBe(hired);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompareByValue_On_Equals_WhenDatesMatch(bool exited)
    {
        var exit = exited ? Maybe<DateOnly>.From(Date) : Maybe<DateOnly>.None;
        var first = EmploymentPeriod.Create(Date, Date, exit).Value;
        var second = EmploymentPeriod.Create(Date, Date, exit).Value;

        first.ShouldNotBeSameAs(second);
        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        new HashSet<EmploymentPeriod> { first, second }.Count.ShouldBe(1);
    }

    [Fact]
    public void RemainUnequal_On_Equals_WhenDatesDiffer()
    {
        var active = EmploymentPeriod.Create(Date, Date).Value;

        active.ShouldNotBe(EmploymentPeriod.Create(Date.AddDays(-1), Date).Value);
        active.ShouldNotBe(EmploymentPeriod.Create(Date, Date, Date).Value);
    }
}
