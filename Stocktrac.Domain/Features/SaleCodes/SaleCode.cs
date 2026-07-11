using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.SaleCodes
{
    public class SaleCode : Entity
    {
        // Invariant: SaleCode belongs to ONE Manufacturer
        // public Manufacturer Manufacturer { get; private set; }

        // TODO: Move these constants to user-configurable settings in the future.
        // For now, they are hard-coded to match the current validation rules in StockTrac.
        public static readonly int MinimumLength = 1;
        public static readonly int NameMaximumLength = 255;
        public static readonly int CodeMaximumLength = 4;
        public static readonly double MinimumValue = 0;
        public static readonly double MaximumDesiredMarginValue = 100;
        public static string InvalidLengthMessage(int minLength, int maxLength) => $"Value must be between {minLength} and {maxLength} characters.";
        public static string InvalidValueMessage(double minValue, double maxValue) => $"Value must be between {minValue} and {maxValue}.";
        public static readonly string RequiredMessage = $"Please include all required items.";
        public static readonly string MinimumValueMessage = $"Value(s) cannot be negative.";
        public static readonly string NonuniqueMessage = $"Code is already in use and must be unique.";

        public string Name { get; private set; }
        public string Code { get; private set; }
        public double LaborRate { get; private set; }
        public double DesiredMargin { get; private set; }
        public SaleCodeShopSupplies ShopSupplies { get; private set; }

        private SaleCode(
            string name,
            string code,
            double laborRate,
            double desiredMargin,
            SaleCodeShopSupplies shopSupplies)
        {
            Name = name;
            Code = code;
            LaborRate = laborRate;
            DesiredMargin = desiredMargin;
            ShopSupplies = shopSupplies;  // By the time we get here, ShopSupplies validation has already occurred; no need to repeat here.
        }
        public static Result<SaleCode> Create(
            string name,
            string code,
            double laborRate,
            double desiredMargin,
            SaleCodeShopSupplies shopSupplies,
            IReadOnlyList<string> saleCodes)
        {
            name = name?.Trim() ?? string.Empty;
            code = code?.Trim().ToUpperInvariant() ?? string.Empty;

            if (name.Length < MinimumLength || name.Length > NameMaximumLength)
                return Result.Failure<SaleCode>(
                    InvalidLengthMessage(MinimumLength, NameMaximumLength));

            if (code.Length < MinimumLength || code.Length > CodeMaximumLength)
                return Result.Failure<SaleCode>(
                    InvalidLengthMessage(MinimumLength, CodeMaximumLength));

            if
                (laborRate < MinimumValue)
                return Result.Failure<SaleCode>(MinimumValueMessage);

            if (desiredMargin < MinimumValue ||
                desiredMargin > MaximumDesiredMarginValue)
            {
                return Result.Failure<SaleCode>(
                    InvalidValueMessage(MinimumValue, MaximumDesiredMarginValue));
            }

            if (shopSupplies is null)
                return Result.Failure<SaleCode>(RequiredMessage);

            if (saleCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
                return Result.Failure<SaleCode>(NonuniqueMessage);

            return Result.Success(
                new SaleCode(
                    name: name,
                    code: code,
                    laborRate: laborRate,
                    desiredMargin: desiredMargin,
                    shopSupplies: shopSupplies));
        }

        public Result<string> SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<string>(RequiredMessage);

            name = (name ?? string.Empty).Trim();

            if (name.Length > NameMaximumLength || name.Length < MinimumLength)
                return Result.Failure<string>(InvalidLengthMessage(MinimumLength, NameMaximumLength));

            return Result.Success(Name = name);
        }

        public Result<string> SetCode(string code, IReadOnlyList<string> saleCodes)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure<string>(RequiredMessage);

            code = (code ?? string.Empty).Trim().ToUpper();

            if (code.Length > CodeMaximumLength || code.Length < MinimumLength)
                return Result.Failure<string>(InvalidLengthMessage(MinimumLength, CodeMaximumLength));

            if (saleCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
                return Result.Failure<string>(NonuniqueMessage);

            return Result.Success(Code = code);
        }

        public Result<double> SetLaborRate(double laborRate) =>
            laborRate < MinimumValue
                ? Result.Failure<double>(MinimumValueMessage)
                : Result.Success(LaborRate = laborRate);

        public Result<double> SetDesiredMargin(double desiredMargin) =>
            desiredMargin < MinimumValue || desiredMargin > MaximumDesiredMarginValue
                ? Result.Failure<double>(InvalidValueMessage(MinimumValue, MaximumDesiredMarginValue))
                : Result.Success(DesiredMargin = desiredMargin);

        public Result<SaleCodeShopSupplies> SetShopSupplies(SaleCodeShopSupplies shopSupplies) =>
            shopSupplies is null
                ? Result.Failure<SaleCodeShopSupplies>(RequiredMessage)
                : Result.Success(ShopSupplies = shopSupplies);

        // EF requires a parameterless constructor
        private SaleCode()
        {
            Name = string.Empty;
            Code = string.Empty;
            LaborRate = 0;
            DesiredMargin = 0;
            ShopSupplies = SaleCodeShopSupplies.Create(0, 0, 0, 0, false, false).Value;
        }
    }
}
