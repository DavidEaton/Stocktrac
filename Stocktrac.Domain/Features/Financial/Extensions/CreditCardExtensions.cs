using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial.Extensions
{
    public static class CreditCardExtensions
    {
        internal static Result<string> ValidateName(string? name) =>
            Result.Success(name?.Trim() ?? string.Empty)
                .Ensure(
                    value => !string.IsNullOrWhiteSpace(value),
                    CreditCard.RequiredMessage)
                .Ensure(
                    value => value.Length is >= CreditCard.MinimumLength and <= CreditCard.MaximumLength,
                    CreditCard.InvalidLengthMessage);
    }
}