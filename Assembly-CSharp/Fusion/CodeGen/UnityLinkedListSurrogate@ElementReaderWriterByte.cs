using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityLinkedListSurrogate@ElementReaderWriterByte : UnityLinkedListSurrogate<byte, ElementReaderWriterByte>
	{
		[WeaverGenerated]
		public override byte[] DataProperty
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
		public UnityLinkedListSurrogate@ElementReaderWriterByte()
		{
		}

		[WeaverGenerated]
		public byte[] Data = Array.Empty<byte>();
	}
}
