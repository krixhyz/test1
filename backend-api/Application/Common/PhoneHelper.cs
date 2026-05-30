using System.Text.RegularExpressions;

namespace WeatherAPI.Application.Common;

public static class PhoneHelper {
    private static readonly Regex _cleanRegex = new(@"[^\d+]", RegexOptions.Compiled);
    private static readonly Regex _nepalPhoneRegex = new(@"^\d{10}$", RegexOptions.Compiled);

    public static bool IsNepalPhoneNumber(string? phone) {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        var normalized = NormalizePhoneNumber(phone);
        return _nepalPhoneRegex.IsMatch(normalized);
    }

    public static string NormalizePhoneNumber(string phone) {
        var cleaned = _cleanRegex.Replace(phone.Trim(), "");
        if (cleaned.StartsWith("++")) cleaned = cleaned[1..];
        if (cleaned.StartsWith("0") && cleaned.Length == 11) cleaned = cleaned[1..];
        if (cleaned.StartsWith("977") && cleaned.Length == 13) cleaned = cleaned[3..];
        return cleaned;
    }
}
