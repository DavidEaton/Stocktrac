using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features
{
    public static class NonEmptyStringConstruction
    {
        public const string RequiredMessage = "Value is required.";
        extension(NonEmptyString)
        {
            public static Result<NonEmptyString> Create(string value) =>
                Result.Success(value)
                    .Ensure(text => !string.IsNullOrWhiteSpace(text), RequiredMessage)
                    .Map(text => new NonEmptyString(text.Trim()));
        }
    }
}