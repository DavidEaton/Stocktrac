using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record PostalCode
{
    public const int MaximumLength = 20;
    public static readonly string InvalidLengthMessage = $"Postal Code must not exceed {MaximumLength} characters.";
    public NonEmptyString Value { get; }

    private PostalCode(NonEmptyString value) => Value = value;

    public static Result<PostalCode> Create(NonEmptyString value) =>
        value.Value.Length <= MaximumLength
            ? Result.Success(new PostalCode(value))
            : Result.Failure<PostalCode>(InvalidLengthMessage);

    public override string ToString() => Value.ToString();
}
