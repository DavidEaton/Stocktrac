namespace Stocktrac.Domain.Features.Contacts;

public readonly record struct ContactPreferences
{
    public bool AllowMail { get; }
    public bool AllowEmail { get; }
    public bool AllowSms { get; }

    private ContactPreferences(bool allowMail, bool allowEmail, bool allowSms)
    {
        AllowMail = allowMail;
        AllowEmail = allowEmail;
        AllowSms = allowSms;
    }

    public static ContactPreferences Create(bool allowMail, bool allowEmail, bool allowSms) =>
        new(allowMail, allowEmail, allowSms);

    public ContactPreferences WithAllowMail(bool allowMail) =>
        new(allowMail, AllowEmail, AllowSms);

    public ContactPreferences WithAllowEmail(bool allowEmail) =>
        new(AllowMail, allowEmail, AllowSms);

    public ContactPreferences WithAllowSms(bool allowSms) =>
        new(AllowMail, AllowEmail, allowSms);
}
