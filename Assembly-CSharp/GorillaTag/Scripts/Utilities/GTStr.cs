using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaTag.Scripts.Utilities
{
	public static class GTStr
	{
		public static void Bullet(StringBuilder builder, IList<string> strings, string bulletStr = "- ")
		{
			for (int i = 0; i < strings.Count; i++)
			{
				builder.Append(bulletStr).Append(strings[i]).Append("\n");
			}
		}

		public static string Bullet(IList<string> strings, string bulletStr = "- ")
		{
			if (strings == null || strings.Count == 0)
			{
				return string.Empty;
			}
			int num = strings.Count * (bulletStr.Length + 1);
			for (int i = 0; i < strings.Count; i++)
			{
				num += strings[i].Length;
			}
			StringBuilder stringBuilder = new StringBuilder(num);
			GTStr.Bullet(stringBuilder, strings, bulletStr);
			return stringBuilder.ToString();
		}
	}
}
