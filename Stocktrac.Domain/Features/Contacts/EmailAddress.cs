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
    public NonEmptyString Value { get; }

    private EmailAddress(NonEmptyString value) => Value = value;

    public static Result<EmailAddress> Create(NonEmptyString value) =>
        Result.Success(value)
            .Ensure(input => input.Value.Length >= MinimumLength, MinimumLengthMessage)
            .Ensure(input => input.Value.Length <= MaximumLength, MaximumLengthMessage)
            .Ensure(input => new EmailAddressAttribute().IsValid(input.Value), InvalidMessage)
            .Map(input => new EmailAddress(input));

    public override string ToString() => Value.ToString();
}
