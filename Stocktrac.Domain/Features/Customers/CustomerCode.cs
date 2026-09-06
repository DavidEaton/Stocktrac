using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Customers;

public sealed record CustomerCode
{
    public const int MaximumLength = 20;
    public static readonly string InvalidLengthMessage = $"Code must be {MaximumLength} characters or less.";
    public string Value { get; }

    private CustomerCode(string value) =>
        Value = value;

    public static Result<CustomerCode> Create(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(normalized.Length > MaximumLength, InvalidLengthMessage))
            .Map(() => new CustomerCode(normalized));
    }
}
