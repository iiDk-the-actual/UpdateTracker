using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;

namespace GorillaTagScripts
{
	[NetworkStructWeaved(13)]
	[StructLayout(LayoutKind.Explicit, Size = 52)]
	public struct FlowersDataStruct : INetworkStruct
	{
		public int FlowerCount { readonly get; set; }

		[Networked]
		[NetworkedWeavedLinkedList(1, 1, typeof(ElementReaderWriterByte))]
		[NetworkedWeaved(1, 6)]
		public NetworkLinkedList<byte> FlowerWateredData
		{
			get
			{
				return new NetworkLinkedList<byte>(Native.ReferenceToPointer<FixedStorage@6>(ref this._FlowerWateredData), 1, ElementReaderWriterByte.GetInstance());
			}
		}

		[Networked]
		[NetworkedWeavedLinkedList(1, 1, typeof(ElementReaderWriterInt32))]
		[NetworkedWeaved(7, 6)]
		public NetworkLinkedList<int> FlowerStateData
		{
			get
			{
				return new NetworkLinkedList<int>(Native.ReferenceToPointer<FixedStorage@6>(ref this._FlowerStateData), 1, ElementReaderWriterInt32.GetInstance());
			}
		}

		public FlowersDataStruct(List<Flower> allFlowers)
		{
			this.FlowerCount = allFlowers.Count;
			foreach (Flower flower in allFlowers)
			{
				this.FlowerWateredData.Add(flower.IsWatered ? 1 : 0);
				this.FlowerStateData.Add((int)flower.GetCurrentState());
			}
		}

		[FixedBufferProperty(typeof(NetworkLinkedList<byte>), typeof(UnityLinkedListSurrogate@ElementReaderWriterByte), 1, order = -2147483647)]
		[WeaverGenerated]
		[SerializeField]
		[FieldOffset(4)]
		private FixedStorage@6 _FlowerWateredData;

		[FixedBufferProperty(typeof(NetworkLinkedList<int>), typeof(UnityLinkedListSurrogate@ElementReaderWriterInt32), 1, order = -2147483647)]
		[WeaverGenerated]
		[SerializeField]
		[FieldOffset(28)]
		private FixedStorage@6 _FlowerStateData;
	}
}
