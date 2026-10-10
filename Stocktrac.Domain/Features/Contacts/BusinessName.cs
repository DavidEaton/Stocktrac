using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record BusinessName
{
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"Business Name must not exceed {MaximumLength} characters.";
    public NonEmptyString Name { get; }
    private BusinessName(NonEmptyString name) => Name = name;
    public static Result<BusinessName> Create(NonEmptyString name) =>
        name.Value.Length <= MaximumLength
            ? Result.Success(new BusinessName(name))
            : Result.Failure<BusinessName>(InvalidLengthMessage);

    public override string ToString() => Name.ToString();
}
