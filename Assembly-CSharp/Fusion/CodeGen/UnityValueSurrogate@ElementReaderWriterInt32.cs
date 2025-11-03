using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityValueSurrogate@ElementReaderWriterInt32 : UnityValueSurrogate<int, ElementReaderWriterInt32>
	{
		[WeaverGenerated]
		public override int DataProperty
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
		public UnityValueSurrogate@ElementReaderWriterInt32()
		{
		}

		[WeaverGenerated]
		public int Data;
	}
}
