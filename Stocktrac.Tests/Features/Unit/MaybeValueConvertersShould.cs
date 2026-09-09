using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Api.Data;

namespace Stocktrac.Tests.Features.Unit;

public class MaybeValueConvertersShould
{
    [Fact]
    public void RoundTripPresentAndAbsentStrings()
    {
        var converter = MaybeValueConverters.String;
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        fromProvider(toProvider("logo.png"))
            .ShouldBe(Maybe<string>.From("logo.png"));
        fromProvider(toProvider(Maybe<string>.None))
            .ShouldBe(Maybe<string>.None);
    }

    [Fact]
    public void RoundTripPresentAndAbsentDates()
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
}
