using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using GorillaNetworking;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ProgressionManager : MonoBehaviour
{
	public static ProgressionManager Instance { get; private set; }

	public event Action OnTreeUpdated;

	public event Action OnInventoryUpdated;

	public event Action<string, int> OnTrackRead;

	public event Action<string, int> OnTrackSet;

	public event Action<string, string> OnNodeUnlocked;

	public event Action<string, int> OnGetShiftCredit;

	public event Action<string, int, int> OnGetShiftCreditCapData;

	public event Action<bool> OnPurchaseShiftCreditCapIncrease;

	public event Action<bool> OnPurchaseShiftCredit;

	public event Action<bool> OnChaosDepositSuccess;

	public event Action<ProgressionManager.JuicerStatusResponse> OnJucierStatusUpdated;

	public event Action<bool> OnPurchaseOverdrive;

	public event Action<ProgressionManager.DockWristStatusResponse> OnDockWristStatusUpdated;

	public event Action<ProgressionManager.GhostReactorStatsResponse> OnGhostReactorStatsUpdated;

	public event Action<ProgressionManager.GhostReactorInventoryResponse> OnGhostReactorInventoryUpdated;

	private void Awake()
	{
		if (ProgressionManager.Instance == null)
		{
			ProgressionManager.Instance = this;
		}
	}

	public async void RefreshProgressionTree()
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		MothershipClientApiUnity.GetPlayerProgressionTreesData(new Action<GetProgressionTreesForPlayerResponse>(this.OnGetTrees), new Action<MothershipError, int>(ProgressionManager.GetMothershipFailure));
	}

	public async void RefreshUserInventory()
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		MothershipClientApiUnity.GetUserInventory(new Action<MothershipGetInventoryResponse>(this.OnGetInventory), new Action<MothershipError, int>(ProgressionManager.GetMothershipFailure));
		this.RefreshShinyRocksTotal();
	}

	public UserHydratedProgressionTreeResponse GetTree(string treeName)
	{
		UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse;
		this._trees.TryGetValue(treeName, out userHydratedProgressionTreeResponse);
		return userHydratedProgressionTreeResponse;
	}

	public bool GetInventoryItem(string inventoryKey, out ProgressionManager.MothershipItemSummary item)
	{
		return this._inventory.TryGetValue((inventoryKey != null) ? inventoryKey.Trim() : null, out item);
	}

	public int GetNodeCost(string treeName, string nodeId, string currencyKey)
	{
		UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse;
		if (!this._trees.TryGetValue(treeName, out userHydratedProgressionTreeResponse) || userHydratedProgressionTreeResponse == null || string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(currencyKey))
		{
			return 0;
		}
		foreach (UserHydratedNodeDefinition userHydratedNodeDefinition in userHydratedProgressionTreeResponse.Nodes)
		{
			if (userHydratedNodeDefinition.id == nodeId && userHydratedNodeDefinition.cost != null && userHydratedNodeDefinition.cost.items != null)
			{
				using (HydratedInventoryChangeMap.HydratedInventoryChangeMapEnumerator enumerator2 = userHydratedNodeDefinition.cost.items.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, MothershipHydratedInventoryChange> keyValuePair = enumerator2.Current;
						string key = keyValuePair.Key;
						if (string.Equals((key != null) ? key.Trim() : null, currencyKey.Trim(), StringComparison.Ordinal))
						{
							return keyValuePair.Value.Delta;
						}
					}
					break;
				}
			}
		}
		return 0;
	}

	public async void GetProgression(string trackId)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoGetProgression(new ProgressionManager.GetProgressionRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			TrackId = trackId
		}));
	}

	public async void SetProgression(string trackId, int progress)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoSetProgression(new ProgressionManager.SetProgressionRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			TrackId = trackId,
			Progress = progress
		}));
	}

	public async void UnlockNode(string treeId, string nodeId)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoUnlockNode(new ProgressionManager.UnlockNodeRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			TreeId = treeId,
			NodeId = nodeId
		}));
	}

	public async void IncrementSIResource(string resourceName, Action<string> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoIncrementSIResource(new ProgressionManager.IncrementSIResourceRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			ResourceType = resourceName
		}, OnSuccess, OnFailure));
	}

	public async void CompleteSIQuest(int questID, Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoQuestCompleteReward(new ProgressionManager.SetSIQuestCompleteRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			QuestID = questID
		}, OnSuccess, OnFailure));
	}

	public async void CompleteSIBonus(Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoBonusCompleteReward(new ProgressionManager.SetSIBonusCompleteRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId
		}, OnSuccess, OnFailure));
	}

	public async void CollectSIIdol(Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoIdolCollectReward(new ProgressionManager.SetSIIdolCollectRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId
		}, OnSuccess, OnFailure));
	}

	public async void GetActiveSIQuests(Action<List<RotatingQuest>> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoGetActiveSIQuests(new ProgressionManager.GetActiveSIQuestsRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId
		}, OnSuccess, OnFailure));
	}

	public async void GetSIQuestStatus(Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoGetSIQuestsStatus(new ProgressionManager.GetSIQuestsStatusRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId
		}, OnSuccess, OnFailure));
	}

	public async void PurchaseTechPoints(int amount, Action OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoPurchaseTechPoints(new ProgressionManager.PurchaseTechPointsRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			TechPointsAmount = amount
		}, OnSuccess, OnFailure));
	}

	public async void PurchaseResources(Action<ProgressionManager.UserInventory> OnSuccess = null, Action<string> OnFailure = null)
	{
		await ProgressionUtil.WaitForMothershipSessionToken();
		base.StartCoroutine(this.DoPurchaseResources(new ProgressionManager.PurchaseResourcesRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipToken = MothershipClientContext.Token,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId
		}, OnSuccess, OnFailure));
	}

	public void PurchaseShiftCreditCapIncrease()
	{
		this.PurchaseShiftCreditCapIncreaseInternal(false);
	}

	private void PurchaseShiftCreditCapIncreaseInternal(bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoPurchaseShiftCreditCapIncrease(new ProgressionManager.PurchaseShiftCreditCapIncreaseRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void PurchaseShiftCredit()
	{
		this.PurchaseShiftCreditInternal(false);
	}

	private void PurchaseShiftCreditInternal(bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoPurchaseShiftCredit(new ProgressionManager.PurchaseShiftCreditRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void GetShiftCredit(string mothershipId)
	{
		base.StartCoroutine(this.DoGetShiftCredit(new ProgressionManager.GetShiftCreditRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			TargetMothershipId = mothershipId
		}));
	}

	public void GetJuicerStatus()
	{
		this.GetJuicerStatusInternal(false);
	}

	private void GetJuicerStatusInternal(bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoGetJuicerStatus(new ProgressionManager.GetJuicerStatusRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void DepositCore(ProgressionManager.CoreType coreType)
	{
		this.DepositCoreInternal(coreType, false);
	}

	private void DepositCoreInternal(ProgressionManager.CoreType coreType, bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoDepositCore(new ProgressionManager.DepositCoreRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			CoreBeingDeposited = coreType,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void PurchaseOverdrive()
	{
		this.PurchaseOverdriveInternal(false);
	}

	private void PurchaseOverdriveInternal(bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoPurchaseOverdrive(new ProgressionManager.PurchaseOverdriveRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void SubtractShiftCredit(int creditsToSubtract)
	{
		this.SubtractShiftCreditInternal(creditsToSubtract, false);
	}

	private void SubtractShiftCreditInternal(int creditsToSubtract, bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoSubtractShiftCredit(new ProgressionManager.SubtractShiftCreditRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			ShiftCreditToRemove = creditsToSubtract,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void AdvanceDockWristUpgradeLevel(ProgressionManager.WristDockUpgradeType upgrade)
	{
		this.AdvanceDockWristUpgradeLevelInternal(upgrade, false);
	}

	private void AdvanceDockWristUpgradeLevelInternal(ProgressionManager.WristDockUpgradeType upgrade, bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoAdvanceDockWristUpgradeLevel(new ProgressionManager.AdvanceDockWristUpgradeRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			Upgrade = upgrade,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void GetDockWristUpgradeStatus()
	{
		base.StartCoroutine(this.DoGetDockWristUpgradeStatus(new ProgressionManager.DockWristUpgradeStatusRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token
		}));
	}

	public void PurchaseDrillUpgrade(ProgressionManager.DrillUpgradeLevel upgrade)
	{
		base.StartCoroutine(this.DoPurchaseDrillUpgrade(new ProgressionManager.PurchaseDrillUpgradeRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			Upgrade = upgrade
		}));
	}

	public void RecycleTool(GRTool.GRToolType toolBeingRecycled, int numberOfPlayers)
	{
		base.StartCoroutine(this.DoRecycleTool(new ProgressionManager.RecycleToolRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			ToolBeingRecycled = toolBeingRecycled,
			NumberOfPlayers = numberOfPlayers
		}));
	}

	public void StartOfShift(string shiftId, int coresRequired, int numberOfPlayers, int depth)
	{
		base.StartCoroutine(this.DoStartOfShift(new ProgressionManager.StartOfShiftRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			ShiftId = shiftId,
			CoresRequired = coresRequired,
			NumberOfPlayers = numberOfPlayers,
			Depth = depth
		}));
	}

	public void EndOfShiftReward(string shiftId)
	{
		this.EndOfShiftRewardInternal(shiftId, false);
	}

	private void EndOfShiftRewardInternal(string shiftId, bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoEndOfShiftReward(new ProgressionManager.EndOfShiftRewardRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			ShiftId = shiftId,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	public void GetGhostReactorStats()
	{
		base.StartCoroutine(this.DoGetGhostReactorStats(new ProgressionManager.GhostReactorStatsRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token
		}));
	}

	public void GetGhostReactorInventory()
	{
		base.StartCoroutine(this.DoGetGhostReactorInventory(new ProgressionManager.GhostReactorInventoryRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token
		}));
	}

	public void SetGhostReactorInventory(string jsonInventory)
	{
		this.SetGhostReactorInventoryInternal(jsonInventory, false);
	}

	private void SetGhostReactorInventoryInternal(string jsonInventory, bool skipUserDataCache = false)
	{
		base.StartCoroutine(this.DoSetGhostReactorInventory(new ProgressionManager.SetGhostReactorInventoryRequest
		{
			MothershipId = MothershipClientContext.MothershipId,
			MothershipTitleId = MothershipClientApiUnity.TitleId,
			MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			MothershipDeploymentId = MothershipClientApiUnity.DeploymentId,
			MothershipToken = MothershipClientContext.Token,
			InventoryJson = jsonInventory,
			SkipUserDataCache = skipUserDataCache
		}));
	}

	private IEnumerator HandleWebRequestRetries<T>(ProgressionManager.RequestType requestType, T data, Action<T> actionToTake, Action failureActionToTake = null)
	{
		if (!this.retryCounters.ContainsKey(requestType))
		{
			this.retryCounters[requestType] = 0;
		}
		if (this.retryCounters[requestType] < this.maxRetriesOnFail)
		{
			float num = Random.Range(0.5f, Mathf.Pow(2f, (float)(this.retryCounters[requestType] + 1)));
			Debug.LogWarning(string.Format("PM: Retrying ... attempt #{0}, waiting {1}s", this.retryCounters[requestType] + 1, num));
			Dictionary<ProgressionManager.RequestType, int> dictionary = this.retryCounters;
			int num2 = dictionary[requestType];
			dictionary[requestType] = num2 + 1;
			yield return new WaitForSeconds(num);
			actionToTake(data);
		}
		else
		{
			Debug.LogError("PM: Maximum retries attempted.");
			this.retryCounters[requestType] = 0;
			if (failureActionToTake != null)
			{
				failureActionToTake();
			}
		}
		yield break;
	}

	private bool HandleWebRequestFailures(UnityWebRequest request, bool retryOnConflict = false)
	{
		bool flag = false;
		Debug.LogError(string.Format("PM: HandleWebRequestFailures Error: {0} -- raw response: ", request.responseCode) + request.downloadHandler.text);
		if (request.result != UnityWebRequest.Result.ProtocolError)
		{
			flag = true;
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode >= 500L)
			{
				if (responseCode >= 600L)
				{
					goto IL_006A;
				}
			}
			else if (responseCode != 408L && responseCode != 429L)
			{
				goto IL_006A;
			}
			bool flag2 = true;
			goto IL_006C;
			IL_006A:
			flag2 = false;
			IL_006C:
			if (flag2 || (retryOnConflict && request.responseCode == 409L))
			{
				flag = true;
				Debug.LogError(string.Format("PM: HTTP {0} error: {1}", request.responseCode, request.error));
			}
		}
		return flag;
	}

	private IEnumerator DoGetProgression(ProgressionManager.GetProgressionRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GetProgressionRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.GetProgression);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			int num = int.Parse(request.downloadHandler.text);
			this._tracks[data.TrackId] = num;
			Debug.Log("PM: GetProgression Success: track is " + data.TrackId + " and progress is " + num.ToString());
			this.retryCounters[ProgressionManager.RequestType.GetProgression] = 0;
			Action<string, int> onTrackRead = this.OnTrackRead;
			if (onTrackRead != null)
			{
				onTrackRead(data.TrackId, num);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<string>(ProgressionManager.RequestType.GetProgression, data.TrackId, delegate(string x)
		{
			this.GetProgression(x);
		}, null);
		yield break;
	}

	private IEnumerator DoSetProgression(ProgressionManager.SetProgressionRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.SetProgressionRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.SetProgression);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.GetProgressionResponse getProgressionResponse = JsonConvert.DeserializeObject<ProgressionManager.GetProgressionResponse>(request.downloadHandler.text);
			this._tracks[data.TrackId] = getProgressionResponse.Progress;
			this.retryCounters[ProgressionManager.RequestType.SetProgression] = 0;
			Action<string, int> onTrackSet = this.OnTrackSet;
			if (onTrackSet != null)
			{
				onTrackSet(data.TrackId, getProgressionResponse.Progress);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ValueTuple<string, int>>(ProgressionManager.RequestType.SetProgression, new ValueTuple<string, int>(data.TrackId, data.Progress), delegate([TupleElementNames(new string[] { "TrackId", "Progress" })] ValueTuple<string, int> x)
		{
			this.SetProgression(x.Item1, x.Item2);
		}, null);
		yield break;
	}

	private IEnumerator DoUnlockNode(ProgressionManager.UnlockNodeRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.UnlockNodeRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.UnlockProgressionTreeNode);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.UnlockProgressionTreeNode] = 0;
			this.RefreshProgressionTree();
			this.RefreshUserInventory();
			Action<string, string> onNodeUnlocked = this.OnNodeUnlocked;
			if (onNodeUnlocked != null)
			{
				onNodeUnlocked(data.TreeId, data.NodeId);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ValueTuple<string, string>>(ProgressionManager.RequestType.UnlockProgressionTreeNode, new ValueTuple<string, string>(data.TreeId, data.NodeId), delegate([TupleElementNames(new string[] { "TreeId", "NodeId" })] ValueTuple<string, string> x)
		{
			this.UnlockNode(x.Item1, x.Item2);
		}, null);
		yield break;
	}

	private IEnumerator DoIncrementSIResource(ProgressionManager.IncrementSIResourceRequest data, Action<string> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.IncrementSIResourceRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.IncrementSIResource);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.IncrementSIResourceResponse incrementSIResourceResponse = JsonConvert.DeserializeObject<ProgressionManager.IncrementSIResourceResponse>(request.downloadHandler.text);
			Action<string> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(incrementSIResourceResponse.ResourceType);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.IncrementSIResourceRequest>(ProgressionManager.RequestType.IncrementSIResource, data, delegate(ProgressionManager.IncrementSIResourceRequest x)
		{
			this.IncrementSIResource(data.ResourceType, OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoQuestCompleteReward(ProgressionManager.SetSIQuestCompleteRequest data, Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.SetSIQuestCompleteRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.CompleteSIQuest);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.GetSIQuestsStatusResponse getSIQuestsStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.GetSIQuestsStatusResponse>(request.downloadHandler.text);
			Action<ProgressionManager.UserQuestsStatusResponse> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(getSIQuestsStatusResponse.Result);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.SetSIQuestCompleteRequest>(ProgressionManager.RequestType.CompleteSIQuest, data, delegate(ProgressionManager.SetSIQuestCompleteRequest x)
		{
			this.CompleteSIQuest(data.QuestID, OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoBonusCompleteReward(ProgressionManager.SetSIBonusCompleteRequest data, Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.SetSIBonusCompleteRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.CompleteSIBonus);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.GetSIQuestsStatusResponse getSIQuestsStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.GetSIQuestsStatusResponse>(request.downloadHandler.text);
			Action<ProgressionManager.UserQuestsStatusResponse> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(getSIQuestsStatusResponse.Result);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.SetSIBonusCompleteRequest>(ProgressionManager.RequestType.CompleteSIBonus, data, delegate(ProgressionManager.SetSIBonusCompleteRequest x)
		{
			this.CompleteSIBonus(OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoIdolCollectReward(ProgressionManager.SetSIIdolCollectRequest data, Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.SetSIIdolCollectRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.CollectSIIdol);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.GetSIQuestsStatusResponse getSIQuestsStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.GetSIQuestsStatusResponse>(request.downloadHandler.text);
			Action<ProgressionManager.UserQuestsStatusResponse> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(getSIQuestsStatusResponse.Result);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.SetSIIdolCollectRequest>(ProgressionManager.RequestType.CollectSIIdol, data, delegate(ProgressionManager.SetSIIdolCollectRequest x)
		{
			this.CollectSIIdol(OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoGetActiveSIQuests(ProgressionManager.GetActiveSIQuestsRequest data, Action<List<RotatingQuest>> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GetActiveSIQuestsRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.GetActiveSIQuests);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.GetActiveSIQuestsResponse getActiveSIQuestsResponse = JsonConvert.DeserializeObject<ProgressionManager.GetActiveSIQuestsResponse>(request.downloadHandler.text);
			Action<List<RotatingQuest>> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(getActiveSIQuestsResponse.Result.Quests);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.GetActiveSIQuestsRequest>(ProgressionManager.RequestType.GetActiveSIQuests, data, delegate(ProgressionManager.GetActiveSIQuestsRequest x)
		{
			this.GetActiveSIQuests(OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoGetSIQuestsStatus(ProgressionManager.GetSIQuestsStatusRequest data, Action<ProgressionManager.UserQuestsStatusResponse> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GetSIQuestsStatusRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.GetSIQuestsStatus);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.GetSIQuestsStatusResponse getSIQuestsStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.GetSIQuestsStatusResponse>(request.downloadHandler.text);
			Action<ProgressionManager.UserQuestsStatusResponse> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(getSIQuestsStatusResponse.Result);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.GetSIQuestsStatusRequest>(ProgressionManager.RequestType.GetSIQuestsStatus, data, delegate(ProgressionManager.GetSIQuestsStatusRequest x)
		{
			this.GetSIQuestStatus(OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoPurchaseTechPoints(ProgressionManager.PurchaseTechPointsRequest data, Action OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.PurchaseTechPointsRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.PurchaseTechPoints);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			Action onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess();
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.PurchaseTechPointsRequest>(ProgressionManager.RequestType.PurchaseTechPoints, data, delegate(ProgressionManager.PurchaseTechPointsRequest x)
		{
			this.PurchaseTechPoints(data.TechPointsAmount, OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoPurchaseResources(ProgressionManager.PurchaseResourcesRequest data, Action<ProgressionManager.UserInventory> OnSuccess, Action<string> OnFailure)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.PurchaseResourcesRequest>(PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl, data, ProgressionManager.RequestType.PurchaseResources);
		yield return request.SendWebRequest();
		if (this.IsSuccessResponse(request.responseCode))
		{
			ProgressionManager.UserInventoryResponse userInventoryResponse = JsonConvert.DeserializeObject<ProgressionManager.UserInventoryResponse>(request.downloadHandler.text);
			Action<ProgressionManager.UserInventory> onSuccess = OnSuccess;
			if (onSuccess != null)
			{
				onSuccess(userInventoryResponse.Result);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			Action<string> onFailure = OnFailure;
			if (onFailure != null)
			{
				onFailure(request.error);
			}
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.PurchaseResourcesRequest>(ProgressionManager.RequestType.PurchaseResources, data, delegate(ProgressionManager.PurchaseResourcesRequest x)
		{
			this.PurchaseResources(OnSuccess, OnFailure);
		}, delegate
		{
			Action<string> onFailure2 = OnFailure;
			if (onFailure2 == null)
			{
				return;
			}
			onFailure2(request.error);
		});
		yield break;
	}

	private IEnumerator DoPurchaseShiftCreditCapIncrease(ProgressionManager.PurchaseShiftCreditCapIncreaseRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.PurchaseShiftCreditCapIncreaseRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.PurchaseShiftCreditCapIncrease);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.PurchaseShiftCreditCapIncreaseResponse purchaseShiftCreditCapIncreaseResponse = JsonConvert.DeserializeObject<ProgressionManager.PurchaseShiftCreditCapIncreaseResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.PurchaseShiftCreditCapIncrease] = 0;
			this.RefreshShinyRocksTotal();
			Action<string, int, int> onGetShiftCreditCapData = this.OnGetShiftCreditCapData;
			if (onGetShiftCreditCapData != null)
			{
				onGetShiftCreditCapData(purchaseShiftCreditCapIncreaseResponse.TargetMothershipId, purchaseShiftCreditCapIncreaseResponse.CurrentShiftCreditCapIncreases, purchaseShiftCreditCapIncreaseResponse.CurrentShiftCreditCapIncreasesMax);
			}
			Action<bool> onPurchaseShiftCreditCapIncrease = this.OnPurchaseShiftCreditCapIncrease;
			if (onPurchaseShiftCreditCapIncrease != null)
			{
				onPurchaseShiftCreditCapIncrease(true);
			}
			yield break;
		}
		if (request.responseCode == 400L && request.downloadHandler.text == "User Already Has Purchased Max Shift Credit Cap")
		{
			Action<bool> onPurchaseShiftCreditCapIncrease2 = this.OnPurchaseShiftCreditCapIncrease;
			if (onPurchaseShiftCreditCapIncrease2 != null)
			{
				onPurchaseShiftCreditCapIncrease2(false);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.PurchaseShiftCreditCapIncreaseRequest>(ProgressionManager.RequestType.PurchaseShiftCreditCapIncrease, data, delegate(ProgressionManager.PurchaseShiftCreditCapIncreaseRequest x)
		{
			this.PurchaseShiftCreditCapIncreaseInternal(request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoPurchaseShiftCredit(ProgressionManager.PurchaseShiftCreditRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.PurchaseShiftCreditRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.PurchaseShiftCredit);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.PurchaseShiftCreditResponse purchaseShiftCreditResponse = JsonConvert.DeserializeObject<ProgressionManager.PurchaseShiftCreditResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.PurchaseShiftCredit] = 0;
			this.RefreshShinyRocksTotal();
			Action<string, int> onGetShiftCredit = this.OnGetShiftCredit;
			if (onGetShiftCredit != null)
			{
				onGetShiftCredit(purchaseShiftCreditResponse.TargetMothershipId, purchaseShiftCreditResponse.CurrentShiftCredits);
			}
			Action<bool> onPurchaseShiftCredit = this.OnPurchaseShiftCredit;
			if (onPurchaseShiftCredit != null)
			{
				onPurchaseShiftCredit(true);
			}
			GRPlayer local = GRPlayer.GetLocal();
			if (local != null)
			{
				local.SendCreditsRefilledTelemetry(100, purchaseShiftCreditResponse.CurrentShiftCredits);
			}
			yield break;
		}
		if (request.responseCode == 400L && request.downloadHandler.text == "User Already at Max Shift Credit")
		{
			Action<bool> onPurchaseShiftCredit2 = this.OnPurchaseShiftCredit;
			if (onPurchaseShiftCredit2 != null)
			{
				onPurchaseShiftCredit2(false);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.PurchaseShiftCreditRequest>(ProgressionManager.RequestType.PurchaseShiftCredit, data, delegate(ProgressionManager.PurchaseShiftCreditRequest x)
		{
			this.PurchaseShiftCreditInternal(request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoGetShiftCredit(ProgressionManager.GetShiftCreditRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GetShiftCreditRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.GetShiftCredit);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.ShiftCreditResponse shiftCreditResponse = JsonConvert.DeserializeObject<ProgressionManager.ShiftCreditResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.GetShiftCredit] = 0;
			Action<string, int> onGetShiftCredit = this.OnGetShiftCredit;
			if (onGetShiftCredit != null)
			{
				onGetShiftCredit(shiftCreditResponse.TargetMothershipId, shiftCreditResponse.CurrentShiftCredits);
			}
			Action<string, int, int> onGetShiftCreditCapData = this.OnGetShiftCreditCapData;
			if (onGetShiftCreditCapData != null)
			{
				onGetShiftCreditCapData(shiftCreditResponse.TargetMothershipId, shiftCreditResponse.CurrentShiftCreditCapIncreases, shiftCreditResponse.CurrentShiftCreditCapIncreasesMax);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.GetShiftCreditRequest>(ProgressionManager.RequestType.GetShiftCredit, data, delegate(ProgressionManager.GetShiftCreditRequest x)
		{
			this.GetShiftCredit(x.TargetMothershipId);
		}, null);
		yield break;
	}

	private IEnumerator DoGetJuicerStatus(ProgressionManager.GetJuicerStatusRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GetJuicerStatusRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.GetJuicerStatus);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.GetJuicerStatus] = 0;
			ProgressionManager.JuicerStatusResponse juicerStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.JuicerStatusResponse>(request.downloadHandler.text);
			Action<ProgressionManager.JuicerStatusResponse> onJucierStatusUpdated = this.OnJucierStatusUpdated;
			if (onJucierStatusUpdated != null)
			{
				onJucierStatusUpdated(juicerStatusResponse);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.GetJuicerStatusRequest>(ProgressionManager.RequestType.GetJuicerStatus, data, delegate(ProgressionManager.GetJuicerStatusRequest x)
		{
			this.GetJuicerStatusInternal(request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoDepositCore(ProgressionManager.DepositCoreRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.DepositCoreRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.DepositCore);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.DepositCore] = 0;
			if (data.CoreBeingDeposited == ProgressionManager.CoreType.ChaosSeed)
			{
				Action<bool> onChaosDepositSuccess = this.OnChaosDepositSuccess;
				if (onChaosDepositSuccess != null)
				{
					onChaosDepositSuccess(true);
				}
				this.GetJuicerStatus();
			}
			else
			{
				ProgressionManager.DepositCoreResponse depositCoreResponse = JsonConvert.DeserializeObject<ProgressionManager.DepositCoreResponse>(request.downloadHandler.text);
				Action<string, int> onGetShiftCredit = this.OnGetShiftCredit;
				if (onGetShiftCredit != null)
				{
					onGetShiftCredit(data.MothershipId, depositCoreResponse.CurrentShiftCredits);
				}
			}
			yield break;
		}
		if (request.responseCode == 400L && request.downloadHandler.text == "DepositGRCore already at seed cap")
		{
			if (data.CoreBeingDeposited == ProgressionManager.CoreType.ChaosSeed)
			{
				Action<bool> onChaosDepositSuccess2 = this.OnChaosDepositSuccess;
				if (onChaosDepositSuccess2 != null)
				{
					onChaosDepositSuccess2(false);
				}
				this.GetJuicerStatus();
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.DepositCoreRequest>(ProgressionManager.RequestType.DepositCore, data, delegate(ProgressionManager.DepositCoreRequest x)
		{
			this.DepositCoreInternal(x.CoreBeingDeposited, request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoPurchaseOverdrive(ProgressionManager.PurchaseOverdriveRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.PurchaseOverdriveRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.PurchaseOverdrive);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.PurchaseOverdrive] = 0;
			this.GetJuicerStatus();
			this.RefreshShinyRocksTotal();
			Action<bool> onPurchaseOverdrive = this.OnPurchaseOverdrive;
			if (onPurchaseOverdrive != null)
			{
				onPurchaseOverdrive(true);
			}
			yield break;
		}
		if (request.responseCode == 400L && (request.downloadHandler.text == "User Already At Overdrive Cap" || request.downloadHandler.text == "User would exceed Overdrive Cap"))
		{
			Action<bool> onPurchaseOverdrive2 = this.OnPurchaseOverdrive;
			if (onPurchaseOverdrive2 != null)
			{
				onPurchaseOverdrive2(false);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.PurchaseOverdriveRequest>(ProgressionManager.RequestType.PurchaseOverdrive, data, delegate(ProgressionManager.PurchaseOverdriveRequest x)
		{
			this.PurchaseOverdriveInternal(request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoSubtractShiftCredit(ProgressionManager.SubtractShiftCreditRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.SubtractShiftCreditRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.SubtractShiftCredit);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.ShiftCreditResponse shiftCreditResponse = JsonConvert.DeserializeObject<ProgressionManager.ShiftCreditResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.SubtractShiftCredit] = 0;
			Action<string, int> onGetShiftCredit = this.OnGetShiftCredit;
			if (onGetShiftCredit != null)
			{
				onGetShiftCredit(data.MothershipId, shiftCreditResponse.CurrentShiftCredits);
			}
			Action<string, int, int> onGetShiftCreditCapData = this.OnGetShiftCreditCapData;
			if (onGetShiftCreditCapData != null)
			{
				onGetShiftCreditCapData(shiftCreditResponse.TargetMothershipId, shiftCreditResponse.CurrentShiftCreditCapIncreases, shiftCreditResponse.CurrentShiftCreditCapIncreasesMax);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.SubtractShiftCreditRequest>(ProgressionManager.RequestType.SubtractShiftCredit, data, delegate(ProgressionManager.SubtractShiftCreditRequest x)
		{
			this.SubtractShiftCreditInternal(data.ShiftCreditToRemove, request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoAdvanceDockWristUpgradeLevel(ProgressionManager.AdvanceDockWristUpgradeRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.AdvanceDockWristUpgradeRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.AdvanceDockWristUpgrade);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.DockWristStatusResponse dockWristStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.DockWristStatusResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.AdvanceDockWristUpgrade] = 0;
			Action<ProgressionManager.DockWristStatusResponse> onDockWristStatusUpdated = this.OnDockWristStatusUpdated;
			if (onDockWristStatusUpdated != null)
			{
				onDockWristStatusUpdated(dockWristStatusResponse);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.AdvanceDockWristUpgradeRequest>(ProgressionManager.RequestType.AdvanceDockWristUpgrade, data, delegate(ProgressionManager.AdvanceDockWristUpgradeRequest x)
		{
			this.AdvanceDockWristUpgradeLevelInternal(data.Upgrade, request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoGetDockWristUpgradeStatus(ProgressionManager.DockWristUpgradeStatusRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.DockWristUpgradeStatusRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.GetDockWristUpgradeStatus);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.DockWristStatusResponse dockWristStatusResponse = JsonConvert.DeserializeObject<ProgressionManager.DockWristStatusResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.GetDockWristUpgradeStatus] = 0;
			Action<ProgressionManager.DockWristStatusResponse> onDockWristStatusUpdated = this.OnDockWristStatusUpdated;
			if (onDockWristStatusUpdated != null)
			{
				onDockWristStatusUpdated(dockWristStatusResponse);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.DockWristUpgradeStatusRequest>(ProgressionManager.RequestType.GetDockWristUpgradeStatus, data, delegate(ProgressionManager.DockWristUpgradeStatusRequest x)
		{
			this.GetDockWristUpgradeStatus();
		}, null);
		yield break;
	}

	private IEnumerator DoPurchaseDrillUpgrade(ProgressionManager.PurchaseDrillUpgradeRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.PurchaseDrillUpgradeRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.PurchaseDrillUpgrade);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.PurchaseDrillUpgrade] = 0;
			this.RefreshUserInventory();
			Action<string, string> onNodeUnlocked = this.OnNodeUnlocked;
			if (onNodeUnlocked != null)
			{
				onNodeUnlocked("", "");
			}
			if (data.Upgrade == ProgressionManager.DrillUpgradeLevel.Base)
			{
				GRPlayer local = GRPlayer.GetLocal();
				if (local != null)
				{
					local.SendPodUpgradeTelemetry(ProgressionManager.DrillUpgradeLevel.Base.ToString(), 0, 2500, 0);
				}
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.PurchaseDrillUpgradeRequest>(ProgressionManager.RequestType.PurchaseDrillUpgrade, data, delegate(ProgressionManager.PurchaseDrillUpgradeRequest x)
		{
			this.PurchaseDrillUpgrade(data.Upgrade);
		}, null);
		yield break;
	}

	private IEnumerator DoRecycleTool(ProgressionManager.RecycleToolRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.RecycleToolRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.RecycleTool);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.ShiftCreditResponse shiftCreditResponse = JsonConvert.DeserializeObject<ProgressionManager.ShiftCreditResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.RecycleTool] = 0;
			Action<string, int> onGetShiftCredit = this.OnGetShiftCredit;
			if (onGetShiftCredit != null)
			{
				onGetShiftCredit(data.MothershipId, shiftCreditResponse.CurrentShiftCredits);
			}
			Action<string, int, int> onGetShiftCreditCapData = this.OnGetShiftCreditCapData;
			if (onGetShiftCreditCapData != null)
			{
				onGetShiftCreditCapData(shiftCreditResponse.TargetMothershipId, shiftCreditResponse.CurrentShiftCreditCapIncreases, shiftCreditResponse.CurrentShiftCreditCapIncreasesMax);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.RecycleToolRequest>(ProgressionManager.RequestType.RecycleTool, data, delegate(ProgressionManager.RecycleToolRequest x)
		{
			this.RecycleTool(data.ToolBeingRecycled, data.NumberOfPlayers);
		}, null);
		yield break;
	}

	private IEnumerator DoStartOfShift(ProgressionManager.StartOfShiftRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.StartOfShiftRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.StartOfShift);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.StartOfShift] = 0;
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.StartOfShiftRequest>(ProgressionManager.RequestType.StartOfShift, data, delegate(ProgressionManager.StartOfShiftRequest x)
		{
			this.StartOfShift(data.ShiftId, data.CoresRequired, data.NumberOfPlayers, data.Depth);
		}, null);
		yield break;
	}

	private IEnumerator DoEndOfShiftReward(ProgressionManager.EndOfShiftRewardRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.EndOfShiftRewardRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.EndOfShiftReward);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.ShiftCreditResponse shiftCreditResponse = JsonConvert.DeserializeObject<ProgressionManager.ShiftCreditResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.EndOfShiftReward] = 0;
			Action<string, int> onGetShiftCredit = this.OnGetShiftCredit;
			if (onGetShiftCredit != null)
			{
				onGetShiftCredit(data.MothershipId, shiftCreditResponse.CurrentShiftCredits);
			}
			Action<string, int, int> onGetShiftCreditCapData = this.OnGetShiftCreditCapData;
			if (onGetShiftCreditCapData != null)
			{
				onGetShiftCreditCapData(shiftCreditResponse.TargetMothershipId, shiftCreditResponse.CurrentShiftCreditCapIncreases, shiftCreditResponse.CurrentShiftCreditCapIncreasesMax);
			}
			yield break;
		}
		if (request.responseCode == 400L && request.error == "EndOfShiftReward Unknown Shift or Mothership Failure.")
		{
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.EndOfShiftRewardRequest>(ProgressionManager.RequestType.EndOfShiftReward, data, delegate(ProgressionManager.EndOfShiftRewardRequest x)
		{
			this.EndOfShiftRewardInternal(data.ShiftId, request.responseCode == 409L);
		}, null);
		yield break;
	}

	private IEnumerator DoGetGhostReactorStats(ProgressionManager.GhostReactorStatsRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GhostReactorStatsRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.GetGhostReactorStats);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.GhostReactorStatsResponse ghostReactorStatsResponse = JsonConvert.DeserializeObject<ProgressionManager.GhostReactorStatsResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.GetGhostReactorStats] = 0;
			Action<ProgressionManager.GhostReactorStatsResponse> onGhostReactorStatsUpdated = this.OnGhostReactorStatsUpdated;
			if (onGhostReactorStatsUpdated != null)
			{
				onGhostReactorStatsUpdated(ghostReactorStatsResponse);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.GhostReactorStatsRequest>(ProgressionManager.RequestType.GetGhostReactorStats, data, delegate(ProgressionManager.GhostReactorStatsRequest x)
		{
			this.GetGhostReactorStats();
		}, null);
		yield break;
	}

	private IEnumerator DoGetGhostReactorInventory(ProgressionManager.GhostReactorInventoryRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.GhostReactorInventoryRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.GetGhostReactorInventory);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			ProgressionManager.GhostReactorInventoryResponse ghostReactorInventoryResponse = JsonConvert.DeserializeObject<ProgressionManager.GhostReactorInventoryResponse>(request.downloadHandler.text);
			this.retryCounters[ProgressionManager.RequestType.GetGhostReactorInventory] = 0;
			Action<ProgressionManager.GhostReactorInventoryResponse> onGhostReactorInventoryUpdated = this.OnGhostReactorInventoryUpdated;
			if (onGhostReactorInventoryUpdated != null)
			{
				onGhostReactorInventoryUpdated(ghostReactorInventoryResponse);
			}
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, false))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.GhostReactorInventoryRequest>(ProgressionManager.RequestType.GetGhostReactorInventory, data, delegate(ProgressionManager.GhostReactorInventoryRequest x)
		{
			this.GetGhostReactorInventory();
		}, null);
		yield break;
	}

	private IEnumerator DoSetGhostReactorInventory(ProgressionManager.SetGhostReactorInventoryRequest data)
	{
		UnityWebRequest request = this.FormatWebRequest<ProgressionManager.SetGhostReactorInventoryRequest>(PlayFabAuthenticatorSettings.ProgressionApiBaseUrl, data, ProgressionManager.RequestType.SetGhostReactorInventory);
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			this.retryCounters[ProgressionManager.RequestType.SetGhostReactorInventory] = 0;
			yield break;
		}
		if (!this.HandleWebRequestFailures(request, true))
		{
			yield break;
		}
		yield return this.HandleWebRequestRetries<ProgressionManager.SetGhostReactorInventoryRequest>(ProgressionManager.RequestType.SetGhostReactorInventory, data, delegate(ProgressionManager.SetGhostReactorInventoryRequest x)
		{
			this.SetGhostReactorInventoryInternal(data.InventoryJson, request.responseCode == 409L);
		}, null);
		yield break;
	}

	private bool IsSuccessResponse(long code)
	{
		return code >= 200L && code < 300L;
	}

	private UnityWebRequest FormatWebRequest<T>(string url, T pendingRequest, ProgressionManager.RequestType type)
	{
		string text = "";
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(pendingRequest));
		switch (type)
		{
		case ProgressionManager.RequestType.GetProgression:
			text = "/api/GetProgression";
			break;
		case ProgressionManager.RequestType.SetProgression:
			text = "/api/SetProgression";
			break;
		case ProgressionManager.RequestType.UnlockProgressionTreeNode:
			text = "/api/UnlockProgressionTreeNode";
			break;
		case ProgressionManager.RequestType.IncrementSIResource:
			text = "/api/IncrementSIResource";
			break;
		case ProgressionManager.RequestType.CompleteSIQuest:
			text = "/api/SetSIQuestComplete";
			break;
		case ProgressionManager.RequestType.CompleteSIBonus:
			text = "/api/SetSIBonusComplete";
			break;
		case ProgressionManager.RequestType.CollectSIIdol:
			text = "/api/SetSIIdolCollect";
			break;
		case ProgressionManager.RequestType.GetActiveSIQuests:
			text = "/api/GetActiveSIQuests";
			break;
		case ProgressionManager.RequestType.GetSIQuestsStatus:
			text = "/api/GetSIQuestsStatus";
			break;
		case ProgressionManager.RequestType.ResetSIQuestsStatus:
			text = "/api/ResetSIQuestsStatus";
			break;
		case ProgressionManager.RequestType.PurchaseTechPoints:
			text = "/api/PurchaseTechPoints";
			break;
		case ProgressionManager.RequestType.PurchaseResources:
			text = "/api/PurchaseResources";
			break;
		case ProgressionManager.RequestType.PurchaseShiftCreditCapIncrease:
			text = "/api/PurchaseShiftCreditCapIncrease";
			break;
		case ProgressionManager.RequestType.PurchaseShiftCredit:
			text = "/api/PurchaseShiftCredit";
			break;
		case ProgressionManager.RequestType.GetJuicerStatus:
			text = "/api/GetJuicerStatus";
			break;
		case ProgressionManager.RequestType.DepositCore:
			text = "/api/DepositGRCore";
			break;
		case ProgressionManager.RequestType.PurchaseOverdrive:
			text = "/api/PurchaseOverdrive";
			break;
		case ProgressionManager.RequestType.GetShiftCredit:
			text = "/api/GetShiftCredit";
			break;
		case ProgressionManager.RequestType.SubtractShiftCredit:
			text = "/api/SubtractShiftCredit";
			break;
		case ProgressionManager.RequestType.AdvanceDockWristUpgrade:
			text = "/api/AdvanceDockWristUpgrade";
			break;
		case ProgressionManager.RequestType.GetDockWristUpgradeStatus:
			text = "/api/GetDockWristUpgradeStatus";
			break;
		case ProgressionManager.RequestType.PurchaseDrillUpgrade:
			text = "/api/PurchaseDrillUpgrade";
			break;
		case ProgressionManager.RequestType.RecycleTool:
			text = "/api/RecycleTool";
			break;
		case ProgressionManager.RequestType.StartOfShift:
			text = "/api/StartOfShift";
			break;
		case ProgressionManager.RequestType.EndOfShiftReward:
			text = "/api/EndOfShiftReward";
			break;
		case ProgressionManager.RequestType.GetGhostReactorStats:
			text = "/api/GetGhostReactorStats";
			break;
		case ProgressionManager.RequestType.GetGhostReactorInventory:
			text = "/api/GetGhostReactorInventory";
			break;
		case ProgressionManager.RequestType.SetGhostReactorInventory:
			text = "/api/SetGhostReactorInventory";
			break;
		}
		UnityWebRequest unityWebRequest = new UnityWebRequest(url + text, "POST");
		unityWebRequest.uploadHandler = new UploadHandlerRaw(bytes);
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		return unityWebRequest;
	}

	private void OnGetTrees(GetProgressionTreesForPlayerResponse response)
	{
		if (((response != null) ? response.Results : null) == null)
		{
			return;
		}
		this._trees.Clear();
		foreach (UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse in response.Results)
		{
			UserHydratedProgressionTreeResponse userHydratedProgressionTreeResponse2 = new UserHydratedProgressionTreeResponse();
			userHydratedProgressionTreeResponse2.Tree = userHydratedProgressionTreeResponse.Tree;
			userHydratedProgressionTreeResponse2.Track = userHydratedProgressionTreeResponse.Track;
			userHydratedProgressionTreeResponse2.Nodes = userHydratedProgressionTreeResponse.Nodes;
			this._trees[userHydratedProgressionTreeResponse.Tree.name] = userHydratedProgressionTreeResponse2;
		}
		Action onTreeUpdated = this.OnTreeUpdated;
		if (onTreeUpdated == null)
		{
			return;
		}
		onTreeUpdated();
	}

	private void OnGetInventory(MothershipGetInventoryResponse response)
	{
		if (((response != null) ? response.Results : null) == null)
		{
			return;
		}
		this._inventory.Clear();
		foreach (KeyValuePair<string, MothershipPlayerInventorySummary> keyValuePair in response.Results)
		{
			MothershipPlayerInventorySummary value = keyValuePair.Value;
			if (((value != null) ? value.entitlements : null) != null)
			{
				foreach (MothershipInventoryItemSummary mothershipInventoryItemSummary in keyValuePair.Value.entitlements)
				{
					string name = mothershipInventoryItemSummary.name;
					string text = ((name != null) ? name.Trim() : null);
					this._inventory[text] = new ProgressionManager.MothershipItemSummary
					{
						EntitlementId = mothershipInventoryItemSummary.entitlement_id,
						InGameId = mothershipInventoryItemSummary.in_game_id,
						Name = mothershipInventoryItemSummary.name,
						Quantity = mothershipInventoryItemSummary.quantity
					};
				}
			}
		}
		Action onInventoryUpdated = this.OnInventoryUpdated;
		if (onInventoryUpdated == null)
		{
			return;
		}
		onInventoryUpdated();
	}

	public int GetShinyRocksTotal()
	{
		if (CosmeticsController.instance != null)
		{
			return CosmeticsController.instance.CurrencyBalance;
		}
		return 0;
	}

	public void RefreshShinyRocksTotal()
	{
		if (CosmeticsController.instance != null)
		{
			CosmeticsController.instance.GetCurrencyBalance();
		}
	}

	public static void GetMothershipFailure(MothershipError callError, int errorCode)
	{
		Debug.LogError("Progression: GetMothershipFailure: " + callError.MothershipErrorCode + ":" + callError.Message);
	}

	private readonly Dictionary<string, UserHydratedProgressionTreeResponse> _trees = new Dictionary<string, UserHydratedProgressionTreeResponse>();

	private readonly Dictionary<string, ProgressionManager.MothershipItemSummary> _inventory = new Dictionary<string, ProgressionManager.MothershipItemSummary>();

	private readonly Dictionary<string, int> _tracks = new Dictionary<string, int>();

	private Dictionary<ProgressionManager.RequestType, int> retryCounters = new Dictionary<ProgressionManager.RequestType, int>();

	private int maxRetriesOnFail = 4;

	public struct MothershipItemSummary
	{
		public string Name;

		public string EntitlementId;

		public string InGameId;

		public int Quantity;
	}

	private enum RequestType
	{
		GetProgression,
		SetProgression,
		UnlockProgressionTreeNode,
		IncrementSIResource,
		CompleteSIQuest,
		CompleteSIBonus,
		CollectSIIdol,
		GetActiveSIQuests,
		GetSIQuestsStatus,
		ResetSIQuestsStatus,
		PurchaseTechPoints,
		PurchaseResources,
		PurchaseShiftCreditCapIncrease,
		PurchaseShiftCredit,
		RegisterToGRShift,
		GetJuicerStatus,
		DepositCore,
		PurchaseOverdrive,
		GetShiftCredit,
		SubtractShiftCredit,
		AdvanceDockWristUpgrade,
		GetDockWristUpgradeStatus,
		PurchaseDrillUpgrade,
		RecycleTool,
		StartOfShift,
		EndOfShiftReward,
		GetGhostReactorStats,
		GetGhostReactorInventory,
		SetGhostReactorInventory
	}

	public enum WristDockUpgradeType
	{
		None,
		Upgrade1,
		Upgrade2,
		Upgrade3
	}

	public enum DrillUpgradeLevel
	{
		None,
		Base,
		Upgrade1,
		Upgrade2,
		Upgrade3
	}

	public enum CoreType
	{
		None,
		Core,
		SuperCore,
		ChaosSeed
	}

	[Serializable]
	private class GetProgressionRequest : ProgressionManager.MothershipRequest
	{
		public string TrackId;
	}

	[Serializable]
	private class GetProgressionResponse
	{
		public string Track;

		public int Progress;

		public int StatusCode;

		public string Error;
	}

	[Serializable]
	private class SetProgressionRequest : ProgressionManager.MothershipRequest
	{
		public string TrackId;

		public int Progress;
	}

	[Serializable]
	private class SetProgressionResponse
	{
		public string Track;

		public int Progress;

		public int StatusCode;

		public string Error;
	}

	[Serializable]
	private class UnlockNodeRequest : ProgressionManager.MothershipRequest
	{
		public string TreeId;

		public string NodeId;
	}

	[Serializable]
	private class UnlockNodeResponse
	{
		public UserHydratedProgressionTreeResponse Tree;

		public int StatusCode;

		public string Error;
	}

	[Serializable]
	private class IncrementSIResourceRequest : ProgressionManager.MothershipRequest
	{
		public string ResourceType;
	}

	[Serializable]
	private class IncrementSIResourceResponse : ProgressionManager.UserInventoryResponse
	{
		public string ResourceType;
	}

	[Serializable]
	private class GetActiveSIQuestsRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	private class GetActiveSIQuestsResponse
	{
		public ProgressionManager.GetActiveSIQuestsResult Result;

		public int StatusCode;

		public string Error;
	}

	[Serializable]
	public class GetActiveSIQuestsResult
	{
		public List<RotatingQuest> Quests;
	}

	[Serializable]
	private class GetSIQuestsStatusRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	private class ResetSIQuestsStatusRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	private class PurchaseTechPointsRequest : ProgressionManager.MothershipRequest
	{
		public int TechPointsAmount;
	}

	private class PurchaseResourcesRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	private class GetSIQuestsStatusResponse
	{
		public ProgressionManager.UserQuestsStatusResponse Result;
	}

	[Serializable]
	private class UserInventoryResponse
	{
		public ProgressionManager.UserInventory Result;
	}

	[Serializable]
	public class UserInventory
	{
		public Dictionary<string, int> Inventory;
	}

	[Serializable]
	private class SetSIQuestCompleteRequest : ProgressionManager.RewardRequest
	{
		public int QuestID;

		public string ClientVersion;
	}

	[Serializable]
	private class SetSIBonusCompleteRequest : ProgressionManager.RewardRequest
	{
		public string ClientVersion;
	}

	[Serializable]
	private class SetSIIdolCollectRequest : ProgressionManager.RewardRequest
	{
	}

	[Serializable]
	private class RewardRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	private class MothershipRequest
	{
		public string MothershipId;

		public string MothershipToken;

		public string MothershipEnvId;

		public string MothershipTitleId;

		public string MothershipDeploymentId;
	}

	[Serializable]
	private class MothershipUserDataWriteRequest : ProgressionManager.MothershipRequest
	{
		public bool SkipUserDataCache;
	}

	[Serializable]
	public class UserQuestsStatusResponse
	{
		public int TodayClaimableQuests;

		public int TodayClaimableBonus;

		public int TodayClaimableIdol;
	}

	[Serializable]
	private class PurchaseShiftCreditCapIncreaseRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
	}

	[Serializable]
	private class PurchaseShiftCreditCapIncreaseResponse
	{
		public int StatusCode;

		public string Error;

		public int CurrentShiftCreditCapIncreases;

		public int CurrentShiftCreditCapIncreasesMax;

		public string TargetMothershipId;
	}

	[Serializable]
	private class PurchaseShiftCreditRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
	}

	[Serializable]
	private class PurchaseShiftCreditResponse
	{
		public int StatusCode;

		public string Error;

		public int CurrentShiftCredits;

		public string TargetMothershipId;
	}

	[Serializable]
	private class GetShiftCreditRequest : ProgressionManager.MothershipRequest
	{
		public string TargetMothershipId;
	}

	[Serializable]
	public class ShiftCreditResponse
	{
		public int StatusCode;

		public string Error;

		public int CurrentShiftCredits;

		public int CurrentShiftCreditCapIncreases;

		public int CurrentShiftCreditCapIncreasesMax;

		public string TargetMothershipId;
	}

	[Serializable]
	private class GetJuicerStatusRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
	}

	[Serializable]
	private class DepositCoreRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
		public ProgressionManager.CoreType CoreBeingDeposited;
	}

	[Serializable]
	private class DepositCoreResponse
	{
		public int StatusCode;

		public string Error;

		public int CurrentShiftCredits;
	}

	[Serializable]
	private class PurchaseOverdriveRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
	}

	[Serializable]
	public class JuicerStatusResponse
	{
		public string MothershipId;

		public int StatusCode;

		public string Error;

		public int CurrentCoreCount;

		public int CoreProcessingTimeSec;

		public float CoreProcessingPercent;

		public int OverdriveSupply;

		public int OverdriveCap;

		public int CoresProcessedByOverdrive;

		public bool RefreshJuice;
	}

	[Serializable]
	private class SubtractShiftCreditRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
		public int ShiftCreditToRemove;
	}

	[Serializable]
	private class AdvanceDockWristUpgradeRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
		public ProgressionManager.WristDockUpgradeType Upgrade;
	}

	[Serializable]
	private class DockWristUpgradeStatusRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	public class DockWristStatusResponse
	{
		public int CurrentUpgrade1Level;

		public int CurrentUpgrade2Level;

		public int CurrentUpgrade3Level;

		public int Upgrade1LevelMax;

		public int Upgrade2LevelMax;

		public int Upgrade3LevelMax;
	}

	[Serializable]
	private class PurchaseDrillUpgradeRequest : ProgressionManager.MothershipRequest
	{
		public ProgressionManager.DrillUpgradeLevel Upgrade;
	}

	[Serializable]
	private class PurchaseDrillUpgradeResponse
	{
		public int StatusCode;

		public string Error;
	}

	[Serializable]
	private class RecycleToolRequest : ProgressionManager.MothershipRequest
	{
		public GRTool.GRToolType ToolBeingRecycled;

		public int NumberOfPlayers;
	}

	[Serializable]
	private class StartOfShiftRequest : ProgressionManager.MothershipRequest
	{
		public string ShiftId;

		public int CoresRequired;

		public int NumberOfPlayers;

		public int Depth;
	}

	[Serializable]
	private class EndOfShiftRewardRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
		public string ShiftId;
	}

	[Serializable]
	private class GhostReactorStatsRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	public class GhostReactorStatsResponse
	{
		public string MothershipId;

		public int MaxDepthReached;
	}

	[Serializable]
	private class GhostReactorInventoryRequest : ProgressionManager.MothershipRequest
	{
	}

	[Serializable]
	public class GhostReactorInventoryResponse
	{
		public string MothershipId;

		public string InventoryJson;
	}

	[Serializable]
	private class SetGhostReactorInventoryRequest : ProgressionManager.MothershipUserDataWriteRequest
	{
		public string InventoryJson;
	}

	[Serializable]
	public class SetGhostReactorInventoryResponse
	{
		public string MothershipId;
	}
}
