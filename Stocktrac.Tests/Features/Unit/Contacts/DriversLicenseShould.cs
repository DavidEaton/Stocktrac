using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class DriversLicenseShould
{
    private static readonly DateTime Start = new(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void PreserveComponents_On_Create_WhenStateIsDefined()
    {
        var number = CreateNumber("A123456");
        var range = CreateRange(Start, Start.AddYears(4));

        var result = DriversLicense.Create(number, State.CA, range);

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
            CreateRange(Start, Start.AddYears(4)));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.StateInvalidMessage);
    }

    [Fact]
    public void ReplaceNumberAndPreserveOtherComponents_On_NewNumber_WhenNumberIsProvided()
    {
        var original = CreateLicense();
        var replacement = CreateNumber("B987654");

        var result = original.NewNumber(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(replacement);
        result.Value.State.ShouldBe(original.State);
        result.Value.ValidDateRange.ShouldBeEquivalentTo(original.ValidDateRange);
        original.Number.ShouldNotBe(replacement);
    }

    [Fact]
    public void ReplaceStateAndPreserveOtherComponents_On_NewState_WhenStateIsDefined()
    {
        var original = CreateLicense();

        var result = original.NewState(State.NY);

        result.IsSuccess.ShouldBeTrue();
        result.Value.State.ShouldBe(State.NY);
        result.Value.Number.ShouldBe(original.Number);
        result.Value.ValidDateRange.ShouldBeEquivalentTo(original.ValidDateRange);
        original.State.ShouldBe(State.CA);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void ReturnStateInvalidError_On_NewState_WhenStateIsUndefined(int stateValue)
    {
        var result = CreateLicense().NewState((State)stateValue);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicense.StateInvalidMessage);
    }

    [Fact]
    public void ReplaceValidDateRangeAndPreserveOtherComponents_On_NewValidDateRange_WhenRangeIsProvided()
    {
        var original = CreateLicense();
        var replacement = CreateRange(Start.AddDays(1), Start.AddYears(5));

        var result = original.NewValidDateRange(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ValidDateRange.ShouldBeEquivalentTo(replacement);
        result.Value.Number.ShouldBe(original.Number);
        result.Value.State.ShouldBe(original.State);
        original.ValidDateRange.ShouldNotBeSameAs(replacement);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenComponentsAreEqual()
    {
        var range = CreateRange(Start, Start.AddYears(4));
        var first = DriversLicense.Create(CreateNumber("A123456"), State.CA, range).Value;
        var second = DriversLicense.Create(CreateNumber("A123456"), State.CA, range).Value;
        var differentNumber = DriversLicense.Create(CreateNumber("B987654"), State.CA, range).Value;
        var differentState = DriversLicense.Create(CreateNumber("A123456"), State.NY, range).Value;
        var differentRange = DriversLicense.Create(
            CreateNumber("A123456"),
            State.CA,
            CreateRange(Start, Start.AddYears(5))).Value;

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
        CreateRange(Start, Start.AddYears(4))).Value;

    private static DriversLicenseNumber CreateNumber(string number) =>
        DriversLicenseNumber.Create(number).Value;

    private static DateTimeRange CreateRange(DateTime start, DateTime end) =>
        DateTimeRange.Create(start, end).Value;
}