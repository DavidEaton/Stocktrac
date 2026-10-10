using Shouldly;
using Stocktrac.Domain.Features.SaleCodes;

namespace Stocktrac.Tests.Features.Unit.SaleCodes;

public class SaleCodeShould
{
    [Fact]
    public void NormalizeAndPreserveComponents_On_Create_WhenInputsAreValid()
    {
        var supplies = CreateSupplies();

        var result = SaleCode.Create(" Repair ", " ab ", 0, 100, supplies, []);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Repair");
        result.Value.Code.ShouldBe("AB");
        result.Value.LaborRate.ShouldBe(0);
        result.Value.DesiredMargin.ShouldBe(100);
        result.Value.ShopSupplies.ShouldBeSameAs(supplies);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(SaleCode.CodeMaximumLength, true)]
    [InlineData(SaleCode.CodeMaximumLength + 1, false)]
    public void EnforceCodeLength_On_Create_WhenLengthIsAtBoundary(int length, bool valid)
    {
        var result = SaleCode.Create("Repair", new string('a', length), 1, 50, CreateSupplies(), []);

        result.IsSuccess.ShouldBe(valid);
        if (!valid)
            result.Error.ShouldBe(SaleCode.InvalidLengthMessage(SaleCode.MinimumLength, SaleCode.CodeMaximumLength));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(SaleCode.NameMaximumLength, true)]
    [InlineData(SaleCode.NameMaximumLength + 1, false)]
    public void EnforceNameLength_On_Create_WhenLengthIsAtBoundary(int length, bool valid)
    {
        var result = SaleCode.Create(new string('a', length), "AB", 1, 50, CreateSupplies(), []);

        result.IsSuccess.ShouldBe(valid);
        if (!valid)
            result.Error.ShouldBe(SaleCode.InvalidLengthMessage(SaleCode.MinimumLength, SaleCode.NameMaximumLength));
    }

    [Theory]
    [InlineData("LaborRate")]
    [InlineData("DesiredMargin")]
    public void ReturnValueError_On_Create_WhenAmountIsNegativeOrNonFinite(string component)
    {
        foreach (var invalid in new[] { -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            var result = SaleCode.Create("Repair", "AB",
                component == "LaborRate" ? invalid : 1,
                component == "DesiredMargin" ? invalid : 50, CreateSupplies(), []);

            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(component == "LaborRate"
                ? SaleCode.MinimumValueMessage
                : SaleCode.InvalidValueMessage(SaleCode.MinimumValue, SaleCode.MaximumDesiredMarginValue));
        }
    }

    [Fact]
    public void ReturnMarginError_On_Create_WhenDesiredMarginExceedsMaximum()
    {
        var result = SaleCode.Create("Repair", "AB", 1, 101, CreateSupplies(), []);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SaleCode.InvalidValueMessage(SaleCode.MinimumValue, SaleCode.MaximumDesiredMarginValue));
    }

    [Fact]
    public void ReturnDuplicateError_On_Create_WhenNormalizedCodeAlreadyExists()
    {
        var result = SaleCode.Create("Repair", " ab ", 1, 50, CreateSupplies(), ["aB"]);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SaleCode.NonuniqueMessage);
    }

    [Theory]
    [InlineData("Supplies")]
    [InlineData("Codes")]
    [InlineData("CodeMember")]
    public void ReturnRequiredError_On_Create_WhenRequiredInputIsNull(string component)
    {
        var result = SaleCode.Create("Repair", "AB", 1, 50,
            component == "Supplies" ? null! : CreateSupplies(),
            component == "Codes" ? null! : component == "CodeMember" ? [null!] : []);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SaleCode.RequiredMessage);
    }

    [Fact]
    public void ReturnFirstError_On_Create_WhenSeveralInputsAreInvalid()
    {
        var result = SaleCode.Create("", "", -1, double.NaN, null!, null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SaleCode.RequiredMessage);
    }

    private static SaleCodeShopSupplies CreateSupplies() =>
        SaleCodeShopSupplies.Create(0, 0, 0, 0, false, false).Value;
}
