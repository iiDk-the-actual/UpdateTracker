using System;
using Photon.Realtime;

public class LegacyWorldTargetItem
{
	public bool IsValid()
	{
		return this.itemIdx != -1 && this.owner != null;
	}

	public void Invalidate()
	{
		this.itemIdx = -1;
		this.owner = null;
	}

	public Player owner;

	public int itemIdx;
}
