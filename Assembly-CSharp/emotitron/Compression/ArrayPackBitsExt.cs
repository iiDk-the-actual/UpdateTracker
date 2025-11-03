using System;

namespace emotitron.Compression
{
	public static class ArrayPackBitsExt
	{
		public unsafe static void WritePackedBits(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = value.UsedBitCount();
			int num2 = bits.UsedBitCount();
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, num2);
			ArraySerializeUnsafe.Write(uPtr, value, ref bitposition, num);
		}

		public static void WritePackedBits(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = value.UsedBitCount();
			int num2 = bits.UsedBitCount();
			buffer.Write((ulong)num, ref bitposition, num2);
			buffer.Write(value, ref bitposition, num);
		}

		public static void WritePackedBits(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = value.UsedBitCount();
			int num2 = bits.UsedBitCount();
			buffer.Write((ulong)((long)num), ref bitposition, num2);
			buffer.Write(value, ref bitposition, num);
		}

		public static void WritePackedBits(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			int num = value.UsedBitCount();
			int num2 = bits.UsedBitCount();
			buffer.Write((ulong)num, ref bitposition, num2);
			buffer.Write(value, ref bitposition, num);
		}

		public unsafe static ulong ReadPackedBits(ulong* uPtr, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = bits.UsedBitCount();
			int num2 = (int)ArraySerializeUnsafe.Read(uPtr, ref bitposition, num);
			return ArraySerializeUnsafe.Read(uPtr, ref bitposition, num2);
		}

		public static ulong ReadPackedBits(this ulong[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = bits.UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return buffer.Read(ref bitposition, num2);
		}

		public static ulong ReadPackedBits(this uint[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = bits.UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return buffer.Read(ref bitposition, num2);
		}

		public static ulong ReadPackedBits(this byte[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = bits.UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num);
			return buffer.Read(ref bitposition, num2);
		}

		public unsafe static void WriteSignedPackedBits(ulong* uPtr, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArrayPackBitsExt.WritePackedBits(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static int ReadSignedPackedBits(ulong* buffer, ref int bitposition, int bits)
		{
			uint num = (uint)ArrayPackBitsExt.ReadPackedBits(buffer, ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBits(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBits(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBits(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBits(this uint[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBits(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBits(this byte[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBits64(this byte[] buffer, long value, ref int bitposition, int bits)
		{
			ulong num = (ulong)((value << 1) ^ (value >> 63));
			buffer.WritePackedBits(num, ref bitposition, bits);
		}

		public static long ReadSignedPackedBits64(this byte[] buffer, ref int bitposition, int bits)
		{
			ulong num = buffer.ReadPackedBits(ref bitposition, bits);
			return (long)((num >> 1) ^ -(long)(num & 1UL));
		}
	}
}
