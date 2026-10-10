using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.SaleCodes
{
    public sealed class SaleCodeShopSupplies : Entity
    {
        public static readonly double MinimumValue = 0;
        public const string MinimumValueMessage = "Value(s) cannot be negative.";
        public const string RequiredMessage = "Please include all required items.";

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
        => Result.Success()
            .Ensure(() => double.IsFinite(percentage) && percentage >= MinimumValue, MinimumValueMessage)
            .Ensure(() => double.IsFinite(minimumJobAmount) && minimumJobAmount >= MinimumValue, MinimumValueMessage)
            .Ensure(() => double.IsFinite(minimumCharge) && minimumCharge >= MinimumValue, MinimumValueMessage)
            .Ensure(() => double.IsFinite(maximumCharge) && maximumCharge >= MinimumValue, MinimumValueMessage)
            .Map(() => new SaleCodeShopSupplies(
                percentage,
                minimumJobAmount,
                minimumCharge,
                maximumCharge,
                includeParts,
                includeLabor));

        public Result<double> UpdatePercentage(double percentage) =>
            !double.IsFinite(percentage) || percentage < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(Percentage = percentage);

        public Result<double> UpdateMinimumJobAmount(double minimumJobAmount) =>
            !double.IsFinite(minimumJobAmount) || minimumJobAmount < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(MinimumJobAmount = minimumJobAmount);

        public Result<double> UpdateMinimumCharge(double minimumCharge) =>
            !double.IsFinite(minimumCharge) || minimumCharge < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(MinimumCharge = minimumCharge);

        public Result<double> UpdateMaximumCharge(double maximumCharge) =>
            !double.IsFinite(maximumCharge) || maximumCharge < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(MaximumCharge = maximumCharge);

        public void UpdateIncludeParts(bool includeParts) => IncludeParts = includeParts;

        public void UpdateIncludeLabor(bool includeLabor) => IncludeLabor = includeLabor;

        // EF requires a parameterless constructor
        private SaleCodeShopSupplies() { }
    }
}
