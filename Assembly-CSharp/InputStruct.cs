using System;
using System.Runtime.InteropServices;
using Fusion;
using UnityEngine;

[NetworkStructWeaved(37)]
[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 148)]
public struct InputStruct : INetworkStruct
{
	[FieldOffset(0)]
	public int headRotation;

	[FieldOffset(4)]
	public long rightHandLong;

	[FieldOffset(12)]
	public long leftHandLong;

	[FieldOffset(20)]
	public long position;

	[FieldOffset(28)]
	public int handPosition;

	[FieldOffset(32)]
	public int packedFields;

	[FieldOffset(36)]
	public short packedCompetitiveData;

	[FieldOffset(40)]
	public Vector3 velocity;

	[FieldOffset(52)]
	public int grabbedRopeIndex;

	[FieldOffset(56)]
	public int ropeBoneIndex;

	[FieldOffset(60)]
	public bool ropeGrabIsLeft;

	[FieldOffset(64)]
	public bool ropeGrabIsBody;

	[FieldOffset(68)]
	public Vector3 ropeGrabOffset;

	[FieldOffset(80)]
	public bool movingSurfaceIsMonkeBlock;

	[FieldOffset(84)]
	public long hoverboardPosRot;

	[FieldOffset(92)]
	public short hoverboardColor;

	[FieldOffset(96)]
	public long propHuntPosRot;

	[FieldOffset(104)]
	public double serverTimeStamp;

	[FieldOffset(112)]
	public short taggedById;

	[FieldOffset(116)]
	public bool isGroundedHand;

	[FieldOffset(120)]
	public bool isGroundedButt;

	[FieldOffset(124)]
	public int leftHandGrabbedActorNumber;

	[FieldOffset(128)]
	public bool leftGrabbedHandIsLeft;

	[FieldOffset(132)]
	public int rightHandGrabbedActorNumber;

	[FieldOffset(136)]
	public bool rightGrabbedHandIsLeft;

	[FieldOffset(140)]
	public float lastTouchedGroundAtTime;

	[FieldOffset(144)]
	public float lastHandTouchedGroundAtTime;
}
