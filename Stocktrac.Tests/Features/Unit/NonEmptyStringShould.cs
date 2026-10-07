using Shouldly;
using Stocktrac.Domain.Features;

namespace Stocktrac.Tests.Features.Unit;

public class NonEmptyStringShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnFailure_On_Create_WhenValueIsMissing(string? value)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        var result = NonEmptyString.Create(value);
#pragma warning restore CS8604 // Possible null reference argument.

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(NonEmptyString.RequiredMessage);
    }

    [Fact]
    public void ReturnTrimmedValue_On_Create_WhenValueHasSurroundingWhitespace()
    {
        var result = NonEmptyString.Create("  meaningful text  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("meaningful text");
    }

    [Fact]
    public void PreserveValue_On_Create_WhenValueIsAlreadyValid()
    {
        const string value = "Already valid text";

        var result = NonEmptyString.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact]
    public void ExposeNoPublicConstructors()
    {
        typeof(NonEmptyString).GetConstructors().ShouldBeEmpty();
    }

    [Fact]
    public void ReturnValue_On_ToString()
    {
        var value = NonEmptyString.Create("meaningful text").Value;

        value.ToString().ShouldBe(value.Value);
    }
}
