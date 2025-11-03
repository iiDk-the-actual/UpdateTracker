using System;
using System.Runtime.InteropServices;

public struct RPCArgBuffer<T> where T : struct
{
	public RPCArgBuffer(T argStruct)
	{
		this.DataLength = Marshal.SizeOf(typeof(T));
		this.Data = new byte[this.DataLength];
		this.Args = argStruct;
	}

	public T Args;

	public byte[] Data;

	public int DataLength;
}
