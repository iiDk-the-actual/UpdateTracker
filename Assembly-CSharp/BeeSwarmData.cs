using System;
using System.Runtime.InteropServices;
using Fusion;

[NetworkStructWeaved(3)]
[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct BeeSwarmData : INetworkStruct
{
	public int TargetActorNumber { readonly get; set; }

	public int CurrentState { readonly get; set; }

	public float CurrentSpeed { readonly get; set; }

	public BeeSwarmData(int actorNr, int state, float speed)
	{
		this.TargetActorNumber = actorNr;
		this.CurrentState = state;
		this.CurrentSpeed = speed;
	}
}
