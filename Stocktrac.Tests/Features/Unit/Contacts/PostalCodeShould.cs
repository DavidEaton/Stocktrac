using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class PostalCodeShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901")]
    [InlineData("12 34")]
    [InlineData("1234A")]
    [InlineData("12-34")]
    public void ReturnInvalidError_On_Create_WhenValueIsNotOneToTwentyDigits(string? value)
    {
        var result = PostalCode.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostalCode.InvalidMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void PreserveTrimmedValue_On_Create_WhenValueIsAtLengthBoundary(int length)
    {
        var value = new string('1', length);

        var result = PostalCode.Create($"  {value}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value);
    }

    [Fact]
    public void PreserveLeadingZeros_On_Create_WhenValueIsValid()
    {
        PostalCode.Create(" 00123 ").Value.Value.ShouldBe("00123");
    }

    [Theory]
    [InlineData("１２３４５")]
    [InlineData("١٢٣٤٥")]
    public void AcceptUnicodeDigits_On_Create_WhenEveryCharacterIsADigit(string value)
    {
        PostalCode.Create(value).Value.Value.ShouldBe(value);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = PostalCode.Create(" 12345 ").Value;
        var second = PostalCode.Create("12345").Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(PostalCode.Create("12346").Value);
    }

    [Fact]
    public void ExposeValidationContractValues()
    {
        PostalCode.MinimumLength.ShouldBe(1);
        PostalCode.MaximumLength.ShouldBe(20);
        PostalCode.InvalidMessage.ShouldBe("Value must be between 1 and 20 characters.");
    }
}
