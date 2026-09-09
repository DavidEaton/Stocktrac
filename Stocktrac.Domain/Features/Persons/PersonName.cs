using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public sealed record PersonName
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"First, last and middle names must be between {MinimumLength} character(s) {MaximumLength} and in length.";
    public static readonly string RequiredMessage = $"First and last names are required.";

    private PersonName(string lastName, string firstName, string? middleName = null)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName;
    }

    public string LastName { get; }
    public string FirstName { get; }
    public Maybe<string> MiddleName { get; }

    public static Result<PersonName> Create(string? lastName, string? firstName, string? middleName = null)
    {
        var normalizedLastName = lastName?.Trim() ?? string.Empty;
        var normalizedFirstName = firstName?.Trim() ?? string.Empty;
        var normalizedMiddleName = string.IsNullOrEmpty(middleName) ? null : middleName.Trim();

        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(
                    string.IsNullOrWhiteSpace(normalizedLastName) || string.IsNullOrWhiteSpace(normalizedFirstName),
                    RequiredMessage),
                Result.FailureIf(
                    normalizedLastName.Length is < MinimumLength or > MaximumLength ||
                    normalizedFirstName.Length is < MinimumLength or > MaximumLength ||
                    (normalizedMiddleName is not null && normalizedMiddleName.Length is < MinimumLength or > MaximumLength),
                    InvalidLengthMessage))
            .Map(() => new PersonName(normalizedLastName, normalizedFirstName, normalizedMiddleName));
    }

    public Result<PersonName> NewLastName(string? newLastName) =>
        Create(newLastName, FirstName, MiddleName.GetValueOrDefault());

    public Result<PersonName> NewFirstName(string? newFirstName) =>
        Create(LastName, newFirstName, MiddleName.GetValueOrDefault());

    public Result<PersonName> NewMiddleName(string? newMiddleName) =>
        Create(LastName, FirstName, newMiddleName);

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
