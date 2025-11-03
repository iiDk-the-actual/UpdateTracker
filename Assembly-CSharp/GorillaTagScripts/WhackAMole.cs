using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using GorillaExtensions;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace GorillaTagScripts
{
	[NetworkBehaviourWeaved(210)]
	public class WhackAMole : NetworkComponent
	{
		private void UpdateMeshRendererList()
		{
			List<MeshRenderer> list = new List<MeshRenderer>();
			ZoneBasedObject[] array = this.zoneBasedVisuals;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (MeshRenderer meshRenderer in array[i].GetComponentsInChildren<MeshRenderer>(true))
				{
					if (meshRenderer.enabled)
					{
						list.Add(meshRenderer);
					}
				}
			}
			this.zoneBasedMeshRenderers = list.ToArray();
		}

		protected override void Awake()
		{
			base.Awake();
			if (this.molesContainerRight != null)
			{
				this.rightMolesList = new List<Mole>(this.molesContainerRight.GetComponentsInChildren<Mole>());
				if (this.rightMolesList.Count > 0)
				{
					this.molesList.AddRange(this.rightMolesList);
				}
			}
			if (this.molesContainerLeft != null)
			{
				this.leftMolesList = new List<Mole>(this.molesContainerLeft.GetComponentsInChildren<Mole>());
				if (this.leftMolesList.Count > 0)
				{
					this.molesList.AddRange(this.leftMolesList);
					foreach (Mole mole in this.leftMolesList)
					{
						mole.IsLeftSideMole = true;
					}
				}
			}
			this.currentLevelIndex = -1;
			foreach (Mole mole2 in this.molesList)
			{
				mole2.OnTapped += this.OnMoleTapped;
			}
			List<Mole> list = this.leftMolesList;
			bool flag;
			if (list != null && list.Count > 0)
			{
				list = this.rightMolesList;
				flag = list != null && list.Count > 0;
			}
			else
			{
				flag = false;
			}
			this.isMultiplayer = flag;
			this.welcomeUI.SetActive(false);
			this.ongoingGameUI.SetActive(false);
			this.levelEndedUI.SetActive(false);
			this.ContinuePressedUI.SetActive(false);
			this.multiplyareScoresUI.SetActive(false);
			this.bestScore = 0;
			this.bestScoreText.text = string.Empty;
			this.highScorePlayerName = string.Empty;
			this.victoryParticles = this.victoryFX.GetComponentsInChildren<ParticleSystem>();
		}

		protected override void Start()
		{
			base.Start();
			this.SwitchState(WhackAMole.GameState.Off);
			if (WhackAMoleManager.instance)
			{
				WhackAMoleManager.instance.Register(this);
			}
		}

		private void OnDestroy()
		{
			NetworkBehaviourUtils.InternalOnDestroy(this);
			foreach (Mole mole in this.molesList)
			{
				mole.OnTapped -= this.OnMoleTapped;
			}
			if (WhackAMoleManager.instance)
			{
				WhackAMoleManager.instance.Unregister(this);
			}
			this.molesList.Clear();
		}

		public void InvokeUpdate()
		{
			bool isMasterClient = NetworkSystem.Instance.IsMasterClient;
			bool flag = this.zoneBasedVisuals[0].IsLocalPlayerInZone();
			if (isMasterClient != this.wasMasterClient || flag != this.wasLocalPlayerInZone)
			{
				MeshRenderer[] array = this.zoneBasedMeshRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = flag;
				}
				bool flag2 = isMasterClient || flag;
				ZoneBasedObject[] array2 = this.zoneBasedVisuals;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].gameObject.SetActive(flag2);
				}
				this.wasMasterClient = isMasterClient;
				this.wasLocalPlayerInZone = flag;
			}
		}

		private void SwitchState(WhackAMole.GameState state)
		{
			this.lastState = this.currentState;
			this.currentState = state;
			switch (this.currentState)
			{
			case WhackAMole.GameState.Off:
				this.ResetGame();
				this.currentLevelIndex = -1;
				this.currentLevel = null;
				this.UpdateLevelUI(1);
				break;
			case WhackAMole.GameState.ContinuePressed:
				this.continuePressedTime = Time.time;
				this.audioSource.GTStop();
				this.audioSource.GTPlayOneShot(this.counterClip, 1f);
				if (base.IsMine)
				{
					this.pickedMolesIndex.Clear();
				}
				this.ResetGame();
				if (base.IsMine)
				{
					this.LoadNextLevel();
				}
				break;
			case WhackAMole.GameState.Ongoing:
				this.UpdateScoreUI(this.currentScore, this.leftPlayerScore, this.rightPlayerScore);
				break;
			case WhackAMole.GameState.TimesUp:
				if (this.currentLevel != null)
				{
					foreach (Mole mole in this.molesList)
					{
						mole.HideMole(false);
					}
					this.curentGameResult = this.GetGameResult();
					this.UpdateResultUI(this.curentGameResult);
					this.levelEndedTotalScoreText.text = "SCORE " + this.totalScore.ToString();
					this.levelEndedCurrentScoreText.text = string.Format("{0}/{1}", this.currentScore, this.currentLevel.GetMinScore(this.isMultiplayer));
					if (this.totalScore > this.bestScore)
					{
						this.bestScore = this.totalScore;
						this.highScorePlayerName = this.playerName;
					}
					this.bestScoreText.text = (this.isMultiplayer ? this.bestScore.ToString() : (this.highScorePlayerName + "  " + this.bestScore.ToString()));
					this.audioSource.GTStop();
					if (this.curentGameResult == WhackAMole.GameResult.LevelComplete)
					{
						this.audioSource.GTPlayOneShot(this.levelCompleteClip, 1f);
						if (NetworkSystem.Instance.LocalPlayer.UserId == this.playerId)
						{
							PlayerGameEvents.MiscEvent("WhackComplete" + this.currentLevel.levelNumber.ToString(), 1);
						}
					}
					else if (this.curentGameResult == WhackAMole.GameResult.GameOver)
					{
						this.audioSource.GTPlayOneShot(this.gameOverClip, 1f);
					}
					else if (this.curentGameResult == WhackAMole.GameResult.Win)
					{
						this.audioSource.GTPlayOneShot(this.winClip, 1f);
						if (this.victoryFX)
						{
							ParticleSystem[] array = this.victoryParticles;
							for (int i = 0; i < array.Length; i++)
							{
								array[i].Play();
							}
						}
						if (NetworkSystem.Instance.LocalPlayer.UserId == this.playerId)
						{
							PlayerGameEvents.MiscEvent("WhackComplete" + this.currentLevel.levelNumber.ToString(), 1);
						}
					}
					int minScore = this.currentLevel.GetMinScore(this.isMultiplayer);
					if (this.levelGoodMolesPicked < minScore)
					{
						GTDev.LogError<string>(string.Format("[WAM] Lvl:{0} Only Picked {1}/{2} good moles!", this.currentLevel.levelNumber, this.levelGoodMolesPicked, minScore), null);
					}
					if (base.IsMine)
					{
						GorillaTelemetry.WamLevelEnd(this.playerId, this.gameId, this.machineId, this.currentLevel.levelNumber, this.levelGoodMolesPicked, this.levelHazardMolesPicked, minScore, this.currentScore, this.levelHazardMolesHit, this.curentGameResult.ToString());
					}
				}
				break;
			}
			this.UpdateScreenData();
		}

		private void UpdateScreenData()
		{
			switch (this.currentState)
			{
			case WhackAMole.GameState.Off:
				this.welcomeUI.SetActive(true);
				this.ContinuePressedUI.SetActive(false);
				this.ongoingGameUI.SetActive(false);
				this.levelEndedUI.SetActive(false);
				this.multiplyareScoresUI.SetActive(false);
				return;
			case WhackAMole.GameState.ContinuePressed:
				this.levelEndedUI.SetActive(false);
				this.welcomeUI.SetActive(false);
				this.ongoingGameUI.SetActive(false);
				this.multiplyareScoresUI.SetActive(false);
				this.ContinuePressedUI.SetActive(true);
				break;
			case WhackAMole.GameState.Ongoing:
				this.ContinuePressedUI.SetActive(false);
				this.welcomeUI.SetActive(false);
				this.ongoingGameUI.SetActive(true);
				this.levelEndedUI.SetActive(false);
				if (this.isMultiplayer)
				{
					this.multiplyareScoresUI.SetActive(true);
					return;
				}
				break;
			case WhackAMole.GameState.PickMoles:
				break;
			case WhackAMole.GameState.TimesUp:
				this.welcomeUI.SetActive(false);
				this.ongoingGameUI.SetActive(false);
				this.ContinuePressedUI.SetActive(false);
				if (this.isMultiplayer)
				{
					this.multiplyareScoresUI.SetActive(true);
				}
				this.levelEndedUI.SetActive(true);
				return;
			default:
				return;
			}
		}

		public static int CreateNewGameID()
		{
			int num = (int)((DateTime.Now - WhackAMole.epoch).TotalSeconds * 8.0 % 2147483646.0) + 1;
			if (num <= WhackAMole.lastAssignedID)
			{
				WhackAMole.lastAssignedID++;
				return WhackAMole.lastAssignedID;
			}
			WhackAMole.lastAssignedID = num;
			return num;
		}

		private void OnMoleTapped(MoleTypes moleType, Vector3 position, bool isLocalTap, bool isLeftHand)
		{
			WhackAMole.GameState gameState = this.currentState;
			if (gameState == WhackAMole.GameState.Off || gameState == WhackAMole.GameState.TimesUp)
			{
				return;
			}
			AudioClip audioClip = (moleType.isHazard ? this.whackHazardClips[Random.Range(0, this.whackHazardClips.Length)] : this.whackMonkeClips[Random.Range(0, this.whackMonkeClips.Length)]);
			if (moleType.isHazard)
			{
				this.audioSource.GTPlayOneShot(audioClip, 1f);
				this.levelHazardMolesHit++;
			}
			else
			{
				this.audioSource.GTPlayOneShot(audioClip, 1f);
			}
			if (moleType.monkeMoleHitMaterial != null)
			{
				moleType.MeshRenderer.material = moleType.monkeMoleHitMaterial;
			}
			this.currentScore += moleType.scorePoint;
			this.totalScore += moleType.scorePoint;
			if (moleType.IsLeftSideMoleType)
			{
				this.leftPlayerScore += moleType.scorePoint;
			}
			else
			{
				this.rightPlayerScore += moleType.scorePoint;
			}
			this.UpdateScoreUI(this.currentScore, this.leftPlayerScore, this.rightPlayerScore);
			moleType.MoleContainerParent.HideMole(true);
		}

		public void HandleOnTimerStopped()
		{
			this.gameEndedTime = Time.time;
			this.SwitchState(WhackAMole.GameState.TimesUp);
		}

		private IEnumerator PlayHazardAudio(AudioClip clip)
		{
			this.audioSource.clip = clip;
			this.audioSource.GTPlay();
			yield return new WaitForSeconds(this.audioSource.clip.length);
			this.audioSource.clip = this.errorClip;
			this.audioSource.GTPlay();
			yield break;
		}

		private bool PickMoles()
		{
			WhackAMole.<>c__DisplayClass85_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			this.pickedMolesIndex.Clear();
			float passedTime = this.timer.GetPassedTime();
			if (passedTime > this.currentLevel.levelDuration - this.currentLevel.showMoleDuration)
			{
				return true;
			}
			float num = passedTime / this.currentLevel.levelDuration;
			CS$<>8__locals1.minMoleCount = Mathf.Lerp(this.currentLevel.minimumMoleCount.x, this.currentLevel.minimumMoleCount.y, num);
			CS$<>8__locals1.maxMoleCount = Mathf.Lerp(this.currentLevel.maximumMoleCount.x, this.currentLevel.maximumMoleCount.y, num);
			this.curentTime = Time.time;
			CS$<>8__locals1.hazardMoleChance = Mathf.Lerp(this.currentLevel.hazardMoleChance.x, this.currentLevel.hazardMoleChance.y, num);
			if (this.isMultiplayer)
			{
				this.<PickMoles>g__PickMolesFrom|85_0(this.rightMolesList, ref CS$<>8__locals1);
				this.<PickMoles>g__PickMolesFrom|85_0(this.leftMolesList, ref CS$<>8__locals1);
			}
			else
			{
				this.<PickMoles>g__PickMolesFrom|85_0(this.molesList, ref CS$<>8__locals1);
			}
			return this.pickedMolesIndex.Count != 0;
		}

		private void LoadNextLevel()
		{
			if (this.currentLevel != null)
			{
				this.resetToFirstLevel = this.currentScore < this.currentLevel.GetMinScore(this.isMultiplayer);
				if (this.resetToFirstLevel)
				{
					this.currentLevelIndex = 0;
				}
				else
				{
					this.currentLevelIndex++;
				}
				if (this.currentLevelIndex >= this.allLevels.Length)
				{
					this.currentLevelIndex = 0;
				}
			}
			else
			{
				this.currentLevelIndex++;
			}
			this.currentLevel = this.allLevels[this.currentLevelIndex];
			this.timer.SetTimerDuration(this.currentLevel.levelDuration);
			this.timer.RestartTimer();
			this.curentTime = Time.time;
			this.currentScore = 0;
			this.leftPlayerScore = 0;
			this.rightPlayerScore = 0;
			this.levelGoodMolesPicked = (this.levelHazardMolesPicked = 0);
			this.levelHazardMolesHit = 0;
			if (this.currentLevelIndex == 0)
			{
				this.totalScore = 0;
			}
			if (this.currentLevelIndex == 0 && base.IsMine)
			{
				this.gameId = WhackAMole.CreateNewGameID();
				Debug.LogWarning("GAME ID" + this.gameId.ToString());
			}
		}

		private bool PickSingleMole(int randomMoleIndex, float hazardMoleChance)
		{
			bool flag = hazardMoleChance > 0f && Random.value <= hazardMoleChance;
			int moleTypeIndex = this.molesList[randomMoleIndex].GetMoleTypeIndex(flag);
			this.molesList[randomMoleIndex].ShowMole(this.currentLevel.showMoleDuration, moleTypeIndex);
			this.pickedMolesIndex.Add(randomMoleIndex, moleTypeIndex);
			if (flag)
			{
				this.levelHazardMolesPicked++;
			}
			else
			{
				this.levelGoodMolesPicked++;
			}
			return flag;
		}

		private void ResetGame()
		{
			foreach (Mole mole in this.molesList)
			{
				mole.ResetPosition();
			}
		}

		private void UpdateScoreUI(int totalScore, int _leftPlayerScore, int _rightPlayerScore)
		{
			if (this.currentLevel != null)
			{
				this.scoreText.text = string.Format("SCORE\n{0}/{1}", totalScore, this.currentLevel.GetMinScore(this.isMultiplayer));
				this.leftPlayerScoreText.text = _leftPlayerScore.ToString();
				this.rightPlayerScoreText.text = _rightPlayerScore.ToString();
			}
		}

		private void UpdateLevelUI(int levelNumber)
		{
			this.arrowTargetRotation = Quaternion.Euler(0f, 0f, (float)(18 * (levelNumber - 1)));
			this.arrowRotationNeedsUpdate = true;
		}

		private void UpdateArrowRotation()
		{
			Quaternion quaternion = Quaternion.Slerp(this.levelArrow.transform.localRotation, this.arrowTargetRotation, Time.deltaTime * 5f);
			if (Quaternion.Angle(quaternion, this.arrowTargetRotation) < 0.1f)
			{
				quaternion = this.arrowTargetRotation;
				this.arrowRotationNeedsUpdate = false;
			}
			this.levelArrow.transform.localRotation = quaternion;
		}

		private void UpdateTimerUI(int time)
		{
			if (time == this.previousTime)
			{
				return;
			}
			this.timeText.text = "TIME " + time.ToString();
			this.previousTime = time;
		}

		private void UpdateResultUI(WhackAMole.GameResult gameResult)
		{
			if (gameResult == WhackAMole.GameResult.LevelComplete)
			{
				this.resultText.text = "LEVEL COMPLETE";
				return;
			}
			if (gameResult == WhackAMole.GameResult.Win)
			{
				this.resultText.text = "YOU WIN!";
				return;
			}
			if (gameResult == WhackAMole.GameResult.GameOver)
			{
				this.resultText.text = "GAME OVER";
			}
		}

		public void OnStartButtonPressed()
		{
			WhackAMole.GameState gameState = this.currentState;
			if (gameState == WhackAMole.GameState.TimesUp || gameState == WhackAMole.GameState.Off)
			{
				base.GetView.RPC("WhackAMoleButtonPressed", RpcTarget.All, Array.Empty<object>());
			}
		}

		[PunRPC]
		private void WhackAMoleButtonPressed(PhotonMessageInfo info)
		{
			this.WhackAMoleButtonPressedShared(info);
		}

		[Rpc]
		private unsafe void RPC_WhackAMoleButtonPressed(RpcInfo info = default(RpcInfo))
		{
			if (!this.InvokeRpc)
			{
				NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
				if (base.Runner.Stage != SimulationStages.Resimulate)
				{
					int localAuthorityMask = base.Object.GetLocalAuthorityMask();
					if ((localAuthorityMask & 7) == 0)
					{
						NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void GorillaTagScripts.WhackAMole::RPC_WhackAMoleButtonPressed(Fusion.RpcInfo)", base.Object, 7);
					}
					else
					{
						int num = 8;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.WhackAMole::RPC_WhackAMoleButtonPressed(Fusion.RpcInfo)", num);
						}
						else
						{
							if (base.Runner.HasAnyActiveConnections())
							{
								SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
								byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
								*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
								int num2 = 8;
								ptr->Offset = num2 * 8;
								base.Runner.SendRpc(ptr);
							}
							if ((localAuthorityMask & 7) != 0)
							{
								info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
								goto IL_0012;
							}
						}
					}
				}
				return;
			}
			this.InvokeRpc = false;
			IL_0012:
			this.WhackAMoleButtonPressedShared(info);
		}

		private void WhackAMoleButtonPressedShared(PhotonMessageInfoWrapped info)
		{
			GorillaNot.IncrementRPCCall(info, "WhackAMoleButtonPressedShared");
			VRRig vrrig = GorillaGameManager.StaticFindRigForPlayer(info.Sender);
			if (vrrig)
			{
				this.playerName = vrrig.playerNameVisible;
				if (this.currentState == WhackAMole.GameState.Off)
				{
					this.playerId = info.Sender.UserId;
					if (NetworkSystem.Instance.LocalPlayer.UserId == this.playerId)
					{
						PlayerGameEvents.MiscEvent("PlayArcadeGame", 1);
					}
				}
			}
			this.SwitchState(WhackAMole.GameState.ContinuePressed);
		}

		private WhackAMole.GameResult GetGameResult()
		{
			if (this.currentScore < this.currentLevel.GetMinScore(this.isMultiplayer))
			{
				return WhackAMole.GameResult.GameOver;
			}
			if (this.currentLevelIndex >= this.allLevels.Length - 1)
			{
				return WhackAMole.GameResult.Win;
			}
			return WhackAMole.GameResult.LevelComplete;
		}

		public int GetCurrentLevel()
		{
			if (this.currentLevel != null)
			{
				return this.currentLevel.levelNumber;
			}
			return 0;
		}

		public int GetTotalLevelNumbers()
		{
			if (this.allLevels != null)
			{
				return this.allLevels.Length;
			}
			return 0;
		}

		[Networked]
		[NetworkedWeaved(0, 210)]
		public unsafe WhackAMole.WhackAMoleData Data
		{
			get
			{
				if (this.Ptr == null)
				{
					throw new InvalidOperationException("Error when accessing WhackAMole.Data. Networked properties can only be accessed when Spawned() has been called.");
				}
				return *(WhackAMole.WhackAMoleData*)(this.Ptr + 0);
			}
			set
			{
				if (this.Ptr == null)
				{
					throw new InvalidOperationException("Error when accessing WhackAMole.Data. Networked properties can only be accessed when Spawned() has been called.");
				}
				*(WhackAMole.WhackAMoleData*)(this.Ptr + 0) = value;
			}
		}

		public override void WriteDataFusion()
		{
			this.Data = new WhackAMole.WhackAMoleData(this.currentState, this.currentLevelIndex, this.currentScore, this.totalScore, this.bestScore, this.rightPlayerScore, this.highScorePlayerName, this.timer.GetRemainingTime(), this.gameEndedTime, this.gameId, this.pickedMolesIndex);
			this.pickedMolesIndex.Clear();
		}

		public override void ReadDataFusion()
		{
			this.ReadDataShared(this.Data.CurrentState, this.Data.CurrentLevelIndex, this.Data.CurrentScore, this.Data.TotalScore, this.Data.BestScore, this.Data.RightPlayerScore, this.Data.HighScorePlayerName.Value, this.Data.RemainingTime, this.Data.GameEndedTime, this.Data.GameId);
			for (int i = 0; i < this.Data.PickedMolesIndexCount; i++)
			{
				int num = this.Data.PickedMolesIndex[i];
				if (i >= 0 && i < this.molesList.Count && this.currentLevel)
				{
					this.molesList[i].ShowMole(this.currentLevel.showMoleDuration, num);
				}
			}
		}

		protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
		{
		}

		protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
		{
		}

		private void ReadDataShared(WhackAMole.GameState _currentState, int _currentLevelIndex, int cScore, int tScore, int bScore, int rPScore, string hScorePName, float _remainingTime, float endedTime, int _gameId)
		{
			WhackAMole.GameState gameState = this.currentState;
			if (_currentState != gameState)
			{
				this.SwitchState(_currentState);
			}
			this.currentLevelIndex = _currentLevelIndex;
			if (this.currentLevelIndex >= 0 && this.currentLevelIndex < this.allLevels.Length)
			{
				this.currentLevel = this.allLevels[this.currentLevelIndex];
				this.UpdateLevelUI(this.currentLevel.levelNumber);
			}
			this.currentScore = cScore;
			this.totalScore = tScore;
			this.bestScore = bScore;
			this.rightPlayerScore = rPScore;
			this.leftPlayerScore = this.currentScore - this.rightPlayerScore;
			this.highScorePlayerName = hScorePName;
			this.bestScoreText.text = (this.isMultiplayer ? this.bestScore.ToString() : (this.highScorePlayerName + "  " + this.bestScore.ToString()));
			this.remainingTime = _remainingTime;
			if (float.IsFinite(this.remainingTime) && this.currentLevel)
			{
				this.remainingTime = this.remainingTime.ClampSafe(0f, this.currentLevel.levelDuration);
				this.UpdateTimerUI((int)this.remainingTime);
			}
			if (float.IsFinite(endedTime))
			{
				this.gameEndedTime = endedTime.ClampSafe(0f, Time.time);
			}
			this.gameId = _gameId;
		}

		protected override void OnOwnerSwitched(NetPlayer newOwningPlayer)
		{
			base.OnOwnerSwitched(newOwningPlayer);
			if (NetworkSystem.Instance.IsMasterClient)
			{
				this.timer.RestartTimer();
				this.timer.SetTimerDuration(this.remainingTime);
				this.curentTime = Time.time;
				if (this.currentLevelIndex >= 0 && this.currentLevelIndex < this.allLevels.Length)
				{
					this.currentLevel = this.allLevels[this.currentLevelIndex];
				}
				this.SwitchState(this.currentState);
			}
		}

		[CompilerGenerated]
		private void <PickMoles>g__PickMolesFrom|85_0(List<Mole> moles, ref WhackAMole.<>c__DisplayClass85_0 A_2)
		{
			int num = Mathf.RoundToInt(Random.Range(A_2.minMoleCount, A_2.maxMoleCount));
			this.potentialMoles.Clear();
			foreach (Mole mole in moles)
			{
				if (mole.CanPickMole())
				{
					this.potentialMoles.Add(mole);
				}
			}
			int num2 = Mathf.Min(num, this.potentialMoles.Count);
			int num3 = Mathf.CeilToInt((float)num2 * A_2.hazardMoleChance);
			int num4 = 0;
			for (int i = 0; i < num2; i++)
			{
				int num5 = Random.Range(0, this.potentialMoles.Count);
				if (this.PickSingleMole(this.molesList.IndexOf(this.potentialMoles[num5]), (num4 < num3) ? A_2.hazardMoleChance : 0f))
				{
					num4++;
				}
				this.potentialMoles.RemoveAt(num5);
			}
		}

		[WeaverGenerated]
		public override void CopyBackingFieldsToState(bool A_1)
		{
			base.CopyBackingFieldsToState(A_1);
			this.Data = this._Data;
		}

		[WeaverGenerated]
		public override void CopyStateToBackingFields()
		{
			base.CopyStateToBackingFields();
			this._Data = this.Data;
		}

		[NetworkRpcWeavedInvoker(1, 7, 7)]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_WhackAMoleButtonPressed@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
			behaviour.InvokeRpc = true;
			((WhackAMole)behaviour).RPC_WhackAMoleButtonPressed(rpcInfo);
		}

		public string machineId = "default";

		public GameObject molesContainerRight;

		[Tooltip("Only for co-op version")]
		public GameObject molesContainerLeft;

		public int betweenLevelPauseDuration = 3;

		public int countdownDuration = 5;

		public WhackAMoleLevelSO[] allLevels;

		[SerializeField]
		private GorillaTimer timer;

		[SerializeField]
		private AudioSource audioSource;

		public GameObject levelArrow;

		public GameObject victoryFX;

		public ZoneBasedObject[] zoneBasedVisuals;

		[SerializeField]
		private MeshRenderer[] zoneBasedMeshRenderers;

		[Space]
		public AudioClip backgroundLoop;

		public AudioClip errorClip;

		public AudioClip counterClip;

		public AudioClip levelCompleteClip;

		public AudioClip winClip;

		public AudioClip gameOverClip;

		public AudioClip[] whackHazardClips;

		public AudioClip[] whackMonkeClips;

		[Space]
		public GameObject welcomeUI;

		public GameObject ongoingGameUI;

		public GameObject levelEndedUI;

		public GameObject ContinuePressedUI;

		public GameObject multiplyareScoresUI;

		[Space]
		public TextMeshPro scoreText;

		public TextMeshPro bestScoreText;

		[Tooltip("Only for co-op version")]
		public TextMeshPro rightPlayerScoreText;

		[Tooltip("Only for co-op version")]
		public TextMeshPro leftPlayerScoreText;

		public TextMeshPro timeText;

		public TextMeshPro counterText;

		public TextMeshPro resultText;

		public TextMeshPro levelEndedOptionsText;

		public TextMeshPro levelEndedCountdownText;

		public TextMeshPro levelEndedTotalScoreText;

		public TextMeshPro levelEndedCurrentScoreText;

		private List<Mole> rightMolesList;

		private List<Mole> leftMolesList;

		private List<Mole> molesList = new List<Mole>();

		private WhackAMoleLevelSO currentLevel;

		private int currentScore;

		private int totalScore;

		private int leftPlayerScore;

		private int rightPlayerScore;

		private int bestScore;

		private float curentTime;

		private int currentLevelIndex;

		private float continuePressedTime;

		private bool resetToFirstLevel;

		private Quaternion arrowTargetRotation;

		private bool arrowRotationNeedsUpdate;

		private List<Mole> potentialMoles = new List<Mole>();

		private Dictionary<int, int> pickedMolesIndex = new Dictionary<int, int>();

		private WhackAMole.GameState currentState;

		private WhackAMole.GameState lastState;

		private float remainingTime;

		private int previousTime = -1;

		private bool isMultiplayer;

		private float gameEndedTime;

		private WhackAMole.GameResult curentGameResult;

		private string playerName = string.Empty;

		private string highScorePlayerName = string.Empty;

		private ParticleSystem[] victoryParticles;

		private int levelHazardMolesPicked;

		private int levelGoodMolesPicked;

		private string playerId;

		private int gameId;

		private int levelHazardMolesHit;

		private static DateTime epoch = new DateTime(2024, 1, 1);

		private static int lastAssignedID;

		private bool wasMasterClient;

		private bool wasLocalPlayerInZone = true;

		[WeaverGenerated]
		[SerializeField]
		[DefaultForProperty("Data", 0, 210)]
		[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
		private WhackAMole.WhackAMoleData _Data;

		public enum GameState
		{
			Off,
			ContinuePressed,
			Ongoing,
			PickMoles,
			TimesUp,
			LevelStarted
		}

		private enum GameResult
		{
			GameOver,
			Win,
			LevelComplete,
			Unknown
		}

		[NetworkStructWeaved(210)]
		[StructLayout(LayoutKind.Explicit, Size = 840)]
		public struct WhackAMoleData : INetworkStruct
		{
			public WhackAMole.GameState CurrentState { readonly get; set; }

			public int CurrentLevelIndex { readonly get; set; }

			public int CurrentScore { readonly get; set; }

			public int TotalScore { readonly get; set; }

			public int BestScore { readonly get; set; }

			public int RightPlayerScore { readonly get; set; }

			[Networked]
			[NetworkedWeaved(6, 129)]
			public unsafe NetworkString<_128> HighScorePlayerName
			{
				readonly get
				{
					return *(NetworkString<_128>*)Native.ReferenceToPointer<FixedStorage@129>(ref this._HighScorePlayerName);
				}
				set
				{
					*(NetworkString<_128>*)Native.ReferenceToPointer<FixedStorage@129>(ref this._HighScorePlayerName) = value;
				}
			}

			public float RemainingTime { readonly get; set; }

			public float GameEndedTime { readonly get; set; }

			public int GameId { readonly get; set; }

			public int PickedMolesIndexCount { readonly get; set; }

			[Networked]
			[Capacity(10)]
			[NetworkedWeavedDictionary(17, 1, 1, typeof(ElementReaderWriterInt32), typeof(ElementReaderWriterInt32))]
			[NetworkedWeaved(139, 71)]
			public unsafe NetworkDictionary<int, int> PickedMolesIndex
			{
				get
				{
					return new NetworkDictionary<int, int>((int*)Native.ReferenceToPointer<FixedStorage@71>(ref this._PickedMolesIndex), 17, ElementReaderWriterInt32.GetInstance(), ElementReaderWriterInt32.GetInstance());
				}
			}

			public WhackAMoleData(WhackAMole.GameState state, int currentLevelIndex, int cScore, int tScore, int bScore, int rPScore, string hScorePName, float remainingTime, float endedTime, int gameId, Dictionary<int, int> moleIndexs)
			{
				this.CurrentState = state;
				this.CurrentLevelIndex = currentLevelIndex;
				this.CurrentScore = cScore;
				this.TotalScore = tScore;
				this.BestScore = bScore;
				this.RightPlayerScore = rPScore;
				this.HighScorePlayerName = hScorePName;
				this.RemainingTime = remainingTime;
				this.GameEndedTime = endedTime;
				this.GameId = gameId;
				this.PickedMolesIndexCount = moleIndexs.Count;
				foreach (KeyValuePair<int, int> keyValuePair in moleIndexs)
				{
					this.PickedMolesIndex.Set(keyValuePair.Key, keyValuePair.Value);
				}
			}

			[FixedBufferProperty(typeof(NetworkString<_128>), typeof(UnityValueSurrogate@ReaderWriter@Fusion_NetworkString), 0, order = -2147483647)]
			[WeaverGenerated]
			[SerializeField]
			[FieldOffset(24)]
			private FixedStorage@129 _HighScorePlayerName;

			[FixedBufferProperty(typeof(NetworkDictionary<int, int>), typeof(UnityDictionarySurrogate@ElementReaderWriterInt32@ElementReaderWriterInt32), 17, order = -2147483647)]
			[WeaverGenerated]
			[SerializeField]
			[FieldOffset(556)]
			private FixedStorage@71 _PickedMolesIndex;
		}
	}
}
