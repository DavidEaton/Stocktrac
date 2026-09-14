using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public class CreditCard : Entity
{
    public const string InvalidFeeTypeMessage = "A valid credit card fee type is required.";
    public const string RequiredMessage = "Please include all required items.";

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
                Result.FailureIf(fee is null, RequiredMessage),
                ValidateFeeType(feeType))
            .Map(() => new CreditCard(
                name,
                feeType,
                fee,
                addedToDeposit));

    public Result<CreditCard> WithName(string name) =>
        CreditCardName.Create(name?.Trim() ?? string.Empty).Map(WithNameValue);

    public Result<CreditCard> WithName(CreditCardName name) =>
        name is null
            ? Result.Failure<CreditCard>(RequiredMessage)
            : CreditCardName.Create(name.Value).Map(WithNameValue);

    public Result<CreditCard> WithFeeType(CreditCardFeeType feeType) =>
        ValidateFeeType(feeType).Map(WithFeeTypeValue);

    public Result<CreditCard> WithFee(Fee fee) =>
        fee is null ? Result.Failure<CreditCard>(RequiredMessage) : Result.Success(Copy(fee: fee));

    public CreditCard WithAddedToDeposit(DateTime addedToDeposit) =>
        Copy(addedToDeposit: addedToDeposit);

    public CreditCard WithoutAddedToDeposit() =>
        Copy(addedToDeposit: Maybe<DateTime>.None);

    private static Result<CreditCardFeeType> ValidateFeeType(CreditCardFeeType feeType) =>
        Enum.IsDefined(feeType)
            ? Result.Success(feeType)
            : Result.Failure<CreditCardFeeType>(InvalidFeeTypeMessage);

    private CreditCard WithNameValue(CreditCardName name) =>
        Copy(name: name);

    private CreditCard WithFeeTypeValue(CreditCardFeeType feeType) =>
        Copy(feeType: feeType);

    private CreditCard Copy(
        CreditCardName? name = null,
        CreditCardFeeType? feeType = null,
        Fee? fee = null,
        Maybe<DateTime>? addedToDeposit = null) =>
        new(name ?? Name, feeType ?? FeeType, fee ?? Fee,
            addedToDeposit ?? AddedToDeposit)
        {
            Id = Id
        };
}
