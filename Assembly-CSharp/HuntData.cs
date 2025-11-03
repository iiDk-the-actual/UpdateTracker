using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(23)]
[StructLayout(LayoutKind.Explicit, Size = 92)]
public struct HuntData : INetworkStruct
{
	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ElementReaderWriterInt32))]
	[NetworkedWeaved(3, 10)]
	public NetworkArray<int> currentHuntedArray
	{
		get
		{
			return new NetworkArray<int>(Native.ReferenceToPointer<FixedStorage@10>(ref this._currentHuntedArray), 10, ElementReaderWriterInt32.GetInstance());
		}
	}

	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ElementReaderWriterInt32))]
	[NetworkedWeaved(13, 10)]
	public NetworkArray<int> currentTargetArray
	{
		get
		{
			return new NetworkArray<int>(Native.ReferenceToPointer<FixedStorage@10>(ref this._currentTargetArray), 10, ElementReaderWriterInt32.GetInstance());
		}
	}

	[FieldOffset(0)]
	public NetworkBool huntStarted;

	[FieldOffset(4)]
	public NetworkBool waitingToStartNextHuntGame;

	[FieldOffset(8)]
	public int countDownTime;

	[FixedBufferProperty(typeof(NetworkArray<int>), typeof(UnityArraySurrogate@ElementReaderWriterInt32), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(12)]
	private FixedStorage@10 _currentHuntedArray;

	[FixedBufferProperty(typeof(NetworkArray<int>), typeof(UnityArraySurrogate@ElementReaderWriterInt32), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(52)]
	private FixedStorage@10 _currentTargetArray;
}
