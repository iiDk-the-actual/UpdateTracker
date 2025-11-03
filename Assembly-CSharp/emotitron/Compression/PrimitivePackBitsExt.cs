using System;

namespace emotitron.Compression
{
	public static class PrimitivePackBitsExt
	{
		public static ulong WritePackedBits(this ulong buffer, uint value, ref int bitposition, int bits)
		{
			int num = ((uint)bits).UsedBitCount();
			int num2 = value.UsedBitCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write((ulong)value, ref bitposition, num2);
			return buffer;
		}

		public static uint WritePackedBits(this uint buffer, ushort value, ref int bitposition, int bits)
		{
			int num = ((uint)bits).UsedBitCount();
			int num2 = value.UsedBitCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write((ulong)value, ref bitposition, num2);
			return buffer;
		}

		public static ushort WritePackedBits(this ushort buffer, byte value, ref int bitposition, int bits)
		{
			int num = ((uint)bits).UsedBitCount();
			int num2 = value.UsedBitCount();
			buffer = buffer.Write((ulong)num2, ref bitposition, num);
			buffer = buffer.Write((ulong)value, ref bitposition, num2);
			return buffer;
		}

		public static ulong ReadPackedBits(this ulong buffer, ref int bitposition, int bits)
		{
			int num = bits.UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return buffer.Read(ref bitposition, num2);
		}

		public static ulong ReadPackedBits(this uint buffer, ref int bitposition, int bits)
		{
			int num = bits.UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return (ulong)buffer.Read(ref bitposition, num2);
		}

		public static ulong ReadPackedBits(this ushort buffer, ref int bitposition, int bits)
		{
			int num = bits.UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return (ulong)buffer.Read(ref bitposition, num2);
		}

		public static ulong WriteSignedPackedBits(this ulong buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer = buffer.WritePackedBits(num, ref bitposition, bits);
			return buffer;
		}

		public static uint WriteSignedPackedBits(this uint buffer, short value, ref int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			buffer = buffer.WritePackedBits((ushort)num, ref bitposition, bits);
			return buffer;
		}

		public static ushort WriteSignedPackedBits(this ushort buffer, sbyte value, ref int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			buffer = buffer.WritePackedBits((byte)num, ref bitposition, bits);
			return buffer;
		}

		public static int ReadSignedPackedBits(this ulong buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static short ReadSignedPackedBits(this uint buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			return (short)((int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U)))));
		}

		public static sbyte ReadSignedPackedBits(this ushort buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			return (sbyte)((int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U)))));
		}
	}
}
