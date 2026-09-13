using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public class CreditCard : Entity
{
    public const string InvalidFeeTypeMessage = "A valid credit card fee type is required.";

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
                ValidateFeeType(feeType))
            .Map(() => new CreditCard(
                name,
                feeType,
                fee,
                addedToDeposit));

    public Result<CreditCard> WithName(string name) =>
        CreditCardName.Create(name.Trim()).Map(WithNameValue);

    public Result<CreditCard> WithName(CreditCardName name) =>
        CreditCardName.Create(name.Value).Map(WithNameValue);

    public Result<CreditCard> WithFeeType(CreditCardFeeType feeType) =>
        ValidateFeeType(feeType).Map(WithFeeTypeValue);

    public CreditCard WithFee(Fee fee) => Copy(fee: fee);

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
