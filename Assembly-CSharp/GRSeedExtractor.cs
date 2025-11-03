using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class GRSeedExtractor : MonoBehaviour
{
	public bool StationOpen
	{
		get
		{
			return this.stationOpen;
		}
	}

	public bool StationOpenForLocalPlayer
	{
		get
		{
			return this.stationOpen && this.currentPlayerActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber;
		}
	}

	public int CurrentPlayerActorNumber
	{
		get
		{
			return this.currentPlayerActorNumber;
		}
	}

	private void Awake()
	{
		this.triggerNotifier.TriggerEnterEvent += this.TriggerEntered;
		this.triggerNotifier.TriggerExitEvent += this.TriggerExited;
		this.coreDepositTriggerNotifier.TriggerEnterEvent += this.DepositorTriggerEntered;
		this.idCardScanner.OnPlayerCardSwipe += this.OnPlayerCardSwipe;
		for (int i = 0; i < this.maxVisualChaosSeedCount; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.chaosSeedVisualPrefab, base.transform);
			gameObject.SetActive(false);
			this.chaosSeedVisuals.Add(gameObject);
		}
		this.UpdateOverdrivePurchaseButtons();
		base.enabled = false;
	}

	public void Init(GRToolProgressionManager progression, GhostReactor gr)
	{
		this.ghostReactor = gr;
		this.toolProgressionManager = progression;
		this.toolProgressionManager.OnProgressionUpdated += this.OnResearchPointsUpdated;
		ProgressionManager.Instance.OnJucierStatusUpdated += this.OnPlayerStatusReceived;
		ProgressionManager.Instance.OnPurchaseOverdrive += this.OnPurchaseOverdrive;
		ProgressionManager.Instance.OnChaosDepositSuccess += this.TryDepositSeedServerResponse;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		this.ClearSeedVisuals();
		this.machineHumAudioSource.gameObject.SetActive(false);
		this.juicerSlowParticles.gameObject.SetActive(false);
		base.StopAllCoroutines();
		for (int i = 0; i < this.disableDuringOverdrive.Count; i++)
		{
			this.disableDuringOverdrive[i].gameObject.SetActive(true);
		}
		for (int j = 0; j < this.enableDuringOverdrive.Count; j++)
		{
			this.enableDuringOverdrive[j].gameObject.SetActive(false);
		}
		this.overdriveLightSpinnerOff.localRotation = this.overdriveLightSpinnerOn.localRotation;
		this.overdriveBeepAudioSource.Stop();
		this.overdriveActive = false;
		this.processingAmount = 0f;
		this.processingAmountVisual = 0f;
		this.overdriveAmount = 0f;
		this.overdriveAmountVisual = 0f;
		this.currentPlayerData = default(GRSeedExtractor.PlayerData);
		this.overdriveLiquidScaleParent.transform.localScale = new Vector3(1f, Mathf.Clamp01(this.overdriveAmountVisual), 1f);
		this.processingLiquidScaleParent.transform.localScale = new Vector3(1f, Mathf.Clamp01(this.processingAmountVisual), 1f);
	}

	private void Update()
	{
		this.ValidateCurrentPlayer();
		if (this.stationOpen && this.shutterDoorOpenAmount < 1f)
		{
			float num = Time.time - this.currentPlayerData.latestRefreshTime;
			if (Time.time - this.stationOpenRequestTime >= 1f || num <= 5f)
			{
				float num2 = 1f / this.shutterDoorAnimTime;
				this.shutterDoorOpenAmount = Mathf.MoveTowards(this.shutterDoorOpenAmount, 1f, num2 * Time.deltaTime);
				Vector3 localPosition = this.shutterDoorParent.transform.localPosition;
				localPosition.y = Mathf.Lerp(this.shutterDoorLiftRange.x, this.shutterDoorLiftRange.y, this.shutterDoorOpenAmount);
				this.shutterDoorParent.transform.localPosition = localPosition;
			}
		}
		else if (!this.stationOpen && this.shutterDoorOpenAmount > 0f)
		{
			float num3 = 1f / this.shutterDoorAnimTime;
			this.shutterDoorOpenAmount = Mathf.MoveTowards(this.shutterDoorOpenAmount, 0f, num3 * Time.deltaTime);
			Vector3 localPosition2 = this.shutterDoorParent.transform.localPosition;
			localPosition2.y = Mathf.Lerp(this.shutterDoorLiftRange.x, this.shutterDoorLiftRange.y, this.shutterDoorOpenAmount);
			this.shutterDoorParent.transform.localPosition = localPosition2;
			if (this.shutterDoorOpenAmount <= 0f)
			{
				this.processingAmount = 0f;
				this.overdriveAmount = 0f;
			}
		}
		bool flag = this.seedProcessingStates.Count > 0 && this.seedProcessingStates[0].dropProgress >= 1f;
		if (this.overdriveActive)
		{
			this.overdriveLightSpinnerOn.Rotate(Vector3.forward, 360f * this.overdriveLightSpinRate * Time.deltaTime, Space.Self);
			this.overdriveAmountVisual = this.overdriveAmount;
			this.overdriveLiquidScaleParent.transform.localScale = new Vector3(1f, Mathf.Clamp01(this.overdriveAmountVisual), 1f);
			this.processingAmountVisual = this.processingAmount;
			this.processingLiquidScaleParent.transform.localScale = new Vector3(1f, Mathf.Clamp01(this.processingAmountVisual), 1f);
		}
		else
		{
			float num4 = 1f / this.overdriveFillTime;
			if (flag || this.overdriveAmount > this.overdriveAmountVisual || !this.stationOpen)
			{
				this.overdriveAmountVisual = Mathf.MoveTowards(this.overdriveAmountVisual, this.overdriveAmount, num4 * Time.deltaTime);
			}
			this.overdriveLiquidScaleParent.transform.localScale = new Vector3(1f, Mathf.Clamp01(this.overdriveAmountVisual), 1f);
			if (this.stationOpen)
			{
				float num5 = Mathf.Max(Time.time - this.currentPlayerData.latestRefreshTime, 0f);
				float num6 = this.currentPlayerData.coreProcessingPercentage + num5 / this.PROCESSING_TIME_SECONDS;
				this.processingAmount = Mathf.Clamp01(num6);
				this.estimatedJuiceTimeRemaining = (1f - this.processingAmount) * this.PROCESSING_TIME_SECONDS;
				if (this.StationOpenForLocalPlayer && num6 >= 1f && Time.time - this.lastServerRequestTime > this.timeBetweenServerRequests)
				{
					this.lastServerRequestTime = Time.time;
					ProgressionManager.Instance.GetJuicerStatus();
				}
			}
			if (flag)
			{
				this.machineHumAudioSource.gameObject.SetActive(true);
				this.juicerSlowParticles.gameObject.SetActive(true);
				this.processingAmountVisual = Mathf.MoveTowards(this.processingAmountVisual, this.processingAmount, num4 * Time.deltaTime);
			}
			else
			{
				this.processingAmountVisual = Mathf.MoveTowards(this.processingAmountVisual, 0f, num4 * Time.deltaTime);
				this.machineHumAudioSource.gameObject.SetActive(false);
				this.juicerSlowParticles.gameObject.SetActive(false);
			}
			this.processingLiquidScaleParent.transform.localScale = new Vector3(1f, Mathf.Clamp01(this.processingAmountVisual), 1f);
		}
		this.StepSeedVisualAnimation(Time.deltaTime);
		this.UpdateScreenDisplay();
		if (!this.stationOpen && this.shutterDoorOpenAmount <= 0f && this.overdriveAmountVisual <= 0f)
		{
			base.enabled = false;
		}
	}

	private void ValidateCurrentPlayer()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			this.CloseStation();
			return;
		}
		if (this.ghostReactor.grManager.IsAuthority() && this.stationOpen)
		{
			bool flag = false;
			NetPlayer player = NetworkSystem.Instance.GetPlayer(this.currentPlayerActorNumber);
			RigContainer rigContainer;
			if (player != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
			{
				float num = 5f;
				if (rigContainer.Rig != null && rigContainer.Rig.OwningNetPlayer == player && (rigContainer.Rig.GetMouthPosition() - base.transform.position).sqrMagnitude < num * num)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorCloseStation, NetworkSystem.Instance.LocalPlayer.ActorNumber, 0);
			}
		}
	}

	public void TriggerEntered(TriggerEventNotifier notifier, Collider other)
	{
		VRRig component = other.GetComponent<VRRig>();
		if (component != null && component.OwningNetPlayer != null && component.OwningNetPlayer.ActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber && NetworkSystem.Instance.InRoom)
		{
			ProgressionManager.Instance.GetJuicerStatus();
		}
	}

	public void TriggerExited(TriggerEventNotifier notifier, Collider other)
	{
		VRRig component = other.GetComponent<VRRig>();
		if (component != null && component.OwningNetPlayer != null)
		{
			if (component.OwningNetPlayer.ActorNumber == this.currentPlayerActorNumber && this.stationOpen && this.ghostReactor.grManager.IsAuthority() && NetworkSystem.Instance.InRoom)
			{
				this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorCloseStation, NetworkSystem.Instance.LocalPlayer.ActorNumber, 0);
			}
			if (component.OwningNetPlayer.ActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				this.localPlayerData = default(GRSeedExtractor.PlayerData);
			}
		}
	}

	public void OnPlayerCardSwipe(int playerActorNumber)
	{
		if (playerActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber && NetworkSystem.Instance.InRoom)
		{
			this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorOpenStation, NetworkSystem.Instance.LocalPlayer.ActorNumber, 0);
			ProgressionManager.Instance.GetJuicerStatus();
		}
	}

	public void DepositorTriggerEntered(TriggerEventNotifier notifier, Collider other)
	{
		if (this.ghostReactor == null || this.ghostReactor.grManager == null || other == null || !NetworkSystem.Instance.InRoom)
		{
			return;
		}
		if (this.ghostReactor.grManager.IsAuthority() && other.attachedRigidbody != null)
		{
			GRCollectible component = other.attachedRigidbody.GetComponent<GRCollectible>();
			GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.zone);
			if (managerForZone != null && component != null && component.type == ProgressionManager.CoreType.ChaosSeed)
			{
				int netIdFromEntityId = managerForZone.GetNetIdFromEntityId(component.entity.id);
				int lastHeldByActorNumber = component.entity.lastHeldByActorNumber;
				bool player = NetworkSystem.Instance.GetPlayer(lastHeldByActorNumber) != null;
				float time = Time.time;
				if (player)
				{
					bool flag = false;
					for (int i = this.seedDepositsPending.Count - 1; i >= 0; i--)
					{
						if (time - this.seedDepositsPending[i].Item3 > 5f || managerForZone.GetGameEntityFromNetId(this.seedDepositsPending[i].Item1) == null || NetworkSystem.Instance.GetPlayer(this.seedDepositsPending[i].Item2) == null)
						{
							this.seedDepositsPending.RemoveAt(i);
						}
						else if (this.seedDepositsPending[i].Item1 == netIdFromEntityId)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						this.seedDepositsPending.Add(new ValueTuple<int, int, float, bool>(netIdFromEntityId, lastHeldByActorNumber, Time.time, false));
						this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorTryDepositSeed, lastHeldByActorNumber, netIdFromEntityId);
					}
				}
			}
		}
	}

	public void OverdrivePurchaseButtonPressed()
	{
		if (this.overdrivePurchasePending)
		{
			this.overdrivePurchasePending = false;
		}
		else if (this.LocalPlayerCanPurchaseOverdrive())
		{
			this.overdrivePurchasePending = true;
		}
		this.UpdateOverdrivePurchaseButtons();
	}

	private bool LocalPlayerCanPurchaseOverdrive()
	{
		if (Time.time - this.overdrivePurchaseTime > 5f)
		{
			this.overdriveServerConfirmationPending = false;
		}
		return this.StationOpenForLocalPlayer && !this.overdriveServerConfirmationPending && CosmeticsController.instance.CurrencyBalance >= 250 && this.localPlayerData.overdriveSupply <= 0f;
	}

	public void OverdrivePurchaseConfirmButtonPressed()
	{
		if (this.overdrivePurchasePending)
		{
			this.overdrivePurchasePending = false;
			if (this.stationOpen && this.currentPlayerActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				this.overdriveServerConfirmationPending = true;
				this.overdrivePurchaseTime = Time.time;
				ProgressionManager.Instance.PurchaseOverdrive();
			}
		}
		this.UpdateOverdrivePurchaseButtons();
	}

	public void OnPlayerStatusReceived(ProgressionManager.JuicerStatusResponse statusResponse)
	{
		if (statusResponse.MothershipId == GRPlayer.GetLocal().mothershipId && statusResponse.RefreshJuice)
		{
			this.toolProgressionManager.UpdateInventory();
		}
		this.PROCESSING_TIME_SECONDS = (float)statusResponse.CoreProcessingTimeSec;
		this.MAX_OVERDRIVE_USES = statusResponse.OverdriveCap / 100;
		float num = Mathf.Clamp01((float)statusResponse.OverdriveSupply / (float)statusResponse.OverdriveCap);
		int num2 = 0;
		bool flag = num < this.localPlayerData.overdriveSupply;
		bool flag2 = this.localPlayerData.overdriveSupply == 0f && this.localPlayerData.coreCount > statusResponse.CurrentCoreCount;
		if (statusResponse.CoresProcessedByOverdrive > 0 && (flag || flag2))
		{
			num2 = statusResponse.CoresProcessedByOverdrive;
		}
		this.localPlayerData.actorNumber = NetworkSystem.Instance.LocalPlayer.ActorNumber;
		this.localPlayerData.coreCount = statusResponse.CurrentCoreCount;
		this.localPlayerData.coreProcessingPercentage = Mathf.Clamp01(statusResponse.CoreProcessingPercent);
		this.localPlayerData.overdriveSupply = num;
		this.localPlayerData.coresProcessedByOverdrive = statusResponse.CoresProcessedByOverdrive;
		this.localPlayerData.coresPendingOverdriveProcessing = this.localPlayerData.coresPendingOverdriveProcessing + num2;
		this.localPlayerData.latestRefreshTime = Time.time;
		this.localPlayerData.researchPoints = this.toolProgressionManager.GetNumberOfResearchPoints();
		if (this.overdriveServerConfirmationPending && (this.localPlayerData.overdriveSupply > 0f || this.localPlayerData.coresProcessedByOverdrive > 0))
		{
			this.overdriveServerConfirmationPending = false;
		}
		if (this.stationOpen && this.currentPlayerActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber && NetworkSystem.Instance.InRoom)
		{
			this.currentPlayerData = this.localPlayerData;
			this.ghostReactor.grManager.RequestApplySeedExtractorState(this.localPlayerData.coreCount, this.localPlayerData.coresProcessedByOverdrive, this.localPlayerData.researchPoints, this.localPlayerData.coreProcessingPercentage, this.localPlayerData.overdriveSupply);
			this.OnStateUpdated();
		}
	}

	private void TryDepositSeedServerResponse(bool succeeded)
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		int num = -1;
		int actorNumber = NetworkSystem.Instance.LocalPlayer.ActorNumber;
		for (int i = 0; i < this.seedDepositsPending.Count; i++)
		{
			if (this.seedDepositsPending[i].Item2 == actorNumber)
			{
				num = this.seedDepositsPending[i].Item1;
			}
		}
		if (num == -1)
		{
			return;
		}
		if (succeeded)
		{
			this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorDepositSeedSucceeded, actorNumber, num);
			this.RemovePendingSeedDeposit(num);
			GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
			grplayer.SendSeedDepositedTelemetry(this.PROCESSING_TIME_SECONDS.ToString(), this.currentPlayerData.coreCount);
			grplayer.IncrementChaosSeedsCollected(1);
			return;
		}
		this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorDepositSeedFailed, actorNumber, num);
	}

	public void CardSwipeSuccess()
	{
		this.idCardScanner.onSucceeded.Invoke();
	}

	public void CardSwipeFail()
	{
		this.idCardScanner.onFailed.Invoke();
	}

	public void TryDepositSeed(int playerActorNumber, int seedNetId)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(playerActorNumber);
		GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.zone);
		if (player == null || managerForZone == null)
		{
			return;
		}
		this.depositorAudioSource.PlayOneShot(this.seedDepositAttemptAudio, this.seedDepositAttemptVolume);
		if (player.ActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			bool flag = false;
			float time = Time.time;
			for (int i = this.seedDepositsPending.Count - 1; i >= 0; i--)
			{
				if (time - this.seedDepositsPending[i].Item3 > 5f || managerForZone.GetGameEntityFromNetId(this.seedDepositsPending[i].Item1) == null || NetworkSystem.Instance.GetPlayer(this.seedDepositsPending[i].Item2) == null)
				{
					this.seedDepositsPending.RemoveAt(i);
				}
				else if (this.seedDepositsPending[i].Item1 == seedNetId)
				{
					flag = true;
					if (this.seedDepositsPending[i].Item2 == NetworkSystem.Instance.LocalPlayer.ActorNumber && !this.seedDepositsPending[i].Item4)
					{
						ValueTuple<int, int, float, bool> valueTuple = this.seedDepositsPending[i];
						valueTuple.Item4 = true;
						this.seedDepositsPending[i] = valueTuple;
						ProgressionManager.Instance.DepositCore(ProgressionManager.CoreType.ChaosSeed);
					}
				}
			}
			if (!flag)
			{
				this.seedDepositsPending.Add(new ValueTuple<int, int, float, bool>(seedNetId, playerActorNumber, Time.time, true));
				ProgressionManager.Instance.DepositCore(ProgressionManager.CoreType.ChaosSeed);
			}
		}
	}

	public bool ValidateSeedDepositSucceeded(int playerActorNumber, int entityNetId)
	{
		if (this.ghostReactor.grManager.IsAuthority())
		{
			bool flag = false;
			for (int i = 0; i < this.seedDepositsPending.Count; i++)
			{
				if (this.seedDepositsPending[i].Item1 == entityNetId && this.seedDepositsPending[i].Item2 == playerActorNumber)
				{
					flag = true;
				}
			}
			return flag;
		}
		return false;
	}

	public void SeedDepositSucceeded(int playerActorNumber, int entityNetId)
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		this.depositorParticles.Play();
		this.depositorAudioSource.PlayOneShot(this.seedDepositAudio, this.seedDepositVolume);
		this.RemovePendingSeedDeposit(entityNetId);
		if (playerActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			ProgressionManager.Instance.GetJuicerStatus();
		}
		if (!this.stationOpen && this.ghostReactor.grManager.IsAuthority())
		{
			this.ghostReactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SeedExtractorOpenStation, playerActorNumber, 0);
		}
	}

	public void SeedDepositFailed(int playerActorNumber, int entityNetId)
	{
		this.depositorAudioSource.PlayOneShot(this.seedDepositFailedAudio, this.seedDepositFailedVolume);
		this.RemovePendingSeedDeposit(entityNetId);
	}

	private void RemovePendingSeedDeposit(int entityId)
	{
		for (int i = this.seedDepositsPending.Count - 1; i >= 0; i--)
		{
			if (this.seedDepositsPending[i].Item1 == entityId)
			{
				this.seedDepositsPending.RemoveAt(i);
			}
		}
	}

	public void ApplyState(int playerActorNumber, int coreCount, int coresProcessedByOverdrive, int researchPoints, float coreProcessingPercentage, float overdriveSupply)
	{
		if (playerActorNumber == this.currentPlayerActorNumber)
		{
			if (this.currentPlayerData.actorNumber != playerActorNumber)
			{
				this.currentPlayerData = default(GRSeedExtractor.PlayerData);
			}
			coreCount = Mathf.Clamp(coreCount, 0, this.maxVisualChaosSeedCount);
			coresProcessedByOverdrive = Mathf.Clamp(coresProcessedByOverdrive, 0, this.MAX_OVERDRIVE_USES);
			coreProcessingPercentage = Mathf.Clamp(coreProcessingPercentage, 0f, 1f);
			overdriveSupply = Mathf.Clamp(overdriveSupply, 0f, 1f);
			bool flag = overdriveSupply < this.currentPlayerData.overdriveSupply;
			bool flag2 = this.currentPlayerData.overdriveSupply == 0f && this.currentPlayerData.coreCount > coreCount;
			if (playerActorNumber != NetworkSystem.Instance.LocalPlayer.ActorNumber && coresProcessedByOverdrive > 0 && (flag || flag2))
			{
				this.currentPlayerData.coresPendingOverdriveProcessing = this.currentPlayerData.coresPendingOverdriveProcessing + coresProcessedByOverdrive;
			}
			this.currentPlayerData.actorNumber = playerActorNumber;
			this.currentPlayerData.coreCount = coreCount;
			this.currentPlayerData.coresProcessedByOverdrive = coresProcessedByOverdrive;
			this.currentPlayerData.coreProcessingPercentage = coreProcessingPercentage;
			this.currentPlayerData.overdriveSupply = overdriveSupply;
			this.currentPlayerData.latestRefreshTime = Time.time;
			this.currentPlayerData.researchPoints = researchPoints;
			this.OnStateUpdated();
		}
	}

	public void OpenStation(int playerActorNumber)
	{
		if (NetworkSystem.Instance.GetPlayer(playerActorNumber) == null)
		{
			return;
		}
		if (!this.stationOpen)
		{
			this.doorAudioSource.PlayOneShot(this.doorOpenAudio, this.doorOpenVolume);
		}
		base.enabled = true;
		this.currentPlayerActorNumber = playerActorNumber;
		this.stationOpen = true;
		this.stationOpenRequestTime = Time.time;
		this.UpdateOverdrivePurchaseButtons();
	}

	public void CloseStation()
	{
		if (this.stationOpen)
		{
			this.doorAudioSource.PlayOneShot(this.doorCloseAudio, this.doorCloseVolume);
		}
		this.currentPlayerActorNumber = -1;
		this.stationOpen = false;
		this.UpdateOverdrivePurchaseButtons();
	}

	private void UpdateOverdrivePurchaseButtons()
	{
		if (!this.LocalPlayerCanPurchaseOverdrive())
		{
			this.overdrivePurchaseButton.myTmpText.text = "";
			this.overdrivePurchaseButton.buttonRenderer.material = this.defaultButtonMaterial;
			this.overdriveConfirmButton.myTmpText.text = "";
			this.overdriveConfirmButton.buttonRenderer.material = this.defaultButtonMaterial;
			return;
		}
		if (this.overdrivePurchasePending)
		{
			this.overdrivePurchaseButton.myTmpText.text = "CANCEL";
			this.overdrivePurchaseButton.buttonRenderer.material = this.redButtonMaterial;
			this.overdriveConfirmButton.myTmpText.text = "CONFIRM";
			this.overdriveConfirmButton.buttonRenderer.material = this.greenButtonMaterial;
			return;
		}
		this.overdrivePurchaseButton.myTmpText.text = "BUY";
		this.overdrivePurchaseButton.buttonRenderer.material = this.defaultButtonMaterial;
		this.overdriveConfirmButton.myTmpText.text = "";
		this.overdriveConfirmButton.buttonRenderer.material = this.defaultButtonMaterial;
	}

	public void OnStateUpdated()
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(this.currentPlayerActorNumber);
		if (player == null)
		{
			this.CloseStation();
		}
		this.UpdateOverdrivePurchaseButtons();
		if (this.stationOpen && player != null)
		{
			if (this.overdriveActive)
			{
				return;
			}
			if (this.currentPlayerData.coresPendingOverdriveProcessing > 0)
			{
				int coresPendingOverdriveProcessing = this.currentPlayerData.coresPendingOverdriveProcessing;
				this.currentPlayerData.coresPendingOverdriveProcessing = 0;
				if (this.StationOpenForLocalPlayer)
				{
					this.localPlayerData.coresPendingOverdriveProcessing = 0;
				}
				this.overdrivePurchaseAnimationRoutine = base.StartCoroutine(this.OverdrivePurchaseAnimationVisual(coresPendingOverdriveProcessing));
				return;
			}
			this.processingAmount = this.currentPlayerData.coreProcessingPercentage;
			this.overdriveAmount = this.currentPlayerData.overdriveSupply;
			int num = Mathf.Clamp(this.currentPlayerData.coreCount, 0, this.maxVisualChaosSeedCount) - this.seedProcessingStates.Count;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					this.DepositSeedVisual();
				}
				return;
			}
			if (num < 0)
			{
				for (int j = 0; j > num; j--)
				{
					this.CompleteSeedVisual();
				}
				return;
			}
		}
		else
		{
			this.screenText.text = "Player Data Lookup Failed.";
			this.overdriveAmount = 0f;
			this.processingAmount = 0f;
		}
	}

	private void DepositSeedVisual()
	{
		for (int i = 0; i < this.chaosSeedVisuals.Count; i++)
		{
			if (!this.chaosSeedVisuals[i].activeSelf)
			{
				GRSeedExtractor.SeedProcessingVisualState seedProcessingVisualState = new GRSeedExtractor.SeedProcessingVisualState
				{
					poolIndex = i,
					rollAngle = 0f,
					speed = 0f,
					rampProgress = 0f,
					dropProgress = 0f
				};
				this.seedProcessingStates.Add(seedProcessingVisualState);
				this.chaosSeedVisuals[i].SetActive(true);
				this.chaosSeedVisuals[i].transform.localPosition = this.seedTubeStart.localPosition;
				this.chaosSeedVisuals[i].transform.localRotation = Quaternion.identity;
				this.chaosSeedVisuals[i].transform.localScale = Vector3.one * this.seedVisualScaleRange.y;
				this.seedTubeAudioSource.PlayOneShot(this.seedMovementAudio, this.seedMovementVolume);
				return;
			}
		}
	}

	private void CompleteSeedVisual()
	{
		if (this.seedProcessingStates.Count > 0)
		{
			GRSeedExtractor.SeedProcessingVisualState seedProcessingVisualState = this.seedProcessingStates[0];
			this.chaosSeedVisuals[seedProcessingVisualState.poolIndex].SetActive(false);
			this.seedProcessingStates.RemoveAt(0);
		}
	}

	private void ClearSeedVisuals()
	{
		int count = this.seedProcessingStates.Count;
		for (int i = 0; i < count; i++)
		{
			this.CompleteSeedVisual();
		}
	}

	private void UpdateScreenDisplay()
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(this.currentPlayerActorNumber);
		if (player == null || !this.stationOpen)
		{
			return;
		}
		int num = (int)this.estimatedJuiceTimeRemaining;
		if (this.currentPlayerActorNumber != this.currentDisplayData.playerActorNumber || this.currentPlayerData.coreCount != this.currentDisplayData.coreCount || this.currentPlayerData.overdriveSupply != this.currentDisplayData.overdriveSupply || this.currentPlayerData.researchPoints != this.currentDisplayData.researchPoints || num != this.currentDisplayData.juiceSecondsLeft)
		{
			this.currentDisplayData.playerActorNumber = this.currentPlayerActorNumber;
			this.currentDisplayData.coreCount = this.currentPlayerData.coreCount;
			this.currentDisplayData.overdriveSupply = this.currentPlayerData.overdriveSupply;
			this.currentDisplayData.researchPoints = this.currentPlayerData.researchPoints;
			this.currentDisplayData.juiceSecondsLeft = num;
			this.UpdateScreenSB.Clear();
			this.UpdateScreenSB.Append(player.SanitizedNickName + "\n");
			this.UpdateScreenSB.Append(string.Format("JUICE: <color=purple>⑮ {0}</color>\n\n", this.currentDisplayData.researchPoints));
			if (this.currentDisplayData.coreCount > 0)
			{
				this.UpdateScreenSB.Append(string.Format("Processing {0} Seeds", this.currentDisplayData.coreCount));
				int num2 = this.currentDisplayData.juiceSecondsLeft % 3;
				if (num2 == 2)
				{
					this.UpdateScreenSB.Append(".");
				}
				else if (num2 == 1)
				{
					this.UpdateScreenSB.Append("..");
				}
				else
				{
					this.UpdateScreenSB.Append("...");
				}
				int num3 = num / 3600;
				int num4 = num / 60 % 60;
				int num5 = num % 60;
				if (num3 > 0)
				{
					this.UpdateScreenSB.Append(string.Format("\nNext <color=purple>⑮</color> in {0}:{1:00}:{2:00}\n", num3, num4, num5));
				}
				else
				{
					this.UpdateScreenSB.Append(string.Format("\nNext <color=purple>⑮</color> in {0}:{1:00}\n", num4, num5));
				}
			}
			else
			{
				this.UpdateScreenSB.Append("Deposit Chaos Seed\nFor Juice Processing\n");
			}
			this.screenText.text = this.UpdateScreenSB.ToString();
		}
	}

	private void StepSeedVisualAnimation(float dt)
	{
		float magnitude = (this.seedTubeStart.position - this.seedTubeEnd.position).magnitude;
		float num = magnitude / this.seedVisualRollTime;
		for (int i = 0; i < this.seedProcessingStates.Count; i++)
		{
			GRSeedExtractor.SeedProcessingVisualState seedProcessingVisualState = this.seedProcessingStates[i];
			float num2 = 2f;
			if (i > 0)
			{
				num2 = this.seedProcessingStates[i - 1].rampProgress - 2f * this.visualChaosSeedRadius / magnitude;
			}
			if (seedProcessingVisualState.rampProgress < 1f)
			{
				GameObject gameObject = this.chaosSeedVisuals[seedProcessingVisualState.poolIndex];
				seedProcessingVisualState.speed = Mathf.MoveTowards(seedProcessingVisualState.speed, num, num * dt);
				float num3 = seedProcessingVisualState.speed * dt;
				float num4 = num3 / magnitude;
				seedProcessingVisualState.rampProgress = Mathf.Clamp01(seedProcessingVisualState.rampProgress + num4);
				if (seedProcessingVisualState.rampProgress >= num2)
				{
					seedProcessingVisualState.rampProgress = num2;
					seedProcessingVisualState.speed = 0f;
					num3 = 0f;
				}
				gameObject.transform.localPosition = Vector3.Lerp(this.seedTubeStart.localPosition, this.seedTubeEnd.localPosition, seedProcessingVisualState.rampProgress);
				seedProcessingVisualState.rollAngle += num3 / this.visualChaosSeedRadius;
				gameObject.transform.localRotation = Quaternion.AngleAxis(seedProcessingVisualState.rollAngle * 57.29578f, Vector3.forward);
			}
			if (i == 0 && seedProcessingVisualState.rampProgress >= 1f)
			{
				GameObject gameObject2 = this.chaosSeedVisuals[seedProcessingVisualState.poolIndex];
				if (seedProcessingVisualState.dropProgress < 1f)
				{
					seedProcessingVisualState.dropProgress += 1f / this.seedVisualDropTime * dt;
					seedProcessingVisualState.rampProgress = 1f + seedProcessingVisualState.dropProgress;
					float num5 = this.tubeEndToProcessingPathY.Evaluate(seedProcessingVisualState.dropProgress);
					float num6 = this.tubeEndToProcessingPathX.Evaluate(seedProcessingVisualState.dropProgress);
					Vector3 localPosition = gameObject2.transform.localPosition;
					localPosition.y = Mathf.Lerp(this.seedTubeEnd.localPosition.y, this.seedProcessingPosition.localPosition.y, num5);
					localPosition.x = Mathf.Lerp(this.seedTubeEnd.localPosition.x, this.seedProcessingPosition.localPosition.x, num6);
					gameObject2.transform.localPosition = localPosition;
					float num7 = seedProcessingVisualState.speed * dt;
					seedProcessingVisualState.rollAngle += num7 / this.visualChaosSeedRadius;
					gameObject2.transform.localRotation = Quaternion.AngleAxis(seedProcessingVisualState.rollAngle * 57.29578f, Vector3.forward);
					if (seedProcessingVisualState.dropProgress >= 1f)
					{
						this.juicerAudioSource.PlayOneShot(this.seedDropAudio, this.seedDropVolume);
					}
				}
				if (seedProcessingVisualState.dropProgress >= 1f && !this.drainingProcessingBeaker)
				{
					gameObject2.transform.localScale = Vector3.one * Mathf.Lerp(this.seedVisualScaleRange.y, this.seedVisualScaleRange.x, this.processingAmountVisual);
				}
			}
			this.seedProcessingStates[i] = seedProcessingVisualState;
		}
	}

	private IEnumerator OverdrivePurchaseAnimationVisual(int coresToProcess)
	{
		this.overdriveActive = true;
		this.overdriveBeepAudioSource.loop = true;
		this.overdriveBeepAudioSource.volume = this.overdriveBeepingVolume;
		this.overdriveBeepAudioSource.clip = this.overdriveBeepingAudio;
		this.overdriveBeepAudioSource.Play();
		int num = Math.Min(coresToProcess + this.currentPlayerData.coreCount, this.maxVisualChaosSeedCount);
		while (this.seedProcessingStates.Count < num)
		{
			this.DepositSeedVisual();
		}
		for (int j = 0; j < this.disableDuringOverdrive.Count; j++)
		{
			this.disableDuringOverdrive[j].gameObject.SetActive(false);
		}
		for (int k = 0; k < this.enableDuringOverdrive.Count; k++)
		{
			this.enableDuringOverdrive[k].gameObject.SetActive(true);
		}
		this.overdriveMeterAudioSource.PlayOneShot(this.overdriveFillAudio, this.overdriveFillVolume);
		float overdriveFillRate = 1f / this.overdriveFillTime;
		float maxOverdriveFill = Mathf.Clamp01(this.currentPlayerData.overdriveSupply + (float)coresToProcess / (float)this.MAX_OVERDRIVE_USES);
		while (this.overdriveAmount < maxOverdriveFill)
		{
			this.overdriveAmount = Mathf.MoveTowards(this.overdriveAmount, maxOverdriveFill, overdriveFillRate * Time.deltaTime);
			yield return null;
		}
		this.overdriveMeterAudioSource.Stop();
		int num6;
		for (int i = 0; i < coresToProcess; i = num6)
		{
			float waitForSeedDepositStartTime = Time.time;
			bool flag = this.seedProcessingStates.Count > 0 && this.seedProcessingStates[0].dropProgress >= 1f;
			while (!flag && Time.time - waitForSeedDepositStartTime < 3f)
			{
				yield return null;
				flag = this.seedProcessingStates.Count > 0 && this.seedProcessingStates[0].dropProgress >= 1f;
			}
			this.juicerAudioSource.PlayOneShot(this.seedJuicingAudio, this.seedJuicingVolume);
			this.juicerOverdriveParticles.gameObject.SetActive(true);
			float num2 = Mathf.Clamp01(1f - this.processingAmount);
			float timeToProcess = num2 * this.overdriveProcessTime;
			float startingProcessingAmount = this.processingAmount;
			float num3 = num2 / (float)this.MAX_OVERDRIVE_USES;
			float startingOverdrive = this.overdriveAmount;
			float resultingOverdrive = Mathf.Clamp01(this.overdriveAmount - num3);
			float timeProcessing = 0f;
			while (timeProcessing < timeToProcess)
			{
				timeProcessing += Time.deltaTime;
				float num4 = timeProcessing / timeToProcess;
				this.overdriveAmount = Mathf.Lerp(startingOverdrive, resultingOverdrive, num4);
				this.processingAmount = Mathf.Lerp(startingProcessingAmount, 1f, num4);
				this.estimatedJuiceTimeRemaining = timeToProcess - timeProcessing;
				yield return null;
			}
			this.CompleteSeedVisual();
			this.juicerOverdriveParticles.gameObject.SetActive(false);
			this.drainingProcessingBeaker = true;
			float timeDepositing = 0f;
			while (timeDepositing < this.juiceDepositTime)
			{
				timeDepositing += Time.deltaTime;
				float num5 = timeDepositing / this.juiceDepositTime;
				this.processingAmount = Mathf.Lerp(1f, 0f, num5);
				yield return null;
			}
			this.drainingProcessingBeaker = false;
			num6 = i + 1;
		}
		if (this.currentPlayerData.coresPendingOverdriveProcessing == 0 && this.currentPlayerData.coreCount == 1)
		{
			if (this.seedProcessingStates.Count == 0)
			{
				this.DepositSeedVisual();
			}
			float timeDepositing = Time.time;
			bool flag2 = this.seedProcessingStates.Count > 0 && this.seedProcessingStates[0].dropProgress >= 1f;
			while (!flag2 && Time.time - timeDepositing < 3f)
			{
				yield return null;
				flag2 = this.seedProcessingStates.Count > 0 && this.seedProcessingStates[0].dropProgress >= 1f;
			}
			float timeProcessing = 0f;
			float resultingOverdrive = this.processingAmount;
			float startingOverdrive = this.overdriveAmount;
			float startingProcessingAmount = Mathf.Clamp01(this.currentPlayerData.coreProcessingPercentage - resultingOverdrive) * this.overdriveProcessTime;
			while (timeProcessing < startingProcessingAmount)
			{
				timeProcessing += Time.deltaTime;
				float num7 = timeProcessing / startingProcessingAmount;
				this.processingAmount = Mathf.Clamp01(Mathf.Lerp(resultingOverdrive, this.currentPlayerData.coreProcessingPercentage, num7));
				this.overdriveAmount = Mathf.Clamp01(Mathf.Lerp(startingOverdrive, this.currentPlayerData.overdriveSupply, num7));
				yield return null;
			}
		}
		for (int l = 0; l < this.disableDuringOverdrive.Count; l++)
		{
			this.disableDuringOverdrive[l].gameObject.SetActive(true);
		}
		for (int m = 0; m < this.enableDuringOverdrive.Count; m++)
		{
			this.enableDuringOverdrive[m].gameObject.SetActive(false);
		}
		this.overdriveLightSpinnerOff.localRotation = this.overdriveLightSpinnerOn.localRotation;
		this.overdriveBeepAudioSource.Stop();
		this.overdriveActive = false;
		if (this.StationOpenForLocalPlayer)
		{
			ProgressionManager.Instance.GetJuicerStatus();
		}
		this.OnStateUpdated();
		yield break;
	}

	public void OnResearchPointsUpdated()
	{
		int numberOfResearchPoints = this.toolProgressionManager.GetNumberOfResearchPoints();
		if (numberOfResearchPoints > this.localPlayerData.researchPoints)
		{
			GRPlayer.GetLocal().SendJuiceCollectedTelemetry(numberOfResearchPoints - this.localPlayerData.researchPoints, this.localPlayerData.coresProcessedByOverdrive);
		}
		this.localPlayerData.researchPoints = numberOfResearchPoints;
		if (this.StationOpenForLocalPlayer)
		{
			bool flag = this.currentPlayerData.researchPoints != this.localPlayerData.researchPoints;
			this.currentPlayerData.researchPoints = this.localPlayerData.researchPoints;
			if (flag)
			{
				this.ghostReactor.grManager.RequestApplySeedExtractorState(this.localPlayerData.coreCount, this.localPlayerData.coresProcessedByOverdrive, this.localPlayerData.researchPoints, this.localPlayerData.coreProcessingPercentage, this.localPlayerData.overdriveSupply);
				this.OnStateUpdated();
			}
		}
	}

	public void OnPurchaseOverdrive(bool success)
	{
		this.overdriveServerConfirmationPending = false;
		if (!success)
		{
			return;
		}
		GRPlayer.GetLocal().SendOverdrivePurchasedTelemetry(250, this.localPlayerData.coreCount);
	}

	private float PROCESSING_TIME_SECONDS = 600f;

	private int MAX_OVERDRIVE_USES = 6;

	[SerializeField]
	private GTZone zone;

	[SerializeField]
	private TriggerEventNotifier triggerNotifier;

	[SerializeField]
	private TriggerEventNotifier coreDepositTriggerNotifier;

	[SerializeField]
	private TMP_Text screenText;

	[SerializeField]
	private IDCardScanner idCardScanner;

	[SerializeField]
	private GameObject chaosSeedVisualPrefab;

	[Header("Overdrive Purchase Buttons")]
	[SerializeField]
	private GorillaPressableButton overdrivePurchaseButton;

	[SerializeField]
	private GorillaPressableButton overdriveConfirmButton;

	[SerializeField]
	private Material defaultButtonMaterial;

	[SerializeField]
	private Material redButtonMaterial;

	[SerializeField]
	private Material greenButtonMaterial;

	[Header("Shutter Door Visual")]
	[SerializeField]
	private Transform shutterDoorParent;

	[SerializeField]
	private Vector2 shutterDoorLiftRange = new Vector2(1.245f, 2.07f);

	[SerializeField]
	private float shutterDoorAnimTime;

	[Header("Seed Processing Visual")]
	[SerializeField]
	private Transform processingLiquidScaleParent;

	[SerializeField]
	[Range(0f, 1f)]
	public float processingAmount;

	private float processingAmountVisual;

	[SerializeField]
	private Transform seedTubeStart;

	[SerializeField]
	private Transform seedTubeEnd;

	[SerializeField]
	private Transform seedProcessingPosition;

	[SerializeField]
	private AnimationCurve tubeEndToProcessingPathY = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve tubeEndToProcessingPathX = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float visualChaosSeedRadius = 1f;

	[SerializeField]
	private int maxVisualChaosSeedCount = 6;

	[SerializeField]
	private float seedVisualRollTime = 2f;

	[SerializeField]
	private float seedVisualDropTime = 0.5f;

	[SerializeField]
	private Vector2 seedVisualScaleRange = new Vector2(0.1f, 1.25f);

	[Header("Overdrive Visual")]
	[SerializeField]
	private Transform overdriveLiquidScaleParent;

	[SerializeField]
	private Transform overdriveLightSpinnerOff;

	[SerializeField]
	private Transform overdriveLightSpinnerOn;

	[SerializeField]
	private List<Transform> enableDuringOverdrive = new List<Transform>();

	[SerializeField]
	private List<Transform> disableDuringOverdrive = new List<Transform>();

	[SerializeField]
	private float overdriveLightSpinRate = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	public float overdriveAmount;

	private float overdriveAmountVisual;

	[Header("VFX")]
	[SerializeField]
	private ParticleSystem depositorParticles;

	[SerializeField]
	private ParticleSystem juicerSlowParticles;

	[SerializeField]
	private ParticleSystem juicerOverdriveParticles;

	[Header("Audio")]
	[SerializeField]
	private AudioSource depositorAudioSource;

	[SerializeField]
	private AudioSource doorAudioSource;

	[SerializeField]
	private AudioSource seedTubeAudioSource;

	[SerializeField]
	private AudioSource juicerAudioSource;

	[SerializeField]
	private AudioSource machineHumAudioSource;

	[SerializeField]
	private AudioSource overdriveMeterAudioSource;

	[SerializeField]
	private AudioSource overdriveBeepAudioSource;

	[SerializeField]
	private AudioClip seedDepositAudio;

	[SerializeField]
	private float seedDepositVolume = 0.5f;

	[SerializeField]
	private AudioClip seedDepositFailedAudio;

	[SerializeField]
	private float seedDepositFailedVolume = 0.5f;

	[SerializeField]
	private AudioClip seedDepositAttemptAudio;

	[SerializeField]
	private float seedDepositAttemptVolume = 0.5f;

	[SerializeField]
	private AudioClip seedMovementAudio;

	[SerializeField]
	private float seedMovementVolume = 0.5f;

	[SerializeField]
	private AudioClip seedDropAudio;

	[SerializeField]
	private float seedDropVolume = 0.5f;

	[SerializeField]
	private AudioClip seedJuicingAudio;

	[SerializeField]
	private float seedJuicingVolume = 0.5f;

	[SerializeField]
	private AudioClip doorOpenAudio;

	[SerializeField]
	private float doorOpenVolume = 0.5f;

	[SerializeField]
	private AudioClip doorCloseAudio;

	[SerializeField]
	private float doorCloseVolume = 0.5f;

	[SerializeField]
	private AudioClip processingHumAudio;

	[SerializeField]
	private float processingHumVolume = 0.5f;

	[SerializeField]
	private AudioClip overdriveFillAudio;

	[SerializeField]
	private float overdriveFillVolume = 0.5f;

	[SerializeField]
	private AudioClip overdriveEngineAudio;

	[SerializeField]
	private float overdriveEngineVolume = 0.5f;

	[SerializeField]
	private AudioClip overdriveBeepingAudio;

	[SerializeField]
	private float overdriveBeepingVolume = 0.5f;

	private GRSeedExtractor.PlayerData localPlayerData;

	private GRSeedExtractor.PlayerData currentPlayerData;

	private GRSeedExtractor.ScreenDisplayData currentDisplayData;

	private bool stationOpen;

	private float stationOpenRequestTime;

	private int currentPlayerActorNumber = -1;

	private float shutterDoorOpenAmount;

	private List<GameObject> chaosSeedVisuals = new List<GameObject>();

	private bool overdrivePurchasePending;

	private bool overdriveServerConfirmationPending;

	private float overdrivePurchaseTime;

	private bool overdriveActive;

	private bool drainingProcessingBeaker;

	private float estimatedJuiceTimeRemaining;

	private float processingLiquidFollowRate = Mathf.Exp(2f);

	private List<ValueTuple<int, int, float, bool>> seedDepositsPending = new List<ValueTuple<int, int, float, bool>>();

	private Coroutine overdrivePurchaseAnimationRoutine;

	private List<GRSeedExtractor.SeedProcessingVisualState> seedProcessingStates = new List<GRSeedExtractor.SeedProcessingVisualState>();

	private float timeBetweenServerRequests = 3f;

	private float lastServerRequestTime;

	private GhostReactor ghostReactor;

	private GRToolProgressionManager toolProgressionManager;

	private StringBuilder UpdateScreenSB = new StringBuilder(256);

	[Header("Debug Animation")]
	public int debugSeedCount;

	public float debugSeedProcessingTime = 10f;

	public float overdriveFillTime = 2f;

	public float overdriveProcessTime = 1.5f;

	public float juiceDepositTime = 0.75f;

	public struct PlayerData
	{
		public int actorNumber;

		public int coreCount;

		public float coreProcessingPercentage;

		public float overdriveSupply;

		public int coresProcessedByOverdrive;

		public int coresPendingOverdriveProcessing;

		public int researchPoints;

		public float latestRefreshTime;
	}

	private struct ScreenDisplayData
	{
		public int playerActorNumber;

		public int coreCount;

		public float overdriveSupply;

		public int researchPoints;

		public int juiceSecondsLeft;
	}

	private struct SeedProcessingVisualState
	{
		public int poolIndex;

		public float speed;

		public float rollAngle;

		public float rampProgress;

		public float dropProgress;
	}
}
