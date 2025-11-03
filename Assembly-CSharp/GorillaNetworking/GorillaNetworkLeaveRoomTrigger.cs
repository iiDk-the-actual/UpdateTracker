using System;
using System.Threading.Tasks;
using GorillaTagScripts;
using UnityEngine;

namespace GorillaNetworking
{
	public class GorillaNetworkLeaveRoomTrigger : GorillaTriggerBox
	{
		public override void OnBoxTriggered()
		{
			base.OnBoxTriggered();
			if (NetworkSystem.Instance.InRoom && (!this.excludePrivateRooms || !NetworkSystem.Instance.SessionIsPrivate))
			{
				if (FriendshipGroupDetection.Instance.IsInParty)
				{
					FriendshipGroupDetection.Instance.LeaveParty();
					this.DisconnectAfterDelay(1f);
					return;
				}
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
		}

		private async void DisconnectAfterDelay(float seconds)
		{
			await Task.Delay((int)(1000f * seconds));
			await NetworkSystem.Instance.ReturnToSinglePlayer();
		}

		[SerializeField]
		private bool excludePrivateRooms;
	}
}
