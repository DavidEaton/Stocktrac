using System.Reflection;
using System.Xml;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// Provides the active ISO 4217 alphabetic codes supplied with the domain assembly.
/// </summary>
internal static class Iso4217CurrencyCodes
{
    private const string ResourceName =
        "Stocktrac.Domain.Features.Financial.ReferenceData.iso-4217-country-codes.xml";

    private static readonly IReadOnlySet<string> Codes = LoadCodes();

    public static bool Contains(string code) =>
        Codes.Contains(code);

    private static IReadOnlySet<string> LoadCodes()
    {
        var assembly = typeof(Iso4217CurrencyCodes).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded ISO 4217 resource '{ResourceName}' was not found.");
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });

        var codes = new HashSet<string>(StringComparer.Ordinal);
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "Ccy")
                continue;

            var code = reader.ReadElementContentAsString().Trim();
            if (code.Length == CurrencyCode.CodeLength &&
                code.All(char.IsAsciiLetterUpper))
            {
                codes.Add(code);
            }
        }

        if (codes.Count == 0)
            throw new InvalidDataException("The embedded ISO 4217 currency list is empty.");

        return codes;
    }
}
