using System;
using UnityEngine;

[Serializable]
public struct StringEnum<TEnum> where TEnum : struct, Enum
{
	public TEnum Value
	{
		get
		{
			return this.m_EnumValue;
		}
	}

	public static implicit operator StringEnum<TEnum>(TEnum e)
	{
		return new StringEnum<TEnum>
		{
			m_EnumValue = e
		};
	}

	public static implicit operator TEnum(StringEnum<TEnum> se)
	{
		return se.m_EnumValue;
	}

	public static bool operator ==(StringEnum<TEnum> left, StringEnum<TEnum> right)
	{
		return left.m_EnumValue.Equals(right.m_EnumValue);
	}

	public static bool operator !=(StringEnum<TEnum> left, StringEnum<TEnum> right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is StringEnum<TEnum>)
		{
			StringEnum<TEnum> stringEnum = (StringEnum<TEnum>)obj;
			return this.m_EnumValue.Equals(stringEnum.m_EnumValue);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.m_EnumValue.GetHashCode();
	}

	[SerializeField]
	private TEnum m_EnumValue;
}
