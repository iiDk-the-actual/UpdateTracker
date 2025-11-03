using System;
using UnityEngine;

namespace CjLib
{
	public class MathUtil
	{
		public static float AsinSafe(float x)
		{
			return Mathf.Asin(Mathf.Clamp(x, -1f, 1f));
		}

		public static float AcosSafe(float x)
		{
			return Mathf.Acos(Mathf.Clamp(x, -1f, 1f));
		}

		public static float CatmullRom(float p0, float p1, float p2, float p3, float t)
		{
			float num = t * t;
			return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * num + (-p0 + 3f * p1 - 3f * p2 + p3) * num * t);
		}

		public static readonly float Pi = 3.1415927f;

		public static readonly float TwoPi = 6.2831855f;

		public static readonly float HalfPi = 1.5707964f;

		public static readonly float ThirdPi = 1.0471976f;

		public static readonly float QuarterPi = 0.7853982f;

		public static readonly float FifthPi = 0.62831855f;

		public static readonly float SixthPi = 0.5235988f;

		public static readonly float Sqrt2 = Mathf.Sqrt(2f);

		public static readonly float Sqrt2Inv = 1f / Mathf.Sqrt(2f);

		public static readonly float Sqrt3 = Mathf.Sqrt(3f);

		public static readonly float Sqrt3Inv = 1f / Mathf.Sqrt(3f);

		public static readonly float Epsilon = 1E-09f;

		public static readonly float EpsilonComp = 1f - MathUtil.Epsilon;

		public static readonly float Rad2Deg = 57.295776f;

		public static readonly float Deg2Rad = 0.017453292f;
	}
}
