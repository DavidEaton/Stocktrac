using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Api.Data;

namespace Stocktrac.Tests.Features.Unit;

public class MaybeValueConvertersShould
{
    [Fact]
    public void RoundTripPresentAndAbsentValues_On_StringConversion()
    {
        var converter = MaybeValueConverters.String;
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider("logo.png"))
            .ShouldBe(Maybe<string>.From("logo.png"));
        fromProvider(toProvider(Maybe<string>.None))
            .ShouldBe(Maybe<string>.None);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnNone_On_StringConversion_WhenStoredValueIsMissing(string? storedValue)
    {
        var fromProvider = MaybeValueConverters.String.ConvertFromProviderExpression.Compile();

        fromProvider(storedValue).ShouldBe(Maybe<string>.None);
    }

    [Fact]
    public void ReturnTrimmedValue_On_StringConversion_WhenStoredValueIsPresent()
    {
        var fromProvider = MaybeValueConverters.String.ConvertFromProviderExpression.Compile();

        fromProvider(" logo.png ").ShouldBe(Maybe<string>.From("logo.png"));
    }

    [Fact]
    public void RoundTripPresentAndAbsentValues_On_DateTimeConversion()
    {
        var converter = MaybeValueConverters.DateTime;
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();
        var value = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);

        fromProvider(toProvider(value))
            .ShouldBe(Maybe<DateTime>.From(value));
        fromProvider(toProvider(Maybe<DateTime>.None))
            .ShouldBe(Maybe<DateTime>.None);
    }

    [Fact]
    public void RoundTripPresentAndAbsentValues_On_Int32Conversion()
    {
        var converter = MaybeValueConverters.Int32;
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider(42)).ShouldBe(Maybe<int>.From(42));
        fromProvider(toProvider(Maybe<int>.None)).ShouldBe(Maybe<int>.None);
    }
}
