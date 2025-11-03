using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[NetworkStructWeaved(18)]
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	internal struct FixedStorage@18 : INetworkStruct
	{
		[FixedBuffer(typeof(int), 18)]
		[WeaverGenerated]
		[FieldOffset(0)]
		public FixedStorage@18.<Data>e__FixedBuffer Data;

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

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(40)]
		private int _10;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(44)]
		private int _11;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(48)]
		private int _12;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(52)]
		private int _13;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(56)]
		private int _14;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(60)]
		private int _15;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(64)]
		private int _16;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(68)]
		private int _17;

		[CompilerGenerated]
		[UnsafeValueType]
		[WeaverGenerated]
		[StructLayout(LayoutKind.Sequential, Size = 72)]
		public struct <Data>e__FixedBuffer
		{
			[WeaverGenerated]
			public int FixedElementField;
		}
	}
}
