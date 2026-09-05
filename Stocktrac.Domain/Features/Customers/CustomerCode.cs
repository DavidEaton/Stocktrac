using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Customers;

public readonly record struct CustomerCode
{
    public static readonly int MaximumLength = 20;
    public static readonly string InvalidLengthMessage = $"Code must be {MaximumLength} characters or less.";
    public string Value { get; }

    private CustomerCode(string value) =>
        Value = value;

    public static Result<CustomerCode> Create(string? value) =>
        Result.Success(value?.Trim() ?? string.Empty)
            .Ensure(
                code => code.Length <= MaximumLength,
                InvalidLengthMessage)
            .Map(code => new CustomerCode(code));
}
