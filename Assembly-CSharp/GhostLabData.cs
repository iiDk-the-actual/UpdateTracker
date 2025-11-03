using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(11)]
[StructLayout(LayoutKind.Explicit, Size = 44)]
public struct GhostLabData : INetworkStruct
{
	public int DoorState { readonly get; set; }

	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ElementReaderWriterNetworkBool))]
	[NetworkedWeaved(1, 10)]
	public NetworkArray<NetworkBool> OpenDoors
	{
		get
		{
			return new NetworkArray<NetworkBool>(Native.ReferenceToPointer<FixedStorage@10>(ref this._OpenDoors), 10, ElementReaderWriterNetworkBool.GetInstance());
		}
	}

	public GhostLabData(int state, bool[] openDoors)
	{
		this.DoorState = state;
		for (int i = 0; i < openDoors.Length; i++)
		{
			bool flag = openDoors[i];
			this.OpenDoors.Set(i, flag);
		}
	}

	[FixedBufferProperty(typeof(NetworkArray<NetworkBool>), typeof(UnityArraySurrogate@ElementReaderWriterNetworkBool), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(4)]
	private FixedStorage@10 _OpenDoors;
}
