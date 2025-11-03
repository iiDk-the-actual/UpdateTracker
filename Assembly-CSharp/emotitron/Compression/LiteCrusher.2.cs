using System;

namespace emotitron.Compression
{
	[Serializable]
	public abstract class LiteCrusher<T> : LiteCrusher where T : struct
	{
		public abstract ulong Encode(T val);

		public abstract T Decode(uint val);

		public abstract ulong WriteValue(T val, byte[] buffer, ref int bitposition);

		public abstract void WriteCValue(uint val, byte[] buffer, ref int bitposition);

		public abstract T ReadValue(byte[] buffer, ref int bitposition);
	}
}
