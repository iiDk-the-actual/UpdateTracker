using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[NetworkStructWeaved(4)]
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	internal struct FixedStorage@4 : INetworkStruct
	{
		[FixedBuffer(typeof(int), 4)]
		[WeaverGenerated]
		[FieldOffset(0)]
		public FixedStorage@4.<Data>e__FixedBuffer Data;

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

		[CompilerGenerated]
		[UnsafeValueType]
		[WeaverGenerated]
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		public struct <Data>e__FixedBuffer
		{
			[WeaverGenerated]
			public int FixedElementField;
		}
	}
}
