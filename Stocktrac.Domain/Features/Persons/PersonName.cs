using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public sealed record PersonName
{
    public const int MaximumLength = 255;
    public const string LastNameRequiredMessage = "Last name is required.";
    public const string FirstNameRequiredMessage = "First name is required.";

    public static readonly string InvalidLengthMessage =
        $"First, last and middle names must not exceed {MaximumLength} characters.";

    public NonEmptyString LastName { get; }
    public NonEmptyString FirstName { get; }
    public Maybe<NonEmptyString> MiddleName { get; }

    private PersonName(
        NonEmptyString lastName,
        NonEmptyString firstName,
        Maybe<NonEmptyString> middleName)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
    }

    public static Result<PersonName> Create(
        NonEmptyString lastName,
        NonEmptyString firstName,
        Maybe<NonEmptyString> middleName = default) =>
        Result.Success((
                LastName: lastName,
                FirstName: firstName,
                MiddleName: middleName))
            .Ensure(
                name => name.LastName.Value.Length <= MaximumLength,
                InvalidLengthMessage)
            .Ensure(
                name => name.FirstName.Value.Length <= MaximumLength,
                InvalidLengthMessage)
            .Ensure(
                name => name.MiddleName.HasNoValue ||
                        name.MiddleName.Value.Value.Length <= MaximumLength,
                InvalidLengthMessage)
            .Map(name => new PersonName(
                name.LastName,
                name.FirstName,
                name.MiddleName));

    public Result<PersonName> ReplaceLastName(NonEmptyString lastName) =>
        Create(lastName, FirstName, MiddleName);

    public Result<PersonName> ReplaceFirstName(NonEmptyString firstName) =>
        Create(LastName, firstName, MiddleName);

    public Result<PersonName> AddOrReplaceMiddleName(NonEmptyString middleName) =>
        Create(LastName, FirstName, middleName);

    public Result<PersonName> RemoveMiddleName() =>
        Create(LastName, FirstName);

    public string LastFirstMiddle =>
        MiddleName.HasNoValue
            ? $"{LastName}, {FirstName}"
            : $"{LastName}, {FirstName} {MiddleName.Value}";

    public string LastFirstMiddleInitial =>
        MiddleName.HasNoValue
            ? $"{LastName}, {FirstName}"
            : $"{LastName}, {FirstName} {MiddleName.Value.Value[0]}.";

    public string FirstMiddleLast =>
        MiddleName.HasNoValue
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName.Value} {LastName}";

    public override string ToString() => LastFirstMiddleInitial;
}