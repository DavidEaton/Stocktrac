using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record BusinessName
{
    public const int MinimumLength = 2;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"Business Name must be between {MinimumLength} and {MaximumLength} character(s) in length.";
    public const string RequiredMessage = "Business Name is required.";
    public string Name { get; }
    private BusinessName(string name) => Name = name;
    public static Result<BusinessName> Create(string name) =>
            Result.Success(name)
                .Map(input => input?.Trim() ?? string.Empty)
                .Ensure(normalized => normalized.IsNonEmptyString(), RequiredMessage)
                .Ensure(normalized => normalized.Length.IsWithin(MinimumLength, MaximumLength), InvalidLengthMessage)
                .Map(normalized => new BusinessName(normalized));

    public override string ToString() => Name;
}
