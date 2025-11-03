using System;
using UnityEngine;

namespace BoingKit
{
	public class QuaternionUtil
	{
		public static float Magnitude(Quaternion q)
		{
			return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
		}

		public static float MagnitudeSqr(Quaternion q)
		{
			return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
		}

		public static Quaternion Normalize(Quaternion q)
		{
			float num = 1f / QuaternionUtil.Magnitude(q);
			return new Quaternion(num * q.x, num * q.y, num * q.z, num * q.w);
		}

		public static Quaternion AxisAngle(Vector3 axis, float angle)
		{
			float num = 0.5f * angle;
			float num2 = Mathf.Sin(num);
			float num3 = Mathf.Cos(num);
			return new Quaternion(num2 * axis.x, num2 * axis.y, num2 * axis.z, num3);
		}

		public static Vector3 GetAxis(Quaternion q)
		{
			Vector3 vector = new Vector3(q.x, q.y, q.z);
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return Vector3.left;
			}
			return vector / magnitude;
		}

		public static float GetAngle(Quaternion q)
		{
			return 2f * Mathf.Acos(Mathf.Clamp(q.w, -1f, 1f));
		}

		public static Quaternion FromAngularVector(Vector3 v)
		{
			float magnitude = v.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return Quaternion.identity;
			}
			v /= magnitude;
			float num = 0.5f * magnitude;
			float num2 = Mathf.Sin(num);
			float num3 = Mathf.Cos(num);
			return new Quaternion(num2 * v.x, num2 * v.y, num2 * v.z, num3);
		}

		public static Vector3 ToAngularVector(Quaternion q)
		{
			Vector3 axis = QuaternionUtil.GetAxis(q);
			return QuaternionUtil.GetAngle(q) * axis;
		}

		public static Quaternion Pow(Quaternion q, float exp)
		{
			Vector3 axis = QuaternionUtil.GetAxis(q);
			float num = QuaternionUtil.GetAngle(q) * exp;
			return QuaternionUtil.AxisAngle(axis, num);
		}

		public static Quaternion Integrate(Quaternion q, Quaternion v, float dt)
		{
			return QuaternionUtil.Pow(v, dt) * q;
		}

		public static Quaternion Integrate(Quaternion q, Vector3 omega, float dt)
		{
			omega *= 0.5f;
			Quaternion quaternion = new Quaternion(omega.x, omega.y, omega.z, 0f) * q;
			return QuaternionUtil.Normalize(new Quaternion(q.x + quaternion.x * dt, q.y + quaternion.y * dt, q.z + quaternion.z * dt, q.w + quaternion.w * dt));
		}

		public static Vector4 ToVector4(Quaternion q)
		{
			return new Vector4(q.x, q.y, q.z, q.w);
		}

		public static Quaternion FromVector4(Vector4 v, bool normalize = true)
		{
			if (normalize)
			{
				float sqrMagnitude = v.sqrMagnitude;
				if (sqrMagnitude < MathUtil.Epsilon)
				{
					return Quaternion.identity;
				}
				v /= Mathf.Sqrt(sqrMagnitude);
			}
			return new Quaternion(v.x, v.y, v.z, v.w);
		}

		public static void DecomposeSwingTwist(Quaternion q, Vector3 twistAxis, out Quaternion swing, out Quaternion twist)
		{
			Vector3 vector = new Vector3(q.x, q.y, q.z);
			if (vector.sqrMagnitude < MathUtil.Epsilon)
			{
				Vector3 vector2 = q * twistAxis;
				Vector3 vector3 = Vector3.Cross(twistAxis, vector2);
				if (vector3.sqrMagnitude > MathUtil.Epsilon)
				{
					float num = Vector3.Angle(twistAxis, vector2);
					swing = Quaternion.AngleAxis(num, vector3);
				}
				else
				{
					swing = Quaternion.identity;
				}
				twist = Quaternion.AngleAxis(180f, twistAxis);
				return;
			}
			Vector3 vector4 = Vector3.Project(vector, twistAxis);
			twist = new Quaternion(vector4.x, vector4.y, vector4.z, q.w);
			twist = QuaternionUtil.Normalize(twist);
			swing = q * Quaternion.Inverse(twist);
		}

		public static Quaternion Sterp(Quaternion a, Quaternion b, Vector3 twistAxis, float t, QuaternionUtil.SterpMode mode = QuaternionUtil.SterpMode.Slerp)
		{
			Quaternion quaternion;
			Quaternion quaternion2;
			return QuaternionUtil.Sterp(a, b, twistAxis, t, out quaternion, out quaternion2, mode);
		}

		public static Quaternion Sterp(Quaternion a, Quaternion b, Vector3 twistAxis, float t, out Quaternion swing, out Quaternion twist, QuaternionUtil.SterpMode mode = QuaternionUtil.SterpMode.Slerp)
		{
			return QuaternionUtil.Sterp(a, b, twistAxis, t, t, out swing, out twist, mode);
		}

		public static Quaternion Sterp(Quaternion a, Quaternion b, Vector3 twistAxis, float tSwing, float tTwist, QuaternionUtil.SterpMode mode = QuaternionUtil.SterpMode.Slerp)
		{
			Quaternion quaternion;
			Quaternion quaternion2;
			return QuaternionUtil.Sterp(a, b, twistAxis, tSwing, tTwist, out quaternion, out quaternion2, mode);
		}

		public static Quaternion Sterp(Quaternion a, Quaternion b, Vector3 twistAxis, float tSwing, float tTwist, out Quaternion swing, out Quaternion twist, QuaternionUtil.SterpMode mode)
		{
			Quaternion quaternion;
			Quaternion quaternion2;
			QuaternionUtil.DecomposeSwingTwist(b * Quaternion.Inverse(a), twistAxis, out quaternion, out quaternion2);
			if (mode == QuaternionUtil.SterpMode.Nlerp || mode != QuaternionUtil.SterpMode.Slerp)
			{
				swing = Quaternion.Lerp(Quaternion.identity, quaternion, tSwing);
				twist = Quaternion.Lerp(Quaternion.identity, quaternion2, tTwist);
			}
			else
			{
				swing = Quaternion.Slerp(Quaternion.identity, quaternion, tSwing);
				twist = Quaternion.Slerp(Quaternion.identity, quaternion2, tTwist);
			}
			return twist * swing;
		}

		public enum SterpMode
		{
			Nlerp,
			Slerp
		}
	}
}
