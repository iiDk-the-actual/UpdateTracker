using System;
using System.Runtime.CompilerServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	internal struct ReaderWriter@Fusion_NetworkString : IElementReaderWriter<NetworkString<_128>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe NetworkString<_128> Read(byte* data, int index)
		{
			return *(NetworkString<_128>*)(data + index * 516);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe ref NetworkString<_128> ReadRef(byte* data, int index)
		{
			return ref *(NetworkString<_128>*)(data + index * 516);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe void Write(byte* data, int index, NetworkString<_128> val)
		{
			*(NetworkString<_128>*)(data + index * 516) = val;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public int GetElementWordCount()
		{
			return 129;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public int GetElementHashCode(NetworkString<_128> val)
		{
			return val.GetHashCode();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public static IElementReaderWriter<NetworkString<_128>> GetInstance()
		{
			if (ReaderWriter@Fusion_NetworkString.Instance == null)
			{
				ReaderWriter@Fusion_NetworkString.Instance = default(ReaderWriter@Fusion_NetworkString);
			}
			return ReaderWriter@Fusion_NetworkString.Instance;
		}

		[WeaverGenerated]
		public static IElementReaderWriter<NetworkString<_128>> Instance;
	}
}
