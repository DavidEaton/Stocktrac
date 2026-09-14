using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public sealed record PersonName
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"First, last and middle names must be between {MinimumLength} character(s) {MaximumLength} and in length.";
    public const string RequiredMessage = "First and last names are required.";

    private PersonName(string lastName, string firstName, Maybe<string> middleName)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
    }

    public string LastName { get; private set; }
    public string FirstName { get; private set; }
    public Maybe<string> MiddleName { get; private set; }

    public static Result<PersonName> Create(string lastName, string firstName, Maybe<string> middleName = default)
    {
        var normalizedLastName = lastName?.Trim() ?? string.Empty;
        var normalizedFirstName = firstName?.Trim() ?? string.Empty;
        var normalizedMiddleName = middleName.Map(value => value.Trim());

        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(
                    string.IsNullOrWhiteSpace(normalizedLastName) || string.IsNullOrWhiteSpace(normalizedFirstName),
                    RequiredMessage),
                Result.FailureIf(
                    normalizedLastName.Length is < MinimumLength or > MaximumLength ||
                    normalizedFirstName.Length is < MinimumLength or > MaximumLength ||
                    (normalizedMiddleName.HasValue && normalizedMiddleName.Value.Length is < MinimumLength or > MaximumLength),
                    InvalidLengthMessage))
            .Map(() => new PersonName(normalizedLastName, normalizedFirstName, normalizedMiddleName));
    }

    public Result<PersonName> WithLastName(string newLastName) =>
        newLastName.AsValidRequiredPersonNamePart()
            .Map(validLastName => this with { LastName = validLastName });

    public Result<PersonName> WithFirstName(string newFirstName) =>
        newFirstName.AsValidRequiredPersonNamePart()
            .Map(validFirstName => this with { FirstName = validFirstName });

    public Result<PersonName> WithMiddleName(string newMiddleName) =>
        newMiddleName.AsValidOptionalPersonNamePart()
            .Map(validMiddleName => this with { MiddleName = validMiddleName });

    public Result<PersonName> WithoutMiddleName() =>
        Result.Success(this with { MiddleName = Maybe<string>.None });

    public string LastFirstMiddle =>
        MiddleName.HasNoValue
            ? $"{LastName}, {FirstName}"
            : $"{LastName}, {FirstName} {MiddleName.Value}";

    public string LastFirstMiddleInitial =>
        MiddleName.HasNoValue
            ? $"{LastName}, {FirstName}"
            : $"{LastName}, {FirstName} {MiddleName.Value[0]}.";

    public string FirstMiddleLast =>
        MiddleName.HasNoValue
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName.Value} {LastName}";

    public override string ToString() => LastFirstMiddleInitial;
}
