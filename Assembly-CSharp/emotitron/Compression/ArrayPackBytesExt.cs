using System;

namespace emotitron.Compression
{
	public static class ArrayPackBytesExt
	{
		public unsafe static void WritePackedBytes(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			ArraySerializeUnsafe.Write(uPtr, (ulong)num2, ref bitposition, num);
			ArraySerializeUnsafe.Write(uPtr, value, ref bitposition, num2 << 3);
		}

		public static void WritePackedBytes(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer.Write((ulong)num2, ref bitposition, num);
			buffer.Write(value, ref bitposition, num2 << 3);
		}

		public static void WritePackedBytes(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer.Write((ulong)num2, ref bitposition, num);
			buffer.Write(value, ref bitposition, num2 << 3);
		}

		public static void WritePackedBytes(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = value.UsedByteCount();
			buffer.Write((ulong)num2, ref bitposition, num);
			buffer.Write(value, ref bitposition, num2 << 3);
		}

		public unsafe static ulong ReadPackedBytes(ulong* uPtr, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = (int)ArraySerializeUnsafe.Read(uPtr, ref bitposition, num) << 3;
			return ArraySerializeUnsafe.Read(uPtr, ref bitposition, num2);
		}

		public static ulong ReadPackedBytes(this ulong[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num) << 3;
			return buffer.Read(ref bitposition, num2);
		}

		public static ulong ReadPackedBytes(this uint[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num) << 3;
			return buffer.Read(ref bitposition, num2);
		}

		public static ulong ReadPackedBytes(this byte[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int num = (bits + 7 >> 3).UsedBitCount();
			int num2 = (int)buffer.Read(ref bitposition, num) << 3;
			return buffer.Read(ref bitposition, num2);
		}

		public unsafe static void WriteSignedPackedBytes(ulong* uPtr, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArrayPackBytesExt.WritePackedBytes(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static int ReadSignedPackedBytes(ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)ArrayPackBytesExt.ReadPackedBytes(uPtr, ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBytes(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBytes((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBytes(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBytes(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBytes((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBytes(this uint[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBytes(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBytes((ulong)num, ref bitposition, bits);
		}

		public static int ReadSignedPackedBytes(this byte[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static void WriteSignedPackedBytes64(this byte[] buffer, long value, ref int bitposition, int bits)
		{
			ulong num = (ulong)((value << 1) ^ (value >> 63));
			buffer.WritePackedBytes(num, ref bitposition, bits);
		}

		public static long ReadSignedPackedBytes64(this byte[] buffer, ref int bitposition, int bits)
		{
			ulong num = buffer.ReadPackedBytes(ref bitposition, bits);
			return (long)((num >> 1) ^ -(long)(num & 1UL));
		}
	}
}
