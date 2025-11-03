using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[NetworkStructWeaved(10)]
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	internal struct FixedStorage@10 : INetworkStruct
	{
		[FixedBuffer(typeof(int), 10)]
		[WeaverGenerated]
		[FieldOffset(0)]
		public FixedStorage@10.<Data>e__FixedBuffer Data;

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

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(24)]
		private int _6;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(28)]
		private int _7;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(32)]
		private int _8;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(36)]
		private int _9;

		[CompilerGenerated]
		[UnsafeValueType]
		[WeaverGenerated]
		[StructLayout(LayoutKind.Sequential, Size = 40)]
		public struct <Data>e__FixedBuffer
		{
			[WeaverGenerated]
			public int FixedElementField;
		}
	}
}
