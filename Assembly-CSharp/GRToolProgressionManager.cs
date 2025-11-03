using System;
using System.Collections.Generic;
using UnityEngine;

public class GRToolProgressionManager : MonoBehaviourTick
{
	public event Action OnProgressionUpdated;

	public void SetPendingTreeToProcess()
	{
		this.pendingTreeToProcess = true;
	}

	public void UpdateInventory()
	{
		this.pendingUpdateInventory = true;
	}

	public void Init(GhostReactor ghostReactor)
	{
		this.reactor = ghostReactor;
		this.PopulateToolPartMetadata();
		this.PopulateEmployeeLevelMetadata();
		if (this.researchStations != null)
		{
			foreach (GRResearchStation grresearchStation in this.researchStations)
			{
				grresearchStation.Init(this, ghostReactor);
			}
		}
		if (this.toolUpgradeStations != null)
		{
			foreach (GRToolUpgradeStation grtoolUpgradeStation in this.toolUpgradeStations)
			{
				grtoolUpgradeStation.Init(this, ghostReactor);
			}
		}
		this.toolProgressionTree.Init(this.reactor, this);
		ProgressionManager.Instance.OnNodeUnlocked += delegate(string a, string b)
		{
			this.NodeUnlocked();
		};
	}

	private void NodeUnlocked()
	{
		this.toolProgressionTree.RefreshUserInventory();
		this.toolProgressionTree.RefreshProgressionTree();
	}

	public override void Tick()
	{
		if (this.sendUpdate)
		{
			Action onProgressionUpdated = this.OnProgressionUpdated;
			if (onProgressionUpdated != null)
			{
				onProgressionUpdated();
			}
			this.sendUpdate = false;
		}
		if (this.pendingTreeToProcess)
		{
			this.toolProgressionTree.RefreshProgressionTree();
			this.pendingTreeToProcess = false;
		}
		if (this.pendingUpdateInventory)
		{
			this.toolProgressionTree.RefreshUserInventory();
			this.pendingUpdateInventory = false;
		}
	}

	public void SendMothershipUpdated()
	{
		this.sendUpdate = true;
	}

	public GRToolProgressionManager.ToolProgressionMetaData GetPartMetadata(GRToolProgressionManager.ToolParts part)
	{
		GRToolProgressionManager.ToolProgressionMetaData toolProgressionMetaData;
		this.partMetadata.TryGetValue(part, out toolProgressionMetaData);
		return toolProgressionMetaData;
	}

	private void PopulateToolPartMetadata()
	{
		this.PopulateClubPartMetadata();
		this.PopulateFlashPartMetadata();
		this.PopulateCollectorPartMetadata();
		this.PopulateLanternPartMetadata();
		this.PopulateShieldGunPartMetadata();
		this.PopulateDirectionalShieldPartMetadata();
		this.PopulateEnergyEfficiencyPartMetadata();
		this.PopulateRevivePartMetadata();
		this.PopulateDockWristPartMetadata();
		this.PopulateDropPodPartMetadata();
	}

	private void PopulateEmployeeLevelMetadata()
	{
		this.employeeLevelMetadata[GRToolProgressionTree.EmployeeLevelRequirement.None] = new GRToolProgressionManager.EmployeeMetadata
		{
			name = "None",
			level = 0
		};
		this.employeeLevelMetadata[GRToolProgressionTree.EmployeeLevelRequirement.Intern] = new GRToolProgressionManager.EmployeeMetadata
		{
			name = "Intern",
			level = 2
		};
		this.employeeLevelMetadata[GRToolProgressionTree.EmployeeLevelRequirement.PartTime] = new GRToolProgressionManager.EmployeeMetadata
		{
			name = "Part Time",
			level = 3
		};
		this.employeeLevelMetadata[GRToolProgressionTree.EmployeeLevelRequirement.FullTime] = new GRToolProgressionManager.EmployeeMetadata
		{
			name = "Full Time",
			level = 4
		};
	}

	private void PopulateClubPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.Baton] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Charge Baton",
			description = "50,000 volts of ghost-zapping power",
			annotation = "Impact Power: ❶❶"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.BatonDamage1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Lead Core",
			description = "Conductive lead sheath",
			annotation = "Attaches to Charge Baton. Impact Power: ❶❶❶"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.BatonDamage2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Osmium Core",
			description = "More mass for more win",
			annotation = "Attaches to Charge Baton. Impact Power: ❶❶❶❶"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.BatonDamage3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Electrified Spikes",
			description = "Impales, shocks, and crushes simultaneously",
			annotation = "Attaches to Charge Baton. Impact Power: ❶❶❶❶❶"
		};
	}

	private void PopulateFlashPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.Flash] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Spectral Flash",
			description = "Makes strong ghosts vulnerable",
			annotation = "Damages ghost armor."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.FlashDamage1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Spectral Lens",
			description = "Safety through momentary paralysis",
			annotation = "Attaches to Spectral Flash. Stuns enemies."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.FlashDamage2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Parabolic Focuser",
			description = "When you want ghosts to feel it",
			annotation = "Attaches to Spectral Flash. Stuns enemies. Disintegrates armor."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.FlashDamage3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Beta Wave Amplifier",
			description = "Exposure with explosive results",
			annotation = "Attaches to Spectral Flash. Stuns enemies. Shatters armor."
		};
	}

	private void PopulateCollectorPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.Collector] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 50,
			name = "Collector",
			description = "Every team needs a sucker",
			annotation = "Collects essence and recharges tools"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.CollectorBonus1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Vortex Intake",
			description = "Harvests ambient essence",
			annotation = "Attaches to Collector.  Recharges over time."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.CollectorBonus2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Cyclone Intake",
			description = "Creates a wormhole to a twin universe",
			annotation = "Attaches to Collector. 2x collection bonus."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.CollectorBonus3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Hurricane Intake",
			description = "A Category 5 commitment to teamwork",
			annotation = "Attaches to Collector. 2x collection bonus.  Area recharge."
		};
	}

	private void PopulateLanternPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.Lantern] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 50,
			name = "Lantern",
			description = "Creates the gentle glow of safety",
			annotation = "Illuminates dark areas."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.LanternIntensity1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Kinetic Power",
			description = "Saves batteries to optimize shareholder value",
			annotation = "Attaches to Lantern. Doesn't need recharge."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.LanternIntensity2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Flare Discharge",
			description = "Blaze the trail for your team",
			annotation = "Attaches to Lantern. Drops long-lasting flares."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.LanternIntensity3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Gamma Burster",
			description = "See through walls. Do not aim at important body parts",
			annotation = "Attaches to Lantern. X-ray ghost vision."
		};
	}

	private void PopulateShieldGunPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.ShieldGun] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Forcefield Gun",
			description = "Corporate armor for fragile assets",
			annotation = "Gives forcefields."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.ShieldGunStrength1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Truebright Nozzle",
			description = "Nuclear protection",
			annotation = "Attaches to Forcefield Gun. Increases light."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.ShieldGunStrength2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Stealth Nozzle",
			description = "Protection they'll never see coming",
			annotation = "Attaches to Forcefield Gun. Gives temporary stealth."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.ShieldGunStrength3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Medic Nozzle",
			description = "Restores productivity through impact therapy",
			annotation = "Attaches to Forcefield Gun. Heals to full."
		};
	}

	private void PopulateDirectionalShieldPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.DirectionalShield] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Umbrella Shield",
			description = "Protects company property",
			annotation = "Blocks attacks."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.DirectionalShieldSize1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Sling Shield",
			description = "Deflects danger and liability",
			annotation = "Attaches to Umbrella Shield. Reflects projectiles."
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.DirectionalShieldSize2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Harmshadow",
			description = "The best defense is a good offense",
			annotation = "Attaches to Umbrella Shield. Impact Power: ❶❶"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.DirectionalShieldSize3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Total Defense Array",
			description = "The only safety device with a kill count",
			annotation = "Attaches to Shield. Reflects projectiles. Impact power: ❶❶"
		};
	}

	private void PopulateEnergyEfficiencyPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.EnergyEff] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Flash",
			description = "Lead Core Does things!"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.EnergyEff1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Regulator",
			description = "Do more with less",
			annotation = "Attaches to most tools. Efficiency: +❶"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.EnergyEff2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Optimizer",
			description = "Half the juice, double the morale",
			annotation = "Attaches to most tools. Efficiency: +❶❶"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.EnergyEff3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Peak Power",
			description = "Efficiency that borders on spiritual enlightenment",
			annotation = "Attaches to most tools. Efficiency: +❶❶❶"
		};
	}

	private void PopulateRevivePartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.Revive] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Revive",
			description = "Turns fatal injuries into teachable moments",
			annotation = "Brings defeated employees back to life."
		};
	}

	private void PopulateDockWristPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.DockWrist] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 500,
			name = "Wrist Dock",
			description = "Wearable storage that maximizes output per limb",
			annotation = "Extra storage slot"
		};
	}

	private void PopulateDropPodPartMetadata()
	{
		this.partMetadata[GRToolProgressionManager.ToolParts.DropPodBasic] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 100,
			name = "Starter Pod",
			description = "Descend with confidence in a personal drop pod!\nSupports drops to 5000m\nUpgradable for deeper drops",
			annotation = "DropPodBasic"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.DropPodChassis1] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 200,
			name = "Reinforced Pod Chassis",
			description = "Upgrade your drop pod to support drops to 10000m",
			annotation = "DropPodChassis1"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.DropPodChassis2] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 600,
			name = "Iron Pod Chassis",
			description = "Upgrade your drop pod to support drops to 15000m",
			annotation = "DropPodChassis2"
		};
		this.partMetadata[GRToolProgressionManager.ToolParts.DropPodChassis3] = new GRToolProgressionManager.ToolProgressionMetaData
		{
			shiftCreditCost = 1400,
			name = "Steel Pod Chassis",
			description = "Upgrade your drop pod to support drops to 20000m",
			annotation = "DropPodChassis3"
		};
	}

	public int GetRequiredEmployeeLevel(GRToolProgressionTree.EmployeeLevelRequirement employeeLevel)
	{
		return this.employeeLevelMetadata[employeeLevel].level;
	}

	public string GetEmployeeLevelDisplayName(GRToolProgressionTree.EmployeeLevelRequirement employeeLevel)
	{
		return this.employeeLevelMetadata[employeeLevel].name;
	}

	public int GetNumberOfResearchPoints()
	{
		return this.toolProgressionTree.GetNumberOfResearchPoints();
	}

	public List<GRTool.GRToolType> GetSupportedTools()
	{
		return this.toolProgressionTree.GetSupportedTools();
	}

	public List<GRToolProgressionTree.GRToolProgressionNode> GetToolUpgrades(GRTool.GRToolType tool)
	{
		return this.toolProgressionTree.GetToolUpgrades(tool);
	}

	public int GetRecycleShiftCredit(GRTool.GRToolType tool)
	{
		if (tool == GRTool.GRToolType.HockeyStick)
		{
			return (int)(10f / (float)this.reactor.vrRigs.Count);
		}
		GRToolProgressionTree.GRToolProgressionNode toolNode = this.toolProgressionTree.GetToolNode(tool);
		if (toolNode != null)
		{
			return (int)((float)(toolNode.partMetadata.shiftCreditCost / 2) / (float)this.reactor.vrRigs.Count);
		}
		return 0;
	}

	public bool GetShiftCreditCost(GRToolProgressionManager.ToolParts part, out int shiftCreditCost)
	{
		shiftCreditCost = 0;
		if (this.partMetadata.ContainsKey(part))
		{
			shiftCreditCost += this.partMetadata[part].shiftCreditCost;
			return true;
		}
		return false;
	}

	public void AttemptToUnlockPart(GRToolProgressionManager.ToolParts part)
	{
		bool flag;
		if (!this.IsPartUnlocked(part, out flag))
		{
			return;
		}
		if (!flag)
		{
			int numberOfResearchPoints = this.GetNumberOfResearchPoints();
			int num;
			if (!this.GetPartUnlockJuiceCost(part, out num))
			{
				return;
			}
			if (numberOfResearchPoints < num)
			{
				return;
			}
			GRToolProgressionTree.EmployeeLevelRequirement employeeLevelRequirement;
			if (!this.GetPartUnlockEmployeeRequiredLevel(part, out employeeLevelRequirement))
			{
				return;
			}
			int requiredEmployeeLevel = this.GetRequiredEmployeeLevel(this.GetCurrentEmployeeLevel());
			int requiredEmployeeLevel2 = this.GetRequiredEmployeeLevel(employeeLevelRequirement);
			if (requiredEmployeeLevel < requiredEmployeeLevel2)
			{
				return;
			}
			this.toolProgressionTree.AttemptToUnlockPart(part);
		}
	}

	public bool IsPartUnlocked(GRToolProgressionManager.ToolParts part, out bool unlocked)
	{
		unlocked = false;
		GRToolProgressionTree.GRToolProgressionNode partNode = this.toolProgressionTree.GetPartNode(part);
		if (partNode == null)
		{
			return false;
		}
		unlocked = partNode.unlocked;
		return true;
	}

	public bool GetPartUnlockEmployeeRequiredLevel(GRToolProgressionManager.ToolParts part, out GRToolProgressionTree.EmployeeLevelRequirement level)
	{
		level = GRToolProgressionTree.EmployeeLevelRequirement.None;
		GRToolProgressionTree.GRToolProgressionNode partNode = this.toolProgressionTree.GetPartNode(part);
		if (partNode == null)
		{
			return false;
		}
		level = partNode.requiredEmployeeLevel;
		return true;
	}

	public bool GetPartUnlockJuiceCost(GRToolProgressionManager.ToolParts part, out int juiceCost)
	{
		juiceCost = 0;
		GRToolProgressionTree.GRToolProgressionNode partNode = this.toolProgressionTree.GetPartNode(part);
		if (partNode == null)
		{
			return false;
		}
		juiceCost = partNode.researchCost;
		return true;
	}

	public bool GetPartUnlockRequiredParentParts(GRToolProgressionManager.ToolParts part, out List<GRToolProgressionManager.ToolParts> requiredParts)
	{
		requiredParts = new List<GRToolProgressionManager.ToolParts>();
		GRToolProgressionTree.GRToolProgressionNode partNode = this.toolProgressionTree.GetPartNode(part);
		if (partNode == null)
		{
			return false;
		}
		foreach (GRToolProgressionTree.GRToolProgressionNode grtoolProgressionNode in partNode.parents)
		{
			requiredParts.Add(grtoolProgressionNode.type);
		}
		return true;
	}

	public bool GetPlayerShiftCredit(out int playerShiftCredit)
	{
		playerShiftCredit = 0;
		if (VRRig.LocalRig != null)
		{
			GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
			if (grplayer != null)
			{
				playerShiftCredit = grplayer.ShiftCredits;
				return true;
			}
		}
		return false;
	}

	public GRToolProgressionTree.EmployeeLevelRequirement GetCurrentEmployeeLevel()
	{
		return this.toolProgressionTree.GetCurrentEmploymentLevel();
	}

	public string GetTreeId()
	{
		return this.toolProgressionTree.GetTreeId();
	}

	public int GetDropPodLevel()
	{
		bool flag;
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodBasic, out flag) && flag)
		{
			return 1;
		}
		return 0;
	}

	public int GetDropPodChasisLevel()
	{
		bool flag;
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodChassis3, out flag) && flag)
		{
			return 3;
		}
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodChassis2, out flag) && flag)
		{
			return 2;
		}
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodChassis1, out flag) && flag)
		{
			return 1;
		}
		return 0;
	}

	public ProgressionManager.DrillUpgradeLevel GetDrillLevel()
	{
		bool flag;
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodChassis3, out flag) && flag)
		{
			return ProgressionManager.DrillUpgradeLevel.Upgrade3;
		}
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodChassis2, out flag) && flag)
		{
			return ProgressionManager.DrillUpgradeLevel.Upgrade2;
		}
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodChassis1, out flag) && flag)
		{
			return ProgressionManager.DrillUpgradeLevel.Upgrade1;
		}
		if (this.IsPartUnlocked(GRToolProgressionManager.ToolParts.DropPodBasic, out flag) && flag)
		{
			return ProgressionManager.DrillUpgradeLevel.Base;
		}
		return ProgressionManager.DrillUpgradeLevel.None;
	}

	public int GetJuiceCostForDrillUpgrade(ProgressionManager.DrillUpgradeLevel upgradeLevel)
	{
		int num = 0;
		switch (upgradeLevel)
		{
		case ProgressionManager.DrillUpgradeLevel.Base:
			this.GetPartUnlockJuiceCost(GRToolProgressionManager.ToolParts.DropPodBasic, out num);
			break;
		case ProgressionManager.DrillUpgradeLevel.Upgrade1:
			this.GetPartUnlockJuiceCost(GRToolProgressionManager.ToolParts.DropPodChassis1, out num);
			break;
		case ProgressionManager.DrillUpgradeLevel.Upgrade2:
			this.GetPartUnlockJuiceCost(GRToolProgressionManager.ToolParts.DropPodChassis2, out num);
			break;
		case ProgressionManager.DrillUpgradeLevel.Upgrade3:
			this.GetPartUnlockJuiceCost(GRToolProgressionManager.ToolParts.DropPodChassis3, out num);
			break;
		}
		return num;
	}

	public int GetSRCostForDrillUpgradeLevel(ProgressionManager.DrillUpgradeLevel level)
	{
		switch (level)
		{
		case ProgressionManager.DrillUpgradeLevel.Base:
			return 3600;
		case ProgressionManager.DrillUpgradeLevel.Upgrade1:
			return 0;
		case ProgressionManager.DrillUpgradeLevel.Upgrade2:
			return 0;
		case ProgressionManager.DrillUpgradeLevel.Upgrade3:
			return 0;
		default:
			return 0;
		}
	}

	[NonSerialized]
	private Dictionary<GRToolProgressionTree.EmployeeLevelRequirement, GRToolProgressionManager.EmployeeMetadata> employeeLevelMetadata = new Dictionary<GRToolProgressionTree.EmployeeLevelRequirement, GRToolProgressionManager.EmployeeMetadata>();

	[NonSerialized]
	private Dictionary<GRToolProgressionManager.ToolParts, GRToolProgressionManager.ToolProgressionMetaData> partMetadata = new Dictionary<GRToolProgressionManager.ToolParts, GRToolProgressionManager.ToolProgressionMetaData>();

	[NonSerialized]
	private GRToolProgressionTree toolProgressionTree = new GRToolProgressionTree();

	[NonSerialized]
	private GhostReactor reactor;

	[SerializeField]
	private List<GRResearchStation> researchStations;

	[SerializeField]
	private List<GRToolUpgradeStation> toolUpgradeStations;

	[NonSerialized]
	private bool pendingTreeToProcess;

	[NonSerialized]
	private bool pendingUpdateInventory;

	private bool sendUpdate;

	public class ToolProgressionMetaData
	{
		public string name;

		public string description;

		public string annotation;

		public int shiftCreditCost;
	}

	public struct ToolData
	{
		public GRToolProgressionTree.GRToolProgressionNode node;

		public GRToolProgressionManager.ToolProgressionMetaData metaData;
	}

	public struct EmployeeMetadata
	{
		public string name;

		public int level;
	}

	public enum ToolParts
	{
		None,
		Baton,
		BatonDamage1,
		BatonDamage2,
		BatonDamage3,
		Flash,
		FlashDamage1,
		FlashDamage2,
		FlashDamage3,
		Collector,
		CollectorBonus1,
		CollectorBonus2,
		CollectorBonus3,
		Lantern,
		LanternIntensity1,
		LanternIntensity2,
		LanternIntensity3,
		ShieldGun,
		ShieldGunStrength1,
		ShieldGunStrength2,
		ShieldGunStrength3,
		DirectionalShield,
		DirectionalShieldSize1,
		DirectionalShieldSize2,
		DirectionalShieldSize3,
		EnergyEff,
		EnergyEff1,
		EnergyEff2,
		EnergyEff3,
		DockWrist,
		Revive,
		DropPodBasic,
		DropPodChassis1,
		DropPodChassis2,
		DropPodChassis3
	}
}
