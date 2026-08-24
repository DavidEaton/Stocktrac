using System.Xml;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// Provides the active ISO 4217 alphabetic codes supplied with the domain assembly.
/// </summary>
public static class Iso4217CountryCurrencyCodes
{
    private const string ResourceName =
        "Stocktrac.Domain.Features.Financial.ReferenceData.iso-4217-country-currency-codes.xml";

    private static readonly HashSet<string> Codes = LoadCodes();

    public static bool Contains(string code) =>
        Codes.Contains(code);

    private static HashSet<string> LoadCodes() =>
        ReadCodes(CreateXmlReader(GetStream()));

    private static HashSet<string> ReadCodes(XmlReader reader) =>
        ReadNodes(reader).Aggregate(
            new HashSet<string>(StringComparer.Ordinal),
            AddCurrencyCodeIfPresent);

    private static IEnumerable<XmlReader> ReadNodes(XmlReader reader)
    {
        while (reader.Read())
            yield return reader;
    }

    private static Stream GetStream() =>
        typeof(Iso4217CountryCurrencyCodes).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded ISO 4217 resource '{ResourceName}' was not found.");

    private static XmlReader CreateXmlReader(Stream stream) =>
        XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });

    private static HashSet<string> AddCurrencyCodeIfPresent(HashSet<string> codes, XmlReader reader) =>
        reader.NodeType == XmlNodeType.Element && reader.LocalName == "Ccy"
            ? AddCodeIfValid(codes, reader.ReadElementContentAsString().Trim())
            : codes;

    private static HashSet<string> AddCodeIfValid(HashSet<string> codes, string code) =>
        code.Length == CurrencyCode.CodeLength && code.All(char.IsAsciiLetterUpper)
            ? codes.Append(code).ToHashSet(codes.Comparer)
            : codes;
}
