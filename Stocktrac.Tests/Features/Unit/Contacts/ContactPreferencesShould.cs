using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class ContactPreferencesShould
{
    [Fact]
    public void ReturnCopy_On_ReplaceAllowMail_WithoutChangingOtherPreferences()
    {
        var original = ContactPreferences.Create(false, true, true);

        var updated = original.ReplaceAllowMail(true);

        updated.AllowMail.ShouldBeTrue();
        updated.AllowEmail.ShouldBeTrue();
        updated.AllowSms.ShouldBeTrue();
        original.AllowMail.ShouldBeFalse();
    }

    [Fact]
    public void ReturnCopy_On_ReplaceAllowEmail_WithoutChangingOtherPreferences()
    {
        var original = ContactPreferences.Create(true, false, true);

        var updated = original.ReplaceAllowEmail(true);

        updated.AllowMail.ShouldBeTrue();
        updated.AllowEmail.ShouldBeTrue();
        updated.AllowSms.ShouldBeTrue();
        original.AllowEmail.ShouldBeFalse();
    }

    [Fact]
    public void ReturnCopy_On_ReplaceAllowSms_WithoutChangingOtherPreferences()
    {
        var original = ContactPreferences.Create(true, true, false);

        var updated = original.ReplaceAllowSms(true);

        updated.AllowMail.ShouldBeTrue();
        updated.AllowEmail.ShouldBeTrue();
        updated.AllowSms.ShouldBeTrue();
        original.AllowSms.ShouldBeFalse();
    }
}
