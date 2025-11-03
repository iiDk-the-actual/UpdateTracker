using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[NetworkStructWeaved(1)]
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	internal struct FixedStorage@1 : INetworkStruct
	{
		[FixedBuffer(typeof(int), 1)]
		[WeaverGenerated]
		[FieldOffset(0)]
		public FixedStorage@1.<Data>e__FixedBuffer Data;

		[CompilerGenerated]
		[UnsafeValueType]
		[WeaverGenerated]
		[StructLayout(LayoutKind.Sequential, Size = 4)]
		public struct <Data>e__FixedBuffer
		{
			[WeaverGenerated]
			public int FixedElementField;
		}
	}
}
