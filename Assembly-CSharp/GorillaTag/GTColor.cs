using System;
using UnityEngine;

namespace GorillaTag
{
	public static class GTColor
	{
		public static Color RandomHSV(GTColor.HSVRanges ranges)
		{
			return Color.HSVToRGB(Random.Range(ranges.h.x, ranges.h.y), Random.Range(ranges.s.x, ranges.s.y), Random.Range(ranges.v.x, ranges.v.y));
		}

		[Serializable]
		public struct HSVRanges
		{
			public HSVRanges(float hMin = 0f, float hMax = 1f, float sMin = 0f, float sMax = 1f, float vMin = 0f, float vMax = 1f)
			{
				this.h = new Vector2(hMin, hMax);
				this.s = new Vector2(sMin, sMax);
				this.v = new Vector2(vMin, vMax);
			}

			public Vector2 h;

			public Vector2 s;

			public Vector2 v;
		}
	}
}
