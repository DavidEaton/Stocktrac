using Stocktrac.Domain.Features.Contact;

namespace Stocktrac.Domain.Features;

public static class DateExtensions
{
    public static bool InRange(this DateTime date, DateTimeRange range) =>
        date >= range.Start && date <= range.End;
}