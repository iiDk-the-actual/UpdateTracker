using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(21)]
[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 84)]
public struct ReliableStateData : INetworkStruct
{
	public long Header { readonly get; set; }

	[Networked]
	[Capacity(5)]
	[NetworkedWeavedArray(5, 2, typeof(ElementReaderWriterInt64))]
	[NetworkedWeaved(11, 10)]
	public NetworkArray<long> TransferrableStates
	{
		get
		{
			return new NetworkArray<long>(Native.ReferenceToPointer<FixedStorage@10>(ref this._TransferrableStates), 5, ElementReaderWriterInt64.GetInstance());
		}
	}

	public int WearablesPackedState { readonly get; set; }

	public int LThrowableProjectileIndex { readonly get; set; }

	public int RThrowableProjectileIndex { readonly get; set; }

	public int SizeLayerMask { readonly get; set; }

	public int RandomThrowableIndex { readonly get; set; }

	public long PackedBeads { readonly get; set; }

	public long PackedBeadsMoreThan6 { readonly get; set; }

	[FixedBufferProperty(typeof(NetworkArray<long>), typeof(UnityArraySurrogate@ElementReaderWriterInt64), 5, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(44)]
	private FixedStorage@10 _TransferrableStates;
}
