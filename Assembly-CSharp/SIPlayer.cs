using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class SIPlayer : MonoBehaviour
{
	public static SIPlayer LocalPlayer
	{
		get
		{
			return SIPlayer.Get(NetworkSystem.Instance.LocalPlayer.ActorNumber);
		}
	}

	public int ActorNr
	{
		get
		{
			if (!this.gamePlayer.rig.isOfflineVRRig)
			{
				return this.gamePlayer.rig.OwningNetPlayer.ActorNumber;
			}
			return NetworkSystem.Instance.LocalPlayerID;
		}
	}

	public SIPlayer.ProgressionData CurrentProgression
	{
		get
		{
			return this.currentProgression;
		}
	}

	private void Awake()
	{
		this.activePlayerGadgets = new List<int>();
		SIPlayer.progressionSO = this.progressionSORef;
		this.clientToAuthorityRPCLimiter = new CallLimiter(25, 1f, 0.5f);
		this.clientToClientRPCLimiter = new CallLimiter(25, 1f, 0.5f);
		this.authorityToClientRPCLimiter = new CallLimiter(25, 1f, 0.5f);
		this.currentProgression = new SIPlayer.ProgressionData(true);
		GamePlayer gamePlayer = this.gamePlayer;
		gamePlayer.OnPlayerLeftZone = (Action)Delegate.Combine(gamePlayer.OnPlayerLeftZone, new Action(this.ClearGadgetsOnLeaveZone));
	}

	private void OnDisable()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.Reset();
	}

	public void Reset()
	{
		if (SIPlayer.LocalPlayer == this)
		{
			SIProgression.StaticSaveQuestProgress();
			SIProgression.StaticClearAllQuestEventListeners();
		}
		this.lastQuestsAvailableToClaim = 999;
		this.tpParticleSystem.Stop();
		this.netInitialized = false;
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public bool ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		return true;
	}

	public static SIPlayer Get(int actorNumber)
	{
		SIPlayer siplayer;
		if (SIPlayer.siPlayerByActorNr.TryGetValue(actorNumber, out siplayer))
		{
			return siplayer;
		}
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(actorNumber);
		if (gamePlayer == null)
		{
			return null;
		}
		SIPlayer.siPlayerByActorNr.Add(actorNumber, gamePlayer.GetComponent<SIPlayer>());
		return SIPlayer.siPlayerByActorNr[actorNumber];
	}

	public static void ClearPlayerCache()
	{
		SIPlayer.siPlayerByActorNr.Clear();
	}

	public static SIPlayer Get(VRRig vrRig)
	{
		if (!vrRig)
		{
			return null;
		}
		return vrRig.GetComponent<SIPlayer>();
	}

	public void SerializeNetworkState(BinaryWriter writer, NetPlayer player)
	{
		for (int i = 0; i < 6; i++)
		{
			writer.Write(this.CurrentProgression.resourceArray[i]);
		}
		for (int j = 0; j < 2; j++)
		{
			writer.Write(this.CurrentProgression.limitedDepositTimeArray[j]);
		}
		for (int k = 0; k < SIPlayer.progressionSO.TreePageCount; k++)
		{
			for (int l = 0; l < SIPlayer.progressionSO.TreeNodeCounts[k]; l++)
			{
				writer.Write(this.CurrentProgression.techTreeData[k][l]);
			}
		}
		writer.Write((byte)this.CurrentProgression.stashedQuests);
		writer.Write((byte)this.CurrentProgression.stashedBonusPoints);
		writer.Write((byte)this.CurrentProgression.bonusProgress);
		for (int m = 0; m < this.CurrentProgression.currentQuestIds.Length; m++)
		{
			writer.Write(this.CurrentProgression.currentQuestIds[m]);
			writer.Write(this.CurrentProgression.currentQuestProgresses[m]);
		}
	}

	public static void DeserializeNetworkStateAndBurn(BinaryReader reader, SIPlayer player, SuperInfectionManager siManager)
	{
		if (player == null || player == SIPlayer.LocalPlayer)
		{
			for (int i = 0; i < 6; i++)
			{
				reader.ReadInt16();
			}
			for (int j = 0; j < SIPlayer.progressionSO.TreePageCount; j++)
			{
				for (int k = 0; k < SIPlayer.progressionSO.TreeNodeCounts[j]; k++)
				{
					reader.ReadBoolean();
				}
			}
			reader.ReadByte();
			reader.ReadByte();
			reader.ReadByte();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			return;
		}
		int[] array = new int[6];
		int[] array2 = new int[2];
		bool[][] array3 = new bool[SIPlayer.progressionSO.TreePageCount][];
		for (int l = 0; l < 6; l++)
		{
			array[l] = reader.ReadInt32();
		}
		for (int m = 0; m < 2; m++)
		{
			array2[m] = reader.ReadInt32();
		}
		for (int n = 0; n < SIPlayer.progressionSO.TreePageCount; n++)
		{
			array3[n] = new bool[SIPlayer.progressionSO.TreeNodeCounts[n]];
			for (int num = 0; num < SIPlayer.progressionSO.TreeNodeCounts[n]; num++)
			{
				array3[n][num] = reader.ReadBoolean();
			}
		}
		int num2 = (int)reader.ReadByte();
		int num3 = (int)reader.ReadByte();
		int num4 = (int)reader.ReadByte();
		int[] array4 = new int[3];
		int[] array5 = new int[3];
		for (int num5 = 0; num5 < 3; num5++)
		{
			array4[num5] = reader.ReadInt32();
			array5[num5] = reader.ReadInt32();
		}
		player.UpdateProgression(array, array2, array3, num2, num3, num4, array4, array5);
	}

	public bool HasLimitedResourceBeenDeposited(SIResource.LimitedDepositType limitedDepositType)
	{
		if (limitedDepositType == SIResource.LimitedDepositType.None)
		{
			return false;
		}
		if (this == SIPlayer.LocalPlayer)
		{
			return SIProgression.Instance.DailyLimitedTurnedIn;
		}
		return this.CurrentProgression.limitedDepositTimeArray[(int)limitedDepositType] > 0;
	}

	public bool CanLimitedResourceBeDeposited(SIResource.LimitedDepositType limitedDepositType)
	{
		return limitedDepositType == SIResource.LimitedDepositType.None || (SIProgression.Instance && SIProgression.Instance.IsLimitedDepositAvailable(limitedDepositType));
	}

	public void GatherResource(SIResource.ResourceType type, SIResource.LimitedDepositType limitedDepositType, int count)
	{
		SuperInfectionManager activeSuperInfectionManager = SuperInfectionManager.activeSuperInfectionManager;
		Dictionary<SIResource.ResourceType, int> resourceDict = SIProgression.Instance.resourceDict;
		resourceDict[type] += count;
		SIProgression.Instance.ApplyLimitedDepositTime(limitedDepositType);
		bool flag = type == SIResource.ResourceType.TechPoint || (limitedDepositType == SIResource.LimitedDepositType.MonkeIdol && SIProgression.Instance.limitedDepositTimeArray[(int)limitedDepositType] != 1) || SIProgression.Instance.TryDepositResources(type, count);
		if (type == SIResource.ResourceType.StrangeWood)
		{
			PlayerGameEvents.MiscEvent("SIStrangeWoodCollect", 1);
		}
		else if (type == SIResource.ResourceType.WeirdGear)
		{
			PlayerGameEvents.MiscEvent("SISWeirdGearCollect", 1);
		}
		else if (type == SIResource.ResourceType.FloppyMetal)
		{
			PlayerGameEvents.MiscEvent("SIFloppyMetalCollect", 1);
		}
		else if (type == SIResource.ResourceType.BouncySand)
		{
			PlayerGameEvents.MiscEvent("SIBouncySandCollect", 1);
		}
		else if (type == SIResource.ResourceType.VibratingSpring)
		{
			PlayerGameEvents.MiscEvent("SIVibratingSpringCollect", 1);
		}
		if (activeSuperInfectionManager != null && activeSuperInfectionManager.zoneSuperInfection != null && flag)
		{
			SIProgression.Instance.CollectResourceTelemetry(type, count);
		}
	}

	public void GetBonusProgress(SuperInfectionManager manager)
	{
		if (SIProgression.Instance.stashedBonusPoints <= 0)
		{
			return;
		}
		SIProgression.Instance.GetBonusProgress();
		this.BonusProgressCelebrate();
		SIPlayer.SetAndBroadcastProgression();
	}

	public int GetResourceAmount(SIResource.ResourceType type)
	{
		return this.CurrentProgression.resourceArray[(int)type];
	}

	public void SetProgressionLocal()
	{
		this.currentProgression = new SIPlayer.ProgressionData(SIProgression.Instance);
		this.gamePlayer.SetInitializePlayer(true);
		this.UpdateVisualsForAvailableQuestRedemption();
	}

	public void UpdateProgression(int[] resourceArray, int[] limitedDepositTimeArray, bool[][] techTreeData, int _stashedQuests, int _stashedBonusPoints, int _bonusProgress, int[] _currentQuestIds, int[] _currentQuestProgresses)
	{
		SIPlayer.ProgressionData progressionData = new SIPlayer.ProgressionData(resourceArray, limitedDepositTimeArray, techTreeData, _stashedQuests, _stashedBonusPoints, _bonusProgress, _currentQuestIds, _currentQuestProgresses);
		if (this.netInitialized)
		{
			this.CelebrateIfQuestProgressMade(progressionData);
		}
		else
		{
			this.netInitialized = true;
			this.currentProgression = progressionData;
			this.gamePlayer.SetInitializePlayer(true);
		}
		this.currentProgression = progressionData;
		this.UpdateVisualsForAvailableQuestRedemption();
	}

	public void CelebrateIfQuestProgressMade(SIPlayer.ProgressionData newProgression)
	{
		int num = this.QuestsAvailableToClaim();
		if (this.currentProgression.bonusProgress < newProgression.bonusProgress && this.currentProgression.stashedBonusPoints == newProgression.stashedBonusPoints && this.currentProgression.stashedBonusPoints > 0)
		{
			this.BonusProgressCelebrate();
		}
		bool flag = num > 0 && this.currentProgression.stashedQuests > newProgression.stashedQuests;
		bool flag2 = this.currentProgression.bonusProgress >= 10 && this.currentProgression.stashedBonusPoints > newProgression.stashedBonusPoints;
		bool flag3 = this.currentProgression.limitedDepositTimeArray[1] == 0 && newProgression.limitedDepositTimeArray[1] == 1;
		if ((flag || flag2 || flag3) && this.currentProgression.resourceArray[0] < newProgression.resourceArray[0])
		{
			this.TechPointGrantedCelebrate();
		}
		if (num > this.lastQuestsAvailableToClaim)
		{
			this.questCompleteCelebrate.SetActive(true);
		}
		this.lastQuestsAvailableToClaim = num;
	}

	public void TechPointGrantedCelebrate()
	{
		SIQuestBoard questBoard = SuperInfectionManager.activeSuperInfectionManager.zoneSuperInfection.questBoard;
		if (this != SIPlayer.LocalPlayer)
		{
			questBoard.GrantBonusPointProgress();
		}
		if (this.techPointGainedCelebrate != null)
		{
			this.techPointGainedCelebrate.SetActive(false);
			this.techPointGainedCelebrate.SetActive(true);
		}
		else
		{
			Debug.LogError("[SIPlayer]  ERROR!!!  Null reference: `techPointGainedCelebrate`.");
		}
		if (!questBoard.celebrateParticle)
		{
			Debug.LogError("[SIPlayer]  ERROR!!!  SuperInfectionManager.zoneSuperInfection.questBoard.celebrateParticle != null");
		}
		questBoard.celebrateParticle.Play();
	}

	public void BonusProgressCelebrate()
	{
		this.bonusProgressionCelebrate.SetActive(false);
		this.bonusProgressionCelebrate.SetActive(true);
	}

	public bool AttemptUnlockNode(SIUpgradeType upgrade, SuperInfectionManager manager)
	{
		if (this.CurrentProgression.techTreeData[upgrade.GetPageId()][upgrade.GetNodeId()])
		{
			return false;
		}
		SITechTreeNode treeNode = SIPlayer.progressionSO.GetTreeNode(upgrade);
		if (!this.PlayerCanAffordNode(treeNode))
		{
			return false;
		}
		this.PurchaseNode(treeNode);
		return true;
	}

	public bool PlayerCanAffordNode(SITechTreeNode node)
	{
		SIProgression.SINode sinode;
		if (SIProgression.Instance.GetOnlineNode(node.upgradeType, out sinode))
		{
			using (Dictionary<SIResource.ResourceType, int>.Enumerator enumerator = sinode.costs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<SIResource.ResourceType, int> keyValuePair = enumerator.Current;
					if (this.CurrentProgression.resourceArray[(int)keyValuePair.Key] < keyValuePair.Value)
					{
						return false;
					}
				}
				return true;
			}
		}
		foreach (SIResource.ResourceCost resourceCost in node.nodeCost)
		{
			if (this.CurrentProgression.resourceArray[(int)resourceCost.type] < resourceCost.amount)
			{
				return false;
			}
		}
		return true;
	}

	public void PurchaseNode(SITechTreeNode node)
	{
		SIProgression.SINode sinode;
		if (SIProgression.Instance.GetOnlineNode(node.upgradeType, out sinode))
		{
			using (Dictionary<SIResource.ResourceType, int>.Enumerator enumerator = sinode.costs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<SIResource.ResourceType, int> keyValuePair = enumerator.Current;
					SIProgression.Instance.GetResourceArray()[(int)keyValuePair.Key] -= keyValuePair.Value;
				}
				goto IL_00A8;
			}
		}
		foreach (SIResource.ResourceCost resourceCost in node.nodeCost)
		{
			SIProgression.Instance.GetResourceArray()[(int)resourceCost.type] -= resourceCost.amount;
		}
		IL_00A8:
		SIProgression.Instance.unlockedTechTreeData[node.upgradeType.GetPageId()][node.upgradeType.GetNodeId()] = true;
		SIPlayer.SetAndBroadcastProgression();
	}

	public bool NodeResearched(SIUpgradeType upgrade)
	{
		return this.CurrentProgression.techTreeData[upgrade.GetPageId()][upgrade.GetNodeId()];
	}

	public SIUpgradeSet GetUpgrades(SITechTreePageId pageId)
	{
		SIUpgradeSet siupgradeSet = default(SIUpgradeSet);
		bool[] array = this.CurrentProgression.techTreeData[(int)pageId];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i])
			{
				siupgradeSet.Add(i);
			}
		}
		return siupgradeSet;
	}

	public bool NodeParentsUnlocked(SIUpgradeType upgrade)
	{
		SITechTreeNode treeNode = SIPlayer.progressionSO.GetTreeNode(upgrade);
		for (int i = 0; i < treeNode.parentUpgrades.Length; i++)
		{
			if (!this.NodeResearched(treeNode.parentUpgrades[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void ResetTechTree()
	{
		SIProgression.Instance.unlockedTechTreeData = new bool[SIPlayer.progressionSO.TreePageCount][];
		for (int i = 0; i < SIPlayer.progressionSO.TreePageCount; i++)
		{
			SIProgression.Instance.unlockedTechTreeData[i] = new bool[SIPlayer.progressionSO.TreeNodeCounts[i]];
		}
		SIPlayer.SetAndBroadcastProgression();
	}

	public void ResetResources()
	{
		SIProgression.Instance.resourceDict = null;
		SIProgression.Instance.EnsureInitialized();
		SIProgression.Instance.limitedDepositTimeArray = new int[2];
		SIPlayer.SetAndBroadcastProgression();
	}

	public static void SetAndBroadcastProgression()
	{
		SIPlayer.LocalPlayer.SetAndBroadcastProgressionLocal();
	}

	public void SetAndBroadcastProgressionLocal()
	{
		SuperInfectionManager activeSuperInfectionManager = SuperInfectionManager.activeSuperInfectionManager;
		this.SetProgressionLocal();
		if (!NetworkSystem.Instance.InRoom || activeSuperInfectionManager == null)
		{
			return;
		}
		activeSuperInfectionManager.CallRPC(SuperInfectionManager.ClientToClientRPC.BroadcastProgression, new object[]
		{
			SIPlayer.LocalPlayer.CurrentProgression.resourceArray,
			SIPlayer.LocalPlayer.currentProgression.limitedDepositTimeArray,
			SIPlayer.LocalPlayer.currentProgression.techTreeData,
			SIPlayer.LocalPlayer.currentProgression.stashedQuests,
			SIPlayer.LocalPlayer.currentProgression.stashedBonusPoints,
			SIPlayer.LocalPlayer.currentProgression.bonusProgress,
			SIPlayer.LocalPlayer.currentProgression.currentQuestIds,
			SIPlayer.LocalPlayer.currentProgression.currentQuestProgresses
		});
		if (activeSuperInfectionManager.zoneSuperInfection != null)
		{
			activeSuperInfectionManager.zoneSuperInfection.RefreshStations(SIPlayer.LocalPlayer.ActorNr);
		}
	}

	public void UpdateVisualsForAvailableQuestRedemption()
	{
		bool flag = SuperInfectionManager.activeSuperInfectionManager != null && SuperInfectionManager.activeSuperInfectionManager.IsZoneReady() && (this.QuestsAvailableToClaim() > 0 || (this.currentProgression.bonusProgress >= 10 && this.currentProgression.stashedBonusPoints > 0));
		if (this.tpParticleSystem.isPlaying && !flag)
		{
			this.tpParticleSystem.Stop();
			return;
		}
		if (!this.tpParticleSystem.isPlaying && flag)
		{
			this.tpParticleSystem.Play();
		}
	}

	public int QuestsAvailableToClaim()
	{
		int num = 0;
		for (int i = 0; i < this.currentProgression.currentQuestIds.Length; i++)
		{
			if (SIProgression.Instance.questSourceList.GetQuestById(this.currentProgression.currentQuestIds[i]) != null && this.currentProgression.currentQuestProgresses[i] >= SIProgression.Instance.questSourceList.GetQuestById(this.currentProgression.currentQuestIds[i]).requiredOccurenceCount)
			{
				num++;
			}
		}
		return num;
	}

	public bool QuestAvailableToClaim(int questIndex)
	{
		return SIProgression.Instance.questSourceList.GetQuestById(this.currentProgression.currentQuestIds[questIndex]) != null && this.currentProgression.currentQuestProgresses[questIndex] >= SIProgression.Instance.questSourceList.GetQuestById(this.currentProgression.currentQuestIds[questIndex]).requiredOccurenceCount;
	}

	public void TriggerIdolDepositedCelebration(Vector3 position)
	{
		SuperInfectionManager activeSuperInfectionManager = SuperInfectionManager.activeSuperInfectionManager;
		if (activeSuperInfectionManager.gameEntityManager.IsAuthority())
		{
			activeSuperInfectionManager.CallRPC(SuperInfectionManager.AuthorityToClientRPC.TriggerMonkeIdolDepositCelebration, new object[] { position });
		}
		this.monkeIdolDepositCelebrate.transform.position = position;
		this.monkeIdolDepositCelebrate.SetActive(false);
		this.monkeIdolDepositCelebrate.SetActive(true);
	}

	public void ClearGadgetsOnLeaveZone()
	{
		if (!SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.IsAuthority())
		{
			return;
		}
		SuperInfectionManager.activeSuperInfectionManager.ClearPlayerGadgets(this);
	}

	private const string preLog = "[SIPlayer]  ";

	private const string preErr = "[SIPlayer]  ERROR!!!  ";

	public GamePlayer gamePlayer;

	private static Dictionary<int, SIPlayer> siPlayerByActorNr = new Dictionary<int, SIPlayer>();

	public CallLimiter clientToAuthorityRPCLimiter;

	public CallLimiter clientToClientRPCLimiter;

	public CallLimiter authorityToClientRPCLimiter;

	public static SITechTreeSO progressionSO;

	public SITechTreeSO progressionSORef;

	public ParticleSystem tpParticleSystem;

	public GameObject bonusProgressionCelebrate;

	[FormerlySerializedAs("testPointGainedCelebrate")]
	public GameObject techPointGainedCelebrate;

	public GameObject monkeIdolDepositCelebrate;

	public GameObject questCompleteCelebrate;

	private int lastQuestsAvailableToClaim;

	[NonSerialized]
	public int totalGadgetLimit = 3;

	public bool netInitialized;

	private SIPlayer.ProgressionData currentProgression;

	public List<int> activePlayerGadgets = new List<int>();

	[Serializable]
	public struct ProgressionData
	{
		public ProgressionData(bool itsNullLol)
		{
			this.resourceArray = new int[6];
			this.limitedDepositTimeArray = new int[2];
			this.techTreeData = new bool[SIPlayer.progressionSO.TreePageCount][];
			for (int i = 0; i < SIPlayer.progressionSO.TreePageCount; i++)
			{
				this.techTreeData[i] = new bool[SIPlayer.progressionSO.TreeNodeCounts[i]];
			}
			this.stashedQuests = 0;
			this.stashedBonusPoints = 0;
			this.bonusProgress = 0;
			this.currentQuestIds = new int[3];
			this.currentQuestProgresses = new int[3];
		}

		public ProgressionData(int[] _resourceArray, int[] _limitedDepositTimeArray, bool[][] _techTreeData, int _stashedQuests, int _stashedBonusPoints, int _bonusProgress, int[] _currentQuestIds, int[] _currentQuestProgresses)
		{
			this.resourceArray = _resourceArray;
			this.limitedDepositTimeArray = _limitedDepositTimeArray;
			this.techTreeData = _techTreeData;
			this.stashedQuests = _stashedQuests;
			this.stashedBonusPoints = _stashedBonusPoints;
			this.bonusProgress = _bonusProgress;
			this.currentQuestIds = _currentQuestIds;
			this.currentQuestProgresses = _currentQuestProgresses;
		}

		public ProgressionData(SIProgression siProgression)
		{
			this.resourceArray = siProgression.GetResourceArray();
			this.limitedDepositTimeArray = siProgression.limitedDepositTimeArray;
			this.techTreeData = siProgression.unlockedTechTreeData;
			this.stashedQuests = siProgression.stashedQuests;
			this.stashedBonusPoints = siProgression.stashedBonusPoints;
			this.bonusProgress = siProgression.bonusProgress;
			this.currentQuestIds = siProgression.ActiveQuestIds;
			this.currentQuestProgresses = siProgression.ActiveQuestProgresses;
		}

		public bool IsUnlocked(SIUpgradeType upgradeType)
		{
			return this.techTreeData[upgradeType.GetPageId()][upgradeType.GetNodeId()];
		}

		public int[] resourceArray;

		public int[] limitedDepositTimeArray;

		public bool[][] techTreeData;

		public int stashedQuests;

		public int stashedBonusPoints;

		public int bonusProgress;

		public int[] currentQuestIds;

		public int[] currentQuestProgresses;
	}
}
