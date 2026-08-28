using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Domain.Features.Financial;

public class CreditCard : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MinimumLength = 2;
    public const int MaximumLength = 50;
    public const string RequiredMessage = "A valid value is required.";
    public const string InvalidLengthMessage = "Value must be between 2 and 50 characters.";

    public CreditCardName Name { get; internal set; }
    public CreditCardFeeType FeeType { get; private set; }
    public Fee Fee { get; private set; }
    public DateTime? AddedToDeposit { get; private set; }
    public bool IsAddedToDeposit => AddedToDeposit.HasValue;

    private CreditCard(
        CreditCardName name,
        CreditCardFeeType feeType,
        Fee fee,
        DateTime? addedToDeposit)
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
        DateTime addedToDeposit) =>
            Result.Success(new CreditCard(name, feeType, fee, addedToDeposit));

    public Result<string> SetName(string? name) =>
       CreditCardExtensions.ValidateName(name)
            .Tap(validName => Name = CreditCardName.Create(validName).Value);

    public Result<CreditCardName> SetName(CreditCardName name) =>
        Result.Success(Name = name);

    public Result<CreditCardFeeType> SetFeeType(CreditCardFeeType feeType) =>
        !Enum.IsDefined(feeType)
            ? Result.Failure<CreditCardFeeType>(RequiredMessage)
            : Result.Success(FeeType = feeType);

    public Result<Fee> SetFee(Fee fee) =>
        Result.Success(Fee = fee);

    public Result<DateTime?> SetAddedToDeposit(DateTime addedToDeposit) =>
        Result.Success(AddedToDeposit = addedToDeposit);

    // EF requires a parameterless constructor
    private CreditCard()
    {
        Name = CreditCardName.Create(string.Empty).Value;
        FeeType = CreditCardFeeType.Flat;
        Fee = Fee.Default;
        AddedToDeposit = DateTime.MinValue;
    }
}
