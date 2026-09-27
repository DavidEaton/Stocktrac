using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record BusinessName
{
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"Business Name must not exceed {MaximumLength} characters.";
    public NonEmptyString Name { get; }
    private BusinessName(NonEmptyString name) => Name = name;
    public static Result<BusinessName> Create(NonEmptyString name) =>
            Result.Success(name)
                .Ensure(value => value.Value.Length <= MaximumLength, InvalidLengthMessage)
                .Map(value => new BusinessName(value));

    public override string ToString() => Name.ToString();
}
