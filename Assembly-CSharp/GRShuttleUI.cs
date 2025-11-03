using System;
using GorillaNetworking;
using TMPro;
using UnityEngine;

[Serializable]
public class GRShuttleUI
{
	public void Setup(GhostReactor reactor, NetPlayer player)
	{
		this.reactor = reactor;
		this.player = player;
		this.RefreshUI();
	}

	public void RefreshUI()
	{
		if (this.playerName != null)
		{
			this.playerName.text = ((this.player == null) ? null : this.player.SanitizedNickName);
		}
		if (this.playerTitle != null)
		{
			GRPlayer grplayer = ((this.player == null) ? null : GRPlayer.Get(this.player.ActorNumber));
			if (grplayer != null)
			{
				this.playerTitle.text = GhostReactorProgression.GetTitleName(grplayer.CurrentProgression.redeemedPoints);
			}
			else
			{
				this.playerTitle.text = null;
			}
		}
		if (this.shuttle != null)
		{
			int targetFloor = this.shuttle.GetTargetFloor();
			if (this.destFloorText != null)
			{
				if (targetFloor == -1)
				{
					this.destFloorText.text = "HQ";
				}
				else
				{
					this.destFloorText.text = (targetFloor + 1).ToString();
				}
			}
			bool flag = targetFloor <= this.shuttle.GetMaxDropFloor();
			this.validScreen.SetActive(flag);
			this.invalidScreen.SetActive(!flag);
			if (flag)
			{
				this.infoText.text = "READY!\n\nDROP TO LEVEL";
				return;
			}
			this.infoText.text = "UNSAFE!\n\nUPGRADE DROP CHASSIS";
		}
	}

	public TMP_Text playerName;

	public TMP_Text playerTitle;

	public TMP_Text destFloorText;

	public TMP_Text infoText;

	public GameObject validScreen;

	public GameObject invalidScreen;

	public GRShuttle shuttle;

	private NetPlayer player;

	private GhostReactor reactor;
}
