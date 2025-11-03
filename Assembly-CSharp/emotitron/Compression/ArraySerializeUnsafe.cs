using System;

namespace emotitron.Compression
{
	public static class ArraySerializeUnsafe
	{
		public unsafe static void WriteSigned(ulong* buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(buffer, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void AppendSigned(ulong* buffer, int value, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Append(buffer, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void AddSigned(this int value, ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Append(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void AddSigned(this short value, ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Append(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void AddSigned(this sbyte value, ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Append(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void InjectSigned(this int value, ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void InjectSigned(this short value, ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void InjectSigned(this sbyte value, ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void PokeSigned(this int value, ulong* uPtr, int bitposition, int bits)
		{
			uint num = (uint)((value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void PokeSigned(this short value, ulong* uPtr, int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static void PokeSigned(this sbyte value, ulong* uPtr, int bitposition, int bits)
		{
			uint num = (uint)(((int)value << 1) ^ (value >> 31));
			ArraySerializeUnsafe.Write(uPtr, (ulong)num, ref bitposition, bits);
		}

		public unsafe static int ReadSigned(ulong* uPtr, ref int bitposition, int bits)
		{
			uint num = (uint)ArraySerializeUnsafe.Read(uPtr, ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public unsafe static int PeekSigned(ulong* uPtr, int bitposition, int bits)
		{
			uint num = (uint)ArraySerializeUnsafe.Read(uPtr, ref bitposition, bits);
			return (int)((ulong)(num >> 1) ^ (ulong)((long)(-(long)(num & 1U))));
		}

		public unsafe static void Append(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 63;
			int num2 = bitposition >> 6;
			ulong num3 = (1UL << num) - 1UL;
			ulong num4 = (uPtr[num2] & num3) | (value << num);
			uPtr[num2] = num4;
			uPtr[num2 + 1] = num4 >> 64 - num;
			bitposition += bits;
		}

		public unsafe static void Write(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = bitposition & 63;
			int num2 = bitposition >> 6;
			ulong num3 = ulong.MaxValue >> 64 - bits;
			ulong num4 = num3 << num;
			ulong num5 = value << num;
			uPtr[num2] = (uPtr[num2] & ~num4) | (num5 & num4);
			num = 64 - num;
			if (num < bits)
			{
				num4 = num3 >> num;
				num5 = value >> num;
				num2++;
				uPtr[num2] = (uPtr[num2] & ~num4) | (num5 & num4);
			}
			bitposition += bits;
		}

		public unsafe static ulong Read(ulong* uPtr, ref int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int i = bitposition & 63;
			int num = bitposition >> 6;
			ulong num2 = ulong.MaxValue >> 64 - bits;
			ulong num3 = uPtr[num] >> i;
			for (i = 64 - i; i < bits; i += 64)
			{
				num++;
				num3 |= uPtr[num] << i;
			}
			bitposition += bits;
			return num3 & num2;
		}

		public unsafe static ulong Read(ulong* uPtr, int bitposition, int bits)
		{
			if (bits == 0)
			{
				return 0UL;
			}
			int i = bitposition & 63;
			int num = bitposition >> 6;
			ulong num2 = ulong.MaxValue >> 64 - bits;
			ulong num3 = uPtr[num] >> i;
			for (i = 64 - i; i < bits; i += 64)
			{
				num++;
				num3 |= uPtr[num] << i;
			}
			bitposition += bits;
			return num3 & num2;
		}

		public unsafe static void Add(this ulong value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, value, ref bitposition, bits);
		}

		public unsafe static void Add(this uint value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void Add(this ushort value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void Add(this byte value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void AddUnsigned(this long value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void AddUnsigned(this int value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void AddUnsigned(this short value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void AddUnsigned(this sbyte value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Append(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void Inject(this ulong value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, value, ref bitposition, bits);
		}

		public unsafe static void Inject(this uint value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void Inject(this ushort value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void Inject(this byte value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void InjectUnsigned(this long value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void InjectUnsigned(this int value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void InjectUnsigned(this short value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void InjectUnsigned(this sbyte value, ulong* uPtr, ref int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void Poke(this ulong value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, value, ref bitposition, bits);
		}

		public unsafe static void Poke(this uint value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void Poke(this ushort value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void Poke(this byte value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void InjectUnsigned(this long value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)value, ref bitposition, bits);
		}

		public unsafe static void InjectUnsigned(this int value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void PokeUnsigned(this short value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void PokeUnsigned(this sbyte value, ulong* uPtr, int bitposition, int bits)
		{
			ArraySerializeUnsafe.Write(uPtr, (ulong)((long)value), ref bitposition, bits);
		}

		public unsafe static void ReadOutUnsafe(ulong* sourcePtr, int sourcePos, ulong* targetPtr, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int num2;
			for (int i = bits; i > 0; i -= num2)
			{
				num2 = ((i > 64) ? 64 : i);
				ulong num3 = ArraySerializeUnsafe.Read(sourcePtr, ref num, num2);
				ArraySerializeUnsafe.Write(targetPtr, num3, ref targetPos, num2);
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this ulong[] source, int sourcePos, byte[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (ulong[] array = source)
			{
				ulong* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (byte[] array2 = target)
				{
					byte* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr2;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr, ref num, num2);
						ArraySerializeUnsafe.Write(ptr3, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this ulong[] source, int sourcePos, uint[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (ulong[] array = source)
			{
				ulong* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (uint[] array2 = target)
				{
					uint* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr2;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr, ref num, num2);
						ArraySerializeUnsafe.Write(ptr3, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this ulong[] source, int sourcePos, ulong[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (ulong[] array = source)
			{
				ulong* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (ulong[] array2 = target)
				{
					ulong* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr, ref num, num2);
						ArraySerializeUnsafe.Write(ptr2, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this uint[] source, int sourcePos, byte[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (uint[] array = source)
			{
				uint* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (byte[] array2 = target)
				{
					byte* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr;
					ulong* ptr4 = (ulong*)ptr2;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr3, ref num, num2);
						ArraySerializeUnsafe.Write(ptr4, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this uint[] source, int sourcePos, uint[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (uint[] array = source)
			{
				uint* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (uint[] array2 = target)
				{
					uint* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr;
					ulong* ptr4 = (ulong*)ptr2;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr3, ref num, num2);
						ArraySerializeUnsafe.Write(ptr4, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this uint[] source, int sourcePos, ulong[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (uint[] array = source)
			{
				uint* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (ulong[] array2 = target)
				{
					ulong* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr3, ref num, num2);
						ArraySerializeUnsafe.Write(ptr2, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this byte[] source, int sourcePos, ulong[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (byte[] array = source)
			{
				byte* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (ulong[] array2 = target)
				{
					ulong* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr3, ref num, num2);
						ArraySerializeUnsafe.Write(ptr2, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this byte[] source, int sourcePos, uint[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (byte[] array = source)
			{
				byte* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (uint[] array2 = target)
				{
					uint* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr;
					ulong* ptr4 = (ulong*)ptr2;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr3, ref num, num2);
						ArraySerializeUnsafe.Write(ptr4, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		public unsafe static void ReadOutUnsafe(this byte[] source, int sourcePos, byte[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
			{
				return;
			}
			int num = sourcePos;
			int i = bits;
			fixed (byte[] array = source)
			{
				byte* ptr;
				if (source == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (byte[] array2 = target)
				{
					byte* ptr2;
					if (target == null || array2.Length == 0)
					{
						ptr2 = null;
					}
					else
					{
						ptr2 = &array2[0];
					}
					ulong* ptr3 = (ulong*)ptr;
					ulong* ptr4 = (ulong*)ptr2;
					while (i > 0)
					{
						int num2 = ((i > 64) ? 64 : i);
						ulong num3 = ArraySerializeUnsafe.Read(ptr3, ref num, num2);
						ArraySerializeUnsafe.Write(ptr4, num3, ref targetPos, num2);
						i -= num2;
					}
				}
			}
			targetPos += bits;
		}

		private const string bufferOverrunMsg = "Byte buffer overrun. Dataloss will occur.";
	}
}
