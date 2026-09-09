using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Stocktrac.Api.Data;

/// <summary>
/// EF Core converters for optional primitive values. Domain configuration should use
/// these converters whenever a <see cref="Maybe{T}"/> property is stored in a nullable
/// database column.
/// </summary>
public static class MaybeValueConverters
{
    public static readonly ValueConverter<Maybe<string>, string?> String = new(
        optional => optional.HasValue ? optional.Value : null,
        stored => stored == null ? Maybe<string>.None : Maybe<string>.From(stored));

    public static readonly ValueConverter<Maybe<int>, int?> Int32 = new(
        optional => optional.HasValue ? optional.Value : null,
        stored => stored.HasValue ? Maybe<int>.From(stored.Value) : Maybe<int>.None);

    public static readonly ValueConverter<Maybe<System.DateTime>, System.DateTime?> DateTime = new(
        optional => optional.HasValue ? optional.Value : null,
        stored => stored.HasValue ? Maybe<System.DateTime>.From(stored.Value) : Maybe<System.DateTime>.None);
}
