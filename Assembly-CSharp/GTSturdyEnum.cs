using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GTSturdyEnum<TEnum> : ISerializationCallbackReceiver where TEnum : struct, Enum
{
	public TEnum Value { readonly get; private set; }

	public static implicit operator GTSturdyEnum<TEnum>(TEnum value)
	{
		return new GTSturdyEnum<TEnum>
		{
			Value = value
		};
	}

	public static implicit operator TEnum(GTSturdyEnum<TEnum> sturdyEnum)
	{
		return sturdyEnum.Value;
	}

	public void OnBeforeSerialize()
	{
		EnumData<TEnum> shared = EnumData<TEnum>.Shared;
		if (!shared.IsBitMaskCompatible)
		{
			this.m_stringValuePairs = new GTSturdyEnum<TEnum>.EnumPair[1];
			GTSturdyEnum<TEnum>.EnumPair[] stringValuePairs = this.m_stringValuePairs;
			int num = 0;
			GTSturdyEnum<TEnum>.EnumPair enumPair = default(GTSturdyEnum<TEnum>.EnumPair);
			TEnum tenum = this.Value;
			enumPair.Name = tenum.ToString();
			enumPair.FallbackValue = this.Value;
			stringValuePairs[num] = enumPair;
			return;
		}
		long num2 = Convert.ToInt64(this.Value);
		if (num2 == 0L)
		{
			GTSturdyEnum<TEnum>.EnumPair[] array = new GTSturdyEnum<TEnum>.EnumPair[1];
			int num3 = 0;
			GTSturdyEnum<TEnum>.EnumPair enumPair = default(GTSturdyEnum<TEnum>.EnumPair);
			TEnum tenum = this.Value;
			enumPair.Name = tenum.ToString();
			enumPair.FallbackValue = this.Value;
			array[num3] = enumPair;
			this.m_stringValuePairs = array;
			return;
		}
		List<GTSturdyEnum<TEnum>.EnumPair> list = new List<GTSturdyEnum<TEnum>.EnumPair>(shared.Values.Length);
		for (int i = 0; i < shared.Values.Length; i++)
		{
			long num4 = shared.LongValues[i];
			if (num4 != 0L && (num2 & num4) == num4)
			{
				TEnum tenum2 = shared.Values[i];
				list.Add(new GTSturdyEnum<TEnum>.EnumPair
				{
					Name = tenum2.ToString(),
					FallbackValue = tenum2
				});
			}
		}
		this.m_stringValuePairs = list.ToArray();
	}

	public void OnAfterDeserialize()
	{
		EnumData<TEnum> shared = EnumData<TEnum>.Shared;
		if (this.m_stringValuePairs == null || this.m_stringValuePairs.Length == 0)
		{
			if (shared.IsBitMaskCompatible)
			{
				this.Value = (TEnum)((object)Enum.ToObject(typeof(TEnum), 0L));
				return;
			}
			this.Value = default(TEnum);
			return;
		}
		else
		{
			if (shared.IsBitMaskCompatible)
			{
				long num = 0L;
				foreach (GTSturdyEnum<TEnum>.EnumPair enumPair in this.m_stringValuePairs)
				{
					TEnum tenum;
					long num2;
					if (shared.NameToEnum.TryGetValue(enumPair.Name, out tenum))
					{
						num |= shared.EnumToLong[tenum];
					}
					else if (shared.EnumToLong.TryGetValue(enumPair.FallbackValue, out num2))
					{
						num |= num2;
					}
				}
				this.Value = (TEnum)((object)Enum.ToObject(typeof(TEnum), num));
				return;
			}
			GTSturdyEnum<TEnum>.EnumPair enumPair2 = this.m_stringValuePairs[0];
			TEnum tenum2;
			if (shared.NameToEnum.TryGetValue(enumPair2.Name, out tenum2))
			{
				this.Value = tenum2;
				return;
			}
			this.Value = enumPair2.FallbackValue;
			return;
		}
	}

	[SerializeField]
	private GTSturdyEnum<TEnum>.EnumPair[] m_stringValuePairs;

	[Serializable]
	private struct EnumPair
	{
		public string Name;

		public TEnum FallbackValue;
	}
}
