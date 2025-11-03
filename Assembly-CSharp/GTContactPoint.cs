using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
public class GTContactPoint
{
	[NonSerialized]
	[FieldOffset(0)]
	public Matrix4x4 data;

	[NonSerialized]
	[FieldOffset(0)]
	public Vector4 data0;

	[NonSerialized]
	[FieldOffset(16)]
	public Vector4 data1;

	[NonSerialized]
	[FieldOffset(32)]
	public Vector4 data2;

	[NonSerialized]
	[FieldOffset(48)]
	public Vector4 data3;

	[FieldOffset(0)]
	public Vector3 contactPoint;

	[FieldOffset(12)]
	public float radius;

	[FieldOffset(16)]
	public Vector3 counterVelocity;

	[FieldOffset(28)]
	public float timestamp;

	[FieldOffset(32)]
	public Color color;

	[FieldOffset(48)]
	public GTContactType contactType;

	[FieldOffset(52)]
	public float lifetime = 1f;

	[FieldOffset(56)]
	public uint free = 1U;
}
