using Shouldly;
using Stocktrac.Domain.Features.SaleCodes;

namespace Stocktrac.Tests.Features.Unit.SaleCodes;

public class SaleCodeShopSuppliesShould
{
    [Theory]
    [InlineData("Percentage")]
    [InlineData("MinimumJobAmount")]
    [InlineData("MinimumCharge")]
    [InlineData("MaximumCharge")]
    public void ReturnMinimumValueError_On_Create_WhenAmountIsNegativeOrNonFinite(string component)
    {
        foreach (var invalid in new[] { -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            var result = SaleCodeShopSupplies.Create(
                component == "Percentage" ? invalid : 0,
                component == "MinimumJobAmount" ? invalid : 0,
                component == "MinimumCharge" ? invalid : 0,
                component == "MaximumCharge" ? invalid : 0,
                true, false);

            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(SaleCodeShopSupplies.MinimumValueMessage);
        }
    }

    [Fact]
    public void PreserveComponents_On_Create_WhenAmountsAreValid()
    {
        var result = SaleCodeShopSupplies.Create(0, 10, 2, 20, true, false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Percentage.ShouldBe(0);
        result.Value.MinimumJobAmount.ShouldBe(10);
        result.Value.MinimumCharge.ShouldBe(2);
        result.Value.MaximumCharge.ShouldBe(20);
        result.Value.IncludeParts.ShouldBeTrue();
        result.Value.IncludeLabor.ShouldBeFalse();
    }

    [Fact]
    public void ReturnOneError_On_Create_WhenAllAmountsAreInvalid()
    {
        var result = SaleCodeShopSupplies.Create(-1, -1, -1, -1, false, false);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SaleCodeShopSupplies.MinimumValueMessage);
    }
}
