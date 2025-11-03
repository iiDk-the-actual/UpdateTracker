using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityArraySurrogate@ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus : UnityArraySurrogate<GorillaPaintbrawlManager.PaintbrawlStatus, ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus>
	{
		[WeaverGenerated]
		public override GorillaPaintbrawlManager.PaintbrawlStatus[] DataProperty
		{
			[WeaverGenerated]
			get
			{
				return this.Data;
			}
			[WeaverGenerated]
			set
			{
				this.Data = value;
			}
		}

		[WeaverGenerated]
		public UnityArraySurrogate@ReaderWriter@GorillaPaintbrawlManager__PaintbrawlStatus()
		{
		}

		[WeaverGenerated]
		public GorillaPaintbrawlManager.PaintbrawlStatus[] Data = Array.Empty<GorillaPaintbrawlManager.PaintbrawlStatus>();
	}
}
