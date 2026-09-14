using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class ContactPreferencesShould
{
    [Fact]
    public void ReturnCopy_On_WithAllowMail_WithoutChangingOtherPreferences()
    {
        var original = ContactPreferences.Create(false, true, true);

        var updated = original.WithAllowMail(true);

        updated.AllowMail.ShouldBeTrue();
        updated.AllowEmail.ShouldBeTrue();
        updated.AllowSms.ShouldBeTrue();
        original.AllowMail.ShouldBeFalse();
    }

    [Fact]
    public void ReturnCopy_On_WithAllowEmail_WithoutChangingOtherPreferences()
    {
        var original = ContactPreferences.Create(true, false, true);

        var updated = original.WithAllowEmail(true);

        updated.AllowMail.ShouldBeTrue();
        updated.AllowEmail.ShouldBeTrue();
        updated.AllowSms.ShouldBeTrue();
        original.AllowEmail.ShouldBeFalse();
    }

    [Fact]
    public void ReturnCopy_On_WithAllowSms_WithoutChangingOtherPreferences()
    {
        var original = ContactPreferences.Create(true, true, false);

        var updated = original.WithAllowSms(true);

        updated.AllowMail.ShouldBeTrue();
        updated.AllowEmail.ShouldBeTrue();
        updated.AllowSms.ShouldBeTrue();
        original.AllowSms.ShouldBeFalse();
    }
}
