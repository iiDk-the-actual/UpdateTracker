using System;
using Fusion.Internal;
using UnityEngine;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityValueSurrogate@ElementReaderWriterVector3 : UnityValueSurrogate<Vector3, ElementReaderWriterVector3>
	{
		[WeaverGenerated]
		public override Vector3 DataProperty
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
		public UnityValueSurrogate@ElementReaderWriterVector3()
		{
		}

		[WeaverGenerated]
		public Vector3 Data;
	}
}
