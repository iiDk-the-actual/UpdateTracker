using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityArraySurrogate@ElementReaderWriterNetworkBool : UnityArraySurrogate<NetworkBool, ElementReaderWriterNetworkBool>
	{
		[WeaverGenerated]
		public override NetworkBool[] DataProperty
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
		public UnityArraySurrogate@ElementReaderWriterNetworkBool()
		{
		}

		[WeaverGenerated]
		public NetworkBool[] Data = Array.Empty<NetworkBool>();
	}
}
