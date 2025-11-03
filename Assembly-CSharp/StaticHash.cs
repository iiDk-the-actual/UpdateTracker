using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class StaticHash
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(int i)
	{
		uint num = (uint)(i + 2127912214 + (i << 12));
		num = num ^ 3345072700U ^ (num >> 19);
		num = num + 374761393U + (num << 5);
		num = (num + 3550635116U) ^ (num << 9);
		num = num + 4251993797U + (num << 3);
		return (int)(num ^ 3042594569U ^ (num >> 16));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(uint u)
	{
		uint num = u + 2127912214U + (u << 12);
		num = num ^ 3345072700U ^ (num >> 19);
		num = num + 374761393U + (num << 5);
		num = (num + 3550635116U) ^ (num << 9);
		num = num + 4251993797U + (num << 3);
		return (int)(num ^ 3042594569U ^ (num >> 16));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int Compute(float f)
	{
		return StaticHash.Compute(*Unsafe.As<float, int>(ref f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(float f1, float f2)
	{
		int num = StaticHash.Compute(f1);
		int num2 = StaticHash.Compute(f2);
		return StaticHash.Compute(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(float f1, float f2, float f3)
	{
		int num = StaticHash.Compute(f1);
		int num2 = StaticHash.Compute(f2);
		int num3 = StaticHash.Compute(f3);
		return StaticHash.Compute(num, num2, num3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(float f1, float f2, float f3, float f4)
	{
		int num = StaticHash.Compute(f1);
		int num2 = StaticHash.Compute(f2);
		int num3 = StaticHash.Compute(f3);
		int num4 = StaticHash.Compute(f4);
		return StaticHash.Compute(num, num2, num3, num4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(long l)
	{
		ulong num = (ulong)(~(ulong)l + (l << 18));
		num ^= num >> 31;
		num *= 21UL;
		num ^= num >> 11;
		num += num << 6;
		num ^= num >> 22;
		return (int)num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(long l1, long l2)
	{
		int num = StaticHash.Compute(l1);
		int num2 = StaticHash.Compute(l2);
		return StaticHash.Compute(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(long l1, long l2, long l3)
	{
		int num = StaticHash.Compute(l1);
		int num2 = StaticHash.Compute(l2);
		int num3 = StaticHash.Compute(l3);
		return StaticHash.Compute(num, num2, num3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(long l1, long l2, long l3, long l4)
	{
		int num = StaticHash.Compute(l1);
		int num2 = StaticHash.Compute(l2);
		int num3 = StaticHash.Compute(l3);
		int num4 = StaticHash.Compute(l4);
		return StaticHash.Compute(num, num2, num3, num4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int Compute(double d)
	{
		return StaticHash.Compute(*Unsafe.As<double, long>(ref d));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(double d1, double d2)
	{
		int num = StaticHash.Compute(d1);
		int num2 = StaticHash.Compute(d2);
		return StaticHash.Compute(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(double d1, double d2, double d3)
	{
		int num = StaticHash.Compute(d1);
		int num2 = StaticHash.Compute(d2);
		int num3 = StaticHash.Compute(d3);
		return StaticHash.Compute(num, num2, num3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(double d1, double d2, double d3, double d4)
	{
		int num = StaticHash.Compute(d1);
		int num2 = StaticHash.Compute(d2);
		int num3 = StaticHash.Compute(d3);
		int num4 = StaticHash.Compute(d4);
		return StaticHash.Compute(num, num2, num3, num4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(bool b)
	{
		if (!b)
		{
			return 1800329511;
		}
		return -1266253386;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(bool b1, bool b2)
	{
		int num = StaticHash.Compute(b1);
		int num2 = StaticHash.Compute(b2);
		return StaticHash.Compute(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(bool b1, bool b2, bool b3)
	{
		int num = StaticHash.Compute(b1);
		int num2 = StaticHash.Compute(b2);
		int num3 = StaticHash.Compute(b3);
		return StaticHash.Compute(num, num2, num3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(bool b1, bool b2, bool b3, bool b4)
	{
		int num = StaticHash.Compute(b1);
		int num2 = StaticHash.Compute(b2);
		int num3 = StaticHash.Compute(b3);
		int num4 = StaticHash.Compute(b4);
		return StaticHash.Compute(num, num2, num3, num4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(DateTime dt)
	{
		return StaticHash.Compute(dt.ToBinary());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(string s)
	{
		if (s == null || s.Length == 0)
		{
			return 0;
		}
		int i = s.Length;
		uint num = (uint)i;
		int num2 = i & 1;
		i >>= 1;
		int num3 = 0;
		while (i > 0)
		{
			num += (uint)s[num3];
			uint num4 = (uint)((uint)s[num3 + 1] << 11) ^ num;
			num = (num << 16) ^ num4;
			num3 += 2;
			num += num >> 11;
			i--;
		}
		if (num2 == 1)
		{
			num += (uint)s[num3];
			num ^= num << 11;
			num += num >> 17;
		}
		num ^= num << 3;
		num += num >> 5;
		num ^= num << 4;
		num += num >> 17;
		num ^= num << 25;
		return (int)(num + (num >> 6));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(string s1, string s2)
	{
		int num = StaticHash.Compute(s1);
		int num2 = StaticHash.Compute(s2);
		return StaticHash.Compute(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(string s1, string s2, string s3)
	{
		int num = StaticHash.Compute(s1);
		int num2 = StaticHash.Compute(s2);
		int num3 = StaticHash.Compute(s3);
		return StaticHash.Compute(num, num2, num3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(string s1, string s2, string s3, string s4)
	{
		int num = StaticHash.Compute(s1);
		int num2 = StaticHash.Compute(s2);
		int num3 = StaticHash.Compute(s3);
		int num4 = StaticHash.Compute(s4);
		return StaticHash.Compute(num, num2, num3, num4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(byte[] bytes)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return 0;
		}
		int i = bytes.Length;
		uint num = (uint)i;
		int num2 = i & 1;
		i >>= 1;
		int num3 = 0;
		while (i > 0)
		{
			num += (uint)bytes[num3];
			uint num4 = (uint)(((int)bytes[num3 + 1] << 11) ^ (int)num);
			num = (num << 16) ^ num4;
			num3 += 2;
			num += num >> 11;
			i--;
		}
		if (num2 == 1)
		{
			num += (uint)bytes[num3];
			num ^= num << 11;
			num += num >> 17;
		}
		num ^= num << 3;
		num += num >> 5;
		num ^= num << 4;
		num += num >> 17;
		num ^= num << 25;
		return (int)(num + (num >> 6));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(int i1, int i2)
	{
		uint num = 3735928567U;
		uint num2 = num;
		uint num3 = num;
		num += (uint)i1;
		num2 += (uint)i2;
		StaticHash.Finalize(ref num, ref num2, ref num3);
		return (int)num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(int i1, int i2, int i3)
	{
		uint num = 3735928571U;
		uint num2 = num;
		uint num3 = num;
		num += (uint)i1;
		num2 += (uint)i2;
		num3 += (uint)i3;
		StaticHash.Finalize(ref num, ref num2, ref num3);
		return (int)num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(int i1, int i2, int i3, int i4)
	{
		uint num = 3735928575U;
		uint num2 = num;
		uint num3 = num;
		num += (uint)i1;
		num2 += (uint)i2;
		num3 += (uint)i3;
		StaticHash.Mix(ref num, ref num2, ref num3);
		num += (uint)i4;
		StaticHash.Finalize(ref num, ref num2, ref num3);
		return (int)num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(int[] values)
	{
		if (values == null || values.Length == 0)
		{
			return 224428569;
		}
		int num = values.Length;
		uint num2 = (uint)(-559038737 + (num << 2));
		uint num3 = num2;
		uint num4 = num2;
		int num5 = 0;
		while (num - num5 > 3)
		{
			num2 += (uint)values[num5];
			num3 += (uint)values[num5 + 1];
			num4 += (uint)values[num5 + 2];
			StaticHash.Mix(ref num2, ref num3, ref num4);
			num5 += 3;
		}
		if (num - num5 > 2)
		{
			num4 += (uint)values[num5 + 2];
		}
		if (num - num5 > 1)
		{
			num3 += (uint)values[num5 + 1];
		}
		if (num - num5 > 0)
		{
			num2 += (uint)values[num5];
			StaticHash.Finalize(ref num2, ref num3, ref num4);
		}
		return (int)num4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(uint[] values)
	{
		if (values == null || values.Length == 0)
		{
			return 224428569;
		}
		int num = values.Length;
		uint num2 = (uint)(-559038737 + (num << 2));
		uint num3 = num2;
		uint num4 = num2;
		int num5 = 0;
		while (num - num5 > 3)
		{
			num2 += values[num5];
			num3 += values[num5 + 1];
			num4 += values[num5 + 2];
			StaticHash.Mix(ref num2, ref num3, ref num4);
			num5 += 3;
		}
		if (num - num5 > 2)
		{
			num4 += values[num5 + 2];
		}
		if (num - num5 > 1)
		{
			num3 += values[num5 + 1];
		}
		if (num - num5 > 0)
		{
			num2 += values[num5];
			StaticHash.Finalize(ref num2, ref num3, ref num4);
		}
		return (int)num4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(uint u1, uint u2)
	{
		uint num = 3735928567U;
		uint num2 = num;
		uint num3 = num;
		num += u1;
		num2 += u2;
		StaticHash.Finalize(ref num, ref num2, ref num3);
		return (int)num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(uint u1, uint u2, uint u3)
	{
		uint num = 3735928571U;
		uint num2 = num;
		uint num3 = num;
		num += u1;
		num2 += u2;
		num3 += u3;
		StaticHash.Finalize(ref num, ref num2, ref num3);
		return (int)num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compute(uint u1, uint u2, uint u3, uint u4)
	{
		uint num = 3735928575U;
		uint num2 = num;
		uint num3 = num;
		num += u1;
		num2 += u2;
		num3 += u3;
		StaticHash.Mix(ref num, ref num2, ref num3);
		num += u4;
		StaticHash.Finalize(ref num, ref num2, ref num3);
		return (int)num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ComputeOrderAgnostic(int[] values)
	{
		if (values == null || values.Length == 0)
		{
			return 0;
		}
		uint num = (uint)StaticHash.Compute(values[0]);
		if (values.Length == 1)
		{
			return (int)num;
		}
		for (int i = 1; i < values.Length; i++)
		{
			num += (uint)StaticHash.Compute(values[i]);
		}
		return (int)num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long Compute128To64(long a, long b)
	{
		ulong num = (ulong)((b ^ a) * -7070675565921424023L);
		num ^= num >> 47;
		long num2 = (a ^ (long)num) * -7070675565921424023L;
		return (num2 ^ (long)((ulong)num2 >> 47)) * -7070675565921424023L;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long Compute128To64(ulong a, ulong b)
	{
		ulong num = (b ^ a) * 11376068507788127593UL;
		num ^= num >> 47;
		ulong num2 = (a ^ num) * 11376068507788127593UL;
		return (long)((num2 ^ (num2 >> 47)) * 11376068507788127593UL);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ComputeTriple32(int i)
	{
		int num = i + 1;
		int num2 = (num ^ (int)((uint)num >> 17)) * -312814405;
		int num3 = (num2 ^ (int)((uint)num2 >> 11)) * -1404298415;
		int num4 = (num3 ^ (int)((uint)num3 >> 15)) * 830770091;
		return num4 ^ (int)((uint)num4 >> 14);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ReverseTriple32(int i)
	{
		uint num = (uint)(i ^ (int)(((uint)i >> 14) ^ ((uint)i >> 28)));
		num *= 850532099U;
		num ^= (num >> 15) ^ (num >> 30);
		num *= 1184763313U;
		num ^= (num >> 11) ^ (num >> 22);
		num *= 2041073779U;
		num ^= num >> 17;
		return (int)(num - 1U);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Mix(ref uint a, ref uint b, ref uint c)
	{
		a -= c;
		a ^= StaticHash.Rotate(c, 4);
		c += b;
		b -= a;
		b ^= StaticHash.Rotate(a, 6);
		a += c;
		c -= b;
		c ^= StaticHash.Rotate(b, 8);
		b += a;
		a -= c;
		a ^= StaticHash.Rotate(c, 16);
		c += b;
		b -= a;
		b ^= StaticHash.Rotate(a, 19);
		a += c;
		c -= b;
		c ^= StaticHash.Rotate(b, 4);
		b += a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Finalize(ref uint a, ref uint b, ref uint c)
	{
		c ^= b;
		c -= StaticHash.Rotate(b, 14);
		a ^= c;
		a -= StaticHash.Rotate(c, 11);
		b ^= a;
		b -= StaticHash.Rotate(a, 25);
		c ^= b;
		c -= StaticHash.Rotate(b, 16);
		a ^= c;
		a -= StaticHash.Rotate(c, 4);
		b ^= a;
		b -= StaticHash.Rotate(a, 14);
		c ^= b;
		c -= StaticHash.Rotate(b, 24);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Rotate(uint x, int k)
	{
		return (x << k) | (x >> 32 - k);
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct SingleInt32
	{
		[FieldOffset(0)]
		public float single;

		[FieldOffset(0)]
		public int int32;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct DoubleInt64
	{
		[FieldOffset(0)]
		public double @double;

		[FieldOffset(0)]
		public long int64;
	}
}
