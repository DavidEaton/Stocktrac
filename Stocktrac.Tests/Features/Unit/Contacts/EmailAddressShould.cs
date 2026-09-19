using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class EmailAddressShould
{
    [Fact]
    public void ReturnEmailAddress_On_Create_WhenValueIsValid()
    {
        var result = EmailAddress.Create("john@doe.com");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("john@doe.com");
        result.Value.ToString().ShouldBe("john@doe.com");
    }

    [Fact]
    public void TrimValue_On_Create_WhenValueContainsSurroundingWhitespace()
    {
        var result = EmailAddress.Create("  john@doe.com  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("john@doe.com");
    }

    [Theory]
    [InlineData("a@b", "Email address cannot be less than 5 character(s) in length.")]
    [InlineData("invalid-email-address.com", "Email address and/or its format is invalid.")]
    [InlineData("", "Email address cannot be empty.")]
    public void ReturnSpecificError_On_Create_WhenValueIsInvalid(string value, string expectedError)
    {
        var result = EmailAddress.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void ReturnFailureResult_On_Create_WhenValueIsNull()
    {
        var result = EmailAddress.Create(null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailAddress.EmptyMessage);
    }

    [Fact]
    public void ReturnEmptyError_On_Create_WhenValueContainsOnlyWhitespace()
    {
        var result = EmailAddress.Create("   ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailAddress.EmptyMessage);
    }

    [Fact]
    public void AcceptMaximumLength_On_Create_WhenValueIsValid()
    {
        var value = $"{new string('a', EmailAddress.MaximumLength - 6)}@x.com";

        var result = EmailAddress.Create(value);

        value.Length.ShouldBe(EmailAddress.MaximumLength);
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact]
    public void ReturnMaximumLengthError_On_Create_WhenValueExceedsMaximumLength()
    {
        var value = $"{new string('a', EmailAddress.MaximumLength - 5)}@x.com";

        var result = EmailAddress.Create(value);

        value.Length.ShouldBe(EmailAddress.MaximumLength + 1);
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailAddress.MaximumLengthMessage);
    }

    [Fact]
    public void EquateDistinctInstances_WhenValuesAreTheSame()
    {
        var first = EmailAddress.Create("john@doe.com").Value;
        var second = EmailAddress.Create("john@doe.com").Value;

        first.ShouldBe(second);
        first.ShouldNotBeSameAs(second);
    }
}
