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

    public static Result<BusinessName> Create(string? name) =>
        CreateNormalized(name);

    public static Result<BusinessName> NewBusinessName(string? name) =>
        CreateNormalized(name);

    private static Result<BusinessName> CreateNormalized(string? name)
    {
        var normalizedName = name?.Trim() ?? string.Empty;

        return Result.Success(normalizedName)
            .Ensure(value => value.Length > 0, RequiredMessage)
            .Ensure(
                value => value.Length is >= MinimumLength and <= MaximumLength,
                $"{InvalidLengthMessage} You entered {normalizedName.Length} character(s).")
            .Map(value => new BusinessName(value));
    }

    public override string ToString() =>
        Name;
}
