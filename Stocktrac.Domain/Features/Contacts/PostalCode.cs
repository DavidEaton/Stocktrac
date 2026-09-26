using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record PostalCode
{
    public const int MaximumLength = 20;
    public static readonly string InvalidLengthMessage = $"Postal Code must not exceed {MaximumLength} characters.";
    public NonEmptyString Value { get; }

    private PostalCode(NonEmptyString value) => Value = value;

    public static Result<PostalCode> Create(NonEmptyString value) =>
            Result.Success(value)
                .Ensure(value => value.Value.Length <= MaximumLength, InvalidLengthMessage)
            .Map(value => new PostalCode(value));

    public override string ToString() => Value.ToString();
}
