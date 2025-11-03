using System;

public static class StringExts
{
	public static string EscapeCsv(this string field)
	{
		if (StringExts._escapeChars == null)
		{
			StringExts._escapeChars = new char[] { ',', '"', '\n', '\r' };
		}
		if (field.IndexOfAny(StringExts._escapeChars) != -1)
		{
			return field.Replace("\"", "\"\"");
		}
		return field;
	}

	private static char[] _escapeChars;
}
