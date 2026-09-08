using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record BusinessName
{
    public const int MinimumLength = 2;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"Business Name must be between {MinimumLength} and {MaximumLength} character(s) in length.";
    public static readonly string RequiredMessage = $"Business Name is required.";

    public string Name { get; }

    private BusinessName(string name) =>
        Name = name;

    public static Result<BusinessName> Create(string name) =>
        CreateNormalized(name);

    public static Result<BusinessName> NewBusinessName(string name) =>
        CreateNormalized(name);

    private static Result<BusinessName> CreateNormalized(string name)
    {
        var normalizedName = name?.Trim() ?? string.Empty;

        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(normalizedName.Length == 0, RequiredMessage),
                Result.FailureIf(
                    normalizedName.Length > 0 && normalizedName.Length is < MinimumLength or > MaximumLength,
                    $"{InvalidLengthMessage} You entered {normalizedName.Length} character(s)."))
            .Map(() => new BusinessName(normalizedName));
    }

    public override string ToString() =>
        Name;
}
