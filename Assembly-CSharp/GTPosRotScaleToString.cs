using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GTPosRotScaleToString
{
	public static string ToString(Vector3 pos, Vector3 rot, Vector3 scale, bool isWorldSpace, string parentPath = null)
	{
		string text = (isWorldSpace ? "WorldPRS" : "LocalPRS");
		string text2 = string.Concat(new string[]
		{
			text,
			" { p=",
			GTPosRotScaleToString.ValToStr(pos),
			", r=",
			GTPosRotScaleToString.ValToStr(rot),
			", s=",
			GTPosRotScaleToString.ValToStr(scale)
		});
		if (!string.IsNullOrEmpty(parentPath))
		{
			text2 = text2 + " parent=\"" + parentPath + "\"";
		}
		return text2 + " }";
	}

	private static string ValToStr(Vector3 v)
	{
		return string.Format("({0:R}, {1:R}, {2:R})", v.x, v.y, v.z);
	}

	public static bool ParseIsWorldSpace(string input)
	{
		return input.Contains("WorldPRS");
	}

	public static string ParseParentPath(string input)
	{
		MatchCollection matchCollection = Regex.Matches(input, "parent\\s*=\\s*\"(?<parent>.*?)\"");
		if (matchCollection.Count <= 0)
		{
			return null;
		}
		return matchCollection[0].Groups["parent"].Value;
	}

	public static bool TryParsePos(string input, out Vector3 v)
	{
		return GTPosRotScaleToString.TryParseVec3_internal(GTRegex.k_Pos, input, out v);
	}

	public static bool TryParseRot(string input, out Vector3 v)
	{
		return GTPosRotScaleToString.TryParseVec3_internal(GTRegex.k_Rot, input, out v);
	}

	public static bool TryParseScale(string input, out Vector3 v)
	{
		return GTPosRotScaleToString.TryParseVec3_internal(GTRegex.k_Scale, input, out v) || GTPosRotScaleToString.TryParseVec3_internal(GTRegex.k_Vec3, input, out v);
	}

	public static bool TryParseVec3(string input, out Vector3 v)
	{
		return GTPosRotScaleToString.TryParseVec3_internal(GTRegex.k_Vec3, input, out v);
	}

	private static bool TryParseVec3_internal(Regex regex, string input, out Vector3 v)
	{
		v = Vector3.zero;
		MatchCollection matchCollection = regex.Matches(input);
		if (matchCollection.Count <= 0)
		{
			return false;
		}
		v = GTPosRotScaleToString.StringToVector3(matchCollection[0]);
		return true;
	}

	private static Vector3 StringToVector3(Match match)
	{
		float num = float.Parse(match.Groups["x"].Value);
		float num2 = float.Parse(match.Groups["y"].Value);
		float num3 = float.Parse(match.Groups["z"].Value);
		return new Vector3(num, num2, num3);
	}

	public const string k_LocalPRSLabel = "LocalPRS";

	public const string k_WorldPRSLabel = "WorldPRS";
}
