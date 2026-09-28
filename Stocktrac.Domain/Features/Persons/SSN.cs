using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public sealed record SSN
{
    private const int AreaNumberLength = 3;
    private const int GroupNumberLength = 2;
    private const int SerialNumberLength = 4;
    private const int NormalizedLength =
        AreaNumberLength + GroupNumberLength + SerialNumberLength;
    private const int FirstHyphenIndex = AreaNumberLength;
    private const int SecondHyphenIndex =
        AreaNumberLength + GroupNumberLength + 1;
    private const int FormattedLength = NormalizedLength + 2;
    public const string InvalidFormatMessage =
        "The Social Security number must contain exactly nine digits.";

    public NonEmptyString Value { get; }

    public string Masked =>
        $"***-**-{Value.Value[^SerialNumberLength..]}";

    private SSN(NonEmptyString value) =>
        Value = value;

    public static Result<SSN> Create(NonEmptyString value) =>
        Result.Success(value)
            .Ensure(input => Normalize(input.Value) is not null, InvalidFormatMessage)
            .Map(input => NonEmptyString.Create(Normalize(input.Value)!).Value)
            .Map(normalized => new SSN(normalized));

    private static string? Normalize(string value) =>
        value.Length switch
        {
            NormalizedLength when value.All(char.IsAsciiDigit) => value,
            FormattedLength when IsFormatted(value) => value.Replace("-", string.Empty),
            _ => null
        };

    public string ToFormattedString()
    {
        var secondGroupStart = AreaNumberLength;
        var serialNumberStart = AreaNumberLength + GroupNumberLength;

        return $"{Value.Value[..AreaNumberLength]}-" +
               $"{Value.Value[secondGroupStart..serialNumberStart]}-" +
               $"{Value.Value[serialNumberStart..]}";
    }

    public override string ToString() => Masked;

    private static bool IsFormatted(string value)
    {
        return value[FirstHyphenIndex] == '-' &&
               value[SecondHyphenIndex] == '-' &&
               value.Where((_, index) =>
                       index != FirstHyphenIndex &&
                       index != SecondHyphenIndex)
                    .All(char.IsAsciiDigit);
    }
}
