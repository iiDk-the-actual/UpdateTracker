using System;
using Fusion.Internal;
using UnityEngine;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityValueSurrogate@ReaderWriter@UnityEngine_Quaternion : UnityValueSurrogate<Quaternion, ReaderWriter@UnityEngine_Quaternion>
	{
		[WeaverGenerated]
		public override Quaternion DataProperty
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
		public UnityValueSurrogate@ReaderWriter@UnityEngine_Quaternion()
		{
		}

		[WeaverGenerated]
		public Quaternion Data;
	}
}
