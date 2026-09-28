using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class EmailAddressShould
{
    [Fact]
    public void Create_WhenGivenAValidNonEmptyString()
    {
        var value = NonEmptyString.Create("john@doe.com").Value;

        var result = EmailAddress.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe("john@doe.com");
    }

    [Theory]
    [InlineData("a@b", "Email address cannot be less than 5 character(s) in length.")]
    [InlineData("invalid-email-address.com", "Email address and/or its format is invalid.")]
    public void ReturnSpecificError_WhenValueViolatesAnEmailInvariant(string input, string expectedError)
    {
        var value = NonEmptyString.Create(input).Value;

        var result = EmailAddress.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void ReturnMaximumLengthError_WhenValueExceedsMaximumLength()
    {
        var value = NonEmptyString.Create($"{new string('a', EmailAddress.MaximumLength - 5)}@x.com").Value;

        var result = EmailAddress.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailAddress.MaximumLengthMessage);
    }

    [Fact]
    public void EquateDistinctInstances_WhenValuesAreTheSame()
    {
        var value = NonEmptyString.Create("john@doe.com").Value;
        var first = EmailAddress.Create(value).Value;
        var second = EmailAddress.Create(value).Value;

        first.ShouldBe(second);
        first.ShouldNotBeSameAs(second);
    }
}
