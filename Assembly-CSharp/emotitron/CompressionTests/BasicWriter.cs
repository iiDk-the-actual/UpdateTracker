using System;

namespace emotitron.CompressionTests
{
	public class BasicWriter
	{
		public static void Reset()
		{
			BasicWriter.pos = 0;
		}

		public static byte[] BasicWrite(byte[] buffer, byte value)
		{
			buffer[BasicWriter.pos] = value;
			BasicWriter.pos++;
			return buffer;
		}

		public static byte BasicRead(byte[] buffer)
		{
			byte b = buffer[BasicWriter.pos];
			BasicWriter.pos++;
			return b;
		}

		public static int pos;
	}
}
