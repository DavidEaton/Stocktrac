using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class DriversLicenseShould
{
    private static readonly DateOnly Start = new(2025, 1, 15);

    [Fact]
    public void PreserveComponents_On_Create_WhenStateIsDefined()
    {
        var number = CreateNumber("A123456");
        var range = CreateRange(Start, Start.AddYears(4));

        var result = DriversLicense.Create(number, State.CA, range, Start);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(number);
        result.Value.State.ShouldBe(State.CA);
        result.Value.ValidDateRange.ShouldBeEquivalentTo(range);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void ReturnStateInvalidError_On_Create_WhenStateIsUndefined(int stateValue)
    {
        var result = DriversLicense.Create(
            CreateNumber("A123456"),
            (State)stateValue,
            CreateRange(Start, Start.AddYears(4)), Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.StateInvalidMessage);
    }

    [Fact]
    public void ReturnEveryError_On_Create_WhenAllComponentsAreInvalid()
    {
        var result = DriversLicense.Create(null!, (State)(-1), null!, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(string.Join(
            Environment.NewLine,
            DriversLicense.RequiredMessage,
            DriversLicense.StateInvalidMessage));
    }

    [Fact]
    public void ReturnRequiredError_On_Create_WhenNumberIsNull()
    {
        var result = DriversLicense.Create(
            null!, State.CA, CreateRange(Start, Start.AddYears(4)), Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.RequiredMessage);
    }

    [Fact]
    public void ReturnRequiredError_On_Create_WhenDateRangeIsNull()
    {
        var result = DriversLicense.Create(
            CreateNumber("A123456"), State.CA, null!, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.RequiredMessage);
    }

    [Fact]
    public void PreserveDateOrder_On_Create_WhenEndFollowsStart()
    {
        var range = CreateRange(Start, Start.AddDays(1));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReturnStartDateTooEarlyError_On_Create_WhenStartPrecedesMinimumDate()
    {
        var range = CreateRange(
            DriversLicense.MinimumValidDate.AddDays(-1),
            DriversLicense.MinimumValidDate.AddYears(4));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.StartDateTooEarlyMessage);
    }

    [Fact]
    public void AcceptMinimumStartDate_On_Create_WhenStartEqualsMinimumDate()
    {
        var range = CreateRange(
            DriversLicense.MinimumValidDate,
            DriversLicense.MinimumValidDate.AddYears(4));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ValidDateRange.ShouldBe(range);
    }

    [Fact]
    public void ReturnStartDateInFutureError_On_Create_WhenStartFollowsCurrentDate()
    {
        var range = CreateRange(Start.AddDays(1), Start.AddYears(4));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.StartDateInFutureMessage);
    }

    [Fact]
    public void AcceptCurrentDate_On_Create_WhenStartEqualsToday()
    {
        var range = CreateRange(Start, Start.AddYears(4));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReturnDateRangeTooLongError_On_Create_WhenValidityExceedsMaximum()
    {
        var range = CreateRange(Start, Start.AddYears(DriversLicense.MaximumValidityYears).AddDays(1));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.DateRangeTooLongMessage);
    }

    [Fact]
    public void AcceptMaximumValidity_On_Create_WhenValidityEqualsMaximum()
    {
        var range = CreateRange(Start, Start.AddYears(DriversLicense.MaximumValidityYears));

        var result = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReplaceNumberAndPreserveOtherComponents_On_WithNumber_WhenNumberIsProvided()
    {
        var original = CreateLicense();
        var replacement = CreateNumber("B987654");

        var result = original.WithNumber(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(replacement);
        result.Value.State.ShouldBe(original.State);
        result.Value.ValidDateRange.ShouldBeEquivalentTo(original.ValidDateRange);
        original.Number.ShouldNotBe(replacement);
    }

    [Fact]
    public void ReturnRequiredError_On_WithNumber_WhenNumberIsNull()
    {
        var original = CreateLicense();

        var result = original.WithNumber(null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.RequiredMessage);
        original.Number.ShouldBe(CreateNumber("A123456"));
    }

    [Fact]
    public void ReplaceStateAndPreserveOtherComponents_On_WithState_WhenStateIsDefined()
    {
        var original = CreateLicense();

        var result = original.WithState(State.NY);

        result.IsSuccess.ShouldBeTrue();
        result.Value.State.ShouldBe(State.NY);
        result.Value.Number.ShouldBe(original.Number);
        result.Value.ValidDateRange.ShouldBeEquivalentTo(original.ValidDateRange);
        original.State.ShouldBe(State.CA);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void ReturnStateInvalidError_On_WithState_WhenStateIsUndefined(int stateValue)
    {
        var result = CreateLicense().WithState((State)stateValue);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.StateInvalidMessage);
    }

    [Fact]
    public void ReplaceValidDateRangeAndPreserveOtherComponents_On_WithValidDateRange_WhenRangeIsProvided()
    {
        var original = CreateLicense();
        var replacement = CreateRange(Start.AddDays(1), Start.AddYears(5));

        var result = original.WithValidDateRange(replacement, Start);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ValidDateRange.ShouldBeEquivalentTo(replacement);
        result.Value.Number.ShouldBe(original.Number);
        result.Value.State.ShouldBe(original.State);
        original.ValidDateRange.ShouldNotBeSameAs(replacement);
    }

    [Fact]
    public void ReturnRequiredError_On_WithValidDateRange_WhenRangeIsNull()
    {
        var original = CreateLicense();

        var result = original.WithValidDateRange(null!, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.RequiredMessage);
        original.ValidDateRange.ShouldBe(CreateRange(Start, Start.AddYears(4)));
    }

    [Fact]
    public void ReturnValidationError_On_WithValidDateRange_WhenRangeExceedsMaximumValidity()
    {
        var original = CreateLicense();
        var invalidRange = CreateRange(
            Start,
            Start.AddYears(DriversLicense.MaximumValidityYears).AddDays(1));

        var result = original.WithValidDateRange(invalidRange, Start);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.DateRangeTooLongMessage);
        original.ValidDateRange.ShouldBe(CreateRange(Start, Start.AddYears(4)));
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    public void CompareCalendarDates_On_IsExpired_WhenCurrentDateDiffersFromEndDate(
        int daysFromEnd,
        bool expected)
    {
        var license = CreateLicense();
        var today = license.ValidDateRange.End.AddDays(-daysFromEnd);

        license.IsExpired(today).ShouldBe(expected);
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        DriversLicense.MinimumValidDate.ShouldBe(new DateOnly(1900, 1, 1));
        DriversLicense.MaximumValidityYears.ShouldBe(50);
        DriversLicense.RequiredMessage.ShouldBe("Driver's license details are required.");
        DriversLicense.StateInvalidMessage.ShouldBe("Please enter a valid state.");
        DriversLicense.DateOrderInvalidMessage.ShouldBe(
            "The driver's license start date must be before its end date.");
        DriversLicense.StartDateTooEarlyMessage.ShouldBe(
            $"The driver's license start date cannot be earlier than {DriversLicense.MinimumValidDate:d}.");
        DriversLicense.StartDateInFutureMessage.ShouldBe(
            "The driver's license start date cannot be in the future.");
        DriversLicense.DateRangeTooLongMessage.ShouldBe(
            "The driver's license validity period cannot exceed 50 years.");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenComponentsAreEqual()
    {
        var range = CreateRange(Start, Start.AddYears(4));
        var first = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start).Value;
        var second = DriversLicense.Create(CreateNumber("A123456"), State.CA, range, Start).Value;
        var differentNumber = DriversLicense.Create(CreateNumber("B987654"), State.CA, range, Start).Value;
        var differentState = DriversLicense.Create(CreateNumber("A123456"), State.NY, range, Start).Value;
        var differentRange = DriversLicense.Create(
            CreateNumber("A123456"),
            State.CA,
            CreateRange(Start, Start.AddYears(5)), Start).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        (first == second).ShouldBeTrue();
        first.ShouldNotBe(differentNumber);
        first.ShouldNotBe(differentState);
        first.ShouldNotBe(differentRange);
        (first != differentNumber).ShouldBeTrue();
    }

    private static DriversLicense CreateLicense() => DriversLicense.Create(
        CreateNumber("A123456"),
        State.CA,
        CreateRange(Start, Start.AddYears(4)), Start).Value;

    private static DriversLicenseNumber CreateNumber(string number) =>
        DriversLicenseNumber.Create(number).Value;

    private static DateTimeRange CreateRange(DateOnly start, DateOnly end) =>
        DateTimeRange.Create(start, end).Value;
}
