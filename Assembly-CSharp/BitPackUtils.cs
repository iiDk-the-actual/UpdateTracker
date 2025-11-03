using System;
using UnityEngine;

public static class BitPackUtils
{
	public static ushort PackRelativePos16(Vector3 pos, Vector3 center, float radius)
	{
		Vector3 vector = pos - center;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude.Approx0(0.001f))
		{
			return 0;
		}
		float num = Mathf.Sqrt(sqrMagnitude);
		float num2 = num.SafeDivide(radius, 1E-06f);
		if (num2 == 0f)
		{
			return 0;
		}
		vector.x /= num;
		vector.y /= num;
		vector.z /= num;
		vector *= Mathf.Clamp(num2, -1f, 1f);
		float x = vector.x;
		float y = vector.y;
		float z = vector.z;
		float num3 = float.MaxValue;
		float num4 = float.MaxValue;
		float num5 = float.MaxValue;
		uint num6 = 0U;
		uint num7 = 0U;
		uint num8 = 0U;
		for (uint num9 = 0U; num9 < 32U; num9 += 1U)
		{
			float num10 = BitPackUtils.kRadialLogLUT[(int)num9];
			if (x.Approx1(1E-06f))
			{
				num6 = 31U;
			}
			else if (x.Approx0(1E-06f))
			{
				num6 = 16U;
			}
			else
			{
				float num11 = Math.Abs(x - num10);
				if (num11 < num3)
				{
					num3 = num11;
					num6 = num9;
				}
			}
			if (y.Approx1(1E-06f))
			{
				num7 = 31U;
			}
			else if (y.Approx0(1E-06f))
			{
				num7 = 16U;
			}
			else
			{
				float num12 = Math.Abs(y - num10);
				if (num12 < num4)
				{
					num4 = num12;
					num7 = num9;
				}
			}
			if (z.Approx1(1E-06f))
			{
				num8 = 31U;
			}
			else if (z.Approx0(1E-06f))
			{
				num8 = 16U;
			}
			else
			{
				float num13 = Math.Abs(z - num10);
				if (num13 < num5)
				{
					num5 = num13;
					num8 = num9;
				}
			}
		}
		return (ushort)((num6 << 10) | (num7 << 5) | num8);
	}

	public static Vector3 UnpackRelativePos16(ushort data, Vector3 center, float radius, bool snapToRadius = false)
	{
		if (data == 0)
		{
			return Vector3.zero;
		}
		float num = BitPackUtils.kRadialLogLUT[(int)(((uint)data >> 10) & 31U)];
		float num2 = BitPackUtils.kRadialLogLUT[(int)(((uint)data >> 5) & 31U)];
		float num3 = BitPackUtils.kRadialLogLUT[(int)(data & 31)];
		float num4 = num * radius;
		float num5 = num2 * radius;
		float num6 = num3 * radius;
		if (snapToRadius)
		{
			float num7 = num4 * num4 + num5 * num5 + num6 * num6;
			float num8 = radius * radius * 0.765625f;
			if (num7 >= num8)
			{
				float num9 = 1f / Mathf.Sqrt(num7);
				num4 *= num9 * radius;
				num5 *= num9 * radius;
				num6 *= num9 * radius;
			}
		}
		return new Vector3(center.x + num4, center.y + num5, center.z + num6);
	}

	public static uint PackRelativePos(Vector3 pos, Vector3 min, Vector3 max)
	{
		Vector3 vector = max - min;
		Vector3 vector2 = (pos - min).SafeDivide(vector);
		uint num = (uint)Mathf.Clamp(vector2.x * 1023f, 0f, 1023f);
		uint num2 = (uint)Mathf.Clamp(vector2.y * 1023f, 0f, 1023f);
		uint num3 = (uint)Mathf.Clamp(vector2.z * 1023f, 0f, 1023f);
		return (num << 20) | (num2 << 10) | num3;
	}

	public static Vector3 UnpackRelativePos(uint data, Vector3 min, Vector3 max)
	{
		Vector3 vector = max - min;
		uint num = (data >> 20) & 1023U;
		uint num2 = (data >> 10) & 1023U;
		uint num3 = data & 1023U;
		return new Vector3(num * 0.0009775171f * vector.x + min.x, num2 * 0.0009775171f * vector.y + min.y, num3 * 0.0009775171f * vector.z + min.z);
	}

	public static uint PackRotation(Quaternion q, bool normalize = true)
	{
		if (normalize)
		{
			q.Normalize();
		}
		float num = Mathf.Abs(q.x);
		float num2 = Mathf.Abs(q.y);
		float num3 = Mathf.Abs(q.z);
		float num4 = Mathf.Abs(q.w);
		int num5;
		if (num > num2 && num > num3 && num > num4)
		{
			num5 = 0;
		}
		else if (num2 > num3 && num2 > num4)
		{
			num5 = 1;
		}
		else if (num3 > num4)
		{
			num5 = 2;
		}
		else
		{
			num5 = 3;
		}
		float num6 = Mathf.Sign(q[num5]);
		Vector3 vector;
		if (num5 == 0)
		{
			vector.x = q.y * num6;
			vector.y = q.z * num6;
			vector.z = q.w * num6;
		}
		else if (num5 == 1)
		{
			vector.x = q.x * num6;
			vector.y = q.z * num6;
			vector.z = q.w * num6;
		}
		else if (num5 == 2)
		{
			vector.x = q.x * num6;
			vector.y = q.y * num6;
			vector.z = q.w * num6;
		}
		else
		{
			vector.x = q.x * num6;
			vector.y = q.y * num6;
			vector.z = q.z * num6;
		}
		uint num7 = (uint)((vector.x * 0.5f + 0.5f) * 1023f);
		uint num8 = (uint)((vector.y * 0.5f + 0.5f) * 1023f);
		uint num9 = (uint)((vector.z * 0.5f + 0.5f) * 1023f);
		return (uint)((num5 << 30) | (int)((int)num7 << 20) | (int)((int)num8 << 10) | (int)num9);
	}

	public static Quaternion UnpackRotation(uint data)
	{
		uint num = data >> 30;
		uint num2 = (data >> 20) & 1023U;
		uint num3 = (data >> 10) & 1023U;
		uint num4 = data & 1023U;
		float num5 = num2 * 0.0009775171f * 2f - 1f;
		float num6 = num3 * 0.0009775171f * 2f - 1f;
		float num7 = num4 * 0.0009775171f * 2f - 1f;
		float num8 = Mathf.Sqrt(1f - (num5 * num5 + num6 * num6 + num7 * num7));
		if (num == 0U)
		{
			return new Quaternion(num8, num5, num6, num7);
		}
		if (num == 1U)
		{
			return new Quaternion(num5, num8, num6, num7);
		}
		if (num == 2U)
		{
			return new Quaternion(num5, num6, num8, num7);
		}
		return new Quaternion(num5, num6, num7, num8);
	}

	static BitPackUtils()
	{
		for (int i = 0; i < 16; i++)
		{
			BitPackUtils.kRadialLogLUT[i + 16] = -Mathf.Log(1f - (float)i / 16f, 16f);
			BitPackUtils.kRadialLogLUT[15 - i] = -BitPackUtils.kRadialLogLUT[i + 16];
		}
	}

	public static int PackQuaternionForNetwork(Quaternion q)
	{
		q.Normalize();
		float num = Mathf.Abs(q.x);
		float num2 = Mathf.Abs(q.y);
		float num3 = Mathf.Abs(q.z);
		float num4 = Mathf.Abs(q.w);
		float num5 = num;
		BitPackUtils.QAxis qaxis = BitPackUtils.QAxis.X;
		if (num2 > num5)
		{
			num5 = num2;
			qaxis = BitPackUtils.QAxis.Y;
		}
		if (num3 > num5)
		{
			num5 = num3;
			qaxis = BitPackUtils.QAxis.Z;
		}
		if (num4 > num5)
		{
			qaxis = BitPackUtils.QAxis.W;
		}
		bool flag;
		float num6;
		float num7;
		float num8;
		switch (qaxis)
		{
		case BitPackUtils.QAxis.X:
			flag = q.x < 0f;
			num6 = q.y;
			num7 = q.z;
			num8 = q.w;
			goto IL_011A;
		case BitPackUtils.QAxis.Y:
			flag = q.y < 0f;
			num6 = q.x;
			num7 = q.z;
			num8 = q.w;
			goto IL_011A;
		case BitPackUtils.QAxis.Z:
			flag = q.z < 0f;
			num6 = q.x;
			num7 = q.y;
			num8 = q.w;
			goto IL_011A;
		}
		flag = q.w < 0f;
		num6 = q.x;
		num7 = q.y;
		num8 = q.z;
		IL_011A:
		if (flag)
		{
			num6 = -num6;
			num7 = -num7;
			num8 = -num8;
		}
		int num9 = Mathf.Clamp(Mathf.RoundToInt((num6 + 0.707107f) * 361.33145f), 0, 511);
		int num10 = Mathf.Clamp(Mathf.RoundToInt((num7 + 0.707107f) * 361.33145f), 0, 511);
		int num11 = Mathf.Clamp(Mathf.RoundToInt((num8 + 0.707107f) * 361.33145f), 0, 511);
		return (int)(num9 + (num10 << 9) + (num11 << 18) + ((int)qaxis << 27));
	}

	public static Quaternion UnpackQuaternionFromNetwork(int data)
	{
		float num = (float)(data & 511) * 0.0027675421f - 0.707107f;
		float num2 = (float)((data >> 9) & 511) * 0.0027675421f - 0.707107f;
		float num3 = (float)((data >> 18) & 511) * 0.0027675421f - 0.707107f;
		float num4 = Mathf.Sqrt(Mathf.Abs(1f - (num * num + num2 * num2 + num3 * num3)));
		switch ((data >> 27) & 3)
		{
		case 0:
			return new Quaternion(num4, num, num2, num3);
		case 1:
			return new Quaternion(num, num4, num2, num3);
		case 2:
			return new Quaternion(num, num2, num4, num3);
		}
		return new Quaternion(num, num2, num3, num4);
	}

	public static long PackHandPosRotForNetwork(Vector3 localPos, Quaternion rot)
	{
		long num = (long)Mathf.Clamp(Mathf.RoundToInt(localPos.x * 512f) + 1024, 0, 2047);
		long num2 = (long)Mathf.Clamp(Mathf.RoundToInt(localPos.y * 512f) + 1024, 0, 2047);
		long num3 = (long)Mathf.Clamp(Mathf.RoundToInt(localPos.z * 512f) + 1024, 0, 2047);
		long num4 = (long)BitPackUtils.PackQuaternionForNetwork(rot);
		return num + (num2 << 11) + (num3 << 22) + (num4 << 33);
	}

	public static void UnpackHandPosRotFromNetwork(long data, out Vector3 localPos, out Quaternion handRot)
	{
		long num = data & 2047L;
		long num2 = (data >> 11) & 2047L;
		long num3 = (data >> 22) & 2047L;
		localPos = new Vector3((float)(num - 1024L) * 0.001953125f, (float)(num2 - 1024L) * 0.001953125f, (float)(num3 - 1024L) * 0.001953125f);
		int num4 = (int)(data >> 33);
		handRot = BitPackUtils.UnpackQuaternionFromNetwork(num4);
	}

	public static long PackWorldPosForNetwork(Vector3 worldPos)
	{
		long num = (long)Mathf.Clamp(Mathf.RoundToInt(worldPos.x * 1024f) + 1048576, 0, 2097151);
		long num2 = (long)Mathf.Clamp(Mathf.RoundToInt(worldPos.y * 1024f) + 1048576, 0, 2097151);
		long num3 = (long)Mathf.Clamp(Mathf.RoundToInt(worldPos.z * 1024f) + 1048576, 0, 2097151);
		return num + (num2 << 21) + (num3 << 42);
	}

	public static Vector3 UnpackWorldPosFromNetwork(long data)
	{
		float num = (float)(data & 2097151L);
		long num2 = (data >> 21) & 2097151L;
		long num3 = (data >> 42) & 2097151L;
		return new Vector3((float)((long)num - 1048576L) * 0.0009765625f, (float)(num2 - 1048576L) * 0.0009765625f, (float)(num3 - 1048576L) * 0.0009765625f);
	}

	public static short PackColorForNetwork(Color col)
	{
		return (short)(Mathf.RoundToInt(col.r * 900f) + Mathf.RoundToInt(col.g * 90f) + Mathf.RoundToInt(col.b * 9f));
	}

	public static Color UnpackColorFromNetwork(short data)
	{
		return new Color((float)(data / 100) / 9f, (float)(data / 10 % 10) / 9f, (float)(data % 10) / 9f);
	}

	public static long PackIntsIntoLong(int value1, int value2)
	{
		return ((long)value1 & (long)((ulong)(-1))) | (((long)value2 & (long)((ulong)(-1))) << 32);
	}

	public static int UnpackValue1FromLong(long value)
	{
		return (int)(value & (long)((ulong)(-1)));
	}

	public static int UnpackValue2FromLong(long value)
	{
		return (int)((value >> 32) & (long)((ulong)(-1)));
	}

	public static void UnpackIntsFromLong(long value, out int value1, out int value2)
	{
		value1 = BitPackUtils.UnpackValue1FromLong(value);
		value2 = BitPackUtils.UnpackValue2FromLong(value);
	}

	private const float STEP_1023 = 0.0009775171f;

	private static readonly float[] kRadialLogLUT = new float[32];

	private const float QPackMax = 0.707107f;

	private const float QPackScale = 361.33145f;

	private const float QPackInvScale = 0.0027675421f;

	private enum QAxis
	{
		X,
		Y,
		Z,
		W
	}
}
