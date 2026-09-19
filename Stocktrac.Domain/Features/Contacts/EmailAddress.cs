using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record EmailAddress
{
    public const int MinimumLength = 5;
    public const int MaximumLength = 254;
    public const string InvalidMessage = "Email address and/or its format is invalid.";
    public static readonly string MinimumLengthMessage = $"Email address cannot be less than {MinimumLength} character(s) in length.";
    public static readonly string MaximumLengthMessage = $"Email address cannot be greater than {MaximumLength} characters in length.";
    public const string EmptyMessage = "Email address cannot be empty.";

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static Result<EmailAddress> Create(string? value) =>
        Result.Success(value)
            .Map(input => input?.Trim() ?? string.Empty)
            .Ensure(normalized => !string.IsNullOrWhiteSpace(normalized), EmptyMessage)
            .Ensure(normalized => normalized.Length >= MinimumLength, MinimumLengthMessage)
            .Ensure(normalized => normalized.Length <= MaximumLength, MaximumLengthMessage)
            .Ensure(normalized => new EmailAddressAttribute().IsValid(normalized), InvalidMessage)
            .Map(normalized => new EmailAddress(normalized));

    public override string ToString() => Value;
}
