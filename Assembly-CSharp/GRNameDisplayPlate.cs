using System;
using TMPro;
using UnityEngine;

public class GRNameDisplayPlate : MonoBehaviour
{
	public void RefreshPlayerName(VRRig vrRig)
	{
		GRPlayer grplayer = GRPlayer.Get(vrRig);
		if (vrRig != null && grplayer != null)
		{
			if (!this.namePlateLabel.text.Equals(vrRig.playerNameVisible))
			{
				this.namePlateLabel.text = vrRig.playerNameVisible;
				return;
			}
		}
		else
		{
			this.namePlateLabel.text = "";
		}
	}

	public void Clear()
	{
		this.namePlateLabel.text = "";
	}

	public TMP_Text namePlateLabel;
}
