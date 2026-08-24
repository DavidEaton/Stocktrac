using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class Iso4217CurrencyCodesShould
{
    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("XAU")]
    public void Contain_Codes_From_The_Embedded_Iso_4217_List(string code) =>
        Iso4217CountryCurrencyCodes.Contains(code).ShouldBeTrue();

    [Theory]
    [InlineData("ZZZ")]
    [InlineData("usd")]
    [InlineData("")]
    public void Not_Contain_Unsupported_Or_Unnormalized_Codes(string code) =>
        Iso4217CountryCurrencyCodes.Contains(code).ShouldBeFalse();
}