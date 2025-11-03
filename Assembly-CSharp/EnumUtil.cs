using System;
using System.Collections.Generic;

public static class EnumUtil
{
	public static string[] GetNames<TEnum>() where TEnum : struct, Enum
	{
		return ArrayUtils.Clone<string>(EnumData<TEnum>.Shared.Names);
	}

	public static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum
	{
		return ArrayUtils.Clone<TEnum>(EnumData<TEnum>.Shared.Values);
	}

	public static long[] GetLongValues<TEnum>() where TEnum : struct, Enum
	{
		return ArrayUtils.Clone<long>(EnumData<TEnum>.Shared.LongValues);
	}

	public static string EnumToName<TEnum>(TEnum e) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.EnumToName[e];
	}

	public static TEnum NameToEnum<TEnum>(string n) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.NameToEnum[n];
	}

	public static int EnumToIndex<TEnum>(TEnum e) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.EnumToIndex[e];
	}

	public static TEnum IndexToEnum<TEnum>(int i) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.IndexToEnum[i];
	}

	public static long EnumToLong<TEnum>(TEnum e) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.EnumToLong[e];
	}

	public static TEnum LongToEnum<TEnum>(long l) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.LongToEnum[l];
	}

	public static TEnum GetValue<TEnum>(int index) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.Values[index];
	}

	public static int GetIndex<TEnum>(TEnum value) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.EnumToIndex[value];
	}

	public static string GetName<TEnum>(TEnum value) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.EnumToName[value];
	}

	public static TEnum GetValue<TEnum>(string name) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.NameToEnum[name];
	}

	public static long GetLongValue<TEnum>(TEnum value) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.EnumToLong[value];
	}

	public static TEnum GetValue<TEnum>(long longValue) where TEnum : struct, Enum
	{
		return EnumData<TEnum>.Shared.LongToEnum[longValue];
	}

	public static TEnum[] SplitBitmask<TEnum>(TEnum bitmask) where TEnum : struct, Enum
	{
		return EnumUtil.SplitBitmask<TEnum>(Convert.ToInt64(bitmask));
	}

	public static TEnum[] SplitBitmask<TEnum>(long bitmaskLong) where TEnum : struct, Enum
	{
		EnumData<TEnum> shared = EnumData<TEnum>.Shared;
		if (!shared.IsBitMaskCompatible)
		{
			throw new ArgumentException("The enum type " + typeof(TEnum).Name + " is not bitmask-compatible.");
		}
		if (bitmaskLong == 0L)
		{
			return new TEnum[] { (TEnum)((object)Enum.ToObject(typeof(TEnum), 0L)) };
		}
		List<TEnum> list = new List<TEnum>(shared.Values.Length);
		for (int i = 0; i < shared.Values.Length; i++)
		{
			TEnum tenum = shared.Values[i];
			long num = shared.LongValues[i];
			if (num != 0L && (bitmaskLong & num) == num)
			{
				list.Add(tenum);
			}
		}
		return list.ToArray();
	}
}
