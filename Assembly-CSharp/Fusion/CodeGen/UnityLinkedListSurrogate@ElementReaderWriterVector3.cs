using System;
using Fusion.Internal;
using UnityEngine;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityLinkedListSurrogate@ElementReaderWriterVector3 : UnityLinkedListSurrogate<Vector3, ElementReaderWriterVector3>
	{
		[WeaverGenerated]
		public override Vector3[] DataProperty
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
		public UnityLinkedListSurrogate@ElementReaderWriterVector3()
		{
		}

		[WeaverGenerated]
		public Vector3[] Data = Array.Empty<Vector3>();
	}
}
