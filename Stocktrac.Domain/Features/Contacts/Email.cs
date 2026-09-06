using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public class Email : Entity, IHasPrimary
{
    public static readonly string InvalidMessage = "Email address and/or its format is invalid";
    public const int MinimumLength = 5;
    public const int MaximumLength = 254;
    public static readonly string MinimumLengthMessage = $"Email address cannot be less than {MinimumLength} character(s) in length.";
    public static readonly string MaximumLengthMessage = $"Email address cannot be greater than {MaximumLength} characters in length.";
    public static readonly string EmptyMessage = "Email address cannot be empty.";
    public static readonly string DuplicateMessage = "Email address already in use. Please enter a unique email address.";

    public string Address { get; private set; }
    public bool IsPrimary { get; private set; } = true;

    private Email(string address, bool isPrimary) =>
        (Address, IsPrimary) = (address, isPrimary);

    public static Result<Email> Create(string address, bool isPrimary)
    {
        var normalized = address?.Trim() ?? string.Empty;
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(string.IsNullOrWhiteSpace(normalized), EmptyMessage),
                Result.FailureIf(normalized.Length > 0 && normalized.Length < MinimumLength, MinimumLengthMessage),
                Result.FailureIf(normalized.Length > MaximumLength, MaximumLengthMessage),
                Result.FailureIf(normalized.Length > 0 && !new EmailAddressAttribute().IsValid(normalized), InvalidMessage))
            .Map(() => new Email(normalized, isPrimary));
    }

    public Result<string> SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure<string>(EmptyMessage);

        address = (address ?? string.Empty).Trim();

        if (address.Length < MinimumLength)
            return Result.Failure<string>(MinimumLengthMessage);

        if (address.Length > MaximumLength)
            return Result.Failure<string>(MaximumLengthMessage);

        var emailAddressAttribute = new EmailAddressAttribute();

        if (!emailAddressAttribute.IsValid(address))
            return Result.Failure<string>(InvalidMessage);

        return Result.Success(Address = address);
    }

    public Result<bool> SetIsPrimary(bool isPrimary) =>
        Result.Success(IsPrimary = isPrimary);

    public override string ToString() =>
        Address;

    // EF requires a parameterless constructor
    protected Email() =>
        Address = string.Empty;
}
