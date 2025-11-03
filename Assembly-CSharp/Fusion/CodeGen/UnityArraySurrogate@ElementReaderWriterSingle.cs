using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityArraySurrogate@ElementReaderWriterSingle : UnityArraySurrogate<float, ElementReaderWriterSingle>
	{
		[WeaverGenerated]
		public override float[] DataProperty
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
		public UnityArraySurrogate@ElementReaderWriterSingle()
		{
		}

		[WeaverGenerated]
		public float[] Data = Array.Empty<float>();
	}
}
