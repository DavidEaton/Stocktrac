using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public readonly record struct PersonName
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage = $"First, last and middle names must be between {MinimumLength} character(s) {MaximumLength} and in length";
    public static readonly string RequiredMessage = $"First and last names are required";

    private PersonName(string lastName, string firstName, string? middleName = null)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName;
    }

    public string LastName { get; }
    public string FirstName { get; }
    public Maybe<string> MiddleName { get; }

    public static Result<PersonName> Create(string lastName, string firstName, string? middleName = null) =>
        Result.Success((
                LastName: lastName?.Trim() ?? string.Empty,
                FirstName: firstName?.Trim() ?? string.Empty,
                MiddleName: string.IsNullOrEmpty(middleName) ? null : middleName.Trim()))
            .Ensure(
                values => !string.IsNullOrWhiteSpace(values.LastName) &&
                          !string.IsNullOrWhiteSpace(values.FirstName),
                RequiredMessage)
            .Ensure(
                values => values.LastName.Length is >= MinimumLength and <= MaximumLength &&
                          values.FirstName.Length is >= MinimumLength and <= MaximumLength &&
                          (values.MiddleName is null ||
                           values.MiddleName.Length is >= MinimumLength and <= MaximumLength),
                InvalidLengthMessage)
            .Map(values => new PersonName(values.LastName, values.FirstName, values.MiddleName));

    public Result<PersonName> NewLastName(string newLastName)
    {
        newLastName = (newLastName ?? string.Empty).Trim();

        if (newLastName.Length < MinimumLength ||
            newLastName.Length > MaximumLength)
            return Result.Failure<PersonName>(InvalidLengthMessage);

        return Result.Success(new PersonName(newLastName, FirstName, MiddleName.GetValueOrDefault()));
    }

    public Result<PersonName> NewFirstName(string newFirstName)
    {
        newFirstName = (newFirstName ?? string.Empty).Trim();

        if (newFirstName.Length < MinimumLength ||
            newFirstName.Length > MaximumLength)
            return Result.Failure<PersonName>(InvalidLengthMessage);

        return Result.Success(new PersonName(LastName, newFirstName, MiddleName.GetValueOrDefault()));
    }

    public Result<PersonName> NewMiddleName(string? newMiddleName)
    {
        if (newMiddleName is null || newMiddleName == string.Empty)
        {
            return Result.Failure<PersonName>(InvalidLengthMessage);
        }

        newMiddleName = newMiddleName.Trim();

        return newMiddleName?.Length < MinimumLength ||
            newMiddleName?.Length > MaximumLength
            ? Result.Failure<PersonName>(InvalidLengthMessage)
            : Result.Success(new PersonName(LastName, FirstName, newMiddleName));
    }

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
