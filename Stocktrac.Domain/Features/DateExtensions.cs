using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features;

public static class DateExtensions
{
    public static bool InRange(this DateOnly date, DateRange range) =>
        range is not null && date >= range.Start && date <= range.End;
}
