using System;
using System.Runtime.InteropServices;
using Fusion;

[NetworkStructWeaved(1)]
[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct HitTargetStruct : INetworkStruct
{
	public HitTargetStruct(int v)
	{
		this.Score = v;
	}

	[FieldOffset(0)]
	public int Score;
}
