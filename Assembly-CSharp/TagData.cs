using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(12)]
[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct TagData : INetworkStruct
{
	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ElementReaderWriterInt32))]
	[NetworkedWeaved(2, 10)]
	public NetworkArray<int> infectedPlayerList
	{
		get
		{
			return new NetworkArray<int>(Native.ReferenceToPointer<FixedStorage@10>(ref this._infectedPlayerList), 10, ElementReaderWriterInt32.GetInstance());
		}
	}

	public int currentItID { readonly get; set; }

	[FieldOffset(4)]
	public NetworkBool isCurrentlyTag;

	[FixedBufferProperty(typeof(NetworkArray<int>), typeof(UnityArraySurrogate@ElementReaderWriterInt32), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(8)]
	private FixedStorage@10 _infectedPlayerList;
}
