namespace NadMatcher.Domain.Entities;

/// <summary>
/// Maps RF band identifiers to their frequency ranges (MHz).
/// Covers 5G NR, LTE (FDD+TDD), UMTS, and GSM bands.
/// </summary>
public static class BandFrequencyMap
{
    private static readonly Dictionary<string, string> Frequencies = new(StringComparer.OrdinalIgnoreCase)
    {
        // 5G NR bands
        ["n1"] = "2100 MHz",
        ["n2"] = "1900 MHz",
        ["n3"] = "1800 MHz",
        ["n5"] = "850 MHz",
        ["n7"] = "2600 MHz",
        ["n8"] = "900 MHz",
        ["n12"] = "700 MHz",
        ["n13"] = "700 MHz",
        ["n14"] = "700 MHz",
        ["n18"] = "850 MHz",
        ["n20"] = "800 MHz",
        ["n25"] = "1900 MHz",
        ["n26"] = "850 MHz",
        ["n28"] = "700 MHz",
        ["n29"] = "700 MHz",
        ["n30"] = "2300 MHz",
        ["n38"] = "2600 MHz TDD",
        ["n40"] = "2300 MHz TDD",
        ["n41"] = "2500 MHz TDD",
        ["n48"] = "3500 MHz TDD",
        ["n53"] = "2400 MHz TDD",
        ["n66"] = "1700/2100 MHz",
        ["n70"] = "1700 MHz",
        ["n71"] = "600 MHz",
        ["n77"] = "3300-4200 MHz",
        ["n78"] = "3300-3800 MHz",
        ["n79"] = "4400-5000 MHz",

        // LTE FDD bands
        ["B1"] = "2100 MHz",
        ["B2"] = "1900 MHz",
        ["B3"] = "1800 MHz",
        ["B4"] = "1700/2100 MHz (AWS)",
        ["B5"] = "850 MHz",
        ["B7"] = "2600 MHz",
        ["B8"] = "900 MHz",
        ["B11"] = "1500 MHz",
        ["B12"] = "700 MHz",
        ["B13"] = "700 MHz",
        ["B14"] = "700 MHz",
        ["B17"] = "700 MHz",
        ["B18"] = "850 MHz",
        ["B19"] = "850 MHz",
        ["B20"] = "800 MHz",
        ["B21"] = "1500 MHz",
        ["B25"] = "1900 MHz",
        ["B26"] = "850 MHz",
        ["B28"] = "700 MHz",
        ["B29"] = "700 MHz (SDL)",
        ["B30"] = "2300 MHz",
        ["B32"] = "1500 MHz (SDL)",
        ["B66"] = "1700/2100 MHz (AWS)",
        ["B71"] = "600 MHz",
        ["B85"] = "700 MHz",

        // LTE TDD bands
        ["B34"] = "2000 MHz TDD",
        ["B38"] = "2600 MHz TDD",
        ["B39"] = "1900 MHz TDD",
        ["B40"] = "2300 MHz TDD",
        ["B41"] = "2500 MHz TDD",
        ["B42"] = "3500 MHz TDD",
        ["B43"] = "3700 MHz TDD",
        ["B48"] = "3500 MHz TDD",

        // UMTS bands
        ["B1_UMTS"] = "2100 MHz",
        ["B2_UMTS"] = "1900 MHz",
        ["B4_UMTS"] = "1700/2100 MHz",
        ["B5_UMTS"] = "850 MHz",
        ["B6_UMTS"] = "800 MHz",
        ["B8_UMTS"] = "900 MHz",
        ["B19_UMTS"] = "850 MHz",

        // GSM bands
        ["GSM850"] = "850 MHz",
        ["GSM900"] = "900 MHz",
        ["EGSM900"] = "900 MHz",
        ["DCS1800"] = "1800 MHz",
        ["PCS1900"] = "1900 MHz",
        ["GSM1800"] = "1800 MHz",
        ["GSM1900"] = "1900 MHz",
    };

    /// <summary>
    /// Gets the frequency description for a band identifier.
    /// </summary>
    public static string? GetFrequency(string bandId)
    {
        // Direct lookup
        if (Frequencies.TryGetValue(bandId, out var freq))
            return freq;

        // Try without leading 'B' or 'n' for UMTS bands (e.g., "1" -> "B1_UMTS")
        // Not needed since UMTS bands in our data use same format as LTE (B1, B2, etc.)

        return null;
    }

    /// <summary>
    /// Formats a band with its frequency, e.g., "B2 (1900 MHz)" or "n77 (3300-4200 MHz)".
    /// Returns just the band name if frequency is unknown.
    /// </summary>
    public static string FormatBandWithFrequency(string bandId)
    {
        var freq = GetFrequency(bandId);
        return freq != null ? $"{bandId} ({freq})" : bandId;
    }

    /// <summary>
    /// Formats a list of bands with their frequencies as a comma-separated string.
    /// </summary>
    public static string FormatBandsWithFrequencies(IEnumerable<string> bands)
    {
        var formatted = bands.Select(FormatBandWithFrequency);
        return string.Join(", ", formatted);
    }
}
