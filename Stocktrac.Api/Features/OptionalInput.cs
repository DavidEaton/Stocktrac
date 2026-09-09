using CSharpFunctionalExtensions;

namespace Stocktrac.Api.Features;

/// <summary>
/// Normalizes nullable transport values before application code invokes domain APIs.
/// Keeping these conversions at the boundary prevents nullable state from leaking into
/// domain factories and mutations.
/// </summary>
public static class OptionalInput
{
    public static Maybe<string> Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Maybe<string>.None
            : value.Trim();

    public static Maybe<T> Normalize<T>(T? value) where T : struct =>
        value.HasValue ? value.Value : Maybe<T>.None;
}
