using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record PostalCode
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 20;
    public static readonly string InvalidMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
    public string Value { get; }

    private PostalCode(string value) =>
        Value = value;

    public static Result<PostalCode> Create(string value) =>
        Result.Success(value)
            .Map(input => input?.Trim() ?? string.Empty)
            .Ensure(normalized => normalized.Length.IsWithin(MinimumLength, MaximumLength), InvalidMessage)
            .Ensure(normalized => normalized.All(char.IsDigit), InvalidMessage)
            .Map(normalized => new PostalCode(normalized));

    public static implicit operator string(PostalCode postalCode) => postalCode.Value;

    public static explicit operator PostalCode(string value) => new(value);

    public override string ToString() =>
        Value;
}
