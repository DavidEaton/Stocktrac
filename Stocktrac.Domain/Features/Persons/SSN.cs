using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Persons;

public sealed class SSN : ValueObject
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

    private SSN(string value)
    {
        Value = value;
    }

    public static Result<SSN> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<SSN>(RequiredMessage);

        var input = value.Trim();

        var normalized = input.Length switch
        {
            NormalizedLength when input.All(char.IsAsciiDigit) =>
                input,

            FormattedLength when IsFormatted(input) =>
                input.Replace("-", string.Empty),

            _ => null
        };

        return normalized is null
            ? Result.Failure<SSN>(InvalidFormatMessage)
            : Result.Success(new SSN(normalized));
    }

    public string ToFormattedString()
    {
        var secondGroupStart = AreaNumberLength;
        var serialNumberStart = AreaNumberLength + GroupNumberLength;

        return $"{Value[..AreaNumberLength]}-" +
               $"{Value[secondGroupStart..serialNumberStart]}-" +
               $"{Value[serialNumberStart..]}";
    }

    public override string ToString() => Masked;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

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