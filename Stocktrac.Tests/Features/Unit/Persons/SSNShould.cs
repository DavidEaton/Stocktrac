using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Persons;

namespace Stocktrac.Tests.Features.Unit.Persons;

public class SSNShould
{
    [Theory]
    [InlineData("123456789")]
    [InlineData("123-45-6789")]
    public void NormalizeAndMaskValue_On_Create_WhenFormatIsValid(string input)
    {
        var result = SSN.Create(NonEmptyString.Create(input).Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.Value.ShouldBe("123456789");
        result.Value.ToFormattedString().ShouldBe("123-45-6789");
        result.Value.ToString().ShouldBe("***-**-6789");
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    [InlineData("123-456-789")]
    [InlineData("12345678a")]
    [InlineData("１２３４５６７８９")]
    public void ReturnFormatError_On_Create_WhenFormatIsInvalid(string input)
    {
        var result = SSN.Create(NonEmptyString.Create(input).Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SSN.InvalidFormatMessage);
    }
}
