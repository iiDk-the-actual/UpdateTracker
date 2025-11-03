using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct m4x4
{
	public m4x4(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
	{
		this = default(m4x4);
		this.m00 = m00;
		this.m01 = m01;
		this.m02 = m02;
		this.m03 = m03;
		this.m10 = m10;
		this.m11 = m11;
		this.m12 = m12;
		this.m13 = m13;
		this.m20 = m20;
		this.m21 = m21;
		this.m22 = m22;
		this.m23 = m23;
		this.m30 = m30;
		this.m31 = m31;
		this.m32 = m32;
		this.m33 = m33;
	}

	public m4x4(Vector4 row0, Vector4 row1, Vector4 row2, Vector4 row3)
	{
		this = default(m4x4);
		this.r0 = row0;
		this.r1 = row1;
		this.r2 = row2;
		this.r3 = row3;
	}

	public void Clear()
	{
		this.m00 = 0f;
		this.m01 = 0f;
		this.m02 = 0f;
		this.m03 = 0f;
		this.m10 = 0f;
		this.m11 = 0f;
		this.m12 = 0f;
		this.m13 = 0f;
		this.m20 = 0f;
		this.m21 = 0f;
		this.m22 = 0f;
		this.m23 = 0f;
		this.m30 = 0f;
		this.m31 = 0f;
		this.m32 = 0f;
		this.m33 = 0f;
	}

	public void SetRow0(ref Vector4 v)
	{
		this.m00 = v.x;
		this.m01 = v.y;
		this.m02 = v.z;
		this.m03 = v.w;
	}

	public void SetRow1(ref Vector4 v)
	{
		this.m10 = v.x;
		this.m11 = v.y;
		this.m12 = v.z;
		this.m13 = v.w;
	}

	public void SetRow2(ref Vector4 v)
	{
		this.m20 = v.x;
		this.m21 = v.y;
		this.m22 = v.z;
		this.m23 = v.w;
	}

	public void SetRow3(ref Vector4 v)
	{
		this.m30 = v.x;
		this.m31 = v.y;
		this.m32 = v.z;
		this.m33 = v.w;
	}

	public void Transpose()
	{
		float num = this.m01;
		float num2 = this.m02;
		float num3 = this.m03;
		float num4 = this.m10;
		float num5 = this.m12;
		float num6 = this.m13;
		float num7 = this.m20;
		float num8 = this.m21;
		float num9 = this.m23;
		float num10 = this.m30;
		float num11 = this.m31;
		float num12 = this.m32;
		this.m01 = num4;
		this.m02 = num7;
		this.m03 = num10;
		this.m10 = num;
		this.m12 = num8;
		this.m13 = num11;
		this.m20 = num2;
		this.m21 = num5;
		this.m23 = num12;
		this.m30 = num3;
		this.m31 = num6;
		this.m32 = num9;
	}

	public void Set(ref Vector4 row0, ref Vector4 row1, ref Vector4 row2, ref Vector4 row3)
	{
		this.r0 = row0;
		this.r1 = row1;
		this.r2 = row2;
		this.r3 = row3;
	}

	public void SetTransposed(ref Vector4 row0, ref Vector4 row1, ref Vector4 row2, ref Vector4 row3)
	{
		this.m00 = row0.x;
		this.m01 = row1.x;
		this.m02 = row2.x;
		this.m03 = row3.x;
		this.m10 = row0.y;
		this.m11 = row1.y;
		this.m12 = row2.y;
		this.m13 = row3.y;
		this.m20 = row0.z;
		this.m21 = row1.z;
		this.m22 = row2.z;
		this.m23 = row3.z;
		this.m30 = row0.w;
		this.m31 = row1.w;
		this.m32 = row2.w;
		this.m33 = row3.w;
	}

	public void Set(ref Matrix4x4 x)
	{
		this.m00 = x.m00;
		this.m01 = x.m01;
		this.m02 = x.m02;
		this.m03 = x.m03;
		this.m10 = x.m10;
		this.m11 = x.m11;
		this.m12 = x.m12;
		this.m13 = x.m13;
		this.m20 = x.m20;
		this.m21 = x.m21;
		this.m22 = x.m22;
		this.m23 = x.m23;
		this.m30 = x.m30;
		this.m31 = x.m31;
		this.m32 = x.m32;
		this.m33 = x.m33;
	}

	public void SetTransposed(ref Matrix4x4 x)
	{
		this.m00 = x.m00;
		this.m01 = x.m10;
		this.m02 = x.m20;
		this.m03 = x.m30;
		this.m10 = x.m01;
		this.m11 = x.m11;
		this.m12 = x.m21;
		this.m13 = x.m31;
		this.m20 = x.m02;
		this.m21 = x.m12;
		this.m22 = x.m22;
		this.m23 = x.m32;
		this.m30 = x.m03;
		this.m31 = x.m13;
		this.m32 = x.m23;
		this.m33 = x.m33;
	}

	public void Push(ref Matrix4x4 x)
	{
		x.m00 = this.m00;
		x.m01 = this.m01;
		x.m02 = this.m02;
		x.m03 = this.m03;
		x.m10 = this.m10;
		x.m11 = this.m11;
		x.m12 = this.m12;
		x.m13 = this.m13;
		x.m20 = this.m20;
		x.m21 = this.m21;
		x.m22 = this.m22;
		x.m23 = this.m23;
		x.m30 = this.m30;
		x.m31 = this.m31;
		x.m32 = this.m32;
		x.m33 = this.m33;
	}

	public void PushTransposed(ref Matrix4x4 x)
	{
		x.m00 = this.m00;
		x.m01 = this.m10;
		x.m02 = this.m20;
		x.m03 = this.m30;
		x.m10 = this.m01;
		x.m11 = this.m11;
		x.m12 = this.m21;
		x.m13 = this.m31;
		x.m20 = this.m02;
		x.m21 = this.m12;
		x.m22 = this.m22;
		x.m23 = this.m32;
		x.m30 = this.m03;
		x.m31 = this.m13;
		x.m32 = this.m23;
		x.m33 = this.m33;
	}

	public static ref m4x4 From(ref Matrix4x4 src)
	{
		return Unsafe.As<Matrix4x4, m4x4>(ref src);
	}

	[FixedBuffer(typeof(float), 16)]
	[NonSerialized]
	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public m4x4.<data_f>e__FixedBuffer data_f;

	[FixedBuffer(typeof(int), 16)]
	[NonSerialized]
	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public m4x4.<data_i>e__FixedBuffer data_i;

	[FixedBuffer(typeof(ushort), 32)]
	[NonSerialized]
	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public m4x4.<data_h>e__FixedBuffer data_h;

	[NonSerialized]
	[FieldOffset(0)]
	public Vector4 r0;

	[NonSerialized]
	[FieldOffset(16)]
	public Vector4 r1;

	[NonSerialized]
	[FieldOffset(32)]
	public Vector4 r2;

	[NonSerialized]
	[FieldOffset(48)]
	public Vector4 r3;

	[NonSerialized]
	[FieldOffset(0)]
	public float m00;

	[NonSerialized]
	[FieldOffset(4)]
	public float m01;

	[NonSerialized]
	[FieldOffset(8)]
	public float m02;

	[NonSerialized]
	[FieldOffset(12)]
	public float m03;

	[NonSerialized]
	[FieldOffset(16)]
	public float m10;

	[NonSerialized]
	[FieldOffset(20)]
	public float m11;

	[NonSerialized]
	[FieldOffset(24)]
	public float m12;

	[NonSerialized]
	[FieldOffset(28)]
	public float m13;

	[NonSerialized]
	[FieldOffset(32)]
	public float m20;

	[NonSerialized]
	[FieldOffset(36)]
	public float m21;

	[NonSerialized]
	[FieldOffset(40)]
	public float m22;

	[NonSerialized]
	[FieldOffset(44)]
	public float m23;

	[NonSerialized]
	[FieldOffset(48)]
	public float m30;

	[NonSerialized]
	[FieldOffset(52)]
	public float m31;

	[NonSerialized]
	[FieldOffset(56)]
	public float m32;

	[NonSerialized]
	[FieldOffset(60)]
	public float m33;

	[HideInInspector]
	[FieldOffset(0)]
	public int i00;

	[HideInInspector]
	[FieldOffset(4)]
	public int i01;

	[HideInInspector]
	[FieldOffset(8)]
	public int i02;

	[HideInInspector]
	[FieldOffset(12)]
	public int i03;

	[HideInInspector]
	[FieldOffset(16)]
	public int i10;

	[HideInInspector]
	[FieldOffset(20)]
	public int i11;

	[HideInInspector]
	[FieldOffset(24)]
	public int i12;

	[HideInInspector]
	[FieldOffset(28)]
	public int i13;

	[HideInInspector]
	[FieldOffset(32)]
	public int i20;

	[HideInInspector]
	[FieldOffset(36)]
	public int i21;

	[HideInInspector]
	[FieldOffset(40)]
	public int i22;

	[HideInInspector]
	[FieldOffset(44)]
	public int i23;

	[HideInInspector]
	[FieldOffset(48)]
	public int i30;

	[HideInInspector]
	[FieldOffset(52)]
	public int i31;

	[HideInInspector]
	[FieldOffset(56)]
	public int i32;

	[HideInInspector]
	[FieldOffset(60)]
	public int i33;

	[NonSerialized]
	[FieldOffset(0)]
	public ushort h00_a;

	[NonSerialized]
	[FieldOffset(2)]
	public ushort h00_b;

	[NonSerialized]
	[FieldOffset(4)]
	public ushort h01_a;

	[NonSerialized]
	[FieldOffset(6)]
	public ushort h01_b;

	[NonSerialized]
	[FieldOffset(8)]
	public ushort h02_a;

	[NonSerialized]
	[FieldOffset(10)]
	public ushort h02_b;

	[NonSerialized]
	[FieldOffset(12)]
	public ushort h03_a;

	[NonSerialized]
	[FieldOffset(14)]
	public ushort h03_b;

	[NonSerialized]
	[FieldOffset(16)]
	public ushort h10_a;

	[NonSerialized]
	[FieldOffset(18)]
	public ushort h10_b;

	[NonSerialized]
	[FieldOffset(20)]
	public ushort h11_a;

	[NonSerialized]
	[FieldOffset(22)]
	public ushort h11_b;

	[NonSerialized]
	[FieldOffset(24)]
	public ushort h12_a;

	[NonSerialized]
	[FieldOffset(26)]
	public ushort h12_b;

	[NonSerialized]
	[FieldOffset(28)]
	public ushort h13_a;

	[NonSerialized]
	[FieldOffset(30)]
	public ushort h13_b;

	[NonSerialized]
	[FieldOffset(32)]
	public ushort h20_a;

	[NonSerialized]
	[FieldOffset(34)]
	public ushort h20_b;

	[NonSerialized]
	[FieldOffset(36)]
	public ushort h21_a;

	[NonSerialized]
	[FieldOffset(38)]
	public ushort h21_b;

	[NonSerialized]
	[FieldOffset(40)]
	public ushort h22_a;

	[NonSerialized]
	[FieldOffset(42)]
	public ushort h22_b;

	[NonSerialized]
	[FieldOffset(44)]
	public ushort h23_a;

	[NonSerialized]
	[FieldOffset(46)]
	public ushort h23_b;

	[NonSerialized]
	[FieldOffset(48)]
	public ushort h30_a;

	[NonSerialized]
	[FieldOffset(50)]
	public ushort h30_b;

	[NonSerialized]
	[FieldOffset(52)]
	public ushort h31_a;

	[NonSerialized]
	[FieldOffset(54)]
	public ushort h31_b;

	[NonSerialized]
	[FieldOffset(56)]
	public ushort h32_a;

	[NonSerialized]
	[FieldOffset(58)]
	public ushort h32_b;

	[NonSerialized]
	[FieldOffset(60)]
	public ushort h33_a;

	[NonSerialized]
	[FieldOffset(62)]
	public ushort h33_b;

	[CompilerGenerated]
	[UnsafeValueType]
	[StructLayout(LayoutKind.Sequential, Size = 64)]
	public struct <data_f>e__FixedBuffer
	{
		public float FixedElementField;
	}

	[CompilerGenerated]
	[UnsafeValueType]
	[StructLayout(LayoutKind.Sequential, Size = 64)]
	public struct <data_h>e__FixedBuffer
	{
		public ushort FixedElementField;
	}

	[CompilerGenerated]
	[UnsafeValueType]
	[StructLayout(LayoutKind.Sequential, Size = 64)]
	public struct <data_i>e__FixedBuffer
	{
		public int FixedElementField;
	}
}
