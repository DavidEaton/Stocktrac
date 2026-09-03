using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class StateShould
{
    [Fact]
    public void ContainEverySupportedUnitedStatesAndCanadianJurisdiction()
    {
        var values = Enum.GetValues<State>();

        values.Length.ShouldBe(64);
        values.ShouldContain(State.AL);
        values.ShouldContain(State.DC);
        values.ShouldContain(State.WY);
        values.ShouldContain(State.AB);
        values.ShouldContain(State.NU);
        values.ShouldContain(State.YT);
        values.Distinct().Count().ShouldBe(values.Length);
    }

    [Fact]
    public void ProvideNonblankDisplayName_ForEveryDefinedValue()
    {
        foreach (var state in Enum.GetValues<State>())
        {
            var member = typeof(State).GetMember(state.ToString()).Single();
            var display = member.GetCustomAttribute<DisplayAttribute>();

            display.ShouldNotBeNull($"{state} should have display metadata");
            display.Name.ShouldNotBeNullOrWhiteSpace($"{state} should have a display name");
        }
    }

    [Theory]
    [InlineData(State.AL, "Alabama")]
    [InlineData(State.DC, "District of Columbia")]
    [InlineData(State.NL, "Newfoundland and Labrador")]
    [InlineData(State.PE, "Prince Edward Island")]
    public void ExposeExpectedDisplayName_On_DisplayAttribute_ForRepresentativeValues(State state, string name)
    {
        var member = typeof(State).GetMember(state.ToString()).Single();

        member.GetCustomAttribute<DisplayAttribute>()!.Name.ShouldBe(name);
    }
}