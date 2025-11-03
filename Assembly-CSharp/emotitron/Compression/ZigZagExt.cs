using System;

namespace emotitron.Compression
{
	public static class ZigZagExt
	{
		public static ulong ZigZag(this long s)
		{
			return (ulong)((s << 1) ^ (s >> 63));
		}

		public static long UnZigZag(this ulong u)
		{
			return (long)((u >> 1) ^ -(long)(u & 1UL));
		}

		public static uint ZigZag(this int s)
		{
			return (uint)((s << 1) ^ (s >> 31));
		}

		public static int UnZigZag(this uint u)
		{
			return (int)((ulong)(u >> 1) ^ (ulong)((long)(-(long)(u & 1U))));
		}

		public static ushort ZigZag(this short s)
		{
			return (ushort)(((int)s << 1) ^ (s >> 15));
		}

		public static short UnZigZag(this ushort u)
		{
			return (short)((u >> 1) ^ (int)(-(int)((short)(u & 1))));
		}

		public static byte ZigZag(this sbyte s)
		{
			return (byte)(((int)s << 1) ^ (s >> 7));
		}

		public static sbyte UnZigZag(this byte u)
		{
			return (sbyte)((u >> 1) ^ (int)(-(int)((sbyte)(u & 1))));
		}
	}
}
