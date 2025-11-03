using System;
using System.Collections.Generic;
using UnityEngine;

public class GRReadyRoom : MonoBehaviour
{
	public void RefreshRigs(List<VRRig> vrRigs)
	{
		for (int i = 0; i < this.nameDisplayPlates.Count; i++)
		{
			if (this.nameDisplayPlates != null)
			{
				if (i < vrRigs.Count && vrRigs[i] != null && vrRigs[i].OwningNetPlayer != null)
				{
					this.nameDisplayPlates[i].RefreshPlayerName(vrRigs[i]);
				}
				else
				{
					this.nameDisplayPlates[i].Clear();
				}
			}
		}
	}

	public List<GRNameDisplayPlate> nameDisplayPlates;
}
