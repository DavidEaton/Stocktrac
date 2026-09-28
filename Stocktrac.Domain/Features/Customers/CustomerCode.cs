using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Customers;

public sealed record CustomerCode
{
    public const int MaximumLength = 20;
    public static readonly string InvalidLengthMessage = $"Code must be {MaximumLength} characters or less.";
    public NonEmptyString Value { get; }

    private CustomerCode(NonEmptyString value) =>
        Value = value;

    public static Result<CustomerCode> Create(NonEmptyString value) =>
        Result.Success(value)
            .Ensure(input => input.Value.Length <= MaximumLength, InvalidLengthMessage)
            .Map(input => new CustomerCode(input));

    public override string ToString() => Value.ToString();
}
