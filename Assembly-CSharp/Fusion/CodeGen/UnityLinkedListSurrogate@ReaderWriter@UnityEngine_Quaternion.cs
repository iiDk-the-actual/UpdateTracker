using System;
using Fusion.Internal;
using UnityEngine;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityLinkedListSurrogate@ReaderWriter@UnityEngine_Quaternion : UnityLinkedListSurrogate<Quaternion, ReaderWriter@UnityEngine_Quaternion>
	{
		[WeaverGenerated]
		public override Quaternion[] DataProperty
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
		public UnityLinkedListSurrogate@ReaderWriter@UnityEngine_Quaternion()
		{
		}

		[WeaverGenerated]
		public Quaternion[] Data = Array.Empty<Quaternion>();
	}
}
