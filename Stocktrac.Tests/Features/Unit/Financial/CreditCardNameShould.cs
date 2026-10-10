using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class CreditCardNameShould
{
    [Theory]
    [InlineData(CreditCardName.MinimumLength, true)]
    [InlineData(CreditCardName.MaximumLength, true)]
    [InlineData(CreditCardName.MaximumLength + 1, false)]
    public void EnforceLengthRange_On_Create_WhenLengthIsAtBoundary(int length, bool valid)
    {
        var value = NonEmptyString.Create(new string('a', length)).Value;

        var result = CreditCardName.Create(value);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
            result.Value.Value.ShouldBeSameAs(value);
        else
            result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
    }

    [Fact]
    public void Create_WhenGivenANonEmptyString()
    {
        var value = NonEmptyString.Create("Visa").Value;

        var result = CreditCardName.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe("Visa");
    }

    [Fact]
    public void ReturnInvalidLengthFailure_WhenNameIsTooLong()
    {
        var value = NonEmptyString.Create(new string('V', CreditCardName.MaximumLength + 1)).Value;

        var result = CreditCardName.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var value = NonEmptyString.Create("Visa").Value;
        var first = CreditCardName.Create(value).Value;
        var second = CreditCardName.Create(value).Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }
}
