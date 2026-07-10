using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features;

public class CreditCard : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MinimumLength = 2;
    public const int MaximumLength = 50;
    public const string RequiredMessage = "A valid value is required.";
    public const string InvalidLengthMessage = "Value must be between 2 and 50 characters.";

    public string Name { get; private set; }
    public CreditCardFeeType FeeType { get; private set; }
    public double Fee { get; private set; }
    public bool? IsAddedToDeposit { get; private set; }

    private CreditCard(
        string name,
        CreditCardFeeType feeType,
        double fee,
        bool? isAddedToDeposit)
    {
        Name = name;
        FeeType = feeType;
        Fee = fee;
        IsAddedToDeposit = isAddedToDeposit;
    }

    public static Result<CreditCard> Create(
        string name,
        CreditCardFeeType feeType,
        double fee,
        bool? isAddedToDeposit)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CreditCard>(RequiredMessage);

        name = (name ?? string.Empty).Trim();

        if (name.Length is < MinimumLength or > MaximumLength)
            return Result.Failure<CreditCard>(InvalidLengthMessage);

        if (!Enum.IsDefined(feeType))
            return Result.Failure<CreditCard>(RequiredMessage);

        return Result.Success(new CreditCard(name, feeType, fee, isAddedToDeposit));
    }

    public Result<string> SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<string>(RequiredMessage);

        name = (name ?? string.Empty).Trim();

        if (name.Length < MinimumLength ||
            name.Length > MaximumLength)
            return Result.Failure<string>(InvalidLengthMessage);

        return Result.Success(Name = name);
    }

    public Result<CreditCardFeeType> SetFeeType(CreditCardFeeType feeType) =>
        !Enum.IsDefined(feeType)
            ? Result.Failure<CreditCardFeeType>(RequiredMessage)
            : Result.Success(FeeType = feeType);

    public Result<double> SetFee(double fee) =>
        Result.Success(Fee = fee);

    public Result<bool?> SetIsAddedToDeposit(bool? isAddedToDeposit) =>
        Result.Success(IsAddedToDeposit = isAddedToDeposit);

    // EF requires a parameterless constructor
    protected CreditCard() { }
}