using System;

namespace emotitron.Compression
{
	public static class PrimitivePackBytesExt
	{
		public static ulong WritePackedBytes(this ulong buffer, ulong value, ref int bitposition, int bits)
		{
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write(value, ref bitposition, num2 << 3);
			return buffer;
		}

		public static uint WritePackedBytes(this uint buffer, uint value, ref int bitposition, int bits)
		{
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write((ulong)value, ref bitposition, num2 << 3);
			return buffer;
		}

		public static void InjectPackedBytes(this ulong value, ref ulong buffer, ref int bitposition, int bits)
		{
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write(value, ref bitposition, num2 << 3);
		}

		public static void InjectPackedBytes(this uint value, ref uint buffer, ref int bitposition, int bits)
		{
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write((ulong)value, ref bitposition, num2 << 3);
		}

		public static ulong ReadPackedBytes(this ulong buffer, ref int bitposition, int bits)
		{
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return buffer.Read(ref bitposition, num2 << 3);
		}

		public static uint ReadPackedBytes(this uint buffer, ref int bitposition, int bits)
		{
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return buffer.Read(ref bitposition, num2 << 3);
		}

		public static ulong WriteSignedPackedBytes(this ulong buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			return buffer.WritePackedBytes((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBytes(this ulong buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}
	}
}
