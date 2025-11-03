using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct SRand
{
	public SRand(int seed)
	{
		this._seed = (uint)seed;
		this._state = this._seed;
	}

	public SRand(uint seed)
	{
		this._seed = seed;
		this._state = this._seed;
	}

	public SRand(long seed)
	{
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public SRand(DateTime seed)
	{
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public SRand(string seed)
	{
		if (string.IsNullOrEmpty(seed))
		{
			throw new ArgumentException("Seed cannot be null or empty", "seed");
		}
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public SRand(byte[] seed)
	{
		if (seed == null || seed.Length == 0)
		{
			throw new ArgumentException("Seed cannot be null or empty", "seed");
		}
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public double NextDouble()
	{
		return this.NextState() % 268435457U * 3.725290298461914E-09;
	}

	public double NextDouble(double max)
	{
		if (max < 0.0)
		{
			return 0.0;
		}
		return this.NextDouble() * max;
	}

	public double NextDouble(double min, double max)
	{
		double num = max - min;
		if (num <= 0.0)
		{
			return min;
		}
		double num2 = this.NextDouble() * num;
		return min + num2;
	}

	public float NextFloat()
	{
		return (float)this.NextDouble();
	}

	public float NextFloat(float max)
	{
		return (float)this.NextDouble((double)max);
	}

	public float NextFloat(float min, float max)
	{
		return (float)this.NextDouble((double)min, (double)max);
	}

	public bool NextBool()
	{
		return this.NextState() % 2U == 1U;
	}

	public uint NextUInt()
	{
		return this.NextState();
	}

	public int NextInt()
	{
		return (int)this.NextState();
	}

	public int NextInt(int max)
	{
		if (max <= 0)
		{
			return 0;
		}
		return (int)((ulong)this.NextState() % (ulong)((long)max));
	}

	public int NextInt(int min, int max)
	{
		int num = max - min;
		if (num <= 0)
		{
			return min;
		}
		return min + this.NextInt(num);
	}

	public int NextIntWithExclusion(int min, int max, int exclude)
	{
		int num = max - min - 1;
		if (num <= 0)
		{
			return min;
		}
		int num2 = min + 1 + this.NextInt(num);
		if (num2 > exclude)
		{
			return num2;
		}
		return num2 - 1;
	}

	public int NextIntWithExclusion2(int min, int max, int exclude, int exclude2)
	{
		if (exclude == exclude2)
		{
			return this.NextIntWithExclusion(min, max, exclude);
		}
		int num = max - min - 2;
		if (num <= 0)
		{
			return min;
		}
		int num2 = min + 2 + this.NextInt(num);
		int num3;
		int num4;
		if (exclude >= exclude2)
		{
			num3 = exclude2 + 1;
			num4 = exclude;
		}
		else
		{
			num3 = exclude + 1;
			num4 = exclude2;
		}
		if (num2 <= num3)
		{
			return num2 - 2;
		}
		if (num2 <= num4)
		{
			return num2 - 1;
		}
		return num2;
	}

	public byte NextByte()
	{
		return (byte)(this.NextState() & 255U);
	}

	public Color32 NextColor32()
	{
		byte b = this.NextByte();
		byte b2 = this.NextByte();
		byte b3 = this.NextByte();
		return new Color32(b, b2, b3, byte.MaxValue);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3 NextPointInsideSphere(float radius)
	{
		float num = this.NextFloat() * 2f - 1f;
		float num2 = this.NextFloat() * 2f - 1f;
		float num3 = this.NextFloat() * 2f - 1f;
		float num4 = MathF.Pow(this.NextFloat(), 0.33333334f);
		float num5 = 1f / MathF.Sqrt(num * num + num2 * num2 + num3 * num3);
		return new Vector3(num * num5 * num4 * radius, num2 * num5 * num4 * radius, num3 * num5 * num4 * radius);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3 NextPointOnSphere(float radius)
	{
		float num = this.NextFloat() * 2f - 1f;
		float num2 = this.NextFloat() * 2f - 1f;
		float num3 = this.NextFloat() * 2f - 1f;
		float num4 = 1f / MathF.Sqrt(num * num + num2 * num2 + num3 * num3);
		return new Vector3(num * num4 * radius, num2 * num4 * radius, num3 * num4 * radius);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3 NextPointInsideBox(Vector3 extents)
	{
		float num = this.NextFloat() - 0.5f;
		float num2 = this.NextFloat() - 0.5f;
		float num3 = this.NextFloat() - 0.5f;
		return new Vector3(num * extents.x, num2 * extents.y, num3 * extents.z);
	}

	public Color NextColor()
	{
		float num = this.NextFloat();
		float num2 = this.NextFloat();
		float num3 = this.NextFloat();
		return new Color(num, num2, num3, 1f);
	}

	public void Shuffle<T>(T[] array)
	{
		int i = array.Length;
		while (i > 1)
		{
			int num = this.NextInt(i--);
			int num2 = i;
			int num3 = num;
			T t = array[num];
			T t2 = array[i];
			array[num2] = t;
			array[num3] = t2;
		}
	}

	public void Shuffle<T>(List<T> list)
	{
		int i = list.Count;
		while (i > 1)
		{
			int num = this.NextInt(i--);
			int num2 = i;
			int num3 = num;
			T t = list[num];
			T t2 = list[i];
			list[num2] = t;
			list[num3] = t2;
		}
	}

	public void Reset()
	{
		this._state = this._seed;
	}

	public void Reset(int seed)
	{
		this._seed = (uint)seed;
		this._state = this._seed;
	}

	public void Reset(uint seed)
	{
		this._seed = seed;
		this._state = this._seed;
	}

	public void Reset(long seed)
	{
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public void Reset(DateTime seed)
	{
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public void Reset(string seed)
	{
		if (string.IsNullOrEmpty(seed))
		{
			throw new ArgumentException("Seed cannot be null or empty", "seed");
		}
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	public void Reset(byte[] seed)
	{
		if (seed == null || seed.Length == 0)
		{
			throw new ArgumentException("Seed cannot be null or empty", "seed");
		}
		this._seed = (uint)StaticHash.Compute(seed);
		this._state = this._seed;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private uint NextState()
	{
		return this._state = this.Mix(this._state + 184402071U);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private uint Mix(uint x)
	{
		x = ((x >> 17) ^ x) * 3982152891U;
		x = ((x >> 11) ^ x) * 2890668881U;
		x = ((x >> 15) ^ x) * 830770091U;
		x = (x >> 14) ^ x;
		return x;
	}

	public override int GetHashCode()
	{
		return StaticHash.Compute((int)this._seed, (int)this._state);
	}

	public override string ToString()
	{
		return string.Format("{0} {{ {1}: {2:X8} {3}: {4:X8} }}", new object[] { "SRand", "_seed", this._seed, "_state", this._state });
	}

	public static SRand New()
	{
		return new SRand(DateTime.UtcNow);
	}

	public static explicit operator SRand(int seed)
	{
		return new SRand(seed);
	}

	public static explicit operator SRand(uint seed)
	{
		return new SRand(seed);
	}

	public static explicit operator SRand(long seed)
	{
		return new SRand(seed);
	}

	public static explicit operator SRand(string seed)
	{
		return new SRand(seed);
	}

	public static explicit operator SRand(byte[] seed)
	{
		return new SRand(seed);
	}

	public static explicit operator SRand(DateTime seed)
	{
		return new SRand(seed);
	}

	[SerializeField]
	private uint _seed;

	[SerializeField]
	private uint _state;

	private const double MAX_AS_DOUBLE = 268435456.0;

	private const uint MAX_PLUS_ONE = 268435457U;

	private const double STEP_SIZE = 3.725290298461914E-09;

	private const float ONE_THIRD = 0.33333334f;
}
