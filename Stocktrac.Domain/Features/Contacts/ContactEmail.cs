using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record ContactEmail : IHasPrimary
{
    public const string DuplicateMessage = "Email address already in use. Please enter a unique email address.";

    public EmailAddress Address { get; }
    public bool IsPrimary { get; }

    private ContactEmail(EmailAddress address, bool isPrimary) =>
        (Address, IsPrimary) = (address, isPrimary);

    public static Result<ContactEmail> Create(EmailAddress address, bool isPrimary) =>
        Result.Success(new ContactEmail(address, isPrimary));

    public Result<ContactEmail> ReplaceAddress(EmailAddress address) =>
        Result.Success(new ContactEmail(address, IsPrimary));

    public Result<ContactEmail> ReplaceIsPrimary(bool isPrimary) =>
        Result.Success(new ContactEmail(Address, isPrimary));

    public override string ToString() => Address.ToString();
}
