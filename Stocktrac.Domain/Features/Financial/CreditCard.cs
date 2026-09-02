using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public class CreditCard : Entity
{
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
}
