using System;
using System.Collections.Generic;

public class EnumData<TEnum> where TEnum : struct, Enum
{
	public static EnumData<TEnum> Shared { get; } = new EnumData<TEnum>();

	private EnumData()
	{
		this.Names = Enum.GetNames(typeof(TEnum));
		this.Values = (TEnum[])Enum.GetValues(typeof(TEnum));
		int num = this.Names.Length;
		this.LongValues = new long[num];
		this.EnumToName = new Dictionary<TEnum, string>(num);
		this.NameToEnum = new Dictionary<string, TEnum>(num * 2);
		this.EnumToIndex = new Dictionary<TEnum, int>(num);
		this.IndexToEnum = new Dictionary<int, TEnum>(num);
		this.EnumToLong = new Dictionary<TEnum, long>(num);
		this.LongToEnum = new Dictionary<long, TEnum>(num);
		long num2 = long.MaxValue;
		long num3 = long.MinValue;
		for (int i = 0; i < this.Names.Length; i++)
		{
			string text = this.Names[i];
			TEnum tenum = this.Values[i];
			long num4 = Convert.ToInt64(tenum);
			this.LongValues[i] = num4;
			this.EnumToName[tenum] = text;
			this.NameToEnum[text] = tenum;
			this.NameToEnum.TryAdd(text.ToLowerInvariant(), tenum);
			this.EnumToIndex[tenum] = i;
			this.IndexToEnum[i] = tenum;
			this.EnumToLong[tenum] = num4;
			this.LongToEnum[num4] = tenum;
			num2 = Math.Min(num4, num2);
			num3 = Math.Max(num4, num3);
		}
		for (int j = 0; j < this.Names.Length; j++)
		{
			string text2 = this.Names[j];
			TEnum tenum2 = this.Values[j];
			this.NameToEnum[text2] = tenum2;
		}
		this.MinValue = this.LongToEnum[num2];
		this.MaxValue = this.LongToEnum[num3];
		this.MinInt = Convert.ToInt32(num2);
		this.MaxInt = Convert.ToInt32(num3);
		this.MinLong = num2;
		this.MaxLong = num3;
		long num5 = 0L;
		bool flag = true;
		foreach (long num6 in this.LongValues)
		{
			if (num6 != 0L && (num6 & (num6 - 1L)) != 0L && (num5 & num6) != num6)
			{
				flag = false;
				break;
			}
			num5 |= num6;
		}
		this.IsBitMaskCompatible = flag;
	}

	public readonly string[] Names;

	public readonly TEnum[] Values;

	public readonly long[] LongValues;

	public readonly bool IsBitMaskCompatible;

	public readonly Dictionary<TEnum, string> EnumToName;

	public readonly Dictionary<string, TEnum> NameToEnum;

	public readonly Dictionary<TEnum, int> EnumToIndex;

	public readonly Dictionary<int, TEnum> IndexToEnum;

	public readonly Dictionary<TEnum, long> EnumToLong;

	public readonly Dictionary<long, TEnum> LongToEnum;

	public readonly TEnum MinValue;

	public readonly TEnum MaxValue;

	public readonly int MinInt;

	public readonly int MaxInt;

	public readonly long MinLong;

	public readonly long MaxLong;
}
