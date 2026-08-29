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

    public CreditCardName Name { get; internal set; }
    public CreditCardFeeType FeeType { get; internal set; }
    public Fee Fee { get; internal set; }
    public DateTime? AddedToDeposit { get; internal set; }

    public static Result<CreditCard> Create(
        CreditCardName name,
        CreditCardFeeType feeType,
        Fee fee,
        DateTime addedToDeposit) =>
            Result.Success(new CreditCard
            {
                Name = name,
                FeeType = feeType,
                Fee = fee,
                AddedToDeposit = addedToDeposit
            });

    // EF requires a parameterless constructor
    private CreditCard()
    {
        Name = CreditCardName.Create("VISA").Value;
        FeeType = CreditCardFeeType.Flat;
        Fee = Fee.Default;
        AddedToDeposit = DateTime.MinValue;
    }
}
