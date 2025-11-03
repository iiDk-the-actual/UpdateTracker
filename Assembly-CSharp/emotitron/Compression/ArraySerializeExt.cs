using System;

namespace emotitron.Compression
{
	public static class ArraySerializeExt
	{
		public static void Zero(this byte[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; i++)
			{
				buffer[i] = 0;
			}
		}

		public static void Zero(this byte[] buffer, int startByte)
		{
			int num = buffer.Length;
			for (int i = startByte; i < num; i++)
			{
				buffer[i] = 0;
			}
		}

		public static void Zero(this byte[] buffer)
		{
			int num = buffer.Length;
			for (int i = 0; i < num; i++)
			{
				buffer[i] = 0;
			}
		}

		public static void Zero(this ushort[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; i++)
			{
				buffer[i] = 0;
			}
		}

		public static void Zero(this ushort[] buffer, int startByte)
		{
			int num = buffer.Length;
			for (int i = startByte; i < num; i++)
			{
				buffer[i] = 0;
			}
		}

		public static void Zero(this ushort[] buffer)
		{
			int num = buffer.Length;
			for (int i = 0; i < num; i++)
			{
				buffer[i] = 0;
			}
		}

		public static void Zero(this uint[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; i++)
			{
				buffer[i] = 0U;
			}
		}

		public static void Zero(this uint[] buffer, int startByte)
		{
			int num = buffer.Length;
			for (int i = startByte; i < num; i++)
			{
				buffer[i] = 0U;
			}
		}

		public static void Zero(this uint[] buffer)
		{
			int num = buffer.Length;
			for (int i = 0; i < num; i++)
			{
				buffer[i] = 0U;
			}
		}

		public static void Zero(this ulong[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; i++)
			{
				buffer[i] = 0UL;
			}
		}

		public static void Zero(this ulong[] buffer, int startByte)
		{
			int num = buffer.Length;
			for (int i = startByte; i < num; i++)
			{
				buffer[i] = 0UL;
			}
		}

		public static void Zero(this ulong[] buffer)
		{
			int num = buffer.Length;
			for (int i = 0; i < num; i++)
			{
				buffer[i] = 0UL;
			}
		}

		public static void WriteSigned(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.Write((ulong)num, ref bitposition, bits);
		}

		public static void WriteSigned(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.Write((ulong)num, ref bitposition, bits);
		}

		public static void WriteSigned(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			buffer.Write((ulong)num, ref bitposition, bits);
		}

		public static void WriteSigned(this byte[] buffer, long value, ref int bitposition, int bits)
		{
			ulong num = (ulong)((value << 1) ^ (value >> 63));
			buffer.Write(num, ref bitposition, bits);
		}

		public static void WriteSigned(this uint[] buffer, long value, ref int bitposition, int bits)
		{
			ulong num = (ulong)((value << 1) ^ (value >> 63));
			buffer.Write(num, ref bitposition, bits);
		}

		public static void WriteSigned(this ulong[] buffer, long value, ref int bitposition, int bits)
		{
			ulong num = (ulong)((value << 1) ^ (value >> 63));
			buffer.Write(num, ref bitposition, bits);
		}

		public static int ReadSigned(this byte[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.Read(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static int ReadSigned(this uint[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.Read(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static int ReadSigned(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint num = (uint)buffer.Read(ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public static long ReadSigned64(this byte[] buffer, ref int bitposition, int bits)
		{
			ulong num = buffer.Read(ref bitposition, bits);
			return (long)((num >> 1) ^ -(long)(num & 1UL));
		}

		public static long ReadSigned64(this uint[] buffer, ref int bitposition, int bits)
		{
			ulong num = buffer.Read(ref bitposition, bits);
			return (long)((num >> 1) ^ -(long)(num & 1UL));
		}

		public static long ReadSigned64(this ulong[] buffer, ref int bitposition, int bits)
		{
			ulong num = buffer.Read(ref bitposition, bits);
			return (long)((num >> 1) ^ -(long)(num & 1UL));
		}

		public static void WriteFloat(this byte[] buffer, float value, ref int bitposition)
		{
			buffer.Write((ulong)value.uint32, ref bitposition, 32);
		}

		public static float ReadFloat(this byte[] buffer, ref int bitposition)
		{
			return buffer.Read(ref bitposition, 32);
		}

		public static void Append(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int i = bitposition & 7;
			int num = bitposition >> 3;
			ulong num2 = (1UL << i) - 1UL;
			ulong num3 = ((ulong)buffer[num] & num2) | (value << i);
			buffer[num] = (byte)num3;
			for (i = 8 - i; i < bits; i += 8)
			{
				num++;
				buffer[num] = (byte)(value >> i);
			}
			bitposition += bits;
		}

		public static void Append(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int i = bitposition & 31;
			int num = bitposition >> 5;
			ulong num2 = (1UL << i) - 1UL;
			ulong num3 = ((ulong)buffer[num] & num2) | (value << i);
			buffer[num] = (uint)num3;
			for (i = 32 - i; i < bits; i += 32)
			{
				num++;
				buffer[num] = (uint)(value >> i);
			}
			bitposition += bits;
		}

		public static void Append(this uint[] buffer, uint value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 31;
			int num2 = bitposition >> 5;
			ulong num3 = (1UL << num) - 1UL;
			ulong num4 = ((ulong)buffer[num2] & num3) | ((ulong)value << num);
			buffer[num2] = (uint)num4;
			buffer[num2 + 1] = (uint)(num4 >> 32);
			bitposition += bits;
		}

		public static void Append(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 63;
			int num2 = bitposition >> 6;
			ulong num3 = (1UL << num) - 1UL;
			ulong num4 = (buffer[num2] & num3) | (value << num);
			buffer[num2] = num4;
			buffer[num2 + 1] = value >> 64 - num;
			bitposition += bits;
		}

		public static void Write(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 7;
			int num2 = bitposition >> 3;
			int i = num + bits;
			ulong num3 = ulong.MaxValue >> 64 - bits;
			ulong num4 = num3 << num;
			ulong num5 = value << num;
			buffer[num2] = (byte)(((ulong)buffer[num2] & ~num4) | (num5 & num4));
			num = 8 - num;
			for (i -= 8; i > 8; i -= 8)
			{
				num2++;
				num5 = value >> num;
				buffer[num2] = (byte)num5;
				num += 8;
			}
			if (i > 0)
			{
				num2++;
				num4 = num3 >> num;
				num5 = value >> num;
				buffer[num2] = (byte)(((ulong)buffer[num2] & ~num4) | (num5 & num4));
			}
			bitposition += bits;
		}

		public static void Write(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 31;
			int num2 = bitposition >> 5;
			int i = num + bits;
			ulong num3 = ulong.MaxValue >> 64 - bits;
			ulong num4 = num3 << num;
			ulong num5 = value << num;
			buffer[num2] = (uint)(((ulong)buffer[num2] & ~num4) | (num5 & num4));
			num = 32 - num;
			for (i -= 32; i > 32; i -= 32)
			{
				num2++;
				num4 = num3 >> num;
				num5 = value >> num;
				buffer[num2] = (uint)(((ulong)buffer[num2] & ~num4) | (num5 & num4));
				num += 32;
			}
			bitposition += bits;
		}

		public static void Write(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 63;
			int num2 = bitposition >> 6;
			int i = num + bits;
			ulong num3 = ulong.MaxValue >> 64 - bits;
			ulong num4 = num3 << num;
			ulong num5 = value << num;
			buffer[num2] = (buffer[num2] & ~num4) | (num5 & num4);
			num = 64 - num;
			for (i -= 64; i > 64; i -= 64)
			{
				num2++;
				num4 = num3 >> num;
				num5 = value >> num;
				buffer[num2] = (buffer[num2] & ~num4) | (num5 & num4);
				num += 64;
			}
			bitposition += bits;
		}

		public static void WriteBool(this ulong[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1L : 0L), ref bitposition, 1);
		}

		public static void WriteBool(this uint[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1L : 0L), ref bitposition, 1);
		}

		public static void WriteBool(this byte[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1L : 0L), ref bitposition, 1);
		}

		public static ulong Read(this byte[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int i = bitposition & 7;
			int num = bitposition >> 3;
			ulong num2 = ulong.MaxValue >> 64 - bits;
			ulong num3 = (ulong)buffer[num] >> i;
			for (i = 8 - i; i < bits; i += 8)
			{
				num++;
				num3 |= (ulong)buffer[num] << i;
			}
			bitposition += bits;
			return num3 & num2;
		}

		public static ulong Read(this uint[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int i = bitposition & 31;
			int num = bitposition >> 5;
			ulong num2 = ulong.MaxValue >> 64 - bits;
			ulong num3 = (ulong)buffer[num] >> i;
			for (i = 32 - i; i < bits; i += 32)
			{
				num++;
				num3 |= (ulong)buffer[num] << i;
			}
			bitposition += bits;
			return num3 & num2;
		}

		public static ulong Read(this ulong[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int i = bitposition & 63;
			int num = bitposition >> 6;
			ulong num2 = ulong.MaxValue >> 64 - bits;
			ulong num3 = buffer[num] >> i;
			for (i = 64 - i; i < bits; i += 64)
			{
				num++;
				num3 |= buffer[num] << i;
			}
			bitposition += bits;
			return num3 & num2;
		}

		[Obsolete("Just use Read(), it return a ulong already.")]
		public static ulong ReadUInt64(this byte[] buffer, ref int bitposition, int bits = 64)
		{
			return buffer.Read(ref bitposition, bits);
		}

		[Obsolete("Just use Read(), it return a ulong already.")]
		public static ulong ReadUInt64(this uint[] buffer, ref int bitposition, int bits = 64)
		{
			return buffer.Read(ref bitposition, bits);
		}

		[Obsolete("Just use Read(), it return a ulong already.")]
		public static ulong ReadUInt64(this ulong[] buffer, ref int bitposition, int bits = 64)
		{
			return buffer.Read(ref bitposition, bits);
		}

		public static uint ReadUInt32(this byte[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)buffer.Read(ref bitposition, bits);
		}

		public static uint ReadUInt32(this uint[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)buffer.Read(ref bitposition, bits);
		}

		public static uint ReadUInt32(this ulong[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)buffer.Read(ref bitposition, bits);
		}

		public static ushort ReadUInt16(this byte[] buffer, ref int bitposition, int bits = 16)
		{
			return (ushort)buffer.Read(ref bitposition, bits);
		}

		public static ushort ReadUInt16(this uint[] buffer, ref int bitposition, int bits = 16)
		{
			return (ushort)buffer.Read(ref bitposition, bits);
		}

		public static ushort ReadUInt16(this ulong[] buffer, ref int bitposition, int bits = 16)
		{
			return (ushort)buffer.Read(ref bitposition, bits);
		}

		public static byte ReadByte(this byte[] buffer, ref int bitposition, int bits = 8)
		{
			return (byte)buffer.Read(ref bitposition, bits);
		}

		public static byte ReadByte(this uint[] buffer, ref int bitposition, int bits = 32)
		{
			return (byte)buffer.Read(ref bitposition, bits);
		}

		public static byte ReadByte(this ulong[] buffer, ref int bitposition, int bits)
		{
			return (byte)buffer.Read(ref bitposition, bits);
		}

		public static bool ReadBool(this ulong[] buffer, ref int bitposition)
		{
			return buffer.Read(ref bitposition, 1) == 1UL;
		}

		public static bool ReadBool(this uint[] buffer, ref int bitposition)
		{
			return buffer.Read(ref bitposition, 1) == 1UL;
		}

		public static bool ReadBool(this byte[] buffer, ref int bitposition)
		{
			return buffer.Read(ref bitposition, 1) == 1UL;
		}

		public static char ReadChar(this ulong[] buffer, ref int bitposition)
		{
			return (char)buffer.Read(ref bitposition, 16);
		}

		public static char ReadChar(this uint[] buffer, ref int bitposition)
		{
			return (char)buffer.Read(ref bitposition, 16);
		}

		public static char ReadChar(this byte[] buffer, ref int bitposition)
		{
			return (char)buffer.Read(ref bitposition, 16);
		}

		public static void ReadOutSafe(this ulong[] source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = srcStartPos;
			int num2;
			for (int i = bits; i > 0; i -= num2)
			{
				num2 = ((i > 64) ? 64 : i);
				ulong num3 = source.Read(ref num, num2);
				target.Write(num3, ref bitposition, num2);
			}
			bitposition += bits;
		}

		public static void ReadOutSafe(this ulong[] source, int srcStartPos, ulong[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = srcStartPos;
			int num2;
			for (int i = bits; i > 0; i -= num2)
			{
				num2 = ((i > 64) ? 64 : i);
				ulong num3 = source.Read(ref num, num2);
				target.Write(num3, ref bitposition, num2);
			}
		}

		public static void ReadOutSafe(this byte[] source, int srcStartPos, ulong[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = srcStartPos;
			int num2;
			for (int i = bits; i > 0; i -= num2)
			{
				num2 = ((i > 8) ? 8 : i);
				ulong num3 = source.Read(ref num, num2);
				target.Write(num3, ref bitposition, num2);
			}
		}

		public static void ReadOutSafe(this byte[] source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = srcStartPos;
			int num2;
			for (int i = bits; i > 0; i -= num2)
			{
				num2 = ((i > 8) ? 8 : i);
				ulong num3 = source.Read(ref num, num2);
				target.Write(num3, ref bitposition, num2);
			}
		}

		public static ulong IndexAsUInt64(this byte[] buffer, int index)
		{
			int num = index << 3;
			return (ulong)buffer[num] | ((ulong)buffer[num + 1] << 8) | ((ulong)buffer[num + 2] << 16) | ((ulong)buffer[num + 3] << 24) | ((ulong)buffer[num + 4] << 32) | ((ulong)buffer[num + 5] << 40) | ((ulong)buffer[num + 6] << 48) | ((ulong)buffer[num + 7] << 56);
		}

		public static ulong IndexAsUInt64(this uint[] buffer, int index)
		{
			int num = index << 1;
			return (ulong)buffer[num] | ((ulong)buffer[num + 1] << 32);
		}

		public static uint IndexAsUInt32(this byte[] buffer, int index)
		{
			int num = index << 3;
			return (uint)((int)buffer[num] | ((int)buffer[num + 1] << 8) | ((int)buffer[num + 2] << 16) | ((int)buffer[num + 3] << 24));
		}

		public static uint IndexAsUInt32(this ulong[] buffer, int index)
		{
			int num = index >> 1;
			int num2 = (index & 1) << 5;
			return (uint)((byte)(buffer[num] >> num2));
		}

		public static byte IndexAsUInt8(this ulong[] buffer, int index)
		{
			int num = index >> 3;
			int num2 = (index & 7) << 3;
			return (byte)(buffer[num] >> num2);
		}

		public static byte IndexAsUInt8(this uint[] buffer, int index)
		{
			int num = index >> 3;
			int num2 = (index & 3) << 3;
			return (byte)((ulong)buffer[num] >> num2);
		}

		private const string bufferOverrunMsg = "Byte buffer length exceeded by write or read. Dataloss will occur. Likely due to a Read/Write mismatch.";
	}
}
