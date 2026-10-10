using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class PostalCodeShould
{
    [Fact]
    public void PreserveValue_On_Create_WhenLengthEqualsMaximum()
    {
        var value = NonEmptyString.Create(new string('a', PostalCode.MaximumLength)).Value;

        var result = PostalCode.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBeSameAs(value);
    }

    [Fact]
    public void Create_WhenGivenANonEmptyValue()
    {
        var value = NonEmptyString.Create("A1B 2C3").Value;

        var result = PostalCode.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe("A1B 2C3");
    }

    [Fact]
    public void ReturnLengthError_On_Create_WhenValueExceedsMaximum()
    {
        var result = PostalCode.Create(
            NonEmptyString.Create(new string('1', PostalCode.MaximumLength + 1)).Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostalCode.InvalidLengthMessage);
    }

    [Fact]
    public void PreserveLeadingZerosAndNonNumericCharacters_On_Create()
    {
        PostalCode.Create(NonEmptyString.Create("00123-A").Value).Value.Value.Value
            .ShouldBe("00123-A");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = PostalCode.Create(NonEmptyString.Create("12345").Value).Value;
        var second = PostalCode.Create(NonEmptyString.Create("12345").Value).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void ExposeValidationContractValues()
    {
        PostalCode.MaximumLength.ShouldBe(20);
        PostalCode.InvalidLengthMessage.ShouldBe("Postal Code must not exceed 20 characters.");
    }
}
