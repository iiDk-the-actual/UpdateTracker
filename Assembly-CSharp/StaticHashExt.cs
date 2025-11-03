using System;

public static class StaticHashExt
{
	public static int GetStaticHash(this int i)
	{
		return StaticHash.Compute(i);
	}

	public static int GetStaticHash(this uint u)
	{
		return StaticHash.Compute(u);
	}

	public static int GetStaticHash(this float f)
	{
		return StaticHash.Compute(f);
	}

	public static int GetStaticHash(this long l)
	{
		return StaticHash.Compute(l);
	}

	public static int GetStaticHash(this double d)
	{
		return StaticHash.Compute(d);
	}

	public static int GetStaticHash(this bool b)
	{
		return StaticHash.Compute(b);
	}

	public static int GetStaticHash(this DateTime dt)
	{
		return StaticHash.Compute(dt);
	}

	public static int GetStaticHash(this string s)
	{
		return StaticHash.Compute(s);
	}

	public static int GetStaticHash(this byte[] bytes)
	{
		return StaticHash.Compute(bytes);
	}
}
