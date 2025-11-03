using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityArraySurrogate@ElementReaderWriterInt32 : UnityArraySurrogate<int, ElementReaderWriterInt32>
	{
		[WeaverGenerated]
		public override int[] DataProperty
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
		public UnityArraySurrogate@ElementReaderWriterInt32()
		{
		}

		[WeaverGenerated]
		public int[] Data = Array.Empty<int>();
	}
}
