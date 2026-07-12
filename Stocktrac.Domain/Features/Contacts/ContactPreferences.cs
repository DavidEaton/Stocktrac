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

    public ContactPreferences NewAllowMail(bool allowMail) =>
        new(allowMail, AllowEmail, AllowSms);

    public ContactPreferences NewAllowEmail(bool allowEmail) =>
        new(AllowMail, allowEmail, AllowSms);

    public ContactPreferences NewAllowSms(bool allowSms) =>
        new(AllowMail, AllowEmail, allowSms);

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return AllowEmail;
        yield return AllowEmail;
        yield return AllowSms;
    }
}
