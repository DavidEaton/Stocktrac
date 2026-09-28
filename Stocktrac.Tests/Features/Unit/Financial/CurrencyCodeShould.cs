using Stocktrac.Domain.Features;
using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

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
        var result = CurrencyCode.Create(NonEmptyString.Create(code).Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(code);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("cAd", "CAD")]
    public void UppercaseCode_On_Create_WhenCodeRequiresNormalization(
        string code,
        string expected)
    {
        var result = CurrencyCode.Create(NonEmptyString.Create(code).Value);

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

            CurrencyCode.Create(NonEmptyString.Create("try").Value).Value.Value.ShouldBe("TRY");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData(" U S D ")]
    [InlineData("US1")]
    [InlineData("U$D")]
    [InlineData("US-")]
    [InlineData("éUR")]
    [InlineData("ＵＳＤ")]
    public void ReturnInvalidFailure_On_Create_WhenCodeIsNotThreeAsciiLetters(string code)
    {
        var result = CurrencyCode.Create(NonEmptyString.Create(code).Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CurrencyCode.InvalidMessage);
    }

    [Theory]
    [InlineData("ZZZ")]
    [InlineData("ABC")]
    public void ReturnUnsupportedFailure_On_Create_WhenNormalizedCodeIsNotActive(string code)
    {
        var result = CurrencyCode.Create(NonEmptyString.Create(code).Value);

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
        var first = CurrencyCode.Create(NonEmptyString.Create("cad").Value).Value;
        var second = CurrencyCode.Create(NonEmptyString.Create("CAD").Value).Value;

        (first == second).ShouldBeTrue();
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void NotBeEqual_WhenCodesDiffer()
    {
        var cad = CurrencyCode.Create(NonEmptyString.Create("CAD").Value).Value;
        var eur = CurrencyCode.Create(NonEmptyString.Create("EUR").Value).Value;

        (cad != eur).ShouldBeTrue();
        cad.Equals(eur).ShouldBeFalse();
    }

    [Fact]
    public void BeEqualToDefault_WhenCodeIsUsd()
    {
        CurrencyCode.Create(NonEmptyString.Create("USD").Value).Value.ShouldBe(default(CurrencyCode));
        CurrencyCode.Usd.ShouldBe(CurrencyCode.Default);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("CAD")]
    [InlineData("EUR")]
    public void ReturnValue_On_ToString_WhenCodeIsValid(string code)
    {
        CurrencyCode.Create(NonEmptyString.Create(code).Value).Value.ToString().ShouldBe(code);
    }

    [Fact]
    public void ExposeDocumentedContractConstants()
    {
        CurrencyCode.CodeLength.ShouldBe(3);
        CurrencyCode.DefaultCode.ShouldBe("USD");
        CurrencyCode.InvalidMessage.ShouldBe("Currency code must be three alphabetic characters.");
        CurrencyCode.UnsupportedMessage.ShouldBe("Currency code is not an active ISO 4217 code.");
    }
}
