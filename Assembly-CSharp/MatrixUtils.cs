using System;
using UnityEngine;

public static class MatrixUtils
{
	public static void MultiplyXYZ3x4(ref Matrix4x4 m, ref Vector4 point)
	{
		float x = point.x;
		float y = point.y;
		float z = point.z;
		point.x = (float)((double)m.m00 * (double)x + (double)m.m01 * (double)y + (double)m.m02 * (double)z) + m.m03;
		point.y = (float)((double)m.m10 * (double)x + (double)m.m11 * (double)y + (double)m.m12 * (double)z) + m.m13;
		point.z = (float)((double)m.m20 * (double)x + (double)m.m21 * (double)y + (double)m.m22 * (double)z) + m.m23;
	}

	public static void MultiplyXYZ(ref Matrix4x4 m, ref Vector4 point)
	{
		float x = point.x;
		float y = point.y;
		float z = point.z;
		point.x = (float)((double)m.m00 * (double)x + (double)m.m01 * (double)y + (double)m.m02 * (double)z) + m.m03;
		point.y = (float)((double)m.m10 * (double)x + (double)m.m11 * (double)y + (double)m.m12 * (double)z) + m.m13;
		point.z = (float)((double)m.m20 * (double)x + (double)m.m21 * (double)y + (double)m.m22 * (double)z) + m.m23;
		float num = 1f / ((float)((double)m.m30 * (double)point.x + (double)m.m31 * (double)point.y + (double)m.m32 * (double)point.z) + m.m33);
		point.x *= num;
		point.y *= num;
		point.z *= num;
	}

	public static void Clear(ref Matrix4x4 m)
	{
		m.m00 = 0f;
		m.m01 = 0f;
		m.m02 = 0f;
		m.m03 = 0f;
		m.m10 = 0f;
		m.m11 = 0f;
		m.m12 = 0f;
		m.m13 = 0f;
		m.m20 = 0f;
		m.m21 = 0f;
		m.m22 = 0f;
		m.m23 = 0f;
		m.m30 = 0f;
		m.m31 = 0f;
		m.m32 = 0f;
		m.m33 = 0f;
	}

	public static void Copy(ref Matrix4x4 from, ref Matrix4x4 to)
	{
		to.m00 = from.m00;
		to.m01 = from.m01;
		to.m02 = from.m02;
		to.m03 = from.m03;
		to.m10 = from.m10;
		to.m11 = from.m11;
		to.m12 = from.m12;
		to.m13 = from.m13;
		to.m20 = from.m20;
		to.m21 = from.m21;
		to.m22 = from.m22;
		to.m23 = from.m23;
		to.m30 = from.m30;
		to.m31 = from.m31;
		to.m32 = from.m32;
		to.m33 = from.m33;
	}
}
