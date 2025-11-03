using System;

namespace GorillaTag.Shared.Scripts.Utilities
{
	public sealed class GTBitArray
	{
		public bool this[int idx]
		{
			get
			{
				if (idx < 0 || idx >= this.Length)
				{
					throw new ArgumentOutOfRangeException();
				}
				int num = idx / 32;
				int num2 = idx % 32;
				return ((ulong)this._data[num] & (ulong)(1L << (num2 & 31))) > 0UL;
			}
			set
			{
				if (idx < 0 || idx >= this.Length)
				{
					throw new ArgumentOutOfRangeException();
				}
				int num = idx / 32;
				int num2 = idx % 32;
				if (value)
				{
					this._data[num] |= 1U << num2;
					return;
				}
				this._data[num] &= ~(1U << num2);
			}
		}

		public GTBitArray(int length)
		{
			this.Length = length;
			this._data = ((length % 32 == 0) ? new uint[length / 32] : new uint[length / 32 + 1]);
			for (int i = 0; i < this._data.Length; i++)
			{
				this._data[i] = 0U;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < this._data.Length; i++)
			{
				this._data[i] = 0U;
			}
		}

		public void CopyFrom(GTBitArray other)
		{
			if (this.Length != other.Length)
			{
				throw new ArgumentException("Can only copy bit arrays of the same length.");
			}
			for (int i = 0; i < this._data.Length; i++)
			{
				this._data[i] = other._data[i];
			}
		}

		public readonly int Length;

		private readonly uint[] _data;
	}
}
