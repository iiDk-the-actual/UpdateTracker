using System;
using UnityEngine;

[Serializable]
public class SITechTreeNode
{
	public EAssetReleaseTier EdReleaseTier
	{
		get
		{
			return this.m_edReleaseTier;
		}
		set
		{
			this.m_edReleaseTier = value;
		}
	}

	public bool IsValid
	{
		get
		{
			EAssetReleaseTier edReleaseTier = this.m_edReleaseTier;
			return edReleaseTier != EAssetReleaseTier.Disabled && edReleaseTier <= EAssetReleaseTier.PublicRC;
		}
	}

	public bool IsDispensableGadget
	{
		get
		{
			return this.IsValid && this.unlockedGadgetPrefab;
		}
	}

	[SerializeField]
	private EAssetReleaseTier m_edReleaseTier = (EAssetReleaseTier)(-1);

	public SIUpgradeType upgradeType;

	public string nickName;

	public string description;

	public SIUpgradeType[] parentUpgrades;

	public GameEntity unlockedGadgetPrefab;

	public SIResource.ResourceCost[] nodeCost;
}
