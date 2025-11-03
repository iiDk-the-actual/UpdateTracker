using System;
using System.Globalization;

namespace emotitron.Compression.HalfFloat
{
	[Serializable]
	public struct Half : IConvertible, IComparable, IComparable<Half>, IEquatable<Half>, IFormattable
	{
		public Half(float value)
		{
			this.value = HalfUtilities.Pack(value);
		}

		public ushort RawValue
		{
			get
			{
				return this.value;
			}
		}

		public static float[] ConvertToFloat(Half[] values)
		{
			float[] array = new float[values.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = HalfUtilities.Unpack(values[i].RawValue);
			}
			return array;
		}

		public static Half[] ConvertToHalf(float[] values)
		{
			Half[] array = new Half[values.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Half(values[i]);
			}
			return array;
		}

		public static bool IsInfinity(Half half)
		{
			return half == Half.PositiveInfinity || half == Half.NegativeInfinity;
		}

		public static bool IsNaN(Half half)
		{
			return half == Half.NaN;
		}

		public static bool IsNegativeInfinity(Half half)
		{
			return half == Half.NegativeInfinity;
		}

		public static bool IsPositiveInfinity(Half half)
		{
			return half == Half.PositiveInfinity;
		}

		public static bool operator <(Half left, Half right)
		{
			return left < right;
		}

		public static bool operator >(Half left, Half right)
		{
			return left > right;
		}

		public static bool operator <=(Half left, Half right)
		{
			return left <= right;
		}

		public static bool operator >=(Half left, Half right)
		{
			return left >= right;
		}

		public static bool operator ==(Half left, Half right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Half left, Half right)
		{
			return !left.Equals(right);
		}

		public static explicit operator Half(float value)
		{
			return new Half(value);
		}

		public static implicit operator float(Half value)
		{
			return HalfUtilities.Unpack(value.value);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, this.ToString(), Array.Empty<object>());
		}

		public string ToString(string format)
		{
			if (format == null)
			{
				return this.ToString();
			}
			return string.Format(CultureInfo.CurrentCulture, this.ToString(format, CultureInfo.CurrentCulture), Array.Empty<object>());
		}

		public string ToString(IFormatProvider formatProvider)
		{
			return string.Format(formatProvider, this.ToString(), Array.Empty<object>());
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (format == null)
			{
				this.ToString(formatProvider);
			}
			return string.Format(formatProvider, this.ToString(format, formatProvider), Array.Empty<object>());
		}

		public override int GetHashCode()
		{
			return (int)((this.value * 3 / 2) ^ this.value);
		}

		public int CompareTo(Half value)
		{
			if (this < value)
			{
				return -1;
			}
			if (this > value)
			{
				return 1;
			}
			if (this != value)
			{
				if (!Half.IsNaN(this))
				{
					return 1;
				}
				if (!Half.IsNaN(value))
				{
					return -1;
				}
			}
			return 0;
		}

		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is Half))
			{
				throw new ArgumentException("The argument value must be a SlimMath.Half.");
			}
			Half half = (Half)value;
			if (this < half)
			{
				return -1;
			}
			if (this > half)
			{
				return 1;
			}
			if (this != half)
			{
				if (!Half.IsNaN(this))
				{
					return 1;
				}
				if (!Half.IsNaN(half))
				{
					return -1;
				}
			}
			return 0;
		}

		public static bool Equals(ref Half value1, ref Half value2)
		{
			return value1.value == value2.value;
		}

		public bool Equals(Half other)
		{
			return other.value == this.value;
		}

		public override bool Equals(object obj)
		{
			return obj != null && !(obj.GetType() != base.GetType()) && this.Equals((Half)obj);
		}

		public TypeCode GetTypeCode()
		{
			return Type.GetTypeCode(typeof(Half));
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException("Invalid cast from SlimMath.Half to System.Char.");
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException("Invalid cast from SlimMath.Half to System.DateTime.");
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return this;
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return ((IConvertible)this).ToType(type, provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}

		private ushort value;

		public const int PrecisionDigits = 3;

		public const int MantissaBits = 11;

		public const int MaximumDecimalExponent = 4;

		public const int MaximumBinaryExponent = 15;

		public const int MinimumDecimalExponent = -4;

		public const int MinimumBinaryExponent = -14;

		public const int ExponentRadix = 2;

		public const int AdditionRounding = 1;

		public static readonly Half Epsilon = new Half(0.0004887581f);

		public static readonly Half MaxValue = new Half(65504f);

		public static readonly Half MinValue = new Half(6.103516E-05f);

		public static readonly Half NaN = new Half(float.NaN);

		public static readonly Half NegativeInfinity = new Half(float.NegativeInfinity);

		public static readonly Half PositiveInfinity = new Half(float.PositiveInfinity);
	}
}
