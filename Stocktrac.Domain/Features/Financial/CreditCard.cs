using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

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
        string? name,
        CreditCardFeeType feeType,
        double fee,
        bool? isAddedToDeposit) =>
        ValidateName(name)
            .Ensure(_ => Enum.IsDefined(feeType), RequiredMessage)
            .Map(validName => new CreditCard(validName, feeType, fee, isAddedToDeposit));

    public Result<string> SetName(string? name) =>
        ValidateName(name)
            .Tap(validName => Name = validName);

    public Result<CreditCardFeeType> SetFeeType(CreditCardFeeType feeType) =>
        !Enum.IsDefined(feeType)
            ? Result.Failure<CreditCardFeeType>(RequiredMessage)
            : Result.Success(FeeType = feeType);

    public Result<double> SetFee(double fee) =>
        Result.Success(Fee = fee);

    public Result<bool?> SetIsAddedToDeposit(bool? isAddedToDeposit) =>
        Result.Success(IsAddedToDeposit = isAddedToDeposit);

    private static Result<string> ValidateName(string? name) =>
        Result.Success(name?.Trim() ?? string.Empty)
            .Ensure(value => !string.IsNullOrWhiteSpace(value), RequiredMessage)
            .Ensure(
                value => value.Length is >= MinimumLength and <= MaximumLength,
                InvalidLengthMessage);

    // EF requires a parameterless constructor
    private CreditCard()
    {
        Name = string.Empty;
        FeeType = CreditCardFeeType.Flat;
        Fee = 0;
        IsAddedToDeposit = false;
    }
}
