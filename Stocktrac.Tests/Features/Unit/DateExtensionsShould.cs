using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit;

public class DateExtensionsShould
{
    private static readonly DateOnly Start = new(2025, 1, 15);
    private static readonly DateTimeRange Range =
        DateTimeRange.Create(Start, Start.AddDays(2)).Value;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReturnTrue_On_InRange_WhenDateIsWithinInclusiveBoundaries(int daysFromStart) =>
        Start.AddDays(daysFromStart).InRange(Range).ShouldBeTrue();

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void ReturnFalse_On_InRange_WhenDateIsOutsideBoundaries(int daysFromStart) =>
        Start.AddDays(daysFromStart).InRange(Range).ShouldBeFalse();

    [Fact]
    public void ReturnFalse_On_InRange_WhenRangeIsNull() =>
        Start.InRange(null!).ShouldBeFalse();
}
