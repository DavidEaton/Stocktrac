using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class CreditCardNameShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void ReturnRequiredFailure_On_Create_WhenNameIsMissing(string? name)
    {
        var result = CreditCardName.Create(name);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.RequiredMessage);
    }

    [Fact]
    public void ReturnInvalidLengthFailure_On_Create_WhenTrimmedNameIsTooLong()
    {
        var result = CreditCardName.Create(
            $"  {new string('V', CreditCardName.MaximumLength + 1)}  ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
    }

    [Theory]
    [InlineData(CreditCardName.MinimumLength)]
    [InlineData(CreditCardName.MaximumLength)]
    public void ReturnSuccessfulResult_On_Create_WhenNameIsAtLengthBoundary(int length)
    {
        var name = new string('V', length);

        var result = CreditCardName.Create(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(name);
    }

    [Theory]
    [InlineData("  Visa  ", "Visa")]
    [InlineData("\tMastercard\r\n", "Mastercard")]
    [InlineData("\u2003Amex\u2003", "Amex")]
    public void TrimName_On_Create_WhenNameHasSurroundingWhitespace(
        string name,
        string expected)
    {
        var result = CreditCardName.Create(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("VISA")]
    [InlineData("visa")]
    [InlineData("Visa Debit")]
    [InlineData("Visa-123")]
    [InlineData("信用卡")]
    public void PreserveName_On_Create_WhenNameIsValid(string name)
    {
        var result = CreditCardName.Create(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(name);
    }

    [Fact]
    public void PreserveInternalWhitespace_On_Create_WhenNameContainsInternalWhitespace()
    {
        const string name = "Visa   Debit\tCard";

        CreditCardName.Create(name).Value.Value.ShouldBe(name);
    }

    [Fact]
    public void ContainAssignedValue_On_Value_WhenInitializedDirectly()
    {
        var name = new CreditCardName { Value = "Direct value" };

        name.Value.ShouldBe("Direct value");
    }

    [Fact]
    public void ContainNullValue_WhenDefaultInitialized()
    {
        default(CreditCardName).Value.ShouldBeNull();
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = CreditCardName.Create("  Visa  ").Value;
        var second = CreditCardName.Create("Visa").Value;

        (first == second).ShouldBeTrue();
        first.Equals(second).ShouldBeTrue();
        first.Equals((object)second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void NotBeEqual_WhenValuesDifferByCase()
    {
        var upperCase = CreditCardName.Create("VISA").Value;
        var titleCase = CreditCardName.Create("Visa").Value;

        (upperCase != titleCase).ShouldBeTrue();
        upperCase.Equals(titleCase).ShouldBeFalse();
    }

    [Fact]
    public void CreateCopyWithUpdatedValue_On_WithExpression_WhenValueIsReassigned()
    {
        var original = CreditCardName.Create("Visa").Value;

        var copy = original with { Value = "Mastercard" };

        copy.Value.ShouldBe("Mastercard");
        original.Value.ShouldBe("Visa");
    }

    [Fact]
    public void ReturnRecordRepresentation_On_ToString_WhenValueIsAssigned()
    {
        var name = CreditCardName.Create("Visa").Value;

        name.ToString().ShouldBe("CreditCardName { Value = Visa }");
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        CreditCardName.MinimumLength.ShouldBe(1);
        CreditCardName.MaximumLength.ShouldBe(255);
        CreditCardName.RequiredMessage.ShouldBe("A valid value is required.");
        CreditCardName.InvalidLengthMessage.ShouldBe(
            "Value must be between 1 and 255 characters.");
    }
}
