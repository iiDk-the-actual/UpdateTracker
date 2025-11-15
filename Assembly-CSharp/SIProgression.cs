using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SIProgression : MonoBehaviour, IGorillaSliceableSimple, GorillaQuestManager
{
	public static SIProgression Instance { get; private set; }

	public event Action OnClientReady;

	public Dictionary<SITechTreePageId, int> HeldOrSnappedByGadgetPageType
	{
		get
		{
			return this.heldOrSnappedByGadgetPageType;
		}
	}

	public bool HeldOrSnappedOthersGadgets
	{
		get
		{
			return this.heldOrSnappedOthersGadgets > 0;
		}
	}

	private void Awake()
	{
		if (SIProgression.Instance == null)
		{
			SIProgression.Instance = this;
		}
		this.emptyNode = default(SIProgression.SINode);
		SIProgression.InitResourceToStringDictionary();
		this.resourceCapsArray = Enumerable.Repeat<int>(int.MaxValue, 6).ToArray<int>();
		for (int i = 0; i < this.resourceCaps.Length; i++)
		{
			this.resourceCapsArray[(int)this.resourceCaps[i].resourceType] = this.resourceCaps[i].resourceMax;
		}
		foreach (object obj in Enum.GetValues(typeof(SITechTreePageId)))
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)obj;
			this.heldOrSnappedByGadgetPageType.Add(sitechTreePageId, 0);
		}
		this.EnsureInitialized();
		SIProgression.InitializeQuests();
		this.ResetTelemetryIntervalData();
		this.LoadSavedTelemetryData();
	}

	public void OnEnable()
	{
		if (ProgressionManager.Instance != null)
		{
			ProgressionManager.Instance.OnTreeUpdated += this.HandleTreeUpdated;
			ProgressionManager.Instance.OnInventoryUpdated += this.HandleInventoryUpdated;
			ProgressionManager.Instance.OnNodeUnlocked += this.HandleNodeUnlocked;
		}
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		if (ProgressionManager.Instance != null)
		{
			ProgressionManager.Instance.OnTreeUpdated -= this.HandleTreeUpdated;
			ProgressionManager.Instance.OnInventoryUpdated -= this.HandleInventoryUpdated;
			ProgressionManager.Instance.OnNodeUnlocked -= this.HandleNodeUnlocked;
		}
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public static string GetResourceString(SIResource.ResourceType resourceType)
	{
		if (SIProgression._resourceToString == null)
		{
			SIProgression.InitResourceToStringDictionary();
		}
		return SIProgression._resourceToString[resourceType];
	}

	private static void InitResourceToStringDictionary()
	{
		SIProgression._resourceToString = new Dictionary<SIResource.ResourceType, string>();
		SIProgression._resourceToString[SIResource.ResourceType.TechPoint] = "SI_TechPoints";
		SIProgression._resourceToString[SIResource.ResourceType.StrangeWood] = "SI_StrangeWood";
		SIProgression._resourceToString[SIResource.ResourceType.WeirdGear] = "SI_WeirdGear";
		SIProgression._resourceToString[SIResource.ResourceType.VibratingSpring] = "SI_VibratingSpring";
		SIProgression._resourceToString[SIResource.ResourceType.BouncySand] = "SI_BouncySand";
		SIProgression._resourceToString[SIResource.ResourceType.FloppyMetal] = "SI_FloppyMetal";
	}

	public void Init()
	{
		SIPlayer.LocalPlayer.SetProgressionLocal();
		if (ProgressionManager.Instance != null)
		{
			ProgressionManager.Instance.RefreshProgressionTree();
			ProgressionManager.Instance.RefreshUserInventory();
		}
		this.ClearAllQuestEventListeners();
		this.SetupAllQuestEventListeners();
	}

	public void EnsureInitialized()
	{
		if (this.techTreeSO != null)
		{
			this.techTreeSO.EnsureInitialized();
		}
		if (SIPlayer.progressionSO == null)
		{
			SIPlayer.progressionSO = this.techTreeSO;
		}
		int num = 6;
		if (this.resourceDict == null || this.resourceDict.Count != num)
		{
			this.resourceDict = new Dictionary<SIResource.ResourceType, int>();
			this.resourceDict[SIResource.ResourceType.TechPoint] = 0;
			this.resourceDict[SIResource.ResourceType.StrangeWood] = 0;
			this.resourceDict[SIResource.ResourceType.VibratingSpring] = 0;
			this.resourceDict[SIResource.ResourceType.BouncySand] = 0;
			this.resourceDict[SIResource.ResourceType.FloppyMetal] = 0;
			this.resourceDict[SIResource.ResourceType.WeirdGear] = 0;
		}
		int num2 = 2;
		if (this.limitedDepositTimeArray == null || this.limitedDepositTimeArray.Length != num2)
		{
			this.limitedDepositTimeArray = new int[num2];
		}
		int treePageCount = SIPlayer.progressionSO.TreePageCount;
		if (this.unlockedTechTreeData == null || this.unlockedTechTreeData.Length != treePageCount)
		{
			this.unlockedTechTreeData = new bool[treePageCount][];
		}
		for (int i = 0; i < treePageCount; i++)
		{
			int num3 = SIPlayer.progressionSO.TreeNodeCounts[i];
			if (this.unlockedTechTreeData[i] == null || this.unlockedTechTreeData[i].Length != num3)
			{
				this.unlockedTechTreeData[i] = new bool[num3];
			}
		}
		if (this.activeQuestIds == null || this.activeQuestIds.Length != 3)
		{
			this.activeQuestIds = new int[3];
		}
		if (this.activeQuestProgresses == null || this.activeQuestProgresses.Length != 3)
		{
			this.activeQuestProgresses = new int[3];
		}
		this.CopySaveDataToDiff();
	}

	private void ApplyServerQuestsStatus(ProgressionManager.UserQuestsStatusResponse userQuestsStatus)
	{
		if (userQuestsStatus == null)
		{
			return;
		}
		this.stashedQuests = userQuestsStatus.TodayClaimableQuests;
		this.stashedBonusPoints = userQuestsStatus.TodayClaimableBonus;
		this.dailyLimitedTurnedIn = userQuestsStatus.TodayClaimableIdol <= 0;
		this.lastQuestGrantTime = DateTime.UtcNow;
		this.RefreshActiveQuests();
		SIPlayer.SetAndBroadcastProgression();
		if (!this.questsInitialized)
		{
			this.questsInitialized = true;
			this.ClientReady = true;
			Action onClientReady = this.OnClientReady;
			if (onClientReady == null)
			{
				return;
			}
			onClientReady();
		}
	}

	public int GetCurrencyAmount(SIResource.ResourceType currencyType)
	{
		ProgressionManager.MothershipItemSummary mothershipItemSummary;
		if (!ProgressionManager.Instance.GetInventoryItem(SIProgression._resourceToString[currencyType], out mothershipItemSummary))
		{
			return 0;
		}
		return mothershipItemSummary.Quantity;
	}

	public bool IsNodeUnlocked(SIUpgradeType upgradeType)
	{
		if (this.siNodes != null)
		{
			SIProgression.SINode sinode;
			return this.siNodes.TryGetValue(upgradeType, out sinode) && sinode.unlocked;
		}
		ProgressionManager instance = ProgressionManager.Instance;
		UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse = ((instance != null) ? instance.GetTree("SI_Gadgets") : null);
		if (userHydratedProgressionTreeResponse != null)
		{
			foreach (UserHydratedNodeDefinition userHydratedNodeDefinition in userHydratedProgressionTreeResponse.Nodes)
			{
				if (userHydratedNodeDefinition.name == upgradeType.ToString())
				{
					return userHydratedNodeDefinition.unlocked;
				}
			}
			return false;
		}
		return false;
	}

	public void UnlockNode(SIUpgradeType upgradeType)
	{
		if (this._treeReady && this._inventoryReady)
		{
			ProgressionManager instance = ProgressionManager.Instance;
			UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse = ((instance != null) ? instance.GetTree("SI_Gadgets") : null);
			SIProgression.SINode sinode;
			if (this.siNodes == null || !this.siNodes.TryGetValue(upgradeType, out sinode) || sinode.unlocked)
			{
				return;
			}
			ProgressionManager.Instance.UnlockNode(userHydratedProgressionTreeResponse.Tree.id, sinode.id);
		}
	}

	private void HandleTreeUpdated()
	{
		this._treeReady = true;
		this.UpdateTree();
		this.UpdateUnlockOnPlayer();
		if (!this._startingPackageGranted)
		{
			if (this.IsNodeUnlocked(SIUpgradeType.Initialize))
			{
				this._startingPackageGranted = true;
			}
			else
			{
				this.startingPackageBackupAttempts++;
				if (this.startingPackageBackupAttempts > 10)
				{
					this._startingPackageGranted = true;
				}
				else
				{
					base.StartCoroutine(this.TryClaimNewPlayerPackage());
				}
			}
		}
		Action onTreeReady = this.OnTreeReady;
		if (onTreeReady == null)
		{
			return;
		}
		onTreeReady();
	}

	private void HandleInventoryUpdated()
	{
		this._inventoryReady = true;
		this.UpdateCurrencyOnPlayer();
		Action onInventoryReady = this.OnInventoryReady;
		if (onInventoryReady == null)
		{
			return;
		}
		onInventoryReady();
	}

	private IEnumerator TryClaimNewPlayerPackage()
	{
		yield return new WaitForSeconds(Mathf.Pow((float)this.startingPackageBackupAttempts, 2f));
		if (!this._startingPackageGranted)
		{
			this.TryUnlock(SIUpgradeType.Initialize);
		}
		yield break;
	}

	private void HandleNodeUnlocked(string treeId, string nodeId)
	{
		this.UpdateTree();
		this.UpdateUnlockOnPlayer();
		SIProgression.SINode nodeFromID = this.GetNodeFromID(nodeId);
		if (!string.IsNullOrEmpty(nodeFromID.id))
		{
			Action<SIUpgradeType> onNodeUnlocked = this.OnNodeUnlocked;
			if (onNodeUnlocked == null)
			{
				return;
			}
			onNodeUnlocked(nodeFromID.upgradeType);
		}
	}

	private void UpdateTree()
	{
		ProgressionManager instance = ProgressionManager.Instance;
		UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse = ((instance != null) ? instance.GetTree("SI_Gadgets") : null);
		this.siNodes = new Dictionary<SIUpgradeType, SIProgression.SINode>();
		foreach (UserHydratedNodeDefinition userHydratedNodeDefinition in userHydratedProgressionTreeResponse.Nodes)
		{
			SIUpgradeType siupgradeType;
			if (!Enum.TryParse<SIUpgradeType>(userHydratedNodeDefinition.name, out siupgradeType))
			{
				siupgradeType = SIUpgradeType.InvalidNode;
			}
			Dictionary<SIResource.ResourceType, int> dictionary = new Dictionary<SIResource.ResourceType, int>();
			HydratedProgressionNodeCost cost = userHydratedNodeDefinition.cost;
			if (((cost != null) ? cost.items : null) != null)
			{
				foreach (KeyValuePair<string, MothershipHydratedInventoryChange> keyValuePair in userHydratedNodeDefinition.cost.items)
				{
					foreach (KeyValuePair<SIResource.ResourceType, string> keyValuePair2 in SIProgression._resourceToString)
					{
						if (keyValuePair2.Value == keyValuePair.Key)
						{
							dictionary[keyValuePair2.Key] = keyValuePair.Value.Delta;
							break;
						}
					}
				}
			}
			SIProgression.SINode sinode = new SIProgression.SINode
			{
				id = userHydratedNodeDefinition.id,
				unlocked = userHydratedNodeDefinition.unlocked,
				costs = dictionary,
				parents = new List<SIProgression.SINode>(),
				upgradeType = siupgradeType
			};
			this.siNodes[siupgradeType] = sinode;
		}
	}

	public bool TryUnlock(SIUpgradeType upgrade)
	{
		if (upgrade == SIUpgradeType.Initialize)
		{
			if (!this._startingPackageGranted)
			{
				this.UnlockNode(upgrade);
				return true;
			}
			return false;
		}
		else
		{
			this.techTreeSO.EnsureInitialized();
			GraphNode<SITechTreeNode> graphNode;
			if (!this.techTreeSO.TryGetNode(upgrade, out graphNode))
			{
				return false;
			}
			SIPlayer localPlayer = SIPlayer.LocalPlayer;
			SITechTreeNode value = graphNode.Value;
			if (localPlayer.NodeResearched(upgrade))
			{
				return false;
			}
			if (!this._treeReady)
			{
				ProgressionManager.Instance.RefreshProgressionTree();
			}
			if (!this._inventoryReady)
			{
				ProgressionManager.Instance.RefreshUserInventory();
			}
			if (!localPlayer.NodeParentsUnlocked(upgrade))
			{
				return false;
			}
			foreach (SIResource.ResourceCost resourceCost in value.nodeCost)
			{
				if (resourceCost.amount > this.GetCurrencyAmount(resourceCost.type))
				{
					return false;
				}
			}
			this.UnlockNode(upgrade);
			return true;
		}
	}

	private SIProgression.SINode GetNodeFromID(string id)
	{
		foreach (KeyValuePair<SIUpgradeType, SIProgression.SINode> keyValuePair in this.siNodes)
		{
			if (keyValuePair.Value.id == id)
			{
				return keyValuePair.Value;
			}
		}
		return default(SIProgression.SINode);
	}

	private void UpdateCurrencyOnPlayer()
	{
		foreach (SIResource.ResourceType resourceType in this.resourceDict.Keys.ToList<SIResource.ResourceType>())
		{
			int num = 0;
			try
			{
				num = this.GetCurrencyAmount(resourceType);
			}
			catch
			{
			}
			this.resourceDict[resourceType] = num;
		}
		SIPlayer.SetAndBroadcastProgression();
		if (!this.ClientReady && this.questSourceList != null)
		{
			this.ClientReady = true;
			Action onClientReady = this.OnClientReady;
			if (onClientReady == null)
			{
				return;
			}
			onClientReady();
		}
	}

	private void UpdateUnlockOnPlayer()
	{
		SIPlayer localPlayer = SIPlayer.LocalPlayer;
		this.techTreeSO.EnsureInitialized();
		foreach (KeyValuePair<SIUpgradeType, SIProgression.SINode> keyValuePair in this.siNodes)
		{
			SIUpgradeType key = keyValuePair.Key;
			if (key >= SIUpgradeType.Thruster_Unlock)
			{
				this.unlockedTechTreeData[key.GetPageId()][key.GetNodeId()] = keyValuePair.Value.unlocked;
			}
		}
		SIPlayer.SetAndBroadcastProgression();
		if (!this.ClientReady && this.questSourceList != null)
		{
			this.ClientReady = true;
			Action onClientReady = this.OnClientReady;
			if (onClientReady == null)
			{
				return;
			}
			onClientReady();
		}
	}

	public int[] ActiveQuestIds
	{
		get
		{
			return this.activeQuestIds;
		}
	}

	public int[] ActiveQuestProgresses
	{
		get
		{
			return this.activeQuestProgresses;
		}
	}

	public bool DailyLimitedTurnedIn
	{
		get
		{
			return this.dailyLimitedTurnedIn;
		}
	}

	public static void InitializeQuests()
	{
		SIProgression.Instance._InitializeQuests();
	}

	private void ProcessAllQuests(Action<RotatingQuest> action)
	{
		foreach (RotatingQuest rotatingQuest in this.questSourceList.quests)
		{
			action(rotatingQuest);
		}
	}

	private void QuestLoadPostProcess(RotatingQuest quest)
	{
		quest.SetRequiredZone();
		if (quest.requiredZones.Count == 1 && quest.requiredZones[0] == GTZone.none)
		{
			quest.requiredZones.Clear();
		}
		quest.isQuestActive = true;
	}

	private void QuestSavePreProcess(RotatingQuest quest)
	{
		if (quest.requiredZones.Count == 0)
		{
			quest.requiredZones.Add(GTZone.none);
		}
	}

	private void _InitializeQuests()
	{
		ProgressionManager.Instance.GetActiveSIQuests(new Action<List<RotatingQuest>>(this.LoadQuestsFromServer), null);
	}

	public void LoadQuestsFromServer(List<RotatingQuest> serverQuests)
	{
		if (serverQuests == null || serverQuests.Count == 0)
		{
			Debug.LogError("[SIProgression] Server returned no quests");
			this.LoadQuestsFromLocalJson();
		}
		else
		{
			this.questSourceList = new SIProgression.SIQuestsList
			{
				quests = serverQuests
			};
			this.ProcessAllQuests(new Action<RotatingQuest>(this.QuestLoadPostProcess));
		}
		this.LoadQuestProgress();
		if (!this.questsInitialized)
		{
			ProgressionManager.Instance.GetSIQuestStatus(new Action<ProgressionManager.UserQuestsStatusResponse>(this.ApplyServerQuestsStatus), null);
		}
	}

	private void LoadQuestsFromLocalJson()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("TestingSuperInfectionQuests");
		this.LoadQuestsFromJson(textAsset.text);
		this.ProcessAllQuests(new Action<RotatingQuest>(this.QuestLoadPostProcess));
	}

	public void SliceUpdate()
	{
		SuperInfectionManager activeSuperInfectionManager = SuperInfectionManager.activeSuperInfectionManager;
		if (activeSuperInfectionManager == null || !activeSuperInfectionManager.IsZoneReady())
		{
			return;
		}
		if (!this.questsInitialized)
		{
			return;
		}
		this.CheckTimeCrossover();
		this.SaveQuestProgress();
		this.CheckTelemetry();
	}

	private void CheckTimeCrossover()
	{
		this.CheckTimeCrossoverServer();
	}

	private void CheckTimeCrossoverServer()
	{
		DateTime utcNow = DateTime.UtcNow;
		DateTime dateTime = utcNow.Date + this.CROSSOVER_TIME_OF_DAY;
		if (dateTime > utcNow)
		{
			dateTime = dateTime.AddDays(-1.0);
		}
		if ((dateTime - this.lastQuestGrantTime).Ticks <= 0L)
		{
			return;
		}
		this.lastQuestGrantTime = utcNow.Date + this.CROSSOVER_TIME_OF_DAY;
		ProgressionManager.Instance.GetSIQuestStatus(new Action<ProgressionManager.UserQuestsStatusResponse>(this.ApplyServerQuestsStatus), null);
	}

	public static void StaticSaveQuestProgress()
	{
		SIProgression.Instance.SaveQuestProgress();
	}

	public void LoadQuestProgress()
	{
		this.LoadQuestProgressServer();
	}

	public void SaveQuestProgress()
	{
		this.SaveQuestProgressServer();
	}

	public void LoadQuestProgressServer()
	{
		int num = 0;
		for (int i = 0; i < this.activeQuestIds.Length; i++)
		{
			int @int = PlayerPrefs.GetInt(string.Format("{0}{1}", "v1_Rotating_Quest_Daily_ID_Key", i), -1);
			int int2 = PlayerPrefs.GetInt(string.Format("{0}{1}", "v1_Rotating_Quest_Daily_Progress_Key", i), -1);
			this.activeQuestIds[i] = @int;
			this.activeQuestProgresses[i] = int2;
			if (@int != -1)
			{
				RotatingQuest questById = this.questSourceList.GetQuestById(@int);
				if (questById == null || !questById.isQuestActive)
				{
					this.activeQuestIds[i] = -1;
					this.activeQuestProgresses[i] = -1;
				}
				else
				{
					num++;
					questById.ApplySavedProgress(int2);
				}
			}
		}
		this.bonusProgress = PlayerPrefs.GetInt("v1_SIProgression:bonusProgress", 0);
		this.CopySaveDataToDiff();
	}

	public void SaveQuestProgressServer()
	{
		int num = 0;
		for (int i = 0; i < this.activeQuestIds.Length; i++)
		{
			if (num >= this.stashedQuests)
			{
				this.activeQuestIds[i] = -1;
				this.activeQuestProgresses[i] = 0;
			}
			RotatingQuest questById = this.questSourceList.GetQuestById(this.activeQuestIds[i]);
			if (questById == null || !questById.isQuestActive)
			{
				this.activeQuestIds[i] = -1;
				this.activeQuestProgresses[i] = 0;
			}
			else
			{
				num++;
			}
			int num2 = -1;
			int num3 = 0;
			if (questById != null)
			{
				num2 = questById.questID;
				num3 = questById.GetProgress();
			}
			this.activeQuestProgresses[i] = num3;
			if (num2 != this.activeQuestIdsDiff[i])
			{
				PlayerPrefs.SetInt(string.Format("{0}{1}", "v1_Rotating_Quest_Daily_ID_Key", i), num2);
			}
			if (num3 != this.activeQuestProgressesDiff[i])
			{
				PlayerPrefs.SetInt(string.Format("{0}{1}", "v1_Rotating_Quest_Daily_Progress_Key", i), num3);
			}
		}
		if (this.bonusProgress != this.bonusProgressDiff)
		{
			PlayerPrefs.SetInt("v1_SIProgression:bonusProgress", this.bonusProgress);
		}
		PlayerPrefs.Save();
		this.CopySaveDataToDiff();
	}

	public void CopySaveDataToDiff()
	{
		this.lastQuestGrantTimeDiff = this.lastQuestGrantTime;
		this.stashedQuestsDiff = this.stashedQuests;
		this.stashedBonusPointsDiff = this.stashedBonusPoints;
		this.bonusProgressDiff = this.bonusProgress;
		int[] array = new int[6];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = this.resourceDict[(SIResource.ResourceType)i];
		}
		SIProgression._SafeShallowCopyArray<int>(array, ref this.resourceArrayDiff);
		SIProgression._SafeShallowCopyArray<int>(this.limitedDepositTimeArray, ref this.limitedDepositTimeDiff);
		SIProgression._SafeShallowCopyArray<int>(this.activeQuestIds, ref this.activeQuestIdsDiff);
		SIProgression._SafeShallowCopyArray<int>(this.activeQuestProgresses, ref this.activeQuestProgressesDiff);
		if (this.unlockedTechTreeDataDiff == null || this.unlockedTechTreeDataDiff.Length != this.unlockedTechTreeData.Length)
		{
			this.unlockedTechTreeDataDiff = new bool[this.unlockedTechTreeData.Length][];
		}
		for (int j = 0; j < this.unlockedTechTreeData.Length; j++)
		{
			SIProgression._SafeShallowCopyArray<bool>(this.unlockedTechTreeData[j], ref this.unlockedTechTreeDataDiff[j]);
		}
	}

	private static void _SafeShallowCopyArray<T>(T[] sourceArray, ref T[] ref_destinationArray)
	{
		if (ref_destinationArray == null || ref_destinationArray.Length != sourceArray.Length)
		{
			ref_destinationArray = new T[sourceArray.Length];
		}
		Array.Copy(sourceArray, ref_destinationArray, sourceArray.Length);
	}

	public int[] GetResourceArray()
	{
		int[] array = new int[this.resourceDict.Count];
		for (int i = 0; i < this.resourceDict.Count; i++)
		{
			array[i] = this.resourceDict[(SIResource.ResourceType)i];
		}
		return array;
	}

	public void SetResourceArray(int[] resourceArray)
	{
		for (int i = 0; i < resourceArray.Length; i++)
		{
			this.resourceDict[(SIResource.ResourceType)i] = resourceArray[i];
		}
	}

	public void HandleQuestCompleted(int questID)
	{
		this.UpdateQuestProgresses();
		SIPlayer.SetAndBroadcastProgression();
		SIPlayer.LocalPlayer.questCompleteCelebrate.SetActive(true);
	}

	public void HandleQuestProgressChanged(bool initialLoad)
	{
		if (this.UpdateQuestProgresses())
		{
			SIPlayer.SetAndBroadcastProgression();
		}
	}

	private bool UpdateQuestProgresses()
	{
		bool flag = false;
		for (int i = 0; i < this.activeQuestIds.Length; i++)
		{
			RotatingQuest questById = this.questSourceList.GetQuestById(this.activeQuestIds[i]);
			int num = 0;
			if (questById != null)
			{
				num = questById.GetProgress();
				if (questById.questType != QuestType.moveDistance || this.activeQuestProgresses[i] / 100 != num / 100)
				{
					flag = true;
				}
			}
			this.activeQuestProgresses[i] = num;
		}
		this.SaveQuestProgress();
		return flag;
	}

	public void AttemptIncrementResource(SIResource.ResourceType resource)
	{
		ProgressionManager.Instance.IncrementSIResource(resource.ToString(), new Action<string>(this.OnSuccessfulIncrementResource), delegate(string err)
		{
			Debug.LogError(err);
		});
	}

	private void OnSuccessfulIncrementResource(string resourceStr)
	{
		if (Enum.Parse<SIResource.ResourceType>(resourceStr) == SIResource.ResourceType.TechPoint)
		{
			SIPlayer.LocalPlayer.TechPointGrantedCelebrate();
		}
		ProgressionManager.Instance.RefreshUserInventory();
	}

	public void AttemptRedeemCompletedQuest(int questIndex)
	{
		RotatingQuest quest = this.questSourceList.GetQuestById(this.activeQuestIds[questIndex]);
		if (quest == null || this.activeQuestIds[questIndex] == -1)
		{
			return;
		}
		if (!quest.isQuestComplete)
		{
			return;
		}
		if (this.redeemingQuestInProgress[questIndex])
		{
			return;
		}
		this.redeemingQuestInProgress[questIndex] = true;
		ProgressionManager.Instance.CompleteSIQuest(quest.questID, delegate(ProgressionManager.UserQuestsStatusResponse status)
		{
			this.OnSuccessfulQuestRedeem(questIndex, quest, status);
		}, delegate(string err)
		{
			if (err.Contains("409") || err.Contains("404"))
			{
				this.OnInvalidQuestRedeemAttempt(questIndex, quest);
			}
			this.redeemingQuestInProgress[questIndex] = false;
			Debug.LogError(err);
		});
	}

	private void OnSuccessfulQuestRedeem(int questIndex, RotatingQuest quest, ProgressionManager.UserQuestsStatusResponse userQuestsStatus)
	{
		this.activeQuestIds[questIndex] = -1;
		this.activeQuestProgresses[questIndex] = 0;
		quest.ApplySavedProgress(0);
		this.redeemingQuestInProgress[questIndex] = false;
		Dictionary<SIResource.ResourceType, int> dictionary = this.resourceDict;
		int num = dictionary[SIResource.ResourceType.TechPoint];
		dictionary[SIResource.ResourceType.TechPoint] = num + 1;
		this.ApplyServerQuestsStatus(userQuestsStatus);
		SIPlayer.LocalPlayer.TechPointGrantedCelebrate();
		ProgressionManager.Instance.RefreshUserInventory();
	}

	private void OnInvalidQuestRedeemAttempt(int questIndex, RotatingQuest quest)
	{
		this.activeQuestIds[questIndex] = -1;
		this.activeQuestProgresses[questIndex] = 0;
		quest.ApplySavedProgress(0);
		ProgressionManager.Instance.GetSIQuestStatus(new Action<ProgressionManager.UserQuestsStatusResponse>(this.ApplyServerQuestsStatus), null);
	}

	public void AttemptRedeemBonusPoint()
	{
		ProgressionManager.Instance.CompleteSIBonus(delegate(ProgressionManager.UserQuestsStatusResponse userQuestsStatus)
		{
			this.OnSuccessfulBonusRedeem(userQuestsStatus);
		}, delegate(string err)
		{
			Debug.LogError(err);
		});
	}

	private void OnSuccessfulBonusRedeem(ProgressionManager.UserQuestsStatusResponse userQuestsStatus)
	{
		this.bonusProgress = 0;
		this.ApplyServerQuestsStatus(userQuestsStatus);
		SIPlayer.LocalPlayer.TechPointGrantedCelebrate();
		ProgressionManager.Instance.RefreshUserInventory();
	}

	public void AttemptCollectMonkeIdol()
	{
		ProgressionManager.Instance.CollectSIIdol(new Action<ProgressionManager.UserQuestsStatusResponse>(this.OnSuccessfulMonkeIdolRedeem), delegate(string err)
		{
			Debug.LogError(err);
		});
	}

	private void OnSuccessfulMonkeIdolRedeem(ProgressionManager.UserQuestsStatusResponse userQuestsStatus)
	{
		this.ApplyServerQuestsStatus(userQuestsStatus);
		this.limitedDepositTimeArray[1] = 1;
		SIPlayer.LocalPlayer.TechPointGrantedCelebrate();
		ProgressionManager.Instance.RefreshUserInventory();
	}

	public void GetBonusProgress()
	{
		this.bonusProgress++;
	}

	public void SetupAllQuestEventListeners()
	{
		for (int i = 0; i < this.activeQuestIds.Length; i++)
		{
			RotatingQuest questById = this.questSourceList.GetQuestById(this.activeQuestIds[i]);
			if (questById != null && this.activeQuestIds[i] != -1)
			{
				questById.questManager = this;
				if (!questById.isQuestComplete)
				{
					questById.AddEventListener();
				}
			}
		}
	}

	public static void StaticClearAllQuestEventListeners()
	{
		SIProgression.Instance.ClearAllQuestEventListeners();
	}

	public void ClearAllQuestEventListeners()
	{
		for (int i = 0; i < this.activeQuestIds.Length; i++)
		{
			RotatingQuest questById = this.questSourceList.GetQuestById(this.activeQuestIds[i]);
			if (questById != null)
			{
				questById.RemoveEventListener();
			}
		}
	}

	public void LoadQuestsFromJson(string jsonString)
	{
		this.questSourceList = JsonConvert.DeserializeObject<SIProgression.SIQuestsList>(jsonString);
		this.ProcessAllQuests(new Action<RotatingQuest>(this.QuestLoadPostProcess));
	}

	public void RefreshActiveQuests()
	{
		this.ClearAllQuestEventListeners();
		this.SelectActiveQuests();
		this.HandleQuestProgressChanged(true);
		this.SetupAllQuestEventListeners();
	}

	private void SelectActiveQuests()
	{
		int num = 0;
		for (int i = 0; i < this.activeQuestIds.Length; i++)
		{
			RotatingQuest questById = this.questSourceList.GetQuestById(this.activeQuestIds[i]);
			if (questById != null && questById.isQuestActive && num < this.stashedQuests)
			{
				this.activeQuestCategories[i] = questById.category;
				num++;
			}
			else
			{
				this.activeQuestIds[i] = -1;
				this.activeQuestProgresses[i] = 0;
				this.activeQuestCategories[i] = QuestCategory.NONE;
				if (questById != null)
				{
					questById.ApplySavedProgress(0);
				}
			}
		}
		int num2 = Mathf.Max(0, this.stashedQuests);
		int num3 = 0;
		while (num3 < this.activeQuestIds.Length && num < num2)
		{
			RotatingQuest questById2 = this.questSourceList.GetQuestById(this.activeQuestIds[num3]);
			if (questById2 == null || !questById2.isQuestActive)
			{
				int num4 = Random.Range(0, this.questSourceList.quests.Count);
				for (int j = 0; j < this.questSourceList.quests.Count; j++)
				{
					int num5 = (num4 + j) % this.questSourceList.quests.Count;
					RotatingQuest questById3 = this.questSourceList.GetQuestById(num5);
					if (questById3 != null && questById3.isQuestActive && this.<SelectActiveQuests>g__GetMatchingCategoryCount|175_0(questById3) < this.perCategoryQuestLimit)
					{
						bool flag = false;
						for (int k = 0; k < this.activeQuestIds.Length; k++)
						{
							if (num5 == this.activeQuestIds[k])
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							this.activeQuestIds[num3] = num5;
							this.activeQuestCategories[num3] = questById3.category;
							questById3.ApplySavedProgress(0);
							this.activeQuestProgresses[num3] = 0;
							num++;
							break;
						}
					}
				}
			}
			num3++;
		}
		this.SaveQuestProgress();
	}

	private void SelectCurrentTurnInDate()
	{
		DateTime dateTime = new DateTime(2025, 1, 10, 18, 0, 0, DateTimeKind.Utc);
		TimeSpan timeSpan = TimeSpan.FromHours(-8.0);
		DateTime dateTime2 = new DateTime(1, 1, 1, 0, 0, 0);
		DateTime dateTime3 = new DateTime(2006, 12, 31, 0, 0, 0);
		TimeSpan timeSpan2 = TimeSpan.FromHours(1.0);
		TimeZoneInfo.TransitionTime transitionTime = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 4, 1, DayOfWeek.Sunday);
		TimeZoneInfo.TransitionTime transitionTime2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 5, DayOfWeek.Sunday);
		DateTime dateTime4 = new DateTime(2007, 1, 1, 0, 0, 0);
		DateTime dateTime5 = new DateTime(9999, 12, 31, 0, 0, 0);
		TimeSpan timeSpan3 = TimeSpan.FromHours(1.0);
		TimeZoneInfo.TransitionTime transitionTime3 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday);
		TimeZoneInfo.TransitionTime transitionTime4 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday);
		TimeZoneInfo timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("Pacific Standard Time", timeSpan, "Pacific Standard Time", "Pacific Standard Time", "Pacific Standard Time", new TimeZoneInfo.AdjustmentRule[]
		{
			TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(dateTime2, dateTime3, timeSpan2, transitionTime, transitionTime2),
			TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(dateTime4, dateTime5, timeSpan3, transitionTime3, transitionTime4)
		});
		if (timeZoneInfo != null && timeZoneInfo.IsDaylightSavingTime(DateTime.UtcNow - timeSpan))
		{
			dateTime -= TimeSpan.FromHours(1.0);
		}
		int days = (DateTime.UtcNow - dateTime).Days;
	}

	public bool TryDepositResources(SIResource.ResourceType type, int count)
	{
		int resourceMaxCap = this.GetResourceMaxCap(type);
		int num = this.resourceDict[type];
		if (resourceMaxCap == num)
		{
			return false;
		}
		count = Math.Min(count, resourceMaxCap - num);
		Dictionary<SIResource.ResourceType, int> dictionary = this.resourceDict;
		dictionary[type] += count;
		this.AttemptIncrementResource(type);
		return true;
	}

	public int GetResourceMaxCap(SIResource.ResourceType type)
	{
		return this.resourceCapsArray[(int)type];
	}

	public bool IsLimitedDepositAvailable(SIResource.LimitedDepositType limitedDepositType)
	{
		return !this.dailyLimitedTurnedIn;
	}

	public void ApplyLimitedDepositTime(SIResource.LimitedDepositType limitedDepositType)
	{
		if (limitedDepositType == SIResource.LimitedDepositType.None)
		{
			return;
		}
		this.AttemptCollectMonkeIdol();
	}

	private void OnDestroy()
	{
		this.SaveQuestProgress();
	}

	public bool GetOnlineNode(SIUpgradeType type, out SIProgression.SINode node)
	{
		if (!this._treeReady)
		{
			node = this.emptyNode;
			return false;
		}
		return this.siNodes.TryGetValue(type, out node);
	}

	public static bool ResourcesMaxed()
	{
		return SIProgression.Instance._ResourcesMaxed();
	}

	public bool _ResourcesMaxed()
	{
		foreach (KeyValuePair<SIResource.ResourceType, int> keyValuePair in this.resourceDict)
		{
			if (keyValuePair.Key != SIResource.ResourceType.TechPoint && keyValuePair.Value < this.GetResourceMaxCap(keyValuePair.Key))
			{
				return false;
			}
		}
		return true;
	}

	public void CheckTelemetry()
	{
		SuperInfectionGame instance = SuperInfectionGame.instance;
		if (instance == null)
		{
			return;
		}
		if (!instance.ValidGameMode())
		{
			this.timeTelemetryLastChecked = Time.time;
			return;
		}
		float num = Time.time - this.timeTelemetryLastChecked;
		this.timeTelemetryLastChecked = Time.time;
		this.totalPlayTime += num;
		if (NetworkSystem.Instance.InRoom)
		{
			this.roomPlayTime += num;
		}
		this.intervalPlayTime += num;
		for (int i = 0; i < 11; i++)
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)i;
			if (SIProgression.Instance.HeldOrSnappedByGadgetPageType[sitechTreePageId] > 0)
			{
				Dictionary<SITechTreePageId, float> dictionary = this.timeUsingGadgetTypeInterval;
				SITechTreePageId sitechTreePageId2 = sitechTreePageId;
				dictionary[sitechTreePageId2] += num;
				dictionary = this.timeUsingGadgetTypeTotal;
				sitechTreePageId2 = sitechTreePageId;
				dictionary[sitechTreePageId2] += num;
			}
		}
		if (SIProgression.Instance.heldOrSnappedOwnGadgets > 0)
		{
			this.timeUsingOwnGadgetsInterval += num;
			this.timeUsingOwnGadgetsTotal += num;
		}
		if (SIProgression.Instance.heldOrSnappedOthersGadgets > 0)
		{
			this.timeUsingOthersGadgetsInterval += num;
			this.timeUsingOthersGadgetsTotal += num;
		}
		if (this.lastTelemetrySent + this.telemetryCooldown < Time.time)
		{
			this.lastTelemetrySent = Time.time;
			this.SaveTelemetryData();
			GorillaTelemetry.SuperInfectionEvent(false, this.totalPlayTime, this.roomPlayTime, Time.time, this.intervalPlayTime, this.activeTerminalTimeTotal, this.activeTerminalTimeInterval, this.timeUsingGadgetTypeTotal, this.timeUsingGadgetTypeInterval, this.timeUsingOwnGadgetsTotal, this.timeUsingOwnGadgetsInterval, this.timeUsingOthersGadgetsTotal, this.timeUsingOthersGadgetsInterval, this.tagsUsingGadgetTypeTotal, this.tagsUsingGadgetTypeInterval, this.tagsHoldingOwnGadgetTotal, this.tagsHoldingOwnGadgetInterval, this.tagsHoldingOthersGadgetTotal, this.tagsHoldingOthersGadgetInterval, this.resourcesCollectedTotal, this.resourcesCollectedInterval, this.roundsPlayedTotal, this.roundsPlayedInterval, SIProgression.Instance.unlockedTechTreeData, NetworkSystem.Instance.RoomPlayerCount);
			this.ResetTelemetryIntervalData();
		}
	}

	public void SendTelemetryData()
	{
		if (Time.time < this.lastDisconnectTelemetrySent + this.minDisconnectTelemetryCooldown)
		{
			return;
		}
		this.lastDisconnectTelemetrySent = Time.time;
		this.SaveTelemetryData();
		GorillaTelemetry.SuperInfectionEvent(true, this.totalPlayTime, this.roomPlayTime, Time.time, this.intervalPlayTime, this.activeTerminalTimeTotal, this.activeTerminalTimeInterval, this.timeUsingGadgetTypeTotal, this.timeUsingGadgetTypeInterval, this.timeUsingOwnGadgetsTotal, this.timeUsingOwnGadgetsInterval, this.timeUsingOthersGadgetsTotal, this.timeUsingOthersGadgetsInterval, this.tagsUsingGadgetTypeTotal, this.tagsUsingGadgetTypeInterval, this.tagsHoldingOwnGadgetTotal, this.tagsHoldingOwnGadgetInterval, this.tagsHoldingOthersGadgetTotal, this.tagsHoldingOthersGadgetInterval, this.resourcesCollectedTotal, this.resourcesCollectedInterval, this.roundsPlayedTotal, this.roundsPlayedInterval, SIProgression.Instance.unlockedTechTreeData, NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.RoomPlayerCount : (-1));
		this.ResetTelemetryIntervalData();
		this.roomPlayTime = 0f;
	}

	public void SendPurchaseResourcesData()
	{
		this.SaveTelemetryData();
		GorillaTelemetry.SuperInfectionEvent("si_fill_resources", 500, -1, this.totalPlayTime, this.roomPlayTime, Time.time);
	}

	public void SendPurchaseTechPointsData(int techPointsPurchased)
	{
		this.SaveTelemetryData();
		GorillaTelemetry.SuperInfectionEvent("si_purchase_tech_points", techPointsPurchased * 100, techPointsPurchased, this.totalPlayTime, this.roomPlayTime, Time.time);
	}

	public void LoadSavedTelemetryData()
	{
		this.totalPlayTime = PlayerPrefs.GetFloat("super_infection_total_play_time", 0f);
		for (int i = 0; i < 11; i++)
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)i;
			this.timeUsingGadgetTypeTotal[sitechTreePageId] = PlayerPrefs.GetFloat("super_infection_time_holding_gadget_type_total" + sitechTreePageId.GetName<SITechTreePageId>(), 0f);
			this.tagsUsingGadgetTypeTotal[sitechTreePageId] = PlayerPrefs.GetInt("super_infection_tags_holding_gadget_type_total" + sitechTreePageId.GetName<SITechTreePageId>(), 0);
		}
		this.activeTerminalTimeTotal = PlayerPrefs.GetFloat("super_infection_terminal_total_time", 0f);
		this.tagsHoldingOthersGadgetTotal = PlayerPrefs.GetInt("super_infection_tags_holding_others_gadgets_total", 0);
		this.tagsHoldingOwnGadgetTotal = PlayerPrefs.GetInt("super_infection_tags_holding_own_gadgets_total", 0);
		for (int j = 0; j < 6; j++)
		{
			SIResource.ResourceType resourceType = (SIResource.ResourceType)j;
			this.resourcesCollectedTotal[resourceType] = PlayerPrefs.GetInt("super_infection_resource_type_collected_total" + resourceType.GetName<SIResource.ResourceType>(), 0);
		}
		this.roundsPlayedTotal = PlayerPrefs.GetInt("super_infection_rounds_played_total", 0);
	}

	private void SaveTelemetryData()
	{
		PlayerPrefs.SetFloat("super_infection_total_play_time", this.totalPlayTime);
		for (int i = 0; i < 11; i++)
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)i;
			PlayerPrefs.SetFloat("super_infection_time_holding_gadget_type_total" + sitechTreePageId.GetName<SITechTreePageId>(), this.timeUsingGadgetTypeTotal[sitechTreePageId]);
			PlayerPrefs.SetInt("super_infection_tags_holding_gadget_type_total" + sitechTreePageId.GetName<SITechTreePageId>(), this.tagsUsingGadgetTypeTotal[sitechTreePageId]);
		}
		PlayerPrefs.SetFloat("super_infection_terminal_total_time", this.activeTerminalTimeTotal);
		PlayerPrefs.SetInt("super_infection_tags_holding_others_gadgets_total", this.tagsHoldingOthersGadgetTotal);
		PlayerPrefs.SetInt("super_infection_tags_holding_own_gadgets_total", this.tagsHoldingOwnGadgetTotal);
		for (int j = 0; j < 6; j++)
		{
			SIResource.ResourceType resourceType = (SIResource.ResourceType)j;
			PlayerPrefs.SetInt("super_infection_resource_type_collected_total" + resourceType.GetName<SIResource.ResourceType>(), this.resourcesCollectedTotal[resourceType]);
		}
		PlayerPrefs.SetInt("super_infection_rounds_played_total", this.roundsPlayedTotal);
		PlayerPrefs.Save();
	}

	public void ResetTelemetryIntervalData()
	{
		this.lastTelemetrySent = Time.time;
		this.intervalPlayTime = 0f;
		this.activeTerminalTimeInterval = 0f;
		for (int i = 0; i < 11; i++)
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)i;
			this.timeUsingGadgetTypeInterval[sitechTreePageId] = 0f;
			this.tagsUsingGadgetTypeInterval[sitechTreePageId] = 0;
		}
		this.timeUsingOwnGadgetsInterval = 0f;
		this.timeUsingOthersGadgetsInterval = 0f;
		this.tagsHoldingOthersGadgetInterval = 0;
		this.tagsHoldingOwnGadgetInterval = 0;
		for (int j = 0; j < 6; j++)
		{
			SIResource.ResourceType resourceType = (SIResource.ResourceType)j;
			this.resourcesCollectedInterval[resourceType] = 0;
		}
		this.roundsPlayedInterval = 0;
	}

	public void HandleTagTelemetry(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		if (taggingPlayer.ActorNumber != SIPlayer.LocalPlayer.ActorNr)
		{
			return;
		}
		for (int i = 0; i < 11; i++)
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)i;
			if (SIProgression.Instance.HeldOrSnappedByGadgetPageType[sitechTreePageId] > 0)
			{
				Dictionary<SITechTreePageId, int> dictionary = this.tagsUsingGadgetTypeTotal;
				SITechTreePageId sitechTreePageId2 = sitechTreePageId;
				int num = dictionary[sitechTreePageId2];
				dictionary[sitechTreePageId2] = num + 1;
				Dictionary<SITechTreePageId, int> dictionary2 = this.tagsUsingGadgetTypeInterval;
				sitechTreePageId2 = sitechTreePageId;
				num = dictionary2[sitechTreePageId2];
				dictionary2[sitechTreePageId2] = num + 1;
			}
		}
		if (SIProgression.Instance.heldOrSnappedOwnGadgets > 0)
		{
			this.tagsHoldingOwnGadgetInterval++;
			this.tagsHoldingOwnGadgetTotal++;
		}
		if (SIProgression.Instance.heldOrSnappedOthersGadgets > 0)
		{
			this.tagsHoldingOthersGadgetInterval++;
			this.tagsHoldingOthersGadgetTotal++;
		}
	}

	public void UpdateHeldGadgetsTelemetry(SITechTreePageId id, bool isMine, int changeAmount)
	{
		Dictionary<SITechTreePageId, int> dictionary = SIProgression.Instance.heldOrSnappedByGadgetPageType;
		dictionary[id] += changeAmount;
		if (isMine)
		{
			SIProgression.Instance.heldOrSnappedOwnGadgets += changeAmount;
			return;
		}
		SIProgression.Instance.heldOrSnappedOthersGadgets += changeAmount;
	}

	public void CollectResourceTelemetry(SIResource.ResourceType type, int count)
	{
		Dictionary<SIResource.ResourceType, int> dictionary = this.resourcesCollectedTotal;
		dictionary[type] += count;
		dictionary = this.resourcesCollectedInterval;
		dictionary[type] += count;
	}

	public void AddRoundTelemetry()
	{
		this.roundsPlayedInterval++;
		this.roundsPlayedTotal++;
	}

	[CompilerGenerated]
	private int <SelectActiveQuests>g__GetMatchingCategoryCount|175_0(RotatingQuest quest)
	{
		if (quest.category == QuestCategory.NONE)
		{
			return 0;
		}
		int num = 0;
		QuestCategory[] array = this.activeQuestCategories;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == quest.category)
			{
				num++;
			}
		}
		return num;
	}

	[SerializeField]
	private SITechTreeSO techTreeSO;

	[SerializeField]
	private int perCategoryQuestLimit = 1;

	public Action OnTreeReady;

	public Action OnInventoryReady;

	public Action<SIUpgradeType> OnNodeUnlocked;

	public bool ClientReady;

	private static Dictionary<SIResource.ResourceType, string> _resourceToString;

	private const string TREE_NAME = "SI_Gadgets";

	private Dictionary<SIUpgradeType, SIProgression.SINode> siNodes;

	private bool _treeReady;

	private bool _inventoryReady;

	public Dictionary<SITechTreePageId, int> heldOrSnappedByGadgetPageType = new Dictionary<SITechTreePageId, int>();

	public int heldOrSnappedOwnGadgets;

	public int heldOrSnappedOthersGadgets;

	public float timeTelemetryLastChecked;

	public float lastTelemetrySent;

	private float telemetryCooldown = 600f;

	private float totalPlayTime;

	private float roomPlayTime;

	private float intervalPlayTime;

	[NonSerialized]
	public float activeTerminalTimeTotal;

	[NonSerialized]
	public float activeTerminalTimeInterval;

	private Dictionary<SITechTreePageId, float> timeUsingGadgetTypeTotal = new Dictionary<SITechTreePageId, float>();

	private Dictionary<SITechTreePageId, float> timeUsingGadgetTypeInterval = new Dictionary<SITechTreePageId, float>();

	private float timeUsingOthersGadgetsTotal;

	private float timeUsingOthersGadgetsInterval;

	private float timeUsingOwnGadgetsTotal;

	private float timeUsingOwnGadgetsInterval;

	private Dictionary<SITechTreePageId, int> tagsUsingGadgetTypeTotal = new Dictionary<SITechTreePageId, int>();

	private Dictionary<SITechTreePageId, int> tagsUsingGadgetTypeInterval = new Dictionary<SITechTreePageId, int>();

	private int tagsHoldingOthersGadgetTotal;

	private int tagsHoldingOthersGadgetInterval;

	private int tagsHoldingOwnGadgetTotal;

	private int tagsHoldingOwnGadgetInterval;

	private Dictionary<SIResource.ResourceType, int> resourcesCollectedTotal = new Dictionary<SIResource.ResourceType, int>();

	private Dictionary<SIResource.ResourceType, int> resourcesCollectedInterval = new Dictionary<SIResource.ResourceType, int>();

	private int roundsPlayedTotal;

	private int roundsPlayedInterval;

	private SIProgression.SINode emptyNode;

	public SIProgression.SIQuestsList questSourceList;

	private const int STARTING_STASHED_QUESTS = 0;

	private const int STARTING_STASHED_BONUS_POINTS = 0;

	public const int SHARED_QUEST_TURNINS_FOR_POINT = 10;

	public const int NEW_QUESTS_PER_DAY = 3;

	public const int NEW_BONUS_POINTS_PER_DAY = 1;

	public const int MAX_STASHED_QUESTS = 6;

	public const int MAX_STASHED_BONUS_POINTS = 2;

	public const int MAX_RESOURCE_COUNT = 30;

	private const int ACTIVE_QUEST_COUNT = 3;

	private const string kLocalQuestPath = "TestingSuperInfectionQuests";

	private const string kVersion = "v1_";

	private const string kLastQuestGrantTime = "v1_SIProgression:lastSharedGrantTime";

	private const string kBonusProgress = "v1_SIProgression:bonusProgress";

	private const string kDailyQuestId = "v1_Rotating_Quest_Daily_ID_Key";

	private const string kDailyQuestProgress = "v1_Rotating_Quest_Daily_Progress_Key";

	private const string kStashedQuests = "v1_SIProgression:stashedQuests";

	private const string kStashedBonusPoints = "v1_SIProgression:stashedBonusPoints";

	private const string kTechTree = "v1_SITechTree:";

	private const string kLimitedDeposit = "v1_SIResource:LimitedDeposit:";

	private const string kTechPoints = "v1_SIResource:techPoints";

	private const string kStrangeWood = "v1_SIResource:strangeWood";

	private const string kWeirdGear = "v1_SIResource:weirdGear";

	private const string kVibratingSpring = "v1_SIResource:vibratingSpring";

	private const string kBouncySand = "v1_SIResource:bouncySand";

	private const string kFloppyMetal = "v1_SIResource:floppyMetal";

	private const string kStartingPackageGranted = "v1_SIProgression:startingPackageGranted";

	public TimeSpan CROSSOVER_TIME_OF_DAY = new TimeSpan(1, 0, 0);

	public DateTime lastQuestGrantTime;

	public int stashedQuests;

	public int completedQuests;

	public int stashedBonusPoints;

	public int completedBonusPoints;

	public int bonusProgress;

	public int questGrantRefreshCooldown = 28800;

	public Dictionary<SIResource.ResourceType, int> resourceDict;

	public int[] limitedDepositTimeArray;

	public bool[][] unlockedTechTreeData;

	[SerializeField]
	private int[] activeQuestIds = new int[3];

	[SerializeField]
	private int[] activeQuestProgresses = new int[3];

	[SerializeField]
	private QuestCategory[] activeQuestCategories = new QuestCategory[3];

	private bool dailyLimitedTurnedIn;

	public SIProgression.SIProgressionResourceCap[] resourceCaps;

	public int[] resourceCapsArray;

	private DateTime lastQuestGrantTimeDiff;

	private int stashedQuestsDiff;

	private int stashedBonusPointsDiff;

	private int bonusProgressDiff;

	private int[] resourceArrayDiff;

	private int[] limitedDepositTimeDiff;

	private bool[][] unlockedTechTreeDataDiff;

	private int[] activeQuestIdsDiff;

	private int[] activeQuestProgressesDiff;

	private bool questsInitialized;

	private bool _startingPackageGranted;

	private float lastStartingPackageAttemptStarted;

	private int startingPackageBackupAttempts;

	private const int STARTING_PACKAGE_MAX_ATTEMPTS = 10;

	private bool[] redeemingQuestInProgress = new bool[3];

	private float lastDisconnectTelemetrySent;

	private float minDisconnectTelemetryCooldown = 60f;

	public struct SINode
	{
		public string id;

		public bool unlocked;

		public Dictionary<SIResource.ResourceType, int> costs;

		public List<SIProgression.SINode> parents;

		public SIUpgradeType upgradeType;
	}

	[Serializable]
	public class SIQuestsList
	{
		public RotatingQuest GetQuestById(int questID)
		{
			foreach (RotatingQuest rotatingQuest in this.quests)
			{
				if (rotatingQuest.questID == questID)
				{
					return rotatingQuest.disable ? null : rotatingQuest;
				}
			}
			return null;
		}

		public List<RotatingQuest> quests;
	}

	[Serializable]
	public struct SIProgressionResourceCap
	{
		public SIResource.ResourceType resourceType;

		public int resourceMax;
	}
}
