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

    private CreditCard()
    {
    }

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
        DateTime? addedToDeposit) =>
        CreditCardName.Create(name.Value)
            .Bind(validName => ValidateFeeType(feeType)
                .Map(validFeeType => new CreditCard(
                    validName,
                    validFeeType,
                    fee,
                    ToMaybe(addedToDeposit))));

    public Result<CreditCard> SetName(string? name) =>
        CreditCardName.Create(name).Map(SetNameValue);

    public Result<CreditCard> SetName(CreditCardName name) =>
        CreditCardName.Create(name.Value).Map(SetNameValue);

    public Result<CreditCard> SetFeeType(CreditCardFeeType feeType) =>
        ValidateFeeType(feeType).Map(SetFeeTypeValue);

    public Result<CreditCard> SetFee(Fee fee) =>
        Result.Success(Copy(fee: fee));

    public Result<CreditCard> SetAddedToDeposit(DateTime addedToDeposit) =>
        Result.Success(Copy(addedToDeposit: addedToDeposit));

    private static Result<CreditCardFeeType> ValidateFeeType(CreditCardFeeType feeType) =>
        Enum.IsDefined(feeType)
            ? Result.Success(feeType)
            : Result.Failure<CreditCardFeeType>(InvalidFeeTypeMessage);

    private CreditCard SetNameValue(CreditCardName name) =>
        Copy(name: name);

    private CreditCard SetFeeTypeValue(CreditCardFeeType feeType) =>
        Copy(feeType: feeType);

    private static Maybe<DateTime> ToMaybe(DateTime? value) =>
        value.HasValue ? value.Value : Maybe<DateTime>.None;

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
