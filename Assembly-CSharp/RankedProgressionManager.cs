using System;
using System.Collections;
using System.Collections.Generic;
using GorillaGameModes;
using GorillaNetworking;
using UnityEngine;

public class RankedProgressionManager : MonoBehaviour
{
	public int MaxRank { get; private set; }

	public float LowTierThreshold { get; set; }

	public float HighTierThreshold { get; set; }

	public List<RankedProgressionManager.RankedProgressionTier> MajorTiers
	{
		get
		{
			return this.majorTiers;
		}
		private set
		{
		}
	}

	private void DebugSetELO()
	{
	}

	[ContextMenu("Reset ELO")]
	private void DebugResetELO()
	{
	}

	private void Awake()
	{
		if (RankedProgressionManager.Instance)
		{
			GTDev.LogError<string>("Duplicate RankedProgressionManager detected. Destroying self.", base.gameObject, null);
			Object.Destroy(this);
			return;
		}
		RankedProgressionManager.Instance = this;
	}

	private void Start()
	{
		if (this.majorTiers.Count < 3)
		{
			GTDev.LogWarning<string>("At least 3 MMR tiers must be defined.", null);
			return;
		}
		GameMode.OnStartGameMode += this.OnJoinedRoom;
		RoomSystem.PlayerJoinedEvent += new Action<NetPlayer>(this.OnPlayerJoined);
		float num = 100f;
		int num2 = 0;
		for (int i = 0; i < this.majorTiers.Count; i++)
		{
			this.majorTiers[i].SetMinThreshold((i == 0) ? 100f : this.majorTiers[i - 1].thresholdMax);
			for (int j = 0; j < this.majorTiers[i].subTiers.Count; j++)
			{
				num2++;
				this.majorTiers[i].subTiers[j].SetMinThreshold(num);
				num = this.majorTiers[i].subTiers[j].thresholdMax;
			}
		}
		this.MaxRank = num2 - 1;
		this.LowTierThreshold = this.majorTiers[0].thresholdMax;
		List<RankedProgressionManager.RankedProgressionTier> list = this.majorTiers;
		this.HighTierThreshold = list[list.Count - 1].GetMinThreshold();
		this.EloScorePC = new RankedMultiplayerStatisticFloat(RankedProgressionManager.RANKED_ELO_PC_KEY, 100f, 100f, 4000f, RankedMultiplayerStatistic.SerializationType.PlayerPrefs);
		this.EloScoreQuest = new RankedMultiplayerStatisticFloat(RankedProgressionManager.RANKED_ELO_KEY, 100f, 100f, 4000f, RankedMultiplayerStatistic.SerializationType.PlayerPrefs);
		this.NewTierGracePeriodIdxPC = new RankedMultiplayerStatisticInt(RankedProgressionManager.RANKED_PROGRESSION_GRACE_PERIOD_KEY, 0, -1, int.MaxValue, RankedMultiplayerStatistic.SerializationType.PlayerPrefs);
		this.NewTierGracePeriodIdxQuest = new RankedMultiplayerStatisticInt(RankedProgressionManager.RANKED_PROGRESSION_GRACE_PERIOD_PC_KEY, 0, -1, int.MaxValue, RankedMultiplayerStatistic.SerializationType.PlayerPrefs);
	}

	private void OnDestroy()
	{
		GameMode.OnStartGameMode += this.OnJoinedRoom;
		RoomSystem.PlayerJoinedEvent -= new Action<NetPlayer>(this.OnPlayerJoined);
	}

	public void RequestUnlockCompetitiveQueue(bool unlock)
	{
		GorillaTagCompetitiveServerApi.Instance.RequestUnlockCompetitiveQueue(unlock, delegate
		{
			this.AcquireLocalPlayerRankInformation();
		});
	}

	public IEnumerator LoadStatsWhenReady()
	{
		yield return new WaitUntil(() => NetworkSystem.Instance.LocalPlayer.UserId != null);
		if (this.HasUnlockedCompetitiveQueue())
		{
			this.RequestUnlockCompetitiveQueue(true);
		}
		else
		{
			this.AcquireLocalPlayerRankInformation();
		}
		yield break;
	}

	private void OnJoinedRoom(GameModeType newGameModeType)
	{
		if (newGameModeType == GameModeType.InfectionCompetitive)
		{
			this.AcquireRoomRankInformation(false);
		}
	}

	private void OnPlayerJoined(NetPlayer player)
	{
		if (GorillaGameManager.instance != null && GorillaGameManager.instance.GameType() == GameModeType.InfectionCompetitive)
		{
			this.AcquireSinglePlayerRankInformation(player);
		}
	}

	private void AcquireLocalPlayerRankInformation()
	{
		List<string> list = new List<string>();
		list.Add(NetworkSystem.Instance.LocalPlayer.UserId);
		GorillaTagCompetitiveServerApi.Instance.RequestGetRankInformation(list, new Action<GorillaTagCompetitiveServerApi.RankedModeProgressionData>(this.OnLocalPlayerRankedInformationAcquired));
	}

	private void AcquireSinglePlayerRankInformation(NetPlayer player)
	{
		if (player == null)
		{
			return;
		}
		List<string> list = new List<string>();
		list.Add(player.UserId);
		GorillaTagCompetitiveServerApi.Instance.RequestGetRankInformation(list, new Action<GorillaTagCompetitiveServerApi.RankedModeProgressionData>(this.OnPlayersRankedInformationAcquired));
	}

	public void AcquireRoomRankInformation(bool includeLocalPlayer = true)
	{
		List<string> list = new List<string>();
		foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
		{
			if (includeLocalPlayer || !netPlayer.IsLocal)
			{
				list.Add(netPlayer.UserId);
			}
		}
		if (list.Count > 0)
		{
			GorillaTagCompetitiveServerApi.Instance.RequestGetRankInformation(list, new Action<GorillaTagCompetitiveServerApi.RankedModeProgressionData>(this.OnPlayersRankedInformationAcquired));
		}
	}

	private void OnPlayersRankedInformationAcquired(GorillaTagCompetitiveServerApi.RankedModeProgressionData rankedModeProgressionData)
	{
		foreach (GorillaTagCompetitiveServerApi.RankedModePlayerProgressionData rankedModePlayerProgressionData in rankedModeProgressionData.playerData)
		{
			if (rankedModePlayerProgressionData != null && rankedModePlayerProgressionData.platformData != null && rankedModePlayerProgressionData.platformData.Length >= 2)
			{
				int num = -1;
				foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
				{
					if (netPlayer.UserId == rankedModePlayerProgressionData.playfabID)
					{
						num = netPlayer.ActorNumber;
						break;
					}
				}
				if (num >= 0)
				{
					GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData rankedModeProgressionPlatformData = rankedModePlayerProgressionData.platformData[1];
					GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData rankedModeProgressionPlatformData2 = rankedModePlayerProgressionData.platformData[0];
					GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData rankedModeProgressionPlatformData3 = rankedModeProgressionPlatformData2;
					int rankFromTiers = RankedProgressionManager.Instance.GetRankFromTiers(rankedModeProgressionPlatformData3.majorTier, rankedModeProgressionPlatformData3.minorTier);
					Action<int, float, int> onPlayerEloAcquired = this.OnPlayerEloAcquired;
					if (onPlayerEloAcquired != null)
					{
						onPlayerEloAcquired(num, rankedModeProgressionPlatformData3.elo, rankFromTiers);
					}
					if (num == NetworkSystem.Instance.LocalPlayerID)
					{
						this.SetLocalProgressionData(rankedModePlayerProgressionData);
					}
					RigContainer rigContainer;
					if (VRRigCache.Instance.TryGetVrrig(num, out rigContainer))
					{
						VRRig rig = rigContainer.Rig;
						if (rig != null)
						{
							int rankFromTiers2 = this.GetRankFromTiers(rankedModeProgressionPlatformData.majorTier, rankedModeProgressionPlatformData.minorTier);
							int rankFromTiers3 = RankedProgressionManager.Instance.GetRankFromTiers(rankedModeProgressionPlatformData2.majorTier, rankedModeProgressionPlatformData2.minorTier);
							rig.SetRankedInfo(rankedModeProgressionPlatformData3.elo, rankFromTiers2, rankFromTiers3, false);
						}
					}
				}
			}
		}
	}

	private void OnLocalPlayerRankedInformationAcquired(GorillaTagCompetitiveServerApi.RankedModeProgressionData rankedModeProgressionData)
	{
		if (rankedModeProgressionData.playerData.Count > 0)
		{
			this.SetLocalProgressionData(rankedModeProgressionData.playerData[0]);
			float eloScore = this.GetEloScore();
			int progressionRankIndexQuest = this.GetProgressionRankIndexQuest();
			int progressionRankIndexPC = this.GetProgressionRankIndexPC();
			int num = progressionRankIndexPC;
			this.HandlePlayerRankedInfoReceived(NetworkSystem.Instance.LocalPlayer.ActorNumber, eloScore, num);
			VRRig.LocalRig.SetRankedInfo(eloScore, progressionRankIndexQuest, progressionRankIndexPC, true);
		}
	}

	public bool AreValuesValid(float elo, int questTier, int pcTier)
	{
		return elo >= 100f && elo <= 4000f && questTier >= 0 && questTier <= this.MaxRank && pcTier >= 0 && pcTier <= this.MaxRank;
	}

	public void HandlePlayerRankedInfoReceived(int actorNum, float elo, int tier)
	{
		Action<int, float, int> onPlayerEloAcquired = this.OnPlayerEloAcquired;
		if (onPlayerEloAcquired == null)
		{
			return;
		}
		onPlayerEloAcquired(actorNum, elo, tier);
	}

	public void SetLocalProgressionData(GorillaTagCompetitiveServerApi.RankedModePlayerProgressionData data)
	{
		this.ProgressionData = data;
	}

	public void LoadStats()
	{
		base.StartCoroutine(this.LoadStatsWhenReady());
	}

	public float GetEloScore()
	{
		return this.GetEloScorePC();
	}

	public void SetEloScore(float val)
	{
		GorillaTagCompetitiveServerApi.Instance.RequestSetEloValue(val, delegate
		{
			this.AcquireLocalPlayerRankInformation();
		});
	}

	public float GetEloScorePC()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return 100f;
		}
		return this.ProgressionData.platformData[0].elo;
	}

	public float GetEloScoreQuest()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return 100f;
		}
		return this.ProgressionData.platformData[1].elo;
	}

	private int GetNewTierGracePeriodIdx()
	{
		return this.NewTierGracePeriodIdxPC;
	}

	private void SetNewTierGracePeriodIdx(int val)
	{
		this.NewTierGracePeriodIdxPC.Set(val);
	}

	private void IncrementNewTierGracePeriodIdx()
	{
		this.NewTierGracePeriodIdxPC.Increment();
	}

	public bool TryGetProgressionSubTier(out RankedProgressionManager.RankedProgressionSubTier subTier, out int index)
	{
		subTier = null;
		index = -1;
		return this.TryGetProgressionSubTier(this.GetEloScore(), out subTier, out index);
	}

	public bool TryGetProgressionSubTier(float elo, out RankedProgressionManager.RankedProgressionSubTier subTier, out int index)
	{
		int num = 0;
		subTier = null;
		index = -1;
		for (int i = 0; i < this.majorTiers.Count; i++)
		{
			float num2 = ((i < this.majorTiers.Count - 1) ? this.majorTiers[i].thresholdMax : 4000.1f);
			if (elo < num2)
			{
				int j = 0;
				while (j < this.majorTiers[i].subTiers.Count)
				{
					float num3 = ((j < this.majorTiers[i].subTiers.Count - 1) ? this.majorTiers[i].subTiers[j].thresholdMax : num2);
					if (elo < num3)
					{
						subTier = this.majorTiers[i].subTiers[j];
						index = num;
						return true;
					}
					j++;
					num++;
				}
			}
			else
			{
				num += this.majorTiers[i].subTiers.Count;
			}
		}
		return false;
	}

	private RankedProgressionManager.RankedProgressionTier GetProgressionMajorTierBySubTierIndex(int idx)
	{
		int num = 0;
		for (int i = 0; i < this.majorTiers.Count; i++)
		{
			int j = 0;
			while (j < this.majorTiers[i].subTiers.Count)
			{
				if (num == idx)
				{
					return this.majorTiers[i];
				}
				j++;
				num++;
			}
		}
		return null;
	}

	private RankedProgressionManager.RankedProgressionSubTier GetProgressionSubTierByIndex(int idx)
	{
		int num = 0;
		for (int i = 0; i < this.majorTiers.Count; i++)
		{
			int j = 0;
			while (j < this.majorTiers[i].subTiers.Count)
			{
				if (num == idx)
				{
					return this.majorTiers[i].subTiers[j];
				}
				j++;
				num++;
			}
		}
		return null;
	}

	private RankedProgressionManager.RankedProgressionSubTier GetNextProgressionSubTierByIndex(int idx)
	{
		RankedProgressionManager.RankedProgressionSubTier progressionSubTierByIndex = this.GetProgressionSubTierByIndex(idx + 1);
		if (progressionSubTierByIndex != null)
		{
			return progressionSubTierByIndex;
		}
		return this.GetProgressionSubTierByIndex(idx);
	}

	private RankedProgressionManager.RankedProgressionSubTier GetPrevProgressionSubTierByIndex(int idx)
	{
		if (idx > 0)
		{
			RankedProgressionManager.RankedProgressionSubTier progressionSubTierByIndex = this.GetProgressionSubTierByIndex(idx - 1);
			if (progressionSubTierByIndex != null)
			{
				return progressionSubTierByIndex;
			}
		}
		return this.GetProgressionSubTierByIndex(idx);
	}

	public string GetProgressionRankName()
	{
		return this.GetProgressionRankName(this.GetEloScore());
	}

	public string GetProgressionRankName(float elo)
	{
		RankedProgressionManager.RankedProgressionSubTier rankedProgressionSubTier;
		int num;
		if (this.TryGetProgressionSubTier(elo, out rankedProgressionSubTier, out num))
		{
			return rankedProgressionSubTier.name;
		}
		return string.Empty;
	}

	public string GetNextProgressionRankName(int subTierIdx)
	{
		RankedProgressionManager.RankedProgressionSubTier nextProgressionSubTierByIndex = this.GetNextProgressionSubTierByIndex(subTierIdx);
		if (nextProgressionSubTierByIndex != null)
		{
			return nextProgressionSubTierByIndex.name;
		}
		return null;
	}

	public string GetPrevProgressionRankName(int subTierIdx)
	{
		RankedProgressionManager.RankedProgressionSubTier prevProgressionSubTierByIndex = this.GetPrevProgressionSubTierByIndex(subTierIdx);
		if (prevProgressionSubTierByIndex != null)
		{
			return prevProgressionSubTierByIndex.name;
		}
		return null;
	}

	public int GetProgressionRankIndex()
	{
		return this.GetProgressionRankIndexPC();
	}

	public RankedProgressionManager.RankedProgressionSubTier GetProgressionSubTier()
	{
		return this.GetProgressionSubTierByIndex(this.GetProgressionRankIndex());
	}

	public int GetProgressionRankIndexQuest()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return 0;
		}
		GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData rankedModeProgressionPlatformData = this.ProgressionData.platformData[1];
		return this.GetRankFromTiers(rankedModeProgressionPlatformData.majorTier, rankedModeProgressionPlatformData.minorTier);
	}

	public int GetProgressionRankIndexPC()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return 0;
		}
		GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData rankedModeProgressionPlatformData = this.ProgressionData.platformData[0];
		return this.GetRankFromTiers(rankedModeProgressionPlatformData.majorTier, rankedModeProgressionPlatformData.minorTier);
	}

	public int GetRankFromTiers(int majorTier, int minorTier)
	{
		int num = 0;
		for (int i = 0; i < this.majorTiers.Count; i++)
		{
			for (int j = 0; j < this.majorTiers[i].subTiers.Count; j++)
			{
				if (i == majorTier && j == minorTier)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public int GetProgressionRankIndex(float elo)
	{
		RankedProgressionManager.RankedProgressionSubTier rankedProgressionSubTier;
		int num;
		if (this.TryGetProgressionSubTier(elo, out rankedProgressionSubTier, out num))
		{
			return num;
		}
		return -1;
	}

	public float GetProgressionRankProgress()
	{
		return this.GetProgressionRankProgressPC();
	}

	public float GetProgressionRankProgressQuest()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return 0f;
		}
		return this.ProgressionData.platformData[1].rankProgress;
	}

	public float GetProgressionRankProgressPC()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return 0f;
		}
		return this.ProgressionData.platformData[0].rankProgress;
	}

	public int ClampProgressionRankIndex(int subTierIdx)
	{
		if (subTierIdx < 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < this.majorTiers.Count; i++)
		{
			int j = 0;
			while (j < this.majorTiers[i].subTiers.Count)
			{
				if (num == subTierIdx)
				{
					return subTierIdx;
				}
				j++;
				num++;
			}
		}
		return num - 1;
	}

	public Sprite GetProgressionRankIcon()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return null;
		}
		int num = ((this.ProgressionData == null) ? 0 : this.ProgressionData.platformData[0].minorTier);
		int num2 = ((this.ProgressionData == null) ? 0 : this.ProgressionData.platformData[0].majorTier);
		RankedProgressionManager.RankedProgressionSubTier rankedProgressionSubTier = this.majorTiers[num2].subTiers[num];
		if (rankedProgressionSubTier == null)
		{
			return null;
		}
		return rankedProgressionSubTier.icon;
	}

	public string GetRankedProgressionTierName()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return "None";
		}
		int minorTier = this.ProgressionData.platformData[0].minorTier;
		int majorTier = this.ProgressionData.platformData[0].majorTier;
		RankedProgressionManager.RankedProgressionSubTier rankedProgressionSubTier = this.majorTiers[majorTier].subTiers[minorTier];
		if (rankedProgressionSubTier != null)
		{
			return rankedProgressionSubTier.name;
		}
		return "None";
	}

	public Sprite GetProgressionRankIcon(float elo)
	{
		RankedProgressionManager.RankedProgressionSubTier rankedProgressionSubTier;
		int num;
		if (this.TryGetProgressionSubTier(elo, out rankedProgressionSubTier, out num))
		{
			return rankedProgressionSubTier.icon;
		}
		return null;
	}

	public Sprite GetProgressionRankIcon(int subTierIdx)
	{
		RankedProgressionManager.RankedProgressionSubTier progressionSubTierByIndex = this.GetProgressionSubTierByIndex(subTierIdx);
		if (progressionSubTierByIndex != null)
		{
			return progressionSubTierByIndex.icon;
		}
		return null;
	}

	public Sprite GetNextProgressionRankIcon(int subTierIdx)
	{
		RankedProgressionManager.RankedProgressionSubTier nextProgressionSubTierByIndex = this.GetNextProgressionSubTierByIndex(subTierIdx);
		if (nextProgressionSubTierByIndex != null)
		{
			return nextProgressionSubTierByIndex.icon;
		}
		return null;
	}

	public Sprite GetPrevProgressionRankIcon(int subTierIdx)
	{
		RankedProgressionManager.RankedProgressionSubTier prevProgressionSubTierByIndex = this.GetPrevProgressionSubTierByIndex(subTierIdx);
		if (prevProgressionSubTierByIndex != null)
		{
			return prevProgressionSubTierByIndex.icon;
		}
		return null;
	}

	public float GetCurrentELO()
	{
		return this.GetEloScore();
	}

	public void GetSubtierRankThresholds(int subTierIdx, out float minThreshold, out float maxThreshold)
	{
		minThreshold = 0f;
		maxThreshold = 1f;
		RankedProgressionManager.RankedProgressionSubTier progressionSubTierByIndex = this.GetProgressionSubTierByIndex(subTierIdx);
		if (progressionSubTierByIndex != null)
		{
			maxThreshold = progressionSubTierByIndex.thresholdMax;
			if (maxThreshold <= 0f)
			{
				RankedProgressionManager.RankedProgressionTier progressionMajorTierBySubTierIndex = this.GetProgressionMajorTierBySubTierIndex(subTierIdx);
				if (progressionMajorTierBySubTierIndex != null)
				{
					maxThreshold = progressionMajorTierBySubTierIndex.thresholdMax;
					if (maxThreshold <= 0f)
					{
						maxThreshold = 4000f;
					}
				}
			}
			minThreshold = progressionSubTierByIndex.GetMinThreshold();
			if (minThreshold <= 0f)
			{
				RankedProgressionManager.RankedProgressionTier progressionMajorTierBySubTierIndex2 = this.GetProgressionMajorTierBySubTierIndex(subTierIdx);
				if (progressionMajorTierBySubTierIndex2 != null)
				{
					minThreshold = progressionMajorTierBySubTierIndex2.GetMinThreshold();
					if (minThreshold <= 0f)
					{
						minThreshold = 100f;
					}
				}
			}
		}
	}

	public static float GetEloWinProbability(float ratingPlayer1, float ratingPlayer2)
	{
		return 1f / (1f + Mathf.Pow(10f, (ratingPlayer1 - ratingPlayer2) / 400f));
	}

	public static float UpdateEloScore(float eloScore, float expectedResult, float actualResult, float k)
	{
		return Mathf.Clamp(eloScore + k * (actualResult - expectedResult), 100f, 4000f);
	}

	public RankedProgressionManager.ERankedMatchmakingTier GetRankedMatchmakingTier()
	{
		if (this.ProgressionData == null || this.ProgressionData.platformData == null || this.ProgressionData.platformData.Length < 2)
		{
			return RankedProgressionManager.ERankedMatchmakingTier.Low;
		}
		return (RankedProgressionManager.ERankedMatchmakingTier)this.ProgressionData.platformData[0].majorTier;
	}

	public float CompetitiveQueueEloFloor
	{
		get
		{
			return this.LowTierThreshold;
		}
	}

	private bool HasUnlockedCompetitiveQueue()
	{
		return GorillaComputer.instance.allowedInCompetitive;
	}

	public static RankedProgressionManager Instance;

	public const float DEFAULT_ELO = 100f;

	public const float MIN_ELO = 100f;

	public const float MAX_ELO = 4000f;

	public const float MAJOR_TIER_MIN_RANGE = 200f;

	public const float SUB_TIER_MIN_RANGE = 20f;

	public static string RANKED_ELO_KEY = "RankedElo";

	public static string RANKED_PROGRESSION_GRACE_PERIOD_KEY = "RankedProgGracePeriod";

	public static string RANKED_ELO_PC_KEY = "RankedEloPC";

	public static string RANKED_PROGRESSION_GRACE_PERIOD_PC_KEY = "RankedProgGracePeriodPC";

	private RankedMultiplayerStatisticFloat EloScorePC;

	private RankedMultiplayerStatisticFloat EloScoreQuest;

	private RankedMultiplayerStatisticInt NewTierGracePeriodIdxPC;

	private RankedMultiplayerStatisticInt NewTierGracePeriodIdxQuest;

	private GorillaTagCompetitiveServerApi.RankedModePlayerProgressionData ProgressionData;

	[SerializeField]
	private List<RankedProgressionManager.RankedProgressionTier> majorTiers = new List<RankedProgressionManager.RankedProgressionTier>();

	[SerializeField]
	private int newTierGracePeriod = 3;

	public float MaxEloConstant = 90f;

	private RankedProgressionManager.RankedProgressionEvent ProgressionEvent;

	public Action<int, float, int> OnPlayerEloAcquired;

	[Space]
	[ContextMenuItem("Set ELO", "DebugSetELO")]
	public int debugEloPoints = 100;

	public enum ERankedMatchmakingTier
	{
		Low,
		Medium,
		High
	}

	public enum ERankedProgressionEventType
	{
		None,
		Progress,
		Promotion,
		Relegation
	}

	public class RankedProgressionEvent
	{
		public override string ToString()
		{
			string text = "Progression Info\n";
			text += string.Format("Event Type: {0}\n", this.evtType.ToString());
			text += string.Format("Left Tier: {0}\n", this.leftName);
			text += string.Format("Right Tier: {0}\n", this.rightName);
			text += string.Format("Left Value: {0}\n", this.minVal.ToString("N0"));
			text += string.Format("Right Value: {0}\n", this.maxVal.ToString("N0"));
			text += string.Format("Elo Delta: {0}\n", this.delta.ToString("N0"));
			if (this.evtType == RankedProgressionManager.ERankedProgressionEventType.Promotion || this.evtType == RankedProgressionManager.ERankedProgressionEventType.Relegation)
			{
				text += string.Format("Fanfare Tier: {0}\n", this.newTierName);
			}
			return text;
		}

		public RankedProgressionManager.ERankedProgressionEventType evtType;

		public Sprite progressIconLeft;

		public Sprite progressIconRight;

		public Sprite newTierIcon;

		public string leftName;

		public string rightName;

		public string newTierName;

		public float minVal;

		public float maxVal;

		public float delta;
	}

	public abstract class RankedProgressionTierBase
	{
		public void SetMinThreshold(float val)
		{
			this.thresholdMin = val;
		}

		public float GetMinThreshold()
		{
			if (this.thresholdMin < 0f)
			{
				GTDev.LogError<string>("Tier min threshold not initialized. Can only be used at runtime.", null);
			}
			return this.thresholdMin;
		}

		public string name;

		public Color color = Color.white;

		public float thresholdMax;

		private float thresholdMin = -1f;
	}

	[Serializable]
	public class RankedProgressionSubTier : RankedProgressionManager.RankedProgressionTierBase
	{
		public Sprite icon;
	}

	[Serializable]
	public class RankedProgressionTier : RankedProgressionManager.RankedProgressionTierBase
	{
		public void InsertSubTierAt(int idx, float tierMin)
		{
			RankedProgressionManager.RankedProgressionSubTier rankedProgressionSubTier = new RankedProgressionManager.RankedProgressionSubTier
			{
				name = "NewTier"
			};
			this.subTiers.Insert(idx, rankedProgressionSubTier);
			this.EnforceSubTierValidity(tierMin);
		}

		public void EnforceSubTierValidity(float thresholdMin)
		{
			float num = (((this.thresholdMax == 0f) ? 4000f : this.thresholdMax) - thresholdMin) / (float)this.subTiers.Count;
			for (int i = 0; i < this.subTiers.Count - 1; i++)
			{
				float num2 = thresholdMin + (float)(i + 1) * num;
				num2 = Mathf.Round(num2 / 10f);
				this.subTiers[i].thresholdMax = num2 * 10f;
			}
		}

		public List<RankedProgressionManager.RankedProgressionSubTier> subTiers = new List<RankedProgressionManager.RankedProgressionSubTier>();
	}
}
