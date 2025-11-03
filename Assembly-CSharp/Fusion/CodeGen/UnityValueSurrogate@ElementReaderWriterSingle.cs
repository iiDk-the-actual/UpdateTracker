using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityValueSurrogate@ElementReaderWriterSingle : UnityValueSurrogate<float, ElementReaderWriterSingle>
	{
		[WeaverGenerated]
		public override float DataProperty
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
		public UnityValueSurrogate@ElementReaderWriterSingle()
		{
		}

		[WeaverGenerated]
		public float Data;
	}
}
