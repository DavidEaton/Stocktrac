namespace Stocktrac.Domain.Features.Contacts;

public record struct ContactPreferences
{
    public bool AllowMail { get; private set; }
    public bool AllowEmail { get; private set; }
    public bool AllowSms { get; private set; }

    private ContactPreferences(bool allowMail, bool allowEmail, bool allowSms)
    {
        AllowMail = allowMail;
        AllowEmail = allowEmail;
        AllowSms = allowSms;
    }

    public static ContactPreferences Create(bool allowMail, bool allowEmail, bool allowSms) =>
        new(allowMail, allowEmail, allowSms);

    public ContactPreferences WithAllowMail(bool allowMail) =>
        this with { AllowMail = allowMail };

    public ContactPreferences WithAllowEmail(bool allowEmail) =>
        this with { AllowEmail = allowEmail };

    public ContactPreferences WithAllowSms(bool allowSms) =>
        this with { AllowSms = allowSms };
}
