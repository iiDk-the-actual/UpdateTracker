using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

[NetworkStructWeaved(31)]
[StructLayout(LayoutKind.Explicit, Size = 124)]
public struct PaintbrawlData : INetworkStruct
{
	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ElementReaderWriterInt32))]
	[NetworkedWeaved(1, 10)]
	public NetworkArray<int> playerLivesArray
	{
		get
		{
			return new NetworkArray<int>(Native.ReferenceToPointer<FixedStorage@10>(ref this._playerLivesArray), 10, ElementReaderWriterInt32.GetInstance());
		}
	}

	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ElementReaderWriterInt32))]
	[NetworkedWeaved(11, 10)]
	public NetworkArray<int> playerActorNumberArray
	{
		get
		{
			return new NetworkArray<int>(Native.ReferenceToPointer<FixedStorage@10>(ref this._playerActorNumberArray), 10, ElementReaderWriterInt32.GetInstance());
		}
	}

	[Networked]
	[Capacity(10)]
	[NetworkedWeavedArray(10, 1, typeof(ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus))]
	[NetworkedWeaved(21, 10)]
	public NetworkArray<GorillaPaintbrawlManager.PaintbrawlStatus> playerStatusArray
	{
		get
		{
			return new NetworkArray<GorillaPaintbrawlManager.PaintbrawlStatus>(Native.ReferenceToPointer<FixedStorage@10>(ref this._playerStatusArray), 10, ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus.GetInstance());
		}
	}

	[FieldOffset(0)]
	public GorillaPaintbrawlManager.PaintbrawlState currentPaintbrawlState;

	[FixedBufferProperty(typeof(NetworkArray<int>), typeof(UnityArraySurrogate@ElementReaderWriterInt32), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(4)]
	private FixedStorage@10 _playerLivesArray;

	[FixedBufferProperty(typeof(NetworkArray<int>), typeof(UnityArraySurrogate@ElementReaderWriterInt32), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(44)]
	private FixedStorage@10 _playerActorNumberArray;

	[FixedBufferProperty(typeof(NetworkArray<GorillaPaintbrawlManager.PaintbrawlStatus>), typeof(UnityArraySurrogate@ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus), 10, order = -2147483647)]
	[WeaverGenerated]
	[SerializeField]
	[FieldOffset(84)]
	private FixedStorage@10 _playerStatusArray;
}
