using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public class ContactPreferences : ValueObject
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

    public static Result<ContactPreferences> Create(bool allowMail, bool allowEmail, bool allowSms) =>
        Result.Success(new ContactPreferences(allowMail, allowEmail, allowSms));

    public Result<ContactPreferences> NewAllowMail(bool allowMail) =>
        Result.Success(new ContactPreferences(allowMail, AllowEmail, AllowSms));

    public Result<ContactPreferences> NewAllowEmail(bool allowEmail) =>
        Result.Success(new ContactPreferences(AllowMail, allowEmail, AllowSms));

    public Result<ContactPreferences> NewAllowSms(bool allowSms) =>
        Result.Success(new ContactPreferences(AllowMail, AllowEmail, allowSms));

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return AllowMail;
        yield return AllowEmail;
        yield return AllowSms;
    }
}
