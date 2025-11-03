using System;
using System.Runtime.CompilerServices;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	internal struct ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus : IElementReaderWriter<GorillaPaintbrawlManager.PaintbrawlStatus>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe GorillaPaintbrawlManager.PaintbrawlStatus Read(byte* data, int index)
		{
			return *(GorillaPaintbrawlManager.PaintbrawlStatus*)(data + index * 4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe ref GorillaPaintbrawlManager.PaintbrawlStatus ReadRef(byte* data, int index)
		{
			return ref *(GorillaPaintbrawlManager.PaintbrawlStatus*)(data + index * 4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public unsafe void Write(byte* data, int index, GorillaPaintbrawlManager.PaintbrawlStatus val)
		{
			*(GorillaPaintbrawlManager.PaintbrawlStatus*)(data + index * 4) = val;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public int GetElementWordCount()
		{
			return 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public int GetElementHashCode(GorillaPaintbrawlManager.PaintbrawlStatus val)
		{
			return val.GetHashCode();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[WeaverGenerated]
		public static IElementReaderWriter<GorillaPaintbrawlManager.PaintbrawlStatus> GetInstance()
		{
			if (ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus.Instance == null)
			{
				ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus.Instance = default(ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus);
			}
			return ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus.Instance;
		}

		[WeaverGenerated]
		public static IElementReaderWriter<GorillaPaintbrawlManager.PaintbrawlStatus> Instance;
	}
}
