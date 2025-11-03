using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(337)]
[StructLayout(LayoutKind.Explicit, Size = 1348)]
public struct FlockingData : INetworkStruct
{
	public int count { readonly get; set; }

	[Networked]
	[Capacity(30)]
	[NetworkedWeavedLinkedList(30, 3, typeof(ElementReaderWriterVector3))]
	[NetworkedWeaved(1, 153)]
	public NetworkLinkedList<Vector3> Positions
	{
		get
		{
			return new NetworkLinkedList<Vector3>(Native.ReferenceToPointer<FixedStorage@153>(ref this._Positions), 30, ElementReaderWriterVector3.GetInstance());
		}
	}

	[Networked]
	[Capacity(30)]
	[NetworkedWeavedLinkedList(30, 4, typeof(ReaderWriter@UnityEngine_Quaternion))]
	[NetworkedWeaved(154, 183)]
	public NetworkLinkedList<Quaternion> Rotations
	{
		get
		{
			return new NetworkLinkedList<Quaternion>(Native.ReferenceToPointer<FixedStorage@183>(ref this._Rotations), 30, ReaderWriter@UnityEngine_Quaternion.GetInstance());
		}
	}

	public FlockingData(List<Flocking> items)
	{
		this.count = items.Count;
		foreach (Flocking flocking in items)
		{
			this.Positions.Add(flocking.pos);
			this.Rotations.Add(flocking.rot);
		}
	}

	[FixedBufferProperty(typeof(NetworkLinkedList<Vector3>), typeof(UnityLinkedListSurrogate@ElementReaderWriterVector3), 30, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(4)]
	private FixedStorage@153 _Positions;

	[FixedBufferProperty(typeof(NetworkLinkedList<Quaternion>), typeof(UnityLinkedListSurrogate@ReaderWriter@UnityEngine_Quaternion), 30, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(616)]
	private FixedStorage@183 _Rotations;
}
