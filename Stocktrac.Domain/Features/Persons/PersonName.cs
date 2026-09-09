using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public sealed record PersonName
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"First, last and middle names must be between {MinimumLength} character(s) {MaximumLength} and in length.";
    public static readonly string RequiredMessage = $"First and last names are required.";

    private PersonName(string lastName, string firstName, Maybe<string> middleName)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
    }

    public string LastName { get; }
    public string FirstName { get; }
    public Maybe<string> MiddleName { get; }

    public static Result<PersonName> Create(string lastName, string firstName, Maybe<string> middleName = default)
    {
        var normalizedLastName = lastName.Trim();
        var normalizedFirstName = firstName.Trim();
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

    public Result<PersonName> NewLastName(string newLastName) =>
        Create(newLastName, FirstName, MiddleName);

    public Result<PersonName> NewFirstName(string newFirstName) =>
        Create(LastName, newFirstName, MiddleName);

    public Result<PersonName> NewMiddleName(string newMiddleName) =>
        Create(LastName, FirstName, newMiddleName);

    public Result<PersonName> ClearMiddleName() =>
        Create(LastName, FirstName, Maybe<string>.None);

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
