using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityDictionarySurrogate@ElementReaderWriterInt32@ElementReaderWriterInt32 : UnityDictionarySurrogate<int, ElementReaderWriterInt32, int, ElementReaderWriterInt32>
	{
		[WeaverGenerated]
		public override SerializableDictionary<int, int> DataProperty
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
		public UnityDictionarySurrogate@ElementReaderWriterInt32@ElementReaderWriterInt32()
		{
		}

		[WeaverGenerated]
		public SerializableDictionary<int, int> Data = SerializableDictionary.Create<int, int>();
	}
}
