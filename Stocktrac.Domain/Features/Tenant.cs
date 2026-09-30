using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features;

public sealed class Tenant : Entity<Guid>
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MinimumNameLength = 2;
    public const int MaximumNameLength = 2048;
    public const int MinimumCompanyNameLength = 2;
    public const int MaximumCompanyNameLength = 2048;
    public const int MaximumLogoUrlLength = 4096;

    public const string NameRequiredMessage =
        "Tenant name is required.";

    public const string CompanyNameRequiredMessage =
        "Company name is required.";

    public const string LogoUrlRequiredMessage =
        "Use RemoveLogoUrl to clear the logo URL.";

    public static readonly string InvalidNameLengthMessage =
        $"Tenant name must be between {MinimumNameLength} and {MaximumNameLength} characters.";

    public static readonly string InvalidCompanyNameLengthMessage =
        $"Company name must be between {MinimumCompanyNameLength} and {MaximumCompanyNameLength} characters.";

    public static readonly string InvalidLogoUrlLengthMessage =
        $"Logo URL must be {MaximumLogoUrlLength} characters or less.";

    public string Name { get; private set; }
    public string CompanyName { get; private set; }
    public Maybe<string> LogoUrl { get; private set; }

    private Tenant(
        Guid id,
        string name,
        string companyName,
        Maybe<string> logoUrl)
    {
        Id = id;
        Name = name;
        CompanyName = companyName;
        LogoUrl = logoUrl;
    }

    public static Result<Tenant> Create(
        string name,
        string companyName,
        Maybe<string> logoUrl = default)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        var normalizedCompanyName = companyName?.Trim() ?? string.Empty;
        var normalizedLogoUrl = logoUrl.Map(value => value.Trim());

        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(string.IsNullOrWhiteSpace(normalizedName), NameRequiredMessage),
                Result.FailureIf(normalizedName.Length is < MinimumNameLength or > MaximumNameLength, InvalidNameLengthMessage),
                Result.FailureIf(string.IsNullOrWhiteSpace(normalizedCompanyName), CompanyNameRequiredMessage),
                Result.FailureIf(normalizedCompanyName.Length is < MinimumCompanyNameLength or > MaximumCompanyNameLength, InvalidCompanyNameLengthMessage),
                ValidateLogoUrl(normalizedLogoUrl))
            .Map(() => new Tenant(Guid.NewGuid(), normalizedName, normalizedCompanyName, normalizedLogoUrl));
    }

    public Result UpdateName(string name)
    {
        name = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(NameRequiredMessage);

        return name.Length < MinimumNameLength ||
               name.Length > MaximumNameLength
            ? Result.Failure(InvalidNameLengthMessage)
            : Result.Success(Name = name);
    }

    public Result UpdateCompanyName(string companyName)
    {
        companyName = companyName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(companyName))
            return Result.Failure(CompanyNameRequiredMessage);

        return companyName.Length < MinimumCompanyNameLength ||
               companyName.Length > MaximumCompanyNameLength
            ? Result.Failure(InvalidCompanyNameLengthMessage)
            : Result.Success(CompanyName = companyName);
    }

    public Result UpdateLogoUrl(string logoUrl)
    {
        logoUrl = logoUrl?.Trim() ?? string.Empty;

        return ValidateLogoUrl(logoUrl)
            .Tap(() => LogoUrl = logoUrl);
    }

    public void RemoveLogoUrl() => LogoUrl = Maybe<string>.None;

    private static Result ValidateLogoUrl(Maybe<string> logoUrl)
    {
        if (logoUrl.HasNoValue)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(logoUrl.Value))
            return Result.Failure(LogoUrlRequiredMessage);

        return logoUrl.Value.Length <= MaximumLogoUrlLength
            ? Result.Success()
            : Result.Failure(InvalidLogoUrlLengthMessage);
    }

    // Required by Entity Framework.
    private Tenant()
    {
        Name = string.Empty;
        CompanyName = string.Empty;
    }
}
