using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features;

public sealed class Tenant : Entity<Guid>
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static readonly int MinimumNameLength = 2;
    public static readonly int MaximumNameLength = 2048;
    public static readonly int MinimumCompanyNameLength = 2;
    public static readonly int MaximumCompanyNameLength = 2048;
    public static readonly int MaximumLogoUrlLength = 4096;

    public static readonly string NameRequiredMessage =
        "Tenant name is required.";

    public static readonly string CompanyNameRequiredMessage =
        "Company name is required.";

    public static readonly string InvalidNameLengthMessage =
        $"Tenant name must be between {MinimumNameLength} and {MaximumNameLength} characters.";

    public static readonly string InvalidCompanyNameLengthMessage =
        $"Company name must be between {MinimumCompanyNameLength} and {MaximumCompanyNameLength} characters.";

    public static readonly string InvalidLogoUrlLengthMessage =
        $"Logo URL must be {MaximumLogoUrlLength} characters or less.";

    public string Name { get; private set; }
    public string CompanyName { get; private set; }
    public string? LogoUrl { get; private set; }

    private Tenant(
        Guid id,
        string name,
        string companyName,
        string? logoUrl)
    {
        Id = id;
        Name = name;
        CompanyName = companyName;
        LogoUrl = logoUrl;
    }

    public static Result<Tenant> Create(
        string? name,
        string? companyName,
        string? logoUrl = null)
    {
        name = name?.Trim() ?? string.Empty;
        companyName = companyName?.Trim() ?? string.Empty;
        logoUrl = NormalizeOptionalValue(logoUrl);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tenant>(NameRequiredMessage);

        if (name.Length < MinimumNameLength ||
            name.Length > MaximumNameLength)
        {
            return Result.Failure<Tenant>(InvalidNameLengthMessage);
        }

        if (string.IsNullOrWhiteSpace(companyName))
            return Result.Failure<Tenant>(CompanyNameRequiredMessage);

        if (companyName.Length < MinimumCompanyNameLength ||
            companyName.Length > MaximumCompanyNameLength)
        {
            return Result.Failure<Tenant>(
                InvalidCompanyNameLengthMessage);
        }

        if (logoUrl?.Length > MaximumLogoUrlLength)
            return Result.Failure<Tenant>(InvalidLogoUrlLengthMessage);

        return Result.Success(
            new Tenant(
                Guid.NewGuid(),
                name,
                companyName,
                logoUrl));
    }

    public Result SetName(string? name)
    {
        name = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(NameRequiredMessage);

        return name.Length < MinimumNameLength ||
               name.Length > MaximumNameLength
            ? Result.Failure(InvalidNameLengthMessage)
            : Result.Success(Name = name);
    }

    public Result SetCompanyName(string? companyName)
    {
        companyName = companyName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(companyName))
            return Result.Failure(CompanyNameRequiredMessage);

        return companyName.Length < MinimumCompanyNameLength ||
               companyName.Length > MaximumCompanyNameLength
            ? Result.Failure(InvalidCompanyNameLengthMessage)
            : Result.Success(CompanyName = companyName);
    }

    public Result SetLogoUrl(string? logoUrl)
    {
        logoUrl = NormalizeOptionalValue(logoUrl);

        return logoUrl?.Length > MaximumLogoUrlLength
            ? Result.Failure(InvalidLogoUrlLengthMessage)
            : Result.Success(LogoUrl = logoUrl);
    }

    private static string? NormalizeOptionalValue(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    // Required by Entity Framework.
    private Tenant()
    {
        Name = string.Empty;
        CompanyName = string.Empty;
    }
}