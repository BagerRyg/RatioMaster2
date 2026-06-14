using System;
using System.Text.RegularExpressions;

namespace RatioMaster;

internal static class SensitiveDataRedactor
{
	private static readonly Regex SensitiveQueryValue = new Regex(
		@"(?i)(peer_id|info_hash|key|pass|password|auth|token)=([^&\s]*)",
		RegexOptions.Compiled);

	private static readonly Regex Url = new Regex(
		@"https?://[^\s""'<>]+",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static readonly Regex PeerIdText = new Regex(
		@"(?i)PeerID\s*=\s*[^),;\s]+",
		RegexOptions.Compiled);

	public static string Sanitize(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value ?? string.Empty;
		}

		string sanitized = SensitiveQueryValue.Replace(value, "$1=[redacted]");
		sanitized = PeerIdText.Replace(sanitized, "PeerID=[redacted]");
		return Url.Replace(sanitized, match => SanitizeUrl(match.Value));
	}

	public static string TrackerEndpoint(string value)
	{
		if (Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
		{
			return uri.GetLeftPart(UriPartial.Authority);
		}
		return "[invalid tracker URL]";
	}

	private static string SanitizeUrl(string value)
	{
		string trimmed = value.TrimEnd('.', ',', ';', ')', ']');
		string suffix = value.Substring(trimmed.Length);
		return TrackerEndpoint(trimmed) + "/[redacted]" + suffix;
	}
}
