using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Customers;

public class CustomerCode : ValueObject
{
    public static readonly int MaximumLength = 20;
    public static readonly string InvalidLengthMessage = $"Code must be {MaximumLength} characters or less.";
    public string Value { get; private set; }

    private CustomerCode(string value) =>
        Value = value;

    public static Result<CustomerCode> Create(string? value)
    {
        value = value?.Trim() ?? string.Empty;
        return value.Length > MaximumLength
            ? Result.Failure<CustomerCode>(InvalidLengthMessage)
            : Result.Success(new CustomerCode(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    // EF requires an empty constructor
    private CustomerCode() =>
        Value = string.Empty;
}