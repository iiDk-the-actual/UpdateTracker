using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(11)]
[StructLayout(LayoutKind.Explicit, Size = 44)]
public struct SkeletonNetData : INetworkStruct
{
	public int CurrentState { readonly get; set; }

	[Networked]
	[NetworkedWeaved(1, 3)]
	public unsafe Vector3 Position
	{
		readonly get
		{
			return *(Vector3*)Native.ReferenceToPointer<FixedStorage@3>(ref this._Position);
		}
		set
		{
			*(Vector3*)Native.ReferenceToPointer<FixedStorage@3>(ref this._Position) = value;
		}
	}

	[Networked]
	[NetworkedWeaved(4, 4)]
	public unsafe Quaternion Rotation
	{
		readonly get
		{
			return *(Quaternion*)Native.ReferenceToPointer<FixedStorage@4>(ref this._Rotation);
		}
		set
		{
			*(Quaternion*)Native.ReferenceToPointer<FixedStorage@4>(ref this._Rotation) = value;
		}
	}

	public int CurrentNode { readonly get; set; }

	public int NextNode { readonly get; set; }

	public int AngerPoint { readonly get; set; }

	public SkeletonNetData(int state, Vector3 pos, Quaternion rot, int cNode, int nNode, int angerPoint)
	{
		this.CurrentState = state;
		this.Position = pos;
		this.Rotation = rot;
		this.CurrentNode = cNode;
		this.NextNode = nNode;
		this.AngerPoint = angerPoint;
	}

	[FixedBufferProperty(typeof(Vector3), typeof(UnityValueSurrogate@ElementReaderWriterVector3), 0, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(4)]
	private FixedStorage@3 _Position;

	[FixedBufferProperty(typeof(Quaternion), typeof(UnityValueSurrogate@ReaderWriter@UnityEngine_Quaternion), 0, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(16)]
	private FixedStorage@4 _Rotation;
}
