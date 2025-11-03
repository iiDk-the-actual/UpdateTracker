using System;
using System.Collections.Generic;
using GorillaNetworking;

public class GRToolProgressionTree
{
	public GRToolProgressionTree()
	{
		this.InitializeToolMapping();
		this.InitializeClubPartMapping();
		this.InitializeFlashPartMapping();
		this.InitializeRevivePartMapping();
		this.InitializeCollectorPartMapping();
		this.InitializeLanternPartMapping();
		this.InitializeShieldGunPartMapping();
		this.InitializeDirectionalShieldPartMapping();
		this.InitializeEnergyEfficiencyPartMapping();
		this.InitializeDockWristPartMapping();
		this.InitializeDropPodPartMapping();
	}

	public void Init(GhostReactor ghostReactor, GRToolProgressionManager toolManager)
	{
		this.reactor = ghostReactor;
		this.manager = toolManager;
		if (ProgressionManager.Instance != null)
		{
			ProgressionManager.Instance.OnTreeUpdated += this.OnProgressionTreeUpdate;
			ProgressionManager.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
		}
		this.RefreshProgressionTree();
		this.RefreshUserInventory();
	}

	public string GetTreeId()
	{
		return this.treeId;
	}

	public List<GRTool.GRToolType> GetSupportedTools()
	{
		List<GRTool.GRToolType> list = new List<GRTool.GRToolType>();
		foreach (GRTool.GRToolType grtoolType in this.toolTree.Keys)
		{
			list.Add(grtoolType);
		}
		return list;
	}

	public List<GRToolProgressionTree.GRToolProgressionNode> GetToolUpgrades(GRTool.GRToolType tool)
	{
		List<GRToolProgressionTree.GRToolProgressionNode> list = new List<GRToolProgressionTree.GRToolProgressionNode>();
		this.AddToolProgressionChildren(this.toolTree[tool], ref list);
		return list;
	}

	public GRToolProgressionTree.GRToolProgressionNode GetToolNode(GRTool.GRToolType tool)
	{
		if (this.toolTree.ContainsKey(tool))
		{
			return this.toolTree[tool];
		}
		return null;
	}

	public GRToolProgressionTree.GRToolProgressionNode GetPartNode(GRToolProgressionManager.ToolParts part)
	{
		if (this.partTree.ContainsKey(part))
		{
			return this.partTree[part];
		}
		return null;
	}

	public void RefreshProgressionTree()
	{
		ProgressionManager.Instance.RefreshProgressionTree();
	}

	public void RefreshUserInventory()
	{
		ProgressionManager.Instance.RefreshUserInventory();
	}

	private void OnProgressionTreeUpdate()
	{
		UserHydratedProgressionTreeResponse tree = ProgressionManager.Instance.GetTree(this.treeName);
		if (tree != null)
		{
			this.ProcessToolProgressionTree(tree);
		}
		GRToolProgressionManager grtoolProgressionManager = this.manager;
		if (grtoolProgressionManager == null)
		{
			return;
		}
		grtoolProgressionManager.SendMothershipUpdated();
	}

	private void OnInventoryUpdated()
	{
		ProgressionManager.MothershipItemSummary mothershipItemSummary;
		if (ProgressionManager.Instance.GetInventoryItem(this.researchPointsEntitlement, out mothershipItemSummary))
		{
			this.currentResearchPoints = mothershipItemSummary.Quantity;
		}
		ProgressionManager.MothershipItemSummary mothershipItemSummary2;
		ProgressionManager.MothershipItemSummary mothershipItemSummary3;
		ProgressionManager.MothershipItemSummary mothershipItemSummary4;
		if (ProgressionManager.Instance.GetInventoryItem(this.fullTimeEntitlement, out mothershipItemSummary2))
		{
			this.currentEmploymentLevel = GRToolProgressionTree.EmployeeLevelRequirement.FullTime;
		}
		else if (ProgressionManager.Instance.GetInventoryItem(this.partTimeEntitlement, out mothershipItemSummary3))
		{
			this.currentEmploymentLevel = GRToolProgressionTree.EmployeeLevelRequirement.PartTime;
		}
		else if (ProgressionManager.Instance.GetInventoryItem(this.internEntitlement, out mothershipItemSummary4))
		{
			this.currentEmploymentLevel = GRToolProgressionTree.EmployeeLevelRequirement.Intern;
		}
		else
		{
			this.currentEmploymentLevel = GRToolProgressionTree.EmployeeLevelRequirement.None;
		}
		GRToolProgressionManager grtoolProgressionManager = this.manager;
		if (grtoolProgressionManager == null)
		{
			return;
		}
		grtoolProgressionManager.SendMothershipUpdated();
	}

	public GRToolProgressionTree.EmployeeLevelRequirement GetCurrentEmploymentLevel()
	{
		return this.currentEmploymentLevel;
	}

	private void AddToolProgressionChildren(GRToolProgressionTree.GRToolProgressionNode currentNode, ref List<GRToolProgressionTree.GRToolProgressionNode> list)
	{
		foreach (GRToolProgressionTree.GRToolProgressionNode grtoolProgressionNode in currentNode.children)
		{
			list.Add(grtoolProgressionNode);
			this.AddToolProgressionChildren(grtoolProgressionNode, ref list);
		}
	}

	public int GetNumberOfResearchPoints()
	{
		return this.currentResearchPoints;
	}

	private void InitializeToolMapping()
	{
		this.toolMapping["ChargeBaton"] = GRTool.GRToolType.Club;
		this.toolMapping["FlashTool"] = GRTool.GRToolType.Flash;
		this.toolMapping["Revive"] = GRTool.GRToolType.Revive;
		this.toolMapping["Collector"] = GRTool.GRToolType.Collector;
		this.toolMapping["Lantern"] = GRTool.GRToolType.Lantern;
		this.toolMapping["ShieldGun"] = GRTool.GRToolType.ShieldGun;
		this.toolMapping["DirectionalShield"] = GRTool.GRToolType.DirectionalShield;
		this.toolMapping["DockWrist"] = GRTool.GRToolType.DockWrist;
		this.toolMapping["EnergyEfficiency"] = GRTool.GRToolType.EnergyEfficiency;
		this.toolMapping["DropPodBasic"] = GRTool.GRToolType.DropPod;
	}

	private void InitializeClubPartMapping()
	{
		this.partMapping["ChargeBaton"] = GRToolProgressionManager.ToolParts.Baton;
		this.partMapping["BatonDamage1"] = GRToolProgressionManager.ToolParts.BatonDamage1;
		this.partMapping["BatonDamage2"] = GRToolProgressionManager.ToolParts.BatonDamage2;
		this.partMapping["BatonDamage3"] = GRToolProgressionManager.ToolParts.BatonDamage3;
	}

	private void InitializeFlashPartMapping()
	{
		this.partMapping["FlashTool"] = GRToolProgressionManager.ToolParts.Flash;
		this.partMapping["FlashDamage1"] = GRToolProgressionManager.ToolParts.FlashDamage1;
		this.partMapping["FlashDamage2"] = GRToolProgressionManager.ToolParts.FlashDamage2;
		this.partMapping["FlashDamage3"] = GRToolProgressionManager.ToolParts.FlashDamage3;
	}

	private void InitializeCollectorPartMapping()
	{
		this.partMapping["Collector"] = GRToolProgressionManager.ToolParts.Collector;
		this.partMapping["CollectorBonus1"] = GRToolProgressionManager.ToolParts.CollectorBonus1;
		this.partMapping["CollectorBonus2"] = GRToolProgressionManager.ToolParts.CollectorBonus2;
		this.partMapping["CollectorBonus3"] = GRToolProgressionManager.ToolParts.CollectorBonus3;
	}

	private void InitializeRevivePartMapping()
	{
		this.partMapping["Revive"] = GRToolProgressionManager.ToolParts.Revive;
	}

	private void InitializeLanternPartMapping()
	{
		this.partMapping["Lantern"] = GRToolProgressionManager.ToolParts.Lantern;
		this.partMapping["LanternIntensity1"] = GRToolProgressionManager.ToolParts.LanternIntensity1;
		this.partMapping["LanternIntensity2"] = GRToolProgressionManager.ToolParts.LanternIntensity2;
		this.partMapping["LanternIntensity3"] = GRToolProgressionManager.ToolParts.LanternIntensity3;
	}

	private void InitializeShieldGunPartMapping()
	{
		this.partMapping["ShieldGun"] = GRToolProgressionManager.ToolParts.ShieldGun;
		this.partMapping["ShieldGunStrength1"] = GRToolProgressionManager.ToolParts.ShieldGunStrength1;
		this.partMapping["ShieldGunStrength2"] = GRToolProgressionManager.ToolParts.ShieldGunStrength2;
		this.partMapping["ShieldGunStrength3"] = GRToolProgressionManager.ToolParts.ShieldGunStrength3;
	}

	private void InitializeDirectionalShieldPartMapping()
	{
		this.partMapping["DirectionalShield"] = GRToolProgressionManager.ToolParts.DirectionalShield;
		this.partMapping["DirectionalShieldSize1"] = GRToolProgressionManager.ToolParts.DirectionalShieldSize1;
		this.partMapping["DirectionalShieldSize2"] = GRToolProgressionManager.ToolParts.DirectionalShieldSize2;
		this.partMapping["DirectionalShieldSize3"] = GRToolProgressionManager.ToolParts.DirectionalShieldSize3;
	}

	private void InitializeEnergyEfficiencyPartMapping()
	{
		this.partMapping["EnergyEfficiency"] = GRToolProgressionManager.ToolParts.EnergyEff;
		this.partMapping["EnergyEff1"] = GRToolProgressionManager.ToolParts.EnergyEff1;
		this.partMapping["EnergyEff2"] = GRToolProgressionManager.ToolParts.EnergyEff2;
		this.partMapping["EnergyEff3"] = GRToolProgressionManager.ToolParts.EnergyEff3;
	}

	private void InitializeDockWristPartMapping()
	{
		this.partMapping["DockWrist"] = GRToolProgressionManager.ToolParts.DockWrist;
	}

	private void InitializeDropPodPartMapping()
	{
		this.partMapping["DropPodBasic"] = GRToolProgressionManager.ToolParts.DropPodBasic;
		this.partMapping["DropPodChassis01"] = GRToolProgressionManager.ToolParts.DropPodChassis1;
		this.partMapping["DropPodChassis02"] = GRToolProgressionManager.ToolParts.DropPodChassis2;
		this.partMapping["DropPodChassis03"] = GRToolProgressionManager.ToolParts.DropPodChassis3;
	}

	private void ProcessNodes()
	{
		foreach (KeyValuePair<string, GRToolProgressionTree.GRToolProgressionRawNode> keyValuePair in this.nodeTree)
		{
			GRToolProgressionTree.GRToolProgressionRawNode value = keyValuePair.Value;
			foreach (string text in value.requiredByIds)
			{
				if (this.nodeTree.ContainsKey(text))
				{
					this.nodeTree[text].progressionNode.children.Add(value.progressionNode);
					value.progressionNode.parents.Add(this.nodeTree[text].progressionNode);
				}
			}
			value.progressionNode.requiredEmployeeLevel = this.GetEmployeeLevel(value.requiredEntitlements);
			string text2 = value.progressionNode.name.Trim();
			if (this.toolMapping.ContainsKey(text2))
			{
				GRTool.GRToolType grtoolType = this.toolMapping[text2];
				if (!value.progressionNode.unlocked && this.autoUnlockNodeId == string.Empty && value.progressionNode.researchCost == 0 && value.progressionNode.requiredEmployeeLevel == GRToolProgressionTree.EmployeeLevelRequirement.None)
				{
					this.autoUnlockNodeId = value.progressionNode.id;
				}
				value.progressionNode.rootNode = true;
				this.toolTree[grtoolType] = value.progressionNode;
			}
			this.partTree[value.progressionNode.type] = value.progressionNode;
		}
	}

	private void PopulateMetadata()
	{
		foreach (KeyValuePair<string, GRToolProgressionTree.GRToolProgressionRawNode> keyValuePair in this.nodeTree)
		{
			keyValuePair.Value.progressionNode.partMetadata = this.manager.GetPartMetadata(keyValuePair.Value.progressionNode.type);
		}
	}

	private GRToolProgressionTree.EmployeeLevelRequirement GetEmployeeLevel(List<string> rawRequiredEntitlements)
	{
		foreach (string text in rawRequiredEntitlements)
		{
			string text2 = text.Trim();
			if (text2 == "Intern")
			{
				return GRToolProgressionTree.EmployeeLevelRequirement.Intern;
			}
			if (text2 == "PartTime")
			{
				return GRToolProgressionTree.EmployeeLevelRequirement.PartTime;
			}
			if (text2 == "FullTime")
			{
				return GRToolProgressionTree.EmployeeLevelRequirement.FullTime;
			}
		}
		return GRToolProgressionTree.EmployeeLevelRequirement.None;
	}

	private void ProcessTreeNode(UserHydratedNodeDefinition treeNode)
	{
		GRToolProgressionTree.GRToolProgressionRawNode grtoolProgressionRawNode = new GRToolProgressionTree.GRToolProgressionRawNode();
		grtoolProgressionRawNode.progressionNode.id = treeNode.id;
		grtoolProgressionRawNode.progressionNode.name = treeNode.name;
		grtoolProgressionRawNode.progressionNode.unlocked = treeNode.unlocked;
		if (this.partMapping.ContainsKey(grtoolProgressionRawNode.progressionNode.name))
		{
			if (this.toolMapping.ContainsKey(grtoolProgressionRawNode.progressionNode.name))
			{
				grtoolProgressionRawNode.progressionNode.rootNode = true;
			}
			grtoolProgressionRawNode.progressionNode.type = this.partMapping[grtoolProgressionRawNode.progressionNode.name];
		}
		if (treeNode.cost != null && treeNode.cost.items != null)
		{
			foreach (KeyValuePair<string, MothershipHydratedInventoryChange> keyValuePair in treeNode.cost.items)
			{
				if (keyValuePair.Key.Trim() == this.researchPointsEntitlement)
				{
					grtoolProgressionRawNode.progressionNode.researchCost = keyValuePair.Value.Delta;
				}
			}
		}
		foreach (MothershipEntitlementCatalogItem mothershipEntitlementCatalogItem in treeNode.prerequisite_entitlements)
		{
			grtoolProgressionRawNode.requiredEntitlements.Add(mothershipEntitlementCatalogItem.name);
		}
		foreach (SWIGTYPE_p_std__variantT_MothershipApiShared__NodeReference_MothershipApiShared__ComplexPrerequisiteNodes_t swigtype_p_std__variantT_MothershipApiShared__NodeReference_MothershipApiShared__ComplexPrerequisiteNodes_t in treeNode.prerequisite_nodes.nodes)
		{
			ComplexPrerequisiteNodes complexPrerequisiteNodes = new ComplexPrerequisiteNodes();
			NodeReference nodeReference = new NodeReference();
			if (!MothershipApi.TryGetComplexPrerequisiteNodeFromVariant(swigtype_p_std__variantT_MothershipApiShared__NodeReference_MothershipApiShared__ComplexPrerequisiteNodes_t, complexPrerequisiteNodes) && MothershipApi.TryGetNodeReferenceFromVariant(swigtype_p_std__variantT_MothershipApiShared__NodeReference_MothershipApiShared__ComplexPrerequisiteNodes_t, nodeReference))
			{
				grtoolProgressionRawNode.requiredByIds.Add(nodeReference.node_id);
			}
		}
		if (this.pendingPartUnlock != GRToolProgressionManager.ToolParts.None && this.pendingPartUnlock == grtoolProgressionRawNode.progressionNode.type)
		{
			GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
			if (this.pendingPartUnlock == GRToolProgressionManager.ToolParts.DropPodBasic || this.pendingPartUnlock == GRToolProgressionManager.ToolParts.DropPodChassis1 || this.pendingPartUnlock == GRToolProgressionManager.ToolParts.DropPodChassis2 || this.pendingPartUnlock == GRToolProgressionManager.ToolParts.DropPodChassis3)
			{
				if (this.pendingPartUnlock != GRToolProgressionManager.ToolParts.DropPodBasic)
				{
					grplayer.SendPodUpgradeTelemetry(grtoolProgressionRawNode.progressionNode.name, treeNode.prerequisite_entitlements.Count, 0, grtoolProgressionRawNode.progressionNode.researchCost);
				}
			}
			else
			{
				grplayer.SendToolUpgradeTelemetry("Research", grtoolProgressionRawNode.progressionNode.name, treeNode.prerequisite_entitlements.Count, grtoolProgressionRawNode.progressionNode.researchCost, 0, 0);
			}
			this.pendingPartUnlock = GRToolProgressionManager.ToolParts.None;
		}
		this.nodeTree[grtoolProgressionRawNode.progressionNode.id] = grtoolProgressionRawNode;
	}

	private void ProcessToolProgressionTree(UserHydratedProgressionTreeResponse tree)
	{
		if (tree.Tree.name != this.treeName)
		{
			return;
		}
		this.toolTree = new Dictionary<GRTool.GRToolType, GRToolProgressionTree.GRToolProgressionNode>();
		this.nodeTree = new Dictionary<string, GRToolProgressionTree.GRToolProgressionRawNode>();
		this.treeId = tree.Tree.id;
		foreach (UserHydratedNodeDefinition userHydratedNodeDefinition in tree.Nodes)
		{
			this.ProcessTreeNode(userHydratedNodeDefinition);
		}
		this.PopulateMetadata();
		this.ProcessNodes();
		GRToolProgressionManager grtoolProgressionManager = this.manager;
		if (grtoolProgressionManager != null)
		{
			grtoolProgressionManager.SendMothershipUpdated();
		}
		if (this.autoUnlockNodeId != string.Empty)
		{
			string text = this.autoUnlockNodeId;
			this.autoUnlockNodeId = string.Empty;
			GhostReactorProgression.instance.UnlockProgressionTreeNode(this.treeId, text, this.reactor);
		}
	}

	public void AttemptToUnlockPart(GRToolProgressionManager.ToolParts part)
	{
		if (this.partTree.ContainsKey(part))
		{
			this.pendingPartUnlock = part;
			GhostReactorProgression.instance.UnlockProgressionTreeNode(this.treeId, this.partTree[part].id, this.reactor);
		}
	}

	private string treeName = "GRTools";

	private string treeId = string.Empty;

	private string researchPointsEntitlement = "GR_ResearchPoints";

	private Dictionary<GRTool.GRToolType, GRToolProgressionTree.GRToolProgressionNode> toolTree = new Dictionary<GRTool.GRToolType, GRToolProgressionTree.GRToolProgressionNode>();

	private Dictionary<GRToolProgressionManager.ToolParts, GRToolProgressionTree.GRToolProgressionNode> partTree = new Dictionary<GRToolProgressionManager.ToolParts, GRToolProgressionTree.GRToolProgressionNode>();

	private Dictionary<string, GRToolProgressionTree.GRToolProgressionRawNode> nodeTree = new Dictionary<string, GRToolProgressionTree.GRToolProgressionRawNode>();

	private Dictionary<string, GRTool.GRToolType> toolMapping = new Dictionary<string, GRTool.GRToolType>();

	private Dictionary<string, GRToolProgressionManager.ToolParts> partMapping = new Dictionary<string, GRToolProgressionManager.ToolParts>();

	private string autoUnlockNodeId = string.Empty;

	private int currentResearchPoints;

	[NonSerialized]
	private GhostReactor reactor;

	[NonSerialized]
	private GRToolProgressionManager manager;

	[NonSerialized]
	private GRToolProgressionTree.EmployeeLevelRequirement currentEmploymentLevel;

	private string internEntitlement = "Intern";

	private string partTimeEntitlement = "PartTime";

	private string fullTimeEntitlement = "FullTime";

	private GRToolProgressionManager.ToolParts pendingPartUnlock;

	public enum EmployeeLevelRequirement
	{
		None,
		Intern,
		PartTime,
		FullTime
	}

	public class GRToolProgressionNode
	{
		public string id;

		public string name;

		public bool unlocked;

		public int researchCost;

		public bool rootNode;

		public GRToolProgressionManager.ToolParts type;

		public GRToolProgressionManager.ToolProgressionMetaData partMetadata;

		public List<GRToolProgressionTree.GRToolProgressionNode> children = new List<GRToolProgressionTree.GRToolProgressionNode>();

		public List<GRToolProgressionTree.GRToolProgressionNode> parents = new List<GRToolProgressionTree.GRToolProgressionNode>();

		public GRToolProgressionTree.EmployeeLevelRequirement requiredEmployeeLevel;
	}

	private class GRToolProgressionRawNode
	{
		public GRToolProgressionTree.GRToolProgressionNode progressionNode = new GRToolProgressionTree.GRToolProgressionNode();

		public List<string> requiredByIds = new List<string>();

		public List<string> requiredEntitlements = new List<string>();
	}
}
