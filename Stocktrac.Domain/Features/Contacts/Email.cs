using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record Email : IHasPrimary
{
    public const int MinimumLength = 5;
    public const int MaximumLength = 254;
    public const string InvalidMessage = "Email address and/or its format is invalid.";
    public static readonly string MinimumLengthMessage = $"Email address cannot be less than {MinimumLength} character(s) in length.";
    public static readonly string MaximumLengthMessage = $"Email address cannot be greater than {MaximumLength} characters in length.";
    public const string EmptyMessage = "Email address cannot be empty.";
    public const string DuplicateMessage = "Email address already in use. Please enter a unique email address.";

    public string Address { get; private set; }
    public bool IsPrimary { get; private set; }

    private Email(string address, bool isPrimary) =>
        (Address, IsPrimary) = (address, isPrimary);

    public static Result<Email> Create(string address, bool isPrimary) =>
        address.AsValidEmailAddress()
            .Map(validAddress => new Email(validAddress, isPrimary));

    public Result<Email> WithAddress(string address) =>
        address.AsValidEmailAddress()
            .Map(validAddress => this with { Address = validAddress });

    public Email WithIsPrimary(bool isPrimary) => this with { IsPrimary = isPrimary };

    public override string ToString() =>
        Address;
}
