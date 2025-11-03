using System;
using Fusion;

[NetworkBehaviourWeaved(0)]
public abstract class FusionGameModeData : NetworkBehaviour
{
	public abstract object Data { get; set; }

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
	}

	protected INetworkStruct data;
}
