using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class CityShould
{
    [Fact]
    public void Create_WhenGivenANonEmptyValue()
    {
        var value = NonEmptyString.Create("New York").Value;

        var result = City.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe("New York");
    }

    [Fact]
    public void ReturnLengthError_On_Create_WhenValueExceedsMaximum()
    {
        var result = City.Create(NonEmptyString.Create(new string('a', City.MaximumLength + 1)).Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(City.InvalidLengthMessage);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = City.Create(NonEmptyString.Create("Albany").Value).Value;
        var second = City.Create(NonEmptyString.Create("Albany").Value).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        City.MaximumLength.ShouldBe(100);
        City.InvalidLengthMessage.ShouldBe("City must not exceed 100 characters.");
    }
}
