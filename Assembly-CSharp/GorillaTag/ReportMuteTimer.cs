using System;
using Photon.Realtime;

namespace GorillaTag
{
	internal class ReportMuteTimer : TickSystemTimerAbstract, ObjectPoolEvents
	{
		public int Muted { get; set; }

		public override void OnTimedEvent()
		{
			if (!NetworkSystem.Instance.InRoom)
			{
				this.Stop();
				return;
			}
			ReportMuteTimer.content[0] = this.m_playerID;
			ReportMuteTimer.content[1] = this.Muted;
			ReportMuteTimer.content[2] = ((this.m_nickName.Length > 12) ? this.m_nickName.Remove(12) : this.m_nickName);
			ReportMuteTimer.content[3] = NetworkSystem.Instance.LocalPlayer.NickName;
			ReportMuteTimer.content[4] = !NetworkSystem.Instance.SessionIsPrivate;
			ReportMuteTimer.content[5] = NetworkSystem.Instance.RoomStringStripped();
			NetworkSystemRaiseEvent.RaiseEvent(51, ReportMuteTimer.content, ReportMuteTimer.netEventOptions, true);
			this.Stop();
		}

		public void SetReportData(string id, string name, int muted)
		{
			this.Muted = muted;
			this.m_playerID = id;
			this.m_nickName = name;
		}

		void ObjectPoolEvents.OnTaken()
		{
		}

		void ObjectPoolEvents.OnReturned()
		{
			if (base.Running)
			{
				this.OnTimedEvent();
			}
			this.m_playerID = string.Empty;
			this.m_nickName = string.Empty;
			this.Muted = 0;
		}

		private static readonly NetEventOptions netEventOptions = new NetEventOptions
		{
			Flags = new WebFlags(3),
			TargetActors = new int[] { -1 }
		};

		private static readonly object[] content = new object[6];

		private const byte evCode = 51;

		private string m_playerID;

		private string m_nickName;
	}
}
