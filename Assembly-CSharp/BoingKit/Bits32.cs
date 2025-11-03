using System;
using UnityEngine;

namespace BoingKit
{
	[Serializable]
	public struct Bits32
	{
		public int IntValue
		{
			get
			{
				return this.m_bits;
			}
		}

		public Bits32(int bits = 0)
		{
			this.m_bits = bits;
		}

		public void Clear()
		{
			this.m_bits = 0;
		}

		public void SetBit(int index, bool value)
		{
			if (value)
			{
				this.m_bits |= 1 << index;
				return;
			}
			this.m_bits &= ~(1 << index);
		}

		public bool IsBitSet(int index)
		{
			return (this.m_bits & (1 << index)) != 0;
		}

		[SerializeField]
		private int m_bits;
	}
}
