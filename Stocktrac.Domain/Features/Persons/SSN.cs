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
    public const string RequiredMessage =
        "A Social Security number is required.";
    public const string InvalidFormatMessage =
        "The Social Security number must contain exactly nine digits.";

    public string Value { get; }

    public string Masked =>
        $"***-**-{Value[^SerialNumberLength..]}";

    private SSN(string value) =>
        Value = value;

    public static Result<SSN> Create(string? value)
    {
        var input = value?.Trim() ?? string.Empty;
        var normalized = Normalize(input);

        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(string.IsNullOrWhiteSpace(input), RequiredMessage),
                Result.FailureIf(!string.IsNullOrWhiteSpace(input) && normalized is null, InvalidFormatMessage))
            .Map(() => new SSN(normalized!));
    }

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

        return $"{Value[..AreaNumberLength]}-" +
               $"{Value[secondGroupStart..serialNumberStart]}-" +
               $"{Value[serialNumberStart..]}";
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
