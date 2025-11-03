using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityArraySurrogate@ElementReaderWriterBoolean : UnityArraySurrogate<bool, ElementReaderWriterBoolean>
	{
		[WeaverGenerated]
		public override bool[] DataProperty
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
		public UnityArraySurrogate@ElementReaderWriterBoolean()
		{
		}

		[WeaverGenerated]
		public bool[] Data = Array.Empty<bool>();
	}
}
