using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class AddressLineShould
{
    [Fact]
    public void Create_WhenGivenANonEmptyValue()
    {
        var value = NonEmptyString.Create("12-B Main Street").Value;

        var result = AddressLine.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe("12-B Main Street");
    }

    [Fact]
    public void ReturnLengthError_On_Create_WhenValueExceedsMaximum()
    {
        var value = NonEmptyString.Create(new string('a', AddressLine.MaximumLength + 1)).Value;

        var result = AddressLine.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AddressLine.InvalidLengthMessage);
    }

    [Fact]
    public void PreserveTheSuppliedValue_On_Create_WhenValueIsAtMaximumLength()
    {
        var value = NonEmptyString.Create(new string('a', AddressLine.MaximumLength)).Value;

        AddressLine.Create(value).Value.Value.ShouldBe(value);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = AddressLine.Create(NonEmptyString.Create("Main").Value).Value;
        var second = AddressLine.Create(NonEmptyString.Create("Main").Value).Value;
        var different = AddressLine.Create(NonEmptyString.Create("main").Value).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(different);
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        AddressLine.MaximumLength.ShouldBe(255);
        AddressLine.InvalidLengthMessage.ShouldBe("Address must not exceed 255 characters.");
    }
}
