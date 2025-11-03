using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityArraySurrogate@ElementReaderWriterInt64 : UnityArraySurrogate<long, ElementReaderWriterInt64>
	{
		[WeaverGenerated]
		public override long[] DataProperty
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
		public UnityArraySurrogate@ElementReaderWriterInt64()
		{
		}

		[WeaverGenerated]
		public long[] Data = Array.Empty<long>();
	}
}
