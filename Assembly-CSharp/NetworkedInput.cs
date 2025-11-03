using System;
using System.Runtime.InteropServices;
using Fusion;
using UnityEngine;

[NetworkInputWeaved(35)]
[StructLayout(LayoutKind.Explicit, Size = 140)]
public struct NetworkedInput : INetworkInput
{
	[FieldOffset(0)]
	public Quaternion headRot_LS;

	[FieldOffset(16)]
	public Vector3 rightHandPos_LS;

	[FieldOffset(28)]
	public Quaternion rightHandRot_LS;

	[FieldOffset(44)]
	public Vector3 leftHandPos_LS;

	[FieldOffset(56)]
	public Quaternion leftHandRot_LS;

	[FieldOffset(72)]
	public Vector3 rootPosition;

	[FieldOffset(84)]
	public Quaternion rootRotation;

	[FieldOffset(100)]
	public bool leftThumbTouch;

	[FieldOffset(104)]
	public bool leftThumbPress;

	[FieldOffset(108)]
	public float leftIndexValue;

	[FieldOffset(112)]
	public float leftMiddleValue;

	[FieldOffset(116)]
	public bool rightThumbTouch;

	[FieldOffset(120)]
	public bool rightThumbPress;

	[FieldOffset(124)]
	public float rightIndexValue;

	[FieldOffset(128)]
	public float rightMiddleValue;

	[FieldOffset(132)]
	public float scale;

	[FieldOffset(136)]
	public int handPoseData;
}
