using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[NetworkStructWeaved(3)]
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	internal struct FixedStorage@3 : INetworkStruct
	{
		[FixedBuffer(typeof(int), 3)]
		[WeaverGenerated]
		[FieldOffset(0)]
		public FixedStorage@3.<Data>e__FixedBuffer Data;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(4)]
		private int _1;

		[WeaverGenerated]
		[NonSerialized]
		[FieldOffset(8)]
		private int _2;

		[CompilerGenerated]
		[UnsafeValueType]
		[WeaverGenerated]
		[StructLayout(LayoutKind.Sequential, Size = 12)]
		public struct <Data>e__FixedBuffer
		{
			[WeaverGenerated]
			public int FixedElementField;
		}
	}
}
