using System;
using System.Collections.Generic;
using GorillaGameModes;
using UnityEngine;

namespace TagEffects
{
	[Serializable]
	public class ModeTagEffect
	{
		public HashSet<GameModeType> Modes
		{
			get
			{
				if (this.modesHash == null)
				{
					this.modesHash = new HashSet<GameModeType>(this.modes);
				}
				return this.modesHash;
			}
		}

		[SerializeField]
		private GameModeType[] modes;

		private HashSet<GameModeType> modesHash;

		public TagEffectPack tagEffect;

		public bool blockTagOverride;

		public bool blockFistBumpOverride;

		public bool blockHiveFiveOverride;
	}
}
