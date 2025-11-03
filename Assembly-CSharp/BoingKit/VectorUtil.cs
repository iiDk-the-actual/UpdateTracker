using System;
using UnityEngine;

namespace BoingKit
{
	public class VectorUtil
	{
		public static Vector3 Rotate2D(Vector3 v, float angle)
		{
			Vector3 vector = v;
			float num = Mathf.Cos(angle);
			float num2 = Mathf.Sin(angle);
			vector.x = num * v.x - num2 * v.y;
			vector.y = num2 * v.x + num * v.y;
			return vector;
		}

		public static Vector4 NormalizeSafe(Vector4 v, Vector4 fallback)
		{
			if (v.sqrMagnitude <= MathUtil.Epsilon)
			{
				return fallback;
			}
			return v.normalized;
		}

		public static Vector3 FindOrthogonal(Vector3 v)
		{
			if (v.x >= MathUtil.Sqrt3Inv)
			{
				return new Vector3(v.y, -v.x, 0f);
			}
			return new Vector3(0f, v.z, -v.y);
		}

		public static void FormOrthogonalBasis(Vector3 v, out Vector3 a, out Vector3 b)
		{
			a = VectorUtil.FindOrthogonal(v);
			b = Vector3.Cross(a, v);
		}

		public static Vector3 Slerp(Vector3 a, Vector3 b, float t)
		{
			float num = Vector3.Dot(a, b);
			if (num > 0.99999f)
			{
				return Vector3.Lerp(a, b, t);
			}
			if (num < -0.99999f)
			{
				Vector3 vector = VectorUtil.FindOrthogonal(a);
				return Quaternion.AngleAxis(180f * t, vector) * a;
			}
			float num2 = MathUtil.AcosSafe(num);
			return (Mathf.Sin((1f - t) * num2) * a + Mathf.Sin(t * num2) * b) / Mathf.Sin(num2);
		}

		public static Vector3 GetClosestPointOnSegment(Vector3 p, Vector3 segA, Vector3 segB)
		{
			Vector3 vector = segB - segA;
			if (vector.sqrMagnitude < MathUtil.Epsilon)
			{
				return 0.5f * (segA + segB);
			}
			float num = Mathf.Clamp01(Vector3.Dot(p - segA, vector.normalized) / vector.magnitude);
			return segA + num * vector;
		}

		public static Vector3 TriLerp(ref Vector3 v000, ref Vector3 v001, ref Vector3 v010, ref Vector3 v011, ref Vector3 v100, ref Vector3 v101, ref Vector3 v110, ref Vector3 v111, float tx, float ty, float tz)
		{
			Vector3 vector = Vector3.Lerp(v000, v001, tx);
			Vector3 vector2 = Vector3.Lerp(v010, v011, tx);
			Vector3 vector3 = Vector3.Lerp(v100, v101, tx);
			Vector3 vector4 = Vector3.Lerp(v110, v111, tx);
			Vector3 vector5 = Vector3.Lerp(vector, vector2, ty);
			Vector3 vector6 = Vector3.Lerp(vector3, vector4, ty);
			return Vector3.Lerp(vector5, vector6, tz);
		}

		public static Vector3 TriLerp(ref Vector3 v000, ref Vector3 v001, ref Vector3 v010, ref Vector3 v011, ref Vector3 v100, ref Vector3 v101, ref Vector3 v110, ref Vector3 v111, bool lerpX, bool lerpY, bool lerpZ, float tx, float ty, float tz)
		{
			Vector3 vector = (lerpX ? Vector3.Lerp(v000, v001, tx) : v000);
			Vector3 vector2 = (lerpX ? Vector3.Lerp(v010, v011, tx) : v010);
			Vector3 vector3 = (lerpX ? Vector3.Lerp(v100, v101, tx) : v100);
			Vector3 vector4 = (lerpX ? Vector3.Lerp(v110, v111, tx) : v110);
			Vector3 vector5 = (lerpY ? Vector3.Lerp(vector, vector2, ty) : vector);
			Vector3 vector6 = (lerpY ? Vector3.Lerp(vector3, vector4, ty) : vector3);
			if (!lerpZ)
			{
				return vector5;
			}
			return Vector3.Lerp(vector5, vector6, tz);
		}

		public static Vector3 TriLerp(ref Vector3 min, ref Vector3 max, bool lerpX, bool lerpY, bool lerpZ, float tx, float ty, float tz)
		{
			Vector3 vector = (lerpX ? Vector3.Lerp(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), tx) : new Vector3(min.x, min.y, min.z));
			Vector3 vector2 = (lerpX ? Vector3.Lerp(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), tx) : new Vector3(min.x, max.y, min.z));
			Vector3 vector3 = (lerpX ? Vector3.Lerp(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), tx) : new Vector3(min.x, min.y, max.z));
			Vector3 vector4 = (lerpX ? Vector3.Lerp(new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z), tx) : new Vector3(min.x, max.y, max.z));
			Vector3 vector5 = (lerpY ? Vector3.Lerp(vector, vector2, ty) : vector);
			Vector3 vector6 = (lerpY ? Vector3.Lerp(vector3, vector4, ty) : vector3);
			if (!lerpZ)
			{
				return vector5;
			}
			return Vector3.Lerp(vector5, vector6, tz);
		}

		public static Vector4 TriLerp(ref Vector4 v000, ref Vector4 v001, ref Vector4 v010, ref Vector4 v011, ref Vector4 v100, ref Vector4 v101, ref Vector4 v110, ref Vector4 v111, bool lerpX, bool lerpY, bool lerpZ, float tx, float ty, float tz)
		{
			Vector4 vector = (lerpX ? Vector4.Lerp(v000, v001, tx) : v000);
			Vector4 vector2 = (lerpX ? Vector4.Lerp(v010, v011, tx) : v010);
			Vector4 vector3 = (lerpX ? Vector4.Lerp(v100, v101, tx) : v100);
			Vector4 vector4 = (lerpX ? Vector4.Lerp(v110, v111, tx) : v110);
			Vector4 vector5 = (lerpY ? Vector4.Lerp(vector, vector2, ty) : vector);
			Vector4 vector6 = (lerpY ? Vector4.Lerp(vector3, vector4, ty) : vector3);
			if (!lerpZ)
			{
				return vector5;
			}
			return Vector4.Lerp(vector5, vector6, tz);
		}

		public static Vector4 TriLerp(ref Vector4 min, ref Vector4 max, bool lerpX, bool lerpY, bool lerpZ, float tx, float ty, float tz)
		{
			Vector4 vector = (lerpX ? Vector4.Lerp(new Vector4(min.x, min.y, min.z), new Vector4(max.x, min.y, min.z), tx) : new Vector4(min.x, min.y, min.z));
			Vector4 vector2 = (lerpX ? Vector4.Lerp(new Vector4(min.x, max.y, min.z), new Vector4(max.x, max.y, min.z), tx) : new Vector4(min.x, max.y, min.z));
			Vector4 vector3 = (lerpX ? Vector4.Lerp(new Vector4(min.x, min.y, max.z), new Vector4(max.x, min.y, max.z), tx) : new Vector4(min.x, min.y, max.z));
			Vector4 vector4 = (lerpX ? Vector4.Lerp(new Vector4(min.x, max.y, max.z), new Vector4(max.x, max.y, max.z), tx) : new Vector4(min.x, max.y, max.z));
			Vector4 vector5 = (lerpY ? Vector4.Lerp(vector, vector2, ty) : vector);
			Vector4 vector6 = (lerpY ? Vector4.Lerp(vector3, vector4, ty) : vector3);
			if (!lerpZ)
			{
				return vector5;
			}
			return Vector4.Lerp(vector5, vector6, tz);
		}

		public static Vector3 ClampLength(Vector3 v, float minLen, float maxLen)
		{
			float sqrMagnitude = v.sqrMagnitude;
			if (sqrMagnitude < MathUtil.Epsilon)
			{
				return v;
			}
			float num = Mathf.Sqrt(sqrMagnitude);
			return v * (Mathf.Clamp(num, minLen, maxLen) / num);
		}

		public static float MinComponent(Vector3 v)
		{
			return Mathf.Min(v.x, Mathf.Min(v.y, v.z));
		}

		public static float MaxComponent(Vector3 v)
		{
			return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
		}

		public static Vector3 ComponentWiseAbs(Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		public static Vector3 ComponentWiseMult(Vector3 a, Vector3 b)
		{
			return Vector3.Scale(a, b);
		}

		public static Vector3 ComponentWiseDiv(Vector3 num, Vector3 den)
		{
			return new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);
		}

		public static Vector3 ComponentWiseDivSafe(Vector3 num, Vector3 den)
		{
			return new Vector3(num.x * MathUtil.InvSafe(den.x), num.y * MathUtil.InvSafe(den.y), num.z * MathUtil.InvSafe(den.z));
		}

		public static Vector3 ClampBend(Vector3 vector, Vector3 reference, float maxBendAngle)
		{
			float sqrMagnitude = vector.sqrMagnitude;
			if (sqrMagnitude < MathUtil.Epsilon)
			{
				return vector;
			}
			float sqrMagnitude2 = reference.sqrMagnitude;
			if (sqrMagnitude2 < MathUtil.Epsilon)
			{
				return vector;
			}
			Vector3 vector2 = vector / Mathf.Sqrt(sqrMagnitude);
			Vector3 vector3 = reference / Mathf.Sqrt(sqrMagnitude2);
			Vector3 vector4 = Vector3.Cross(vector3, vector2);
			float num = Vector3.Dot(vector3, vector2);
			Vector3 vector5 = ((vector4.sqrMagnitude > MathUtil.Epsilon) ? vector4.normalized : VectorUtil.FindOrthogonal(vector3));
			if (Mathf.Acos(Mathf.Clamp01(num)) <= maxBendAngle)
			{
				return vector;
			}
			return QuaternionUtil.AxisAngle(vector5, maxBendAngle) * reference * (Mathf.Sqrt(sqrMagnitude) / Mathf.Sqrt(sqrMagnitude2));
		}

		public static readonly Vector3 Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		public static readonly Vector3 Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	}
}
