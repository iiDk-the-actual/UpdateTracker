using System;
using UnityEngine;

namespace emotitron.Compression
{
	[Serializable]
	public abstract class LiteCrusher
	{
		public static int GetBitsForMaxValue(uint maxvalue)
		{
			for (int i = 0; i < 32; i++)
			{
				if (maxvalue >> i == 0U)
				{
					return i;
				}
			}
			return 32;
		}

		[SerializeField]
		protected int bits;
	}
}
