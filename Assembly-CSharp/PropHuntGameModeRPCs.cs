using System;

internal class PropHuntGameModeRPCs : RPCNetworkBase
{
	public override void SetClassTarget(IWrappedSerializable target, GorillaWrappedSerializer netHandler)
	{
		this.propHuntManager = (GorillaPropHuntGameManager)target;
		this.serializer = (GameModeSerializer)netHandler;
	}

	private GameModeSerializer serializer;

	private GorillaPropHuntGameManager propHuntManager;
}
