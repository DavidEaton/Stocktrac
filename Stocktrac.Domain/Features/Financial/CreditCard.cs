using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public sealed class CreditCard : Entity
{
    public const string InvalidFeeTypeMessage = "A valid credit card fee type is required.";
    public const string RequiredMessage = "Please include all required items.";
    public const string InvalidAddedToDepositMessage = "A valid added-to-deposit date is required.";
    public const string NotAddedToDepositMessage = "The credit card has not been added to a deposit.";
    public CreditCardName Name { get; private set; }
    public CreditCardFeeType FeeType { get; private set; }
    public Fee Fee { get; private set; }
    public Maybe<DateTime> AddedToDeposit { get; private set; }
    public bool IsAddedToDeposit => AddedToDeposit.HasValue;

    private CreditCard(
        CreditCardName name,
        CreditCardFeeType feeType,
        Fee fee,
        Maybe<DateTime> addedToDeposit)
    {
        Name = name;
        FeeType = feeType;
        Fee = fee;
        AddedToDeposit = addedToDeposit;
    }

    public static Result<CreditCard> Create(
        CreditCardName name,
        CreditCardFeeType feeType,
        Fee fee,
        Maybe<DateTime> addedToDeposit) =>
        Result.Combine(
                Environment.NewLine,
                Result.FailureIf(name is null, RequiredMessage),
                ValidateFeeType(feeType))
            .Map(() => new CreditCard(name!, feeType, fee!, addedToDeposit));

    public Result<CreditCard> ChangeName(string name) =>
        CreditCardName.Create(name?.Trim() ?? string.Empty)
            .Tap(validName => Name = validName)
            .Map(_ => this);

    public Result<CreditCard> ChangeName(CreditCardName name) =>
        name is null
            ? Result.Failure<CreditCard>(RequiredMessage)
            : Result.Success(name)
                .Tap(validName => Name = validName)
                .Map(_ => this);

    public Result<CreditCard> ChangeFeeType(
        CreditCardFeeType feeType) =>
        ValidateFeeType(feeType)
            .Tap(validFeeType => FeeType = validFeeType)
            .Map(_ => this);

    public Result<CreditCard> ChangeFee(Fee fee) =>
        Result.Success(fee)
                .Tap(validFee => Fee = validFee)
                .Map(_ => this);

    public Result<CreditCard> MarkAddedToDeposit(
        DateTime addedToDeposit) =>
        Result.Success(addedToDeposit)
            .Ensure(date => date != default, InvalidAddedToDepositMessage)
            .Tap(date => AddedToDeposit = date)
            .Map(_ => this);

    public Result<CreditCard> RemoveFromDeposit() =>
        Result.Success(this)
            .Ensure(card => card.IsAddedToDeposit, NotAddedToDepositMessage)
            .Tap(card => card.AddedToDeposit = Maybe<DateTime>.None);

    private static Result<CreditCardFeeType> ValidateFeeType(
        CreditCardFeeType feeType) =>
        Enum.IsDefined(feeType)
            ? Result.Success(feeType)
            : Result.Failure<CreditCardFeeType>(
                InvalidFeeTypeMessage);
}