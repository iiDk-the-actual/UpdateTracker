using System;
using Fusion.Internal;

namespace Fusion.CodeGen
{
	[WeaverGenerated]
	[Serializable]
	internal class UnityValueSurrogate@ReaderWriter@BarrelCannon__BarrelCannonState : UnityValueSurrogate<BarrelCannon.BarrelCannonState, ReaderWriter@BarrelCannon__BarrelCannonState>
	{
		[WeaverGenerated]
		public override BarrelCannon.BarrelCannonState DataProperty
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
		public UnityValueSurrogate@ReaderWriter@BarrelCannon__BarrelCannonState()
		{
		}

		[WeaverGenerated]
		public BarrelCannon.BarrelCannonState Data;
	}
}
