using System;
using System.Runtime.CompilerServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	internal struct ReaderWriter@BarrelCannon__BarrelCannonState : IElementReaderWriter<BarrelCannon.BarrelCannonState>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe BarrelCannon.BarrelCannonState Read(byte* data, int index)
		{
			return *(BarrelCannon.BarrelCannonState*)(data + index * 4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe ref BarrelCannon.BarrelCannonState ReadRef(byte* data, int index)
		{
			return ref *(BarrelCannon.BarrelCannonState*)(data + index * 4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe void Write(byte* data, int index, BarrelCannon.BarrelCannonState val)
		{
			*(BarrelCannon.BarrelCannonState*)(data + index * 4) = val;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public int GetElementWordCount()
		{
			return 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public int GetElementHashCode(BarrelCannon.BarrelCannonState val)
		{
			return val.GetHashCode();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public static IElementReaderWriter<BarrelCannon.BarrelCannonState> GetInstance()
		{
			if (ReaderWriter@BarrelCannon__BarrelCannonState.Instance == null)
			{
				ReaderWriter@BarrelCannon__BarrelCannonState.Instance = default(ReaderWriter@BarrelCannon__BarrelCannonState);
			}
			return ReaderWriter@BarrelCannon__BarrelCannonState.Instance;
		}

		[WeaverGenerated]
		public static IElementReaderWriter<BarrelCannon.BarrelCannonState> Instance;
	}
}
