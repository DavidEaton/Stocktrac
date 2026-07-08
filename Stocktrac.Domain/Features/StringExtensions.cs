using System;

namespace Stocktrac.Domain.Features
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength) =>
            string.IsNullOrEmpty(value)
                ? value
                : value[..Math.Min(value.Length, maxLength)];
    }
}