using System;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTagScripts
{
	public class PlayerTimerBoardLine : MonoBehaviour
	{
		public void ResetData()
		{
			this.linePlayer = null;
			this.currentNickname = string.Empty;
			this.playerTimeStr = string.Empty;
			this.playerTimeSeconds = 0f;
		}

		public void SetLineData(NetPlayer netPlayer)
		{
			if (!netPlayer.InRoom || netPlayer == this.linePlayer)
			{
				return;
			}
			this.linePlayer = netPlayer;
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(netPlayer, out rigContainer))
			{
				this.rigContainer = rigContainer;
				this.playerVRRig = rigContainer.Rig;
			}
			this.InitializeLine();
		}

		public void InitializeLine()
		{
			this.currentNickname = string.Empty;
			this.UpdatePlayerText();
			this.UpdateTimeText();
		}

		public void UpdateLine()
		{
			if (this.linePlayer != null)
			{
				if (this.playerNameVisible != this.playerVRRig.playerNameVisible)
				{
					this.UpdatePlayerText();
					this.parentBoard.IsDirty = true;
				}
				string text = this.playerTimeStr;
				this.UpdateTimeText();
				if (!this.playerTimeStr.Equals(text))
				{
					this.parentBoard.IsDirty = true;
				}
			}
		}

		private void UpdatePlayerText()
		{
			try
			{
				if (this.rigContainer.IsNull() || this.playerVRRig.IsNull())
				{
					this.playerNameVisible = this.NormalizeName(this.linePlayer.NickName != this.currentNickname, this.linePlayer.NickName);
					this.currentNickname = this.linePlayer.NickName;
				}
				else if (this.rigContainer.Initialized)
				{
					this.playerNameVisible = this.playerVRRig.playerNameVisible;
				}
				else if (this.currentNickname.IsNullOrEmpty() || GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(this.linePlayer.UserId))
				{
					this.playerNameVisible = this.NormalizeName(this.linePlayer.NickName != this.currentNickname, this.linePlayer.NickName);
				}
			}
			catch (Exception)
			{
				this.playerNameVisible = this.linePlayer.DefaultName;
				GorillaNot.instance.SendReport("NmError", this.linePlayer.UserId, this.linePlayer.NickName);
			}
		}

		private void UpdateTimeText()
		{
			if (this.linePlayer == null || !(PlayerTimerManager.instance != null))
			{
				this.playerTimeStr = "--:--:--";
				return;
			}
			this.playerTimeSeconds = PlayerTimerManager.instance.GetLastDurationForPlayer(this.linePlayer.ActorNumber);
			if (this.playerTimeSeconds > 0f)
			{
				this.playerTimeStr = TimeSpan.FromSeconds((double)this.playerTimeSeconds).ToString("mm\\:ss\\:ff");
				return;
			}
			this.playerTimeStr = "--:--:--";
		}

		public string NormalizeName(bool doIt, string text)
		{
			if (doIt)
			{
				if (GorillaComputer.instance.CheckAutoBanListForName(text))
				{
					text = new string(Array.FindAll<char>(text.ToCharArray(), (char c) => Utils.IsASCIILetterOrDigit(c)));
					if (text.Length > 12)
					{
						text = text.Substring(0, 11);
					}
					text = text.ToUpper();
				}
				else
				{
					text = "BADGORILLA";
					GorillaNot.instance.SendReport("evading the name ban", this.linePlayer.UserId, this.linePlayer.NickName);
				}
			}
			return text;
		}

		public static int CompareByTotalTime(PlayerTimerBoardLine lineA, PlayerTimerBoardLine lineB)
		{
			if (lineA.playerTimeSeconds > 0f && lineB.playerTimeSeconds > 0f)
			{
				return lineA.playerTimeSeconds.CompareTo(lineB.playerTimeSeconds);
			}
			if (lineA.playerTimeSeconds <= 0f)
			{
				return 1;
			}
			if (lineB.playerTimeSeconds <= 0f)
			{
				return -1;
			}
			return 0;
		}

		public string playerNameVisible;

		public string playerTimeStr;

		private float playerTimeSeconds;

		public NetPlayer linePlayer;

		public VRRig playerVRRig;

		public PlayerTimerBoard parentBoard;

		internal RigContainer rigContainer;

		private string currentNickname;
	}
}
