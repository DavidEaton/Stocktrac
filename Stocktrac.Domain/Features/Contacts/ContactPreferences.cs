using CSharpFunctionalExtensions;

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

    public static Result<ContactPreferences> Create(bool allowMail, bool allowEmail, bool allowSms) =>
        Result.Success(new ContactPreferences(allowMail, allowEmail, allowSms));

    public Result<ContactPreferences> NewAllowMail(bool allowMail) =>
        Result.Success(new ContactPreferences(allowMail, AllowEmail, AllowSms));

    public Result<ContactPreferences> NewAllowEmail(bool allowEmail) =>
        Result.Success(new ContactPreferences(AllowMail, allowEmail, AllowSms));

    public Result<ContactPreferences> NewAllowSms(bool allowSms) =>
        Result.Success(new ContactPreferences(AllowMail, AllowEmail, allowSms));
}
