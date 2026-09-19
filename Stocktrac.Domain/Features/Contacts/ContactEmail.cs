using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record ContactEmail : IHasPrimary
{
    public const string DuplicateMessage = "Email address already in use. Please enter a unique email address.";

    public EmailAddress Address { get; }
    public bool IsPrimary { get; }

    private ContactEmail(EmailAddress address, bool isPrimary) =>
        (Address, IsPrimary) = (address, isPrimary);

    public static Result<ContactEmail> Create(string address, bool isPrimary) =>
        EmailAddress.Create(address)
            .Map(validAddress => new ContactEmail(validAddress, isPrimary));

    public Result<ContactEmail> WithAddress(string address) =>
        EmailAddress.Create(address)
            .Map(validAddress => new ContactEmail(validAddress, IsPrimary));

    public ContactEmail WithIsPrimary(bool isPrimary) => new(Address, isPrimary);

    public override string ToString() => Address.ToString();
}
