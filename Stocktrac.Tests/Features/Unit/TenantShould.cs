using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;

namespace Stocktrac.Tests.Features.Unit;

public class TenantShould
{
    [Fact]
    public void ReturnFirstError_On_Create_WhenSeveralInputsAreInvalid()
    {
        var result = Tenant.Create("   ", "", "   ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Tenant.NameRequiredMessage);
    }

    [Fact]
    public void NormalizeValues_On_Create_WhenInputsAreValid()
    {
        var result = Tenant.Create(" Tenant ", " Company ", " https://example.test/logo.png ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Tenant");
        result.Value.CompanyName.ShouldBe("Company");
        result.Value.LogoUrl.Value.ShouldBe("https://example.test/logo.png");
    }

    [Fact]
    public void ReturnFailureAndPreserveAbsence_On_UpdateLogoUrl_WhenValueIsMissing()
    {
        var tenant = CreateTenant();

        var result = tenant.UpdateLogoUrl("   ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Tenant.LogoUrlRequiredMessage);
        tenant.LogoUrl.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void StoreTrimmedValue_On_UpdateLogoUrl_WhenValueIsPresent()
    {
        var tenant = CreateTenant();

        var result = tenant.UpdateLogoUrl("  https://example.test/logo.png  ");

        result.IsSuccess.ShouldBeTrue();
        tenant.LogoUrl.Value.ShouldBe("https://example.test/logo.png");
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenPresentLogoUrlIsMissing()
    {
        var result = Tenant.Create("Tenant", "Company", Maybe<string>.From("   "));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain(Tenant.LogoUrlRequiredMessage);
    }

    private static Tenant CreateTenant() =>
        Tenant.Create("Tenant", "Company").Value;
}
