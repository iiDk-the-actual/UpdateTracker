using System;

namespace GorillaNetworking
{
	public class GorillaNetworkRankedJoinTrigger : GorillaNetworkJoinTrigger
	{
		public override string GetFullDesiredGameModeString()
		{
			return this.networkZone + base.GetDesiredGameType();
		}

		public override void OnBoxTriggered()
		{
			GorillaComputer.instance.allowedMapsToJoin = this.myCollider.myAllowedMapsToJoin;
			PhotonNetworkController.Instance.ClearDeferredJoin();
			PhotonNetworkController.Instance.AttemptToJoinRankedPublicRoom(this, JoinType.Solo);
		}
	}
}
