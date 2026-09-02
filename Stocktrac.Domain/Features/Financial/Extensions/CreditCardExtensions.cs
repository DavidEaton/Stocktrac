using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Domain.Features.Financial.Extensions
{
    public static class CreditCardExtensions
    {
        public static bool IsAddedToDeposit(this CreditCard creditCard) =>
            creditCard.AddedToDeposit.HasValue;

        public static Result<string> SetName(this CreditCard creditCard, string? name) =>
            ValidateName(name)
                .Tap(validName => creditCard.Name = CreditCardName.Create(validName).Value);

        public static Result<CreditCardName> SetName(this CreditCard creditCard,
            CreditCardName name) =>
                Result.Success(creditCard.Name = name);

        public static Result<CreditCardFeeType> SetFeeType(
            this CreditCard creditCard,
            CreditCardFeeType feeType) =>
                !Enum.IsDefined(feeType)
                ? Result.Failure<CreditCardFeeType>(CreditCardName.InvalidLengthMessage)
                : Result.Success(creditCard.FeeType = feeType);

        public static Result<Fee> SetFee(this CreditCard creditCard, Fee fee) =>
            Result.Success(creditCard.Fee = fee);

        public static Result<DateTime?> SetAddedToDeposit(
            this CreditCard creditCard,
            DateTime addedToDeposit) =>
                Result.Success(creditCard.AddedToDeposit = addedToDeposit);

        internal static Result<string> ValidateName(string? name) =>
            Result.Success(name?.Trim() ?? string.Empty)
                .Ensure(
                    value => !string.IsNullOrWhiteSpace(value),
                    CreditCardName.InvalidLengthMessage)
                .Ensure(
                    value => value.Length is >= CreditCardName.MinimumLength and <= CreditCardName.MaximumLength,
                    CreditCardName.InvalidLengthMessage);
    }
}