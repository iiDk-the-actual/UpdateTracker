using System;

public struct SIUpgradeSet
{
	public void Clear()
	{
		this.backingBits = 0;
	}

	public SIUpgradeSet(int bits)
	{
		this.backingBits = bits;
	}

	public int GetBits()
	{
		return this.backingBits;
	}

	public void SetBits(int bits)
	{
		this.backingBits = bits;
	}

	public long GetCreateData(SIPlayer player)
	{
		return ((long)this.backingBits << 32) | (long)player.ActorNr;
	}

	public void Add(SIUpgradeType upgrade)
	{
		this.backingBits |= 1 << upgrade.GetNodeId();
	}

	public void Add(int nodeId)
	{
		this.backingBits |= 1 << nodeId;
	}

	public void Remove(SIUpgradeType upgrade)
	{
		this.backingBits &= ~(1 << upgrade.GetNodeId());
	}

	public bool Contains(SIUpgradeType upgrade)
	{
		return (this.backingBits & (1 << upgrade.GetNodeId())) != 0;
	}

	public bool ContainsAny(params SIUpgradeType[] upgrades)
	{
		int num = 0;
		foreach (SIUpgradeType siupgradeType in upgrades)
		{
			num |= 1 << siupgradeType.GetNodeId();
		}
		return (this.backingBits & num) != 0;
	}

	public string GetString(SITechTreePageId pageId)
	{
		string text = "";
		int i = this.backingBits;
		int num = 0;
		bool flag = true;
		while (i > 0)
		{
			if ((i & 1) != 0)
			{
				if (!flag)
				{
					text += "|";
				}
				text += SIUpgradeTypeSystem.GetUpgradeType((int)pageId, num).ToString();
				flag = false;
			}
			i >>= 1;
			num++;
		}
		return text;
	}

	private int backingBits;
}
