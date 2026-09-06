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

    public static Result<PostalCode> Create(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(normalized.Length < MinimumLength, InvalidMessage),
                Result.FailureIf(normalized.Length > MaximumLength, InvalidMessage),
                Result.FailureIf(!normalized.All(char.IsDigit), InvalidMessage))
            .Map(() => new PostalCode(normalized));
    }

    public override string ToString() =>
        Value;
}
