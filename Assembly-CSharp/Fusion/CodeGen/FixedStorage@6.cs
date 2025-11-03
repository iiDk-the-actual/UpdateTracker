using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[NetworkStructWeaved(6)]
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	internal struct FixedStorage@6 : INetworkStruct
	{
		[FixedBuffer(typeof(int), 6)]
		[WeaverGenerated]
		[FieldOffset(0)]
		public FixedStorage@6.<Data>e__FixedBuffer Data;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(4)]
		private int _1;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(8)]
		private int _2;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(12)]
		private int _3;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(16)]
		private int _4;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(20)]
		private int _5;

		[CompilerGenerated]
		[UnsafeValueType]
		[WeaverGenerated]
		[StructLayout(LayoutKind.Sequential, Size = 24)]
		public struct <Data>e__FixedBuffer
		{
			[WeaverGenerated]
			public int FixedElementField;
		}
	}
}
