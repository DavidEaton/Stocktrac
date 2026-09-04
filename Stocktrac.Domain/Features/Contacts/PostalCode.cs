using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public readonly record struct PostalCode
{
    public static readonly int MinimumLength = 1;
    public static readonly int MaximumLength = 20;
    public static readonly string InvalidMessage = $"Value must be between {MinimumLength} and {MaximumLength} characters.";
    public string Value { get; }

    private PostalCode(string value) =>
        Value = value;

    public static Result<PostalCode> Create(string? value) =>
        Result.Success(value?.Trim() ?? string.Empty)
            .Ensure(code => code.Length >= MinimumLength, InvalidMessage)
            .Ensure(code => code.Length <= MaximumLength, InvalidMessage)
            .Ensure(code => code.All(char.IsDigit), InvalidMessage)
            .Map(code => new PostalCode(code));

    public override string ToString() =>
        Value;
}
