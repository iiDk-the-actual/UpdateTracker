using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag
{
	[Serializable]
	internal class ExpectedUsersDecayTimer : TickSystemTimerAbstract
	{
		public override void OnTimedEvent()
		{
			if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.IsMasterClient)
			{
				int num = 0;
				if (PhotonNetwork.CurrentRoom.ExpectedUsers != null && PhotonNetwork.CurrentRoom.ExpectedUsers.Length != 0)
				{
					foreach (string text in PhotonNetwork.CurrentRoom.ExpectedUsers)
					{
						float num2;
						if (this.expectedUsers.TryGetValue(text, out num2))
						{
							if (num2 + this.decayTime < Time.time)
							{
								num++;
							}
						}
						else
						{
							this.expectedUsers.Add(text, Time.time);
						}
					}
					if (num >= PhotonNetwork.CurrentRoom.ExpectedUsers.Length && num != 0)
					{
						PhotonNetwork.CurrentRoom.ClearExpectedUsers();
						this.expectedUsers.Clear();
					}
				}
			}
		}

		public override void Stop()
		{
			base.Stop();
			this.expectedUsers.Clear();
		}

		public float decayTime = 15f;

		private Dictionary<string, float> expectedUsers = new Dictionary<string, float>(10);
	}
}
