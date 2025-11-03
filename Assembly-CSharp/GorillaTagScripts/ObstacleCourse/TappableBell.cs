using System;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts.ObstacleCourse
{
	public class TappableBell : Tappable
	{
		public event TappableBell.ObstacleCourseTriggerEvent OnTapped;

		public override void OnTapLocal(float tapStrength, float tapTime, PhotonMessageInfoWrapped info)
		{
			if (!PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				return;
			}
			if (!this.rpcCooldown.CheckCallTime(Time.time))
			{
				return;
			}
			this.winnerRig = GorillaGameManager.StaticFindRigForPlayer(info.Sender);
			if (this.winnerRig != null)
			{
				TappableBell.ObstacleCourseTriggerEvent onTapped = this.OnTapped;
				if (onTapped == null)
				{
					return;
				}
				onTapped(this.winnerRig);
			}
		}

		private VRRig winnerRig;

		public CallLimiter rpcCooldown;

		public delegate void ObstacleCourseTriggerEvent(VRRig vrrig);
	}
}
