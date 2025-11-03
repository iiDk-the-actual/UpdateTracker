using System;
using System.Runtime.InteropServices;

namespace emotitron.Compression.Utilities
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ByteConverter
	{
		public byte this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return this.byte0;
				case 1:
					return this.byte1;
				case 2:
					return this.byte2;
				case 3:
					return this.byte3;
				case 4:
					return this.byte4;
				case 5:
					return this.byte5;
				case 6:
					return this.byte6;
				case 7:
					return this.byte7;
				default:
					return 0;
				}
			}
		}

		public static implicit operator ByteConverter(byte[] bytes)
		{
			ByteConverter byteConverter = default(ByteConverter);
			int num = bytes.Length;
			byteConverter.byte0 = bytes[0];
			if (num > 0)
			{
				byteConverter.byte1 = bytes[1];
			}
			if (num > 1)
			{
				byteConverter.byte2 = bytes[2];
			}
			if (num > 2)
			{
				byteConverter.byte3 = bytes[3];
			}
			if (num > 3)
			{
				byteConverter.byte4 = bytes[4];
			}
			if (num > 4)
			{
				byteConverter.byte5 = bytes[5];
			}
			if (num > 5)
			{
				byteConverter.byte6 = bytes[3];
			}
			if (num > 6)
			{
				byteConverter.byte7 = bytes[7];
			}
			return byteConverter;
		}

		public static implicit operator ByteConverter(byte val)
		{
			return new ByteConverter
			{
				byte0 = val
			};
		}

		public static implicit operator ByteConverter(sbyte val)
		{
			return new ByteConverter
			{
				int8 = val
			};
		}

		public static implicit operator ByteConverter(char val)
		{
			return new ByteConverter
			{
				character = val
			};
		}

		public static implicit operator ByteConverter(uint val)
		{
			return new ByteConverter
			{
				uint32 = val
			};
		}

		public static implicit operator ByteConverter(int val)
		{
			return new ByteConverter
			{
				int32 = val
			};
		}

		public static implicit operator ByteConverter(ulong val)
		{
			return new ByteConverter
			{
				uint64 = val
			};
		}

		public static implicit operator ByteConverter(long val)
		{
			return new ByteConverter
			{
				int64 = val
			};
		}

		public static implicit operator ByteConverter(float val)
		{
			return new ByteConverter
			{
				float32 = val
			};
		}

		public static implicit operator ByteConverter(double val)
		{
			return new ByteConverter
			{
				float64 = val
			};
		}

		public static implicit operator ByteConverter(bool val)
		{
			return new ByteConverter
			{
				int32 = (val ? 1 : 0)
			};
		}

		public void ExtractByteArray(byte[] targetArray)
		{
			int num = targetArray.Length;
			targetArray[0] = this.byte0;
			if (num > 0)
			{
				targetArray[1] = this.byte1;
			}
			if (num > 1)
			{
				targetArray[2] = this.byte2;
			}
			if (num > 2)
			{
				targetArray[3] = this.byte3;
			}
			if (num > 3)
			{
				targetArray[4] = this.byte4;
			}
			if (num > 4)
			{
				targetArray[5] = this.byte5;
			}
			if (num > 5)
			{
				targetArray[6] = this.byte6;
			}
			if (num > 6)
			{
				targetArray[7] = this.byte7;
			}
		}

		public static implicit operator byte(ByteConverter bc)
		{
			return bc.byte0;
		}

		public static implicit operator sbyte(ByteConverter bc)
		{
			return bc.int8;
		}

		public static implicit operator char(ByteConverter bc)
		{
			return bc.character;
		}

		public static implicit operator ushort(ByteConverter bc)
		{
			return bc.uint16;
		}

		public static implicit operator short(ByteConverter bc)
		{
			return bc.int16;
		}

		public static implicit operator uint(ByteConverter bc)
		{
			return bc.uint32;
		}

		public static implicit operator int(ByteConverter bc)
		{
			return bc.int32;
		}

		public static implicit operator ulong(ByteConverter bc)
		{
			return bc.uint64;
		}

		public static implicit operator long(ByteConverter bc)
		{
			return bc.int64;
		}

		public static implicit operator float(ByteConverter bc)
		{
			return bc.float32;
		}

		public static implicit operator double(ByteConverter bc)
		{
			return bc.float64;
		}

		public static implicit operator bool(ByteConverter bc)
		{
			return bc.int32 != 0;
		}

		[FieldOffset(0)]
		public float float32;

		[FieldOffset(0)]
		public double float64;

		[FieldOffset(0)]
		public sbyte int8;

		[FieldOffset(0)]
		public short int16;

		[FieldOffset(0)]
		public ushort uint16;

		[FieldOffset(0)]
		public char character;

		[FieldOffset(0)]
		public int int32;

		[FieldOffset(0)]
		public uint uint32;

		[FieldOffset(0)]
		public long int64;

		[FieldOffset(0)]
		public ulong uint64;

		[FieldOffset(0)]
		public byte byte0;

		[FieldOffset(1)]
		public byte byte1;

		[FieldOffset(2)]
		public byte byte2;

		[FieldOffset(3)]
		public byte byte3;

		[FieldOffset(4)]
		public byte byte4;

		[FieldOffset(5)]
		public byte byte5;

		[FieldOffset(6)]
		public byte byte6;

		[FieldOffset(7)]
		public byte byte7;

		[FieldOffset(4)]
		public uint uint16_B;
	}
}
