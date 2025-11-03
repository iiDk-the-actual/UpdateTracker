using System;
using System.Diagnostics;
using UnityEngine;

[Serializable]
public struct Arc
{
	public Vector3[] GetArcPoints(int count = 12)
	{
		return Arc.ComputeArcPoints(this.start, this.end, new Vector3?(this.control), count);
	}

	[Conditional("UNITY_EDITOR")]
	public void DrawGizmo()
	{
	}

	public static Arc From(Vector3 start, Vector3 end)
	{
		Vector3 vector = Arc.DeriveArcControlPoint(start, end, null, null);
		return new Arc
		{
			start = start,
			end = end,
			control = vector
		};
	}

	public static Vector3[] ComputeArcPoints(Vector3 a, Vector3 b, Vector3? c = null, int count = 12)
	{
		Vector3[] array = new Vector3[count];
		float num = 1f / (float)count;
		Vector3 vector = c.GetValueOrDefault();
		if (c == null)
		{
			vector = Arc.DeriveArcControlPoint(a, b, null, null);
			c = new Vector3?(vector);
		}
		for (int i = 0; i < count; i++)
		{
			float num2;
			if (i == 0)
			{
				num2 = 0f;
			}
			else if (i == count - 1)
			{
				num2 = 1f;
			}
			else
			{
				num2 = num * (float)i;
			}
			array[i] = Arc.BezierLerp(a, b, c.Value, num2);
		}
		return array;
	}

	public static Vector3 BezierLerp(Vector3 a, Vector3 b, Vector3 c, float t)
	{
		Vector3 vector = Vector3.Lerp(a, c, t);
		Vector3 vector2 = Vector3.Lerp(c, b, t);
		return Vector3.Lerp(vector, vector2, t);
	}

	public static Vector3 DeriveArcControlPoint(Vector3 a, Vector3 b, Vector3? dir = null, float? height = null)
	{
		Vector3 vector = (b - a) * 0.5f;
		Vector3 normalized = vector.normalized;
		float num = height.GetValueOrDefault();
		if (height == null)
		{
			num = vector.magnitude;
			height = new float?(num);
		}
		if (dir == null)
		{
			Vector3 vector2 = Vector3.Cross(normalized, Vector3.up);
			dir = new Vector3?(Vector3.Cross(normalized, vector2));
		}
		Vector3 vector3 = dir.Value * -height.Value;
		return a + vector + vector3;
	}

	public Vector3 start;

	public Vector3 end;

	public Vector3 control;
}
