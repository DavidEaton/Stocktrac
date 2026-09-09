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

    public ContactPreferences NewAllowMail(bool allowMail) =>
        new(allowMail, AllowEmail, AllowSms);

    public ContactPreferences NewAllowEmail(bool allowEmail) =>
        new(AllowMail, allowEmail, AllowSms);

    public ContactPreferences NewAllowSms(bool allowSms) =>
        new(AllowMail, AllowEmail, allowSms);
}
