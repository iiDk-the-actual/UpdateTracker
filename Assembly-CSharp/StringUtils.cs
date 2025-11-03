using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Text;
using UnityEngine;

public static class StringUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty(this string s)
	{
		return string.IsNullOrEmpty(s);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrWhiteSpace(this string s)
	{
		return string.IsNullOrWhiteSpace(s);
	}

	public static string ToAlphaNumeric(this string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return string.Empty;
		}
		string text;
		using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder())
		{
			foreach (char c in s)
			{
				if (char.IsLetterOrDigit(c))
				{
					utf16ValueStringBuilder.Append(c);
				}
			}
			text = utf16ValueStringBuilder.ToString();
		}
		return text;
	}

	public static string Capitalize(this string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return s;
		}
		char[] array = s.ToCharArray();
		array[0] = char.ToUpperInvariant(array[0]);
		return new string(array);
	}

	public static string Concat(this IEnumerable<string> source)
	{
		return string.Concat(source);
	}

	public static string Join(this IEnumerable<string> source, string separator)
	{
		return string.Join(separator, source);
	}

	public static string Join(this IEnumerable<string> source, char separator)
	{
		return string.Join<string>(separator, source);
	}

	public static string RemoveAll(this string s, string value, StringComparison mode = StringComparison.OrdinalIgnoreCase)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		return s.Replace(value, string.Empty, mode);
	}

	public static string RemoveAll(this string s, char value, StringComparison mode = StringComparison.OrdinalIgnoreCase)
	{
		return s.RemoveAll(value.ToString(), mode);
	}

	public static byte[] ToBytesASCII(this string s)
	{
		return Encoding.ASCII.GetBytes(s);
	}

	public static byte[] ToBytesUTF8(this string s)
	{
		return Encoding.UTF8.GetBytes(s);
	}

	public static byte[] ToBytesUnicode(this string s)
	{
		return Encoding.Unicode.GetBytes(s);
	}

	public static string ComputeSHV2(this string s)
	{
		return Hash128.Compute(s).ToString();
	}

	public static string ToQueryString(this Dictionary<string, string> d)
	{
		if (d == null)
		{
			return null;
		}
		return "?" + string.Join("&", d.Select((KeyValuePair<string, string> x) => x.Key + "=" + x.Value));
	}

	public static string Combine(string separator, params string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = !string.IsNullOrEmpty(separator);
		for (int i = 0; i < values.Length; i++)
		{
			if (flag)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(values);
		}
		return stringBuilder.ToString();
	}

	public static string ToUpperCamelCase(this string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		string[] array = Regex.Split(input, "[^A-Za-z0-9]+");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length > 0)
			{
				string[] array2 = array;
				int num = i;
				string text = char.ToUpper(array[i][0]).ToString();
				string text2;
				if (array[i].Length <= 1)
				{
					text2 = "";
				}
				else
				{
					string text3 = array[i];
					text2 = text3.Substring(1, text3.Length - 1).ToLower();
				}
				array2[num] = text + text2;
			}
		}
		return string.Join("", array);
	}

	public static string ToUpperCaseFromCamelCase(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		input = input.Trim();
		string text;
		using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder())
		{
			bool flag = true;
			foreach (char c in input)
			{
				if (char.IsUpper(c) && !flag)
				{
					utf16ValueStringBuilder.Append(' ');
				}
				utf16ValueStringBuilder.Append(char.ToUpper(c));
				flag = char.IsUpper(c);
			}
			text = utf16ValueStringBuilder.ToString().Trim();
		}
		return text;
	}

	public static string RemoveStart(this string s, string value, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
	{
		if (string.IsNullOrEmpty(s) || !s.StartsWith(value, comparison))
		{
			return s;
		}
		return s.Substring(value.Length);
	}

	public static string RemoveEnd(this string s, string value, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
	{
		if (string.IsNullOrEmpty(s) || !s.EndsWith(value, comparison))
		{
			return s;
		}
		return s.Substring(0, s.Length - value.Length);
	}

	public static string RemoveBothEnds(this string s, string value, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
	{
		return s.RemoveEnd(value, comparison).RemoveStart(value, comparison);
	}

	public static string TrailingSpace(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			Debug.LogError("[STRING::UTILS] Trying to add Space, but string is null or empty");
			return s;
		}
		if (s[s.Length - 1] == ' ')
		{
			return s;
		}
		return s + " ";
	}

	public const string kForwardSlash = "/";

	public const string kBackSlash = "/";

	public const string kBackTick = "`";

	public const string kMinusDash = "-";

	public const string kPeriod = ".";

	public const string kUnderScore = "_";

	public const string kColon = ":";
}
