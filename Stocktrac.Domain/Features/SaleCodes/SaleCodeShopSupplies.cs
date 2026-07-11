using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.SaleCodes
{
    public class SaleCodeShopSupplies : Entity
    {
        public static readonly double MinimumValue = 0;
        public static readonly string MinimumValueMessage = $"Value(s) cannot be negative.";
        public static readonly string RequiredMessage = $"Please include all required items.";

        public double Percentage { get; private set; }
        public double MinimumJobAmount { get; private set; }
        public double MinimumCharge { get; private set; }
        public double MaximumCharge { get; private set; }
        public bool IncludeParts { get; private set; }
        public bool IncludeLabor { get; private set; }

        private SaleCodeShopSupplies(
            double percentage,
            double minimumJobAmount,
            double minimumCharge,
            double maximumCharge,
            bool includeParts,
            bool includeLabor)
        {
            Percentage = percentage;
            MinimumJobAmount = minimumJobAmount;
            MinimumCharge = minimumCharge;
            MaximumCharge = maximumCharge;
            IncludeParts = includeParts;
            IncludeLabor = includeLabor;
        }

        public static Result<SaleCodeShopSupplies> Create(
            double percentage,
            double minimumJobAmount,
            double minimumCharge,
            double maximumCharge,
            bool includeParts,
            bool includeLabor)
        {
            if (percentage < MinimumValue)
                return Result.Failure<SaleCodeShopSupplies>(MinimumValueMessage);

            if (minimumJobAmount < MinimumValue)
                return Result.Failure<SaleCodeShopSupplies>(MinimumValueMessage);

            if (minimumCharge < MinimumValue)
                return Result.Failure<SaleCodeShopSupplies>(MinimumValueMessage);

            if (maximumCharge < MinimumValue)
                return Result.Failure<SaleCodeShopSupplies>(MinimumValueMessage);

            return Result.Success(new SaleCodeShopSupplies(percentage, minimumJobAmount, minimumCharge, maximumCharge, includeParts, includeLabor));
        }

        public Result<double> SetPercentage(double percentage) =>
            percentage < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(Percentage = percentage);

        public Result<double> SetMinimumJobAmount(double minimumJobAmount) =>
            minimumJobAmount < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(MinimumJobAmount = minimumJobAmount);

        public Result<double> SetMinimumCharge(double minimumCharge) =>
            minimumCharge < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(MinimumCharge = minimumCharge);

        public Result<double> SetMaximumCharge(double maximumCharge) =>
            maximumCharge < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(MaximumCharge = maximumCharge);

        public Result<bool> SetIncludeParts(bool includeParts) =>
            Result.Success(IncludeParts = includeParts);

        public Result<bool> SetIncludeLabor(bool includeLabor) =>
            Result.Success(IncludeLabor = includeLabor);

        // EF requires a parameterless constructor
        protected SaleCodeShopSupplies() { }
    }
}