using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class CurrencyCodeShould
{
    [Theory]
    [InlineData("USD")]
    [InlineData("CAD")]
    [InlineData("EUR")]
    [InlineData("JPY")]
    [InlineData("XAU")]
    public void ReturnSuccessfulResult_On_Create_WhenCodeIsActive(string code)
    {
        var result = CurrencyCode.Create(code);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(code);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("cAd", "CAD")]
    [InlineData("  EUR  ", "EUR")]
    [InlineData("\tjpY\r\n", "JPY")]
    public void TrimAndUppercaseCode_On_Create_WhenCodeRequiresNormalization(
        string code,
        string expected)
    {
        var result = CurrencyCode.Create(code);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(expected);
    }

    [Fact]
    public void NormalizeCodeInvariantly_On_Create_WhenCurrentCultureHasSpecialCasingRules()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");

            CurrencyCode.Create("try").Value.Value.ShouldBe("TRY");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData(" U S D ")]
    [InlineData("US1")]
    [InlineData("U$D")]
    [InlineData("US-")]
    [InlineData("éUR")]
    [InlineData("ＵＳＤ")]
    public void ReturnInvalidFailure_On_Create_WhenCodeIsNotThreeAsciiLetters(string? code)
    {
        var result = CurrencyCode.Create(code);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CurrencyCode.InvalidMessage);
    }

    [Theory]
    [InlineData("ZZZ")]
    [InlineData("ABC")]
    [InlineData(" zzz ")]
    public void ReturnUnsupportedFailure_On_Create_WhenNormalizedCodeIsNotActive(string code)
    {
        var result = CurrencyCode.Create(code);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CurrencyCode.UnsupportedMessage);
    }

    [Fact]
    public void ContainUsd_On_Usd()
    {
        CurrencyCode.Usd.Value.ShouldBe("USD");
    }

    [Fact]
    public void ContainUsd_On_Default()
    {
        CurrencyCode.Default.Value.ShouldBe("USD");
    }

    [Fact]
    public void ContainUsd_WhenDefaultInitialized()
    {
        default(CurrencyCode).Value.ShouldBe("USD");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenNormalizedCodesAreEqual()
    {
        var first = CurrencyCode.Create(" cad ").Value;
        var second = CurrencyCode.Create("CAD").Value;

        (first == second).ShouldBeTrue();
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void NotBeEqual_WhenCodesDiffer()
    {
        var cad = CurrencyCode.Create("CAD").Value;
        var eur = CurrencyCode.Create("EUR").Value;

        (cad != eur).ShouldBeTrue();
        cad.Equals(eur).ShouldBeFalse();
    }

    [Fact]
    public void BeEqualToDefault_WhenCodeIsUsd()
    {
        CurrencyCode.Create("USD").Value.ShouldBe(default(CurrencyCode));
        CurrencyCode.Usd.ShouldBe(CurrencyCode.Default);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("CAD")]
    [InlineData("EUR")]
    public void ReturnValue_On_ToString_WhenCodeIsValid(string code)
    {
        CurrencyCode.Create(code).Value.ToString().ShouldBe(code);
    }

    [Fact]
    public void ExposeDocumentedContractConstants()
    {
        CurrencyCode.CodeLength.ShouldBe(3);
        CurrencyCode.DefaultCode.ShouldBe("USD");
        CurrencyCode.RequiredMessage.ShouldBe("Currency code is required.");
        CurrencyCode.InvalidMessage.ShouldBe("Currency code must be three alphabetic characters.");
        CurrencyCode.UnsupportedMessage.ShouldBe("Currency code is not an active ISO 4217 code.");
    }
}
