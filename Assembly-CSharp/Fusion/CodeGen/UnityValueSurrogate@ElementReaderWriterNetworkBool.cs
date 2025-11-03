using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityValueSurrogate@ElementReaderWriterNetworkBool : UnityValueSurrogate<NetworkBool, ElementReaderWriterNetworkBool>
	{
		[WeaverGenerated]
		public override NetworkBool DataProperty
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
		public UnityValueSurrogate@ElementReaderWriterNetworkBool()
		{
		}

		[WeaverGenerated]
		public NetworkBool Data;
	}
}
