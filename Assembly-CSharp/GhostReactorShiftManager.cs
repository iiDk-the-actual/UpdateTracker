using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class GhostReactorShiftManager : MonoBehaviourTick
{
	public int ShiftTotalEarned
	{
		get
		{
			return this.shiftTotalEarned;
		}
	}

	public bool ShiftActive
	{
		get
		{
			return this.shiftStarted;
		}
	}

	public double ShiftStartNetworkTime
	{
		get
		{
			return this.shiftStartNetworkTime;
		}
	}

	public bool LocalPlayerInside
	{
		get
		{
			return this.localPlayerInside;
		}
	}

	public float TotalPlayTime
	{
		get
		{
			return this.totalPlayTime;
		}
	}

	public string ShiftId
	{
		get
		{
			return this.gameIdGuid;
		}
	}

	public void SetShiftId(string shiftId)
	{
		this.gameIdGuid = shiftId;
	}

	public void Init(GhostReactorManager grManager)
	{
		this.grManager = grManager;
		this.SetState(GhostReactorShiftManager.State.WaitingForConnect, true);
		this.depthDisplay.Setup();
	}

	public void RefreshShiftStatsDisplay()
	{
		this.shiftStatsText.text = string.Concat(new string[]
		{
			"\n\n",
			this.shiftStats.GetShiftStat(GRShiftStatType.EnemyDeaths).ToString("D2"),
			"\n",
			this.shiftStats.GetShiftStat(GRShiftStatType.CoresCollected).ToString("D2"),
			"\n",
			this.shiftStats.GetShiftStat(GRShiftStatType.SentientCoresCollected).ToString("D2"),
			"\n",
			this.shiftStats.GetShiftStat(GRShiftStatType.PlayerDeaths).ToString("D2")
		});
		this.depthDisplay.RefreshObjectives();
	}

	public void StartShiftButtonPressed()
	{
		this.RequestShiftStart();
	}

	public void RequestShiftStart()
	{
	}

	public void EndShift()
	{
		this.grManager.RequestShiftEnd();
	}

	public void ClearEntities()
	{
		Debug.LogError("Need to re-implement whatever this was doing");
	}

	public void RefreshShiftTimer()
	{
		if (this.shiftTimerText != null)
		{
			this.shiftTimerText.text = Mathf.FloorToInt(this.shiftDurationMinutes).ToString("D2") + ":00";
		}
	}

	public void UpdateLogoAnimations(List<TMP_Text> frames)
	{
		float num = 300f;
		float num2 = 0.5f;
		double time = PhotonNetwork.Time;
		if (frames.Count < 4)
		{
			return;
		}
		if (this.lastReactorLogoAnimationTime + (double)num < time || time < this.lastReactorLogoAnimationTime)
		{
			this.isPlayingLogoAnimation = true;
			this.lastReactorLogoAnimationTime = time;
		}
		if (this.isPlayingLogoAnimation)
		{
			if (this.lastReactorLogoAnimationTime + (double)num2 < time)
			{
				this.isPlayingLogoAnimation = false;
			}
			float num3 = Mathf.Clamp01((float)(time - this.lastReactorLogoAnimationTime) / num2) * 3.1415925f;
			int num4 = (int)(3.5f - Mathf.Abs(Mathf.Cos(num3) * 3f));
			if (!this.isPlayingLogoAnimation)
			{
				num4 = 0;
			}
			if (this.lastReactorLogoAnimFrame != num4)
			{
				frames[this.lastReactorLogoAnimFrame].gameObject.SetActive(false);
				frames[num4].gameObject.SetActive(true);
				this.lastReactorLogoAnimFrame = num4;
			}
			return;
		}
	}

	public void UpdateReactorDisplayMainShared(float countDownTotal)
	{
		if (this.reactorTextMain == null)
		{
			return;
		}
		double time = PhotonNetwork.Time;
		float num = 0.5f;
		if (this.lastReactorDisplayUpdate < time && this.lastReactorDisplayUpdate + (double)num > time)
		{
			return;
		}
		this.lastReactorDisplayUpdate = time;
		this.cachedStringBuilder.Clear();
		int num2 = Mathf.FloorToInt(countDownTotal / 60f);
		int num3 = Mathf.FloorToInt(countDownTotal % 60f);
		switch (this.state)
		{
		case GhostReactorShiftManager.State.WaitingForShiftStart:
		case GhostReactorShiftManager.State.WaitingForFirstShiftStart:
			this.cachedStringBuilder.AppendLine(string.Format("DEPTH {0}m", this.reactor.GetDepthLevel() * 1000 + 1000));
			this.cachedStringBuilder.AppendLine("STAND BY");
			this.depthDisplay.jumbotronTitle.text = string.Format("<size=1>CURRENT DEPTH</size>\n{0}m", this.reactor.GetDepthLevel() * 1000 + 1000);
			break;
		case GhostReactorShiftManager.State.ShiftActive:
		{
			int shiftStat = this.shiftStats.GetShiftStat(GRShiftStatType.CoresCollected);
			int num4 = this.coresRequiredToDelveDeeper;
			this.depthDisplay.jumbotronTitle.text = string.Format("<size=1>CURRENT DEPTH</size>\n{0}m", this.reactor.GetDepthLevel() * 1000 + 1000);
			this.cachedStringBuilder.AppendLine(string.Format("DEPTH {0}m", this.reactor.GetDepthLevel() * 1000 + 1000));
			this.cachedStringBuilder.AppendLine("ANOMALY COLLAPSE IN " + num2.ToString("D2") + ":" + num3.ToString("D2"));
			if (shiftStat >= num4)
			{
				this.cachedStringBuilder.Append("\nPOWER REQUIREMENTS MET\n");
			}
			else
			{
				this.cachedStringBuilder.Append(string.Format("\nCORES REQUIRED ({0}/{1})\n", shiftStat, num4));
			}
			int num5 = (int)((float)shiftStat / (float)num4 * 30f);
			if (shiftStat > 1 && num5 == 0)
			{
				num5 = 1;
			}
			int num6 = num5 / 3;
			int num7 = num5 - num6 * 3;
			for (int i = 0; i < 10; i++)
			{
				if (i < num6)
				{
					this.cachedStringBuilder.Append("▐█");
				}
				else if (i > num6 || num7 == 0)
				{
					this.cachedStringBuilder.Append(" ░");
				}
				else if (num7 == 1)
				{
					this.cachedStringBuilder.Append("▐░");
				}
				else
				{
					this.cachedStringBuilder.Append("▐▌");
				}
			}
			this.cachedStringBuilder.Append("\n");
			if (shiftStat > 0)
			{
				this.cachedStringBuilder.Append(string.Format("\nTOTAL BONUS EARNED: +⑭{0}", shiftStat * 5));
			}
			break;
		}
		case GhostReactorShiftManager.State.PreparingToDrill:
			this.cachedStringBuilder.AppendLine(string.Format("DEPTH {0}m", this.reactor.GetDepthLevel() * 1000));
			this.cachedStringBuilder.AppendLine("STAND BY");
			this.depthDisplay.jumbotronTitle.text = string.Format("<size=1>CURRENT DEPTH</size>\n{0}m", this.reactor.GetDepthLevel() * 1000 + 1000);
			break;
		case GhostReactorShiftManager.State.Drilling:
		{
			int num8 = (int)((time - this.stateStartTime) / (double)this.GetDrillingDuration() * 1000.0);
			this.cachedStringBuilder.AppendLine(string.Format("DEPTH {0}m", this.reactor.GetDepthLevel() * 1000 + num8));
			this.cachedStringBuilder.AppendLine("DRILLING");
			this.depthDisplay.jumbotronTitle.text = string.Format("<size=1>CURRENT DEPTH</size>\n{0}m", this.reactor.GetDepthLevel() * 1000 + num8);
			break;
		}
		}
		this.reactorTextMain.text = this.cachedStringBuilder.ToString();
	}

	public void OnShiftStarted(string gameId, double shiftStartTime, bool wasPlayerInAtStart, bool isFirstShift)
	{
		this.gameIdGuid = gameId;
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (!this.shiftStarted && grplayer != null)
		{
			float num = (float)(PhotonNetwork.Time - shiftStartTime);
			grplayer.ResetTelemetryTracking(this.gameIdGuid, num);
			grplayer.IncrementShiftsPlayed(1);
			grplayer.SendFloorStartedTelemetry(num, wasPlayerInAtStart, this.reactor.GetDepthLevel(), this.reactor.GetCurrLevelGenConfig().name, "");
			if (grplayer.isFirstShift)
			{
				grplayer.SendGameStartedTelemetry(num, wasPlayerInAtStart, this.reactor.GetDepthLevel());
				grplayer.gameStartTime = (float)PhotonNetwork.Time;
			}
		}
		this.shiftStarted = true;
		this.shiftJustStarted = true;
		this.shiftStartNetworkTime = shiftStartTime;
		this.frontGate.OpenGate();
		this.ringTransform.gameObject.SetActive(false);
		this.anomalyLoop1.Stop();
		this.anomalyLoop2.Stop();
		this.anomalyLoop3.Stop();
		this.anomalyAlert.Stop();
		this.gateBlockerTransform.gameObject.SetActive(false);
		this.prevCountDownTotal = this.shiftDurationMinutes * 60f;
		this.shiftTotalEarned = -1;
		this.authorizedToDelveDeeper = false;
		this.ResetJoinTimes();
		this.reactor.RefreshScoreboards();
		this.reactor.RefreshDepth();
		this.isRoomClosed = false;
		if (grplayer != null)
		{
			grplayer.RefreshPlayerVisuals();
		}
	}

	public void OnShiftEnded(double shiftEndTime, bool isShiftActuallyEnding, ZoneClearReason zoneClearReason = ZoneClearReason.JoinZone)
	{
		if (this.shiftStarted)
		{
			GRPlayer component = VRRig.LocalRig.GetComponent<GRPlayer>();
			if (component != null)
			{
				component.SendFloorEndedTelemetry(isShiftActuallyEnding, (float)this.shiftStartNetworkTime, zoneClearReason, this.reactor.GetDepthLevel(), this.reactor.GetCurrLevelGenConfig().name, "", this.authorizedToDelveDeeper, ((this.reactor.GetDepthLevel() + 1) / 5).ToString(), this.authorizedToDelveDeeper ? (10 * this.reactor.GetDepthLevel()) : 0);
			}
		}
		this.shiftStarted = false;
		this.shiftEndNetworkTime = shiftEndTime;
		this.RefreshShiftTimer();
		this.frontGate.CloseGate();
		this.ringTransform.gameObject.SetActive(false);
		this.anomalyLoop1.Stop();
		this.anomalyLoop2.Stop();
		this.anomalyLoop3.Stop();
		this.anomalyAlert.Stop();
		this.TeleportLocalPlayerIfOutOfBounds();
		if (this.shiftEndNetworkTime > 0.0 && this.shiftStats.GetShiftStat(GRShiftStatType.EnemyDeaths) > this.shiftStats.GetShiftStat(GRShiftStatType.PlayerDeaths))
		{
			PlayerGameEvents.MiscEvent("GRShiftGoodKD", 1);
		}
		if (PhotonNetwork.InRoom && !NetworkSystem.Instance.SessionIsPrivate && this.grManager.IsAuthority())
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("ghostReactorShiftStarted", "false");
			PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable, null, null);
			this.isRoomClosed = false;
		}
	}

	public override void Tick()
	{
		if (this.grManager == null)
		{
			return;
		}
		double num = PhotonNetwork.Time - this.shiftStartNetworkTime;
		float num2 = 60f * this.shiftDurationMinutes - (float)num;
		if (this.grManager.IsAuthority())
		{
			this.AuthorityUpdate(num2);
		}
		num2 = Mathf.Clamp(num2, 0f, 60f * this.shiftDurationMinutes);
		this.SharedUpdate(num2);
		this.prevCountDownTotal = num2;
	}

	private void AuthorityUpdate(float countDownTotal)
	{
		if (PhotonNetwork.InRoom && this.grManager.IsAuthority())
		{
			if (this.shiftStarted && !NetworkSystem.Instance.SessionIsPrivate && !this.isRoomClosed && 60f * this.shiftDurationMinutes - countDownTotal >= this.roomCloseTimeSeconds)
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("ghostReactorShiftStarted", "true");
				PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable, null, null);
				this.isRoomClosed = true;
			}
			if (this.shiftStarted && countDownTotal <= 0f)
			{
				this.grManager.RequestShiftEnd();
			}
			this.UpdateStateAuthority();
		}
	}

	private void SharedUpdate(float countDownTotal)
	{
		this.UpdateStateShared();
		this.UpdateReactorDisplayMainShared(countDownTotal);
		if (this.lastLeaderboardRefreshTime + (double)this.leaderboardUpdateFrequency < (double)Time.time || (double)Time.time < this.lastLeaderboardRefreshTime)
		{
			this.RefreshShiftLeaderboard();
			this.lastLeaderboardRefreshTime = (double)Time.time;
		}
		if (this.shiftStarted)
		{
			if (this.debugFastForwarding)
			{
				float num = this.debugFastForwardRate * Time.deltaTime;
				this.shiftStartNetworkTime -= (double)num;
			}
			int num2 = Mathf.FloorToInt(countDownTotal / 60f);
			int num3 = Mathf.FloorToInt(countDownTotal % 60f);
			this.shiftTimerText.text = num2.ToString("D2") + ":" + num3.ToString("D2");
			for (int i = 0; i < this.warnings.Count; i++)
			{
				if (countDownTotal < (float)this.warnings[i].time && this.prevCountDownTotal >= (float)this.warnings[i].time && !this.shiftJustStarted)
				{
					this.warnings[i].sound.Play(this.announceAudioSource);
					break;
				}
			}
			if (this.state == GhostReactorShiftManager.State.ShiftActive && countDownTotal > 0f && countDownTotal < this.anomalyAlertCountdownTimeToStartPlayingInMinutes * 60f && !this.anomalyAlert.isPlaying)
			{
				this.anomalyAlert.Play();
			}
			if (this.localPlayerInside)
			{
				if (countDownTotal >= 0f && countDownTotal < this.ringClosingDuration * 60f)
				{
					this.ringTransform.gameObject.SetActive(true);
					float num4 = Mathf.Lerp(this.ringClosingMinRadius, this.ringClosingMaxRadius, countDownTotal / (this.ringClosingDuration * 60f));
					this.ringTransform.localScale = new Vector3(num4, 1f, num4);
					Vector3 position = VRRig.LocalRig.bodyTransform.position;
					Vector3 vector = position - this.ringTransform.position;
					vector.y = 0f;
					Vector3 normalized = vector.normalized;
					float num5 = 0.5235988f;
					Vector3 vector2 = this.ringTransform.position + normalized * num4;
					Quaternion quaternion = Quaternion.AngleAxis(num5, Vector3.up);
					Quaternion quaternion2 = Quaternion.AngleAxis(-num5, Vector3.up);
					Vector3 vector3 = this.ringTransform.position + quaternion * normalized * num4;
					Vector3 vector4 = this.ringTransform.position + quaternion2 * normalized * num4;
					vector2.y = position.y;
					vector3.y = position.y;
					vector4.y = position.y;
					this.anomalyLoop1.transform.position = vector2;
					this.anomalyLoop2.transform.position = vector3;
					this.anomalyLoop3.transform.position = vector4;
					if (!this.anomalyLoop1.isPlaying)
					{
						this.anomalyLoop1.Play();
					}
					if (!this.anomalyLoop2.isPlaying)
					{
						this.anomalyLoop2.Play();
					}
					if (!this.anomalyLoop3.isPlaying)
					{
						this.anomalyLoop3.Play();
					}
					if (vector.sqrMagnitude > num4 * num4)
					{
						this.TeleportLocalPlayerIfOutOfBounds();
					}
				}
			}
			else if (this.ringTransform.gameObject.activeSelf)
			{
				this.ringTransform.gameObject.SetActive(false);
			}
			this.shiftJustStarted = false;
			return;
		}
		if (!this.shiftStarted)
		{
			this.TeleportLocalPlayerIfOutOfBounds();
		}
	}

	private void TeleportLocalPlayerIfOutOfBounds()
	{
		if (this.localPlayerInside || (this.localPlayerOverlapping && Vector3.Dot(GTPlayer.Instance.headCollider.transform.position - this.gatePlaneTransform.position, this.gatePlaneTransform.forward) < 0f))
		{
			this.grManager.ReportLocalPlayerHit();
			GRPlayer component = VRRig.LocalRig.GetComponent<GRPlayer>();
			component.ChangePlayerState(GRPlayer.GRPlayerState.Ghost, this.grManager);
			GTPlayer.Instance.TeleportTo(this.playerTeleportTransform, true, true);
			this.localPlayerInside = false;
			this.localPlayerOverlapping = false;
			component.caughtByAnomaly = true;
		}
	}

	public void RevealJudgment(int evaluation)
	{
		if (evaluation <= 0)
		{
			this.shiftJugmentText.text = "DON'T QUIT YOUR DAY JOB.";
			return;
		}
		switch (evaluation)
		{
		case 1:
			this.shiftJugmentText.text = "YOU'RE LEARNING. GOOD.";
			return;
		case 2:
			this.shiftJugmentText.text = "YOU MIGHT EARN A PROMOTION.";
			return;
		case 3:
			this.shiftJugmentText.text = "YOU DID A MANAGER-TIER JOB.";
			return;
		case 4:
			this.shiftJugmentText.text = "NICE. YOU GET EXTRA SHIFTS.";
			return;
		default:
			this.shiftJugmentText.text = "YOU WORK FOR US NOW.";
			if (this.wrongStumpGoo != null)
			{
				this.wrongStumpGoo.SetActive(true);
			}
			return;
		}
	}

	public void ResetJudgment()
	{
		this.shiftJugmentText.text = "";
		if (this.wrongStumpGoo != null)
		{
			this.wrongStumpGoo.SetActive(false);
		}
	}

	public void ResetJoinTimes()
	{
		int count = this.reactor.vrRigs.Count;
		this.totalPlayTime = 0f;
		for (int i = 0; i < count; i++)
		{
			GRPlayer.Get(this.reactor.vrRigs[i]).shiftJoinTime = this.shiftStartNetworkTime;
		}
	}

	public void CalculatePlayerPercentages()
	{
		int count = this.reactor.vrRigs.Count;
		this.totalPlayTime = 0f;
		for (int i = 0; i < count; i++)
		{
			GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
			if (this.reactor.vrRigs[i] != null && grplayer != null)
			{
				if (this.reactor.vrRigs[i].OwningNetPlayer == null)
				{
					grplayer.ShiftPlayTime = 0.1f;
				}
				else if (this.shiftStarted)
				{
					grplayer.ShiftPlayTime = Mathf.Min(this.shiftDurationMinutes * 60f, (float)(PhotonNetwork.Time - grplayer.shiftJoinTime));
				}
				else
				{
					grplayer.ShiftPlayTime = Mathf.Min(this.shiftDurationMinutes * 60f, (float)(this.shiftEndNetworkTime - grplayer.shiftJoinTime));
				}
				this.totalPlayTime += grplayer.ShiftPlayTime;
			}
		}
	}

	public void CalculateShiftTotal()
	{
		this.shiftTotalEarned = 0;
		int count = this.reactor.vrRigs.Count;
		double num = 0.0;
		for (int i = 0; i < count; i++)
		{
			GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
			if (this.reactor.vrRigs[i] != null && grplayer != null)
			{
				this.shiftTotalEarned += grplayer.ShiftCredits;
				if (this.reactor.vrRigs[i].OwningNetPlayer == null)
				{
					grplayer.ShiftPlayTime = 0.1f;
				}
				else
				{
					grplayer.ShiftPlayTime = Mathf.Min(this.shiftDurationMinutes * 60f, (float)(PhotonNetwork.Time - grplayer.shiftJoinTime));
				}
				num += (double)grplayer.ShiftPlayTime;
			}
		}
		this.shiftTotalEarned = Mathf.Clamp(this.shiftTotalEarned, 0, this.shiftSanityMaximumEarned);
		num = (double)Mathf.Clamp((float)num, 0.1f, this.shiftDurationMinutes * 10f * 60f);
		for (int j = 0; j < count; j++)
		{
			GRPlayer grplayer2 = GRPlayer.Get(this.reactor.vrRigs[j]);
			if (this.reactor.vrRigs[j] != null && grplayer2 != null && this.depthDisplay != null)
			{
				int rewardXP = this.depthDisplay.GetRewardXP();
				if (this.authorizedToDelveDeeper)
				{
					grplayer2.LastShiftCut = rewardXP;
					grplayer2.CollectShiftCut();
				}
			}
		}
		this.reactor.RefreshScoreboards();
		this.reactor.promotionBot.Refresh();
		this.reactor.RefreshDepth();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			this.localPlayerOverlapping = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			bool flag = Vector3.Dot(other.transform.position - this.gatePlaneTransform.position, this.gatePlaneTransform.forward) < 0f;
			this.localPlayerInside = flag;
			this.localPlayerOverlapping = false;
		}
	}

	public void OnButtonDelveDeeper()
	{
		if (this.ShiftActive)
		{
			bool flag = this.authorizedToDelveDeeper;
			return;
		}
	}

	public void OnButtonDEBUGResetDepth()
	{
		this.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.DEBUG_ResetDepth);
	}

	public void OnButtonDEBUGDelveDeeper()
	{
		this.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.DEBUG_DelveDeeper);
	}

	public void OnButtonDEBUGDelveShallower()
	{
		this.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.DEBUG_DelveShallower);
	}

	public void RequestState(GhostReactorShiftManager.State newState)
	{
		if (!this.grManager.IsAuthority())
		{
			return;
		}
		this.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.DelveState, (int)newState);
	}

	public void SetState(GhostReactorShiftManager.State newState, bool force = false)
	{
		if (this.state == newState && !force)
		{
			return;
		}
		GhostReactorShiftManager.State state = this.state;
		if (state != GhostReactorShiftManager.State.ReadyForShift)
		{
			if (state == GhostReactorShiftManager.State.Drilling)
			{
				this.reactor.shiftManager.depthDisplay.StopDelveDeeperFX();
			}
		}
		else if (this.startShiftButton != null)
		{
			this.startShiftButton.SetActive(false);
		}
		this.state = newState;
		this.stateStartTime = PhotonNetwork.Time;
		switch (this.state)
		{
		case GhostReactorShiftManager.State.WaitingForShiftStart:
			this.announceBell.Play(this.announceBellAudioSource);
			this.announceTip.Play(this.announceAudioSource);
			goto IL_021F;
		case GhostReactorShiftManager.State.WaitingForFirstShiftStart:
			break;
		case GhostReactorShiftManager.State.ReadyForShift:
			goto IL_021F;
		case GhostReactorShiftManager.State.ShiftActive:
		{
			this.announceStartShift.Play(this.announceAudioSource);
			using (List<VRRig>.Enumerator enumerator = this.reactor.vrRigs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VRRig vrrig = enumerator.Current;
					GRPlayer component = vrrig.GetComponent<GRPlayer>();
					if (component != null)
					{
						component.startingShiftCreditCache = component.ShiftCredits;
					}
				}
				goto IL_021F;
			}
			break;
		}
		case GhostReactorShiftManager.State.PostShift:
			if (this.authorizedToDelveDeeper)
			{
				this.announceCompleteShift.Play(this.announceAudioSource);
				if (!string.IsNullOrEmpty(this.ShiftId))
				{
					ProgressionManager.Instance.EndOfShiftReward(this.ShiftId);
					int count = this.reactor.vrRigs.Count;
					for (int i = 0; i < count; i++)
					{
						GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
						if (grplayer != null)
						{
							grplayer.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.EarnedCredits, (float)this.shiftRewardCredits);
						}
					}
				}
				Debug.LogError("ShiftId is null or empty, skipping reward of end of shift.");
				goto IL_021F;
			}
			this.announceFailShift.Play(this.announceAudioSource);
			goto IL_021F;
		case GhostReactorShiftManager.State.PreparingToDrill:
			this.announcePrepareDrill.Play(this.announceAudioSource);
			goto IL_021F;
		case GhostReactorShiftManager.State.Drilling:
			this.reactor.DelveToNextDepth();
			this.reactor.shiftManager.depthDisplay.StartDelveDeeperFX();
			goto IL_021F;
		default:
			goto IL_021F;
		}
		this.announceBell.Play(this.announceBellAudioSource);
		this.announceTip.Play(this.announceAudioSource);
		IL_021F:
		this.RefreshDepthDisplay();
	}

	public GhostReactorShiftManager.State GetState()
	{
		return this.state;
	}

	public bool IsSoaking()
	{
		return GhostReactorSoak.instance != null && GhostReactorSoak.instance.IsSoaking();
	}

	private int GetPreShiftDuration()
	{
		if (this.IsSoaking())
		{
			return 5;
		}
		return this.preShiftDuration;
	}

	private int GetPreShiftDurationFirstArrive()
	{
		if (this.IsSoaking())
		{
			return 5;
		}
		return this.preShiftDurationFirstArrive;
	}

	private int GetPostShiftDuration()
	{
		if (this.IsSoaking())
		{
			return 5;
		}
		return this.postShiftDuration;
	}

	private int GetPreparingToDrillDuration()
	{
		this.IsSoaking();
		return 5;
	}

	public int GetDrillingDuration()
	{
		if (this.IsSoaking())
		{
			return 5;
		}
		return this.drillDuration;
	}

	private void UpdateStateAuthority()
	{
		if (!this.grManager.IsAuthority())
		{
			return;
		}
		double time = PhotonNetwork.Time;
		switch (this.state)
		{
		case GhostReactorShiftManager.State.WaitingForConnect:
			if (this.reactor.grManager.IsZoneReady())
			{
				this.RequestState(GhostReactorShiftManager.State.WaitingForFirstShiftStart);
				return;
			}
			break;
		case GhostReactorShiftManager.State.WaitingForShiftStart:
			if (time - this.stateStartTime > (double)this.GetPreShiftDuration())
			{
				this.reactor.grManager.RequestShiftStartAuthority(false);
				return;
			}
			break;
		case GhostReactorShiftManager.State.WaitingForFirstShiftStart:
			if (time - this.stateStartTime > (double)this.GetPreShiftDurationFirstArrive())
			{
				this.reactor.grManager.RequestShiftStartAuthority(true);
				return;
			}
			break;
		case GhostReactorShiftManager.State.ReadyForShift:
		case GhostReactorShiftManager.State.ShiftActive:
			break;
		case GhostReactorShiftManager.State.PostShift:
			if (time - this.stateStartTime > (double)this.GetPostShiftDuration())
			{
				if (this.authorizedToDelveDeeper)
				{
					this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.DelveDeeper);
					this.RequestState(GhostReactorShiftManager.State.PreparingToDrill);
					return;
				}
				this.RequestState(GhostReactorShiftManager.State.WaitingForShiftStart);
				return;
			}
			break;
		case GhostReactorShiftManager.State.PreparingToDrill:
			if (time - this.stateStartTime > (double)this.GetPreparingToDrillDuration())
			{
				this.RequestState(GhostReactorShiftManager.State.Drilling);
				return;
			}
			break;
		case GhostReactorShiftManager.State.Drilling:
			if (time - this.stateStartTime > (double)this.GetDrillingDuration())
			{
				this.RequestState(GhostReactorShiftManager.State.WaitingForShiftStart);
			}
			break;
		default:
			return;
		}
	}

	private void UpdateStateShared()
	{
		double time = PhotonNetwork.Time;
		switch (this.state)
		{
		case GhostReactorShiftManager.State.WaitingForShiftStart:
		{
			int num = this.GetPreShiftDuration() - Mathf.FloorToInt((float)(time - this.stateStartTime));
			num = Mathf.Max(0, num);
			this.shiftTimerText.text = ":" + num.ToString("D2");
			return;
		}
		case GhostReactorShiftManager.State.WaitingForFirstShiftStart:
		{
			int num2 = this.GetPreShiftDurationFirstArrive() - Mathf.FloorToInt((float)(time - this.stateStartTime));
			num2 = Mathf.Max(0, num2);
			this.shiftTimerText.text = ":" + num2.ToString("D2");
			return;
		}
		case GhostReactorShiftManager.State.ReadyForShift:
		case GhostReactorShiftManager.State.ShiftActive:
			break;
		case GhostReactorShiftManager.State.PostShift:
		{
			int num3 = this.GetPostShiftDuration() - Mathf.FloorToInt((float)(time - this.stateStartTime));
			num3 = Mathf.Max(0, num3);
			this.shiftTimerText.text = ":" + num3.ToString("D2");
			return;
		}
		case GhostReactorShiftManager.State.PreparingToDrill:
		{
			int num4 = 5 - Mathf.FloorToInt((float)(time - this.stateStartTime));
			num4 = Mathf.Max(0, num4);
			this.shiftTimerText.text = ":" + num4.ToString("D2");
			return;
		}
		case GhostReactorShiftManager.State.Drilling:
		{
			int num5 = this.GetDrillingDuration() - Mathf.FloorToInt((float)(time - this.stateStartTime));
			num5 = Mathf.Max(0, num5);
			this.shiftTimerText.text = ":" + num5.ToString("D2");
			this.UpdateLogoAnimations(this.depthDisplay.logoFrames);
			break;
		}
		default:
			return;
		}
	}

	public void RefreshDepthDisplay()
	{
		GhostReactorLevelGenConfig currLevelGenConfig = this.reactor.GetCurrLevelGenConfig();
		int num = this.reactor.GetDepthLevel() + 1;
		int num2 = num / 4 + 1 + ((num % 5 == 4) ? 2 : 0);
		this.shiftRewardCoresForMothership = currLevelGenConfig.coresRequired + num2;
		this.coresRequiredToDelveDeeper = ((currLevelGenConfig.coresRequired > 0) ? ((int)(this.reactor.difficultyScalingForCurrentFloor * (float)currLevelGenConfig.coresRequired) + num2) : 0);
		this.killsRequiredToDelveDeeper = currLevelGenConfig.minEnemyKills;
		this.shiftRewardCredits = currLevelGenConfig.coresRequired * 5;
		this.sentientCoresRequiredToDelveDeeper = (int)(this.reactor.difficultyScalingForCurrentFloor * (float)currLevelGenConfig.sentientCoresRequired);
		this.shiftDurationMinutes = (float)(currLevelGenConfig.shiftDuration / 60);
		if (this.IsSoaking())
		{
			this.shiftDurationMinutes = (float)Random.Range(1, 3);
		}
		this.maxPlayerDeaths = currLevelGenConfig.maxPlayerDeaths;
		if (this.depthDisplay != null)
		{
			this.depthDisplay.RefreshDisplay();
		}
		this.RefreshShiftTimer();
	}

	public void RefreshShiftLeaderboard()
	{
		if (this.nextRefreshLeaderboardSafety)
		{
			this.RefreshShiftLeaderboard_Safety();
		}
		else
		{
			this.RefreshShiftLeaderboard_Efficiency();
		}
		this.nextRefreshLeaderboardSafety = !this.nextRefreshLeaderboardSafety;
	}

	public void RefreshShiftLeaderboard_Safety()
	{
		if (this.shiftLeaderboardSafety == null)
		{
			return;
		}
		int count = this.reactor.vrRigs.Count;
		this.totalPlayTime = 0f;
		this.leaderboardDisplay.Clear();
		this.leaderboardDisplay.Append("<color=#c0c0c0c0><size=-0.4>SAFETY          GHOSTS   WORKPLACE  TEAM    CHAOS\nREPORT          BANISHED INCIDENTS  ASSISTS EXPOSURE\n----------------------------------------------------</size></color>\n");
		for (int i = 0; i < count; i++)
		{
			GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
			if (!(this.reactor.vrRigs[i] == null) && !(grplayer == null) && !(grplayer.gamePlayer == null))
			{
				string playerNameVisible = grplayer.gamePlayer.rig.playerNameVisible;
				int num = (int)grplayer.synchronizedSessionStats[4];
				int num2 = (int)grplayer.synchronizedSessionStats[5];
				int num3 = (int)grplayer.synchronizedSessionStats[6];
				float num4 = grplayer.synchronizedSessionStats[7];
				int num5 = (int)num4 / 60;
				int num6 = (int)num4 % 60;
				this.leaderboardDisplay.Append((i % 2 == 0) ? "<color=#e0e0ff>" : "<color=#a0a0ff>");
				this.leaderboardDisplay.Append(string.Format("{0,-12}{1,5}{2,7}{3,7}{4,10}", new object[]
				{
					playerNameVisible,
					num2,
					num,
					num3,
					string.Format("{0,3}:{1:00}", num5, num6)
				}));
				this.leaderboardDisplay.Append("</color>\n");
			}
		}
		this.shiftLeaderboardSafety.text = this.leaderboardDisplay.ToString();
	}

	public void RefreshShiftLeaderboard_Efficiency()
	{
		if (this.shiftLeaderboardEfficiency == null)
		{
			return;
		}
		int count = this.reactor.vrRigs.Count;
		this.totalPlayTime = 0f;
		this.leaderboardDisplay.Clear();
		this.leaderboardDisplay.Append("<color=#c0c0c0c0><size=-0.4>KEY PERFORMANCE   CORES   EARNED   SPENT    DISTANCE\nINDICATORS        FOUND   CREDITS  CREDITS  TRAVELED\n----------------------------------------------------</size></color>\n");
		for (int i = 0; i < count; i++)
		{
			GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
			if (!(this.reactor.vrRigs[i] == null) && !(grplayer == null) && !(grplayer.gamePlayer == null))
			{
				string playerNameVisible = grplayer.gamePlayer.rig.playerNameVisible;
				int num = (int)grplayer.synchronizedSessionStats[0];
				int num2 = (int)grplayer.synchronizedSessionStats[1];
				int num3 = (int)grplayer.synchronizedSessionStats[2];
				int num4 = (int)grplayer.synchronizedSessionStats[3];
				this.leaderboardDisplay.Append((i % 2 == 0) ? "<color=#e0e0ff>" : "<color=#a0a0ff>");
				this.leaderboardDisplay.Append(string.Format("{0,-12}{1,6}{2,7}{3,7}{4,8}", new object[] { playerNameVisible, num, num2, num3, num4 }));
				this.leaderboardDisplay.Append("</color>\n");
			}
		}
		this.shiftLeaderboardEfficiency.text = this.leaderboardDisplay.ToString();
	}

	private const string EVENT_GOOD_KD = "GRShiftGoodKD";

	[SerializeField]
	private GhostReactor reactor;

	[SerializeField]
	private GRMetalEnergyGate frontGate;

	[SerializeField]
	private GameObject startShiftButton;

	[SerializeField]
	private TMP_Text shiftTimerText;

	[SerializeField]
	private TMP_Text shiftStatsText;

	[SerializeField]
	private TMP_Text shiftJugmentText;

	[SerializeField]
	private TMP_Text reactorTextMain;

	[SerializeField]
	private GameObject wrongStumpGoo;

	[SerializeField]
	private float shiftDurationMinutes = 20f;

	[SerializeField]
	private Transform playerTeleportTransform;

	[SerializeField]
	private Transform gatePlaneTransform;

	[SerializeField]
	private Transform gateBlockerTransform;

	[SerializeField]
	private AudioSource anomalyLoop1;

	[SerializeField]
	private AudioSource anomalyLoop2;

	[SerializeField]
	private AudioSource anomalyLoop3;

	[SerializeField]
	private AudioSource anomalyAlert;

	[SerializeField]
	private float anomalyAlertCountdownTimeToStartPlayingInMinutes = 3f;

	[SerializeField]
	private float roomCloseTimeSeconds = 60f;

	private bool isRoomClosed;

	[SerializeField]
	private int preShiftDuration = 10;

	private int preShiftDurationFirstArrive = 60;

	private int postShiftDuration = 10;

	[SerializeField]
	public int drillDuration = 50;

	private bool bIsStartingFloorAuthorityOnly;

	[Header("Drill Announcements")]
	[SerializeField]
	private AudioSource announceAudioSource;

	[SerializeField]
	private AudioSource announceBellAudioSource;

	public AbilitySound announcePrepareShift;

	public AbilitySound announceStartShift;

	public AbilitySound announceCompleteShift;

	public AbilitySound announceFailShift;

	public AbilitySound announcePrepareDrill;

	public AbilitySound announceTip;

	public AbilitySound announceBell;

	[Header("Warning")]
	public List<GhostReactorShiftManager.WarningPres> warnings;

	[SerializeField]
	private AudioClip warningAudio;

	[SerializeField]
	[Tooltip("Must be ordered from largest time (first played) to smallest time (last played)")]
	private List<int> warningClipPlayTimes = new List<int>();

	[Header("Ring")]
	[SerializeField]
	private Transform ringTransform;

	[SerializeField]
	private float ringClosingDuration = 3f;

	[SerializeField]
	private float ringClosingMaxRadius = 100f;

	[SerializeField]
	private float ringClosingMinRadius = 7f;

	[Header("Debug")]
	[SerializeField]
	private float debugFastForwardRate = 30f;

	[SerializeField]
	private bool debugFastForwarding;

	private bool shiftStarted;

	private bool shiftJustStarted;

	private double shiftStartNetworkTime;

	private double shiftEndNetworkTime;

	private float prevCountDownTotal;

	[SerializeField]
	private int shiftTotalEarned = -1;

	[SerializeField]
	private int shiftSanityMaximumEarned = 10000;

	public GhostReactorShiftDepthDisplay depthDisplay;

	public bool authorizedToDelveDeeper;

	public int shiftRewardCoresForMothership;

	public int coresRequiredToDelveDeeper;

	public int sentientCoresRequiredToDelveDeeper;

	public List<GREnemyCount> killsRequiredToDelveDeeper;

	public int maxPlayerDeaths;

	public int shiftRewardCredits;

	private bool localPlayerInside;

	private bool localPlayerOverlapping;

	private float totalPlayTime;

	private string gameIdGuid = "";

	public GRShiftStat shiftStats = new GRShiftStat();

	[NonSerialized]
	private GhostReactorManager grManager;

	[SerializeField]
	private TMP_Text shiftLeaderboardEfficiency;

	[SerializeField]
	private TMP_Text shiftLeaderboardSafety;

	private double lastLeaderboardRefreshTime;

	private float leaderboardUpdateFrequency = 0.5f;

	private GhostReactorShiftManager.State state;

	public double stateStartTime;

	private double lastReactorLogoAnimationTime;

	private int lastReactorLogoAnimFrame;

	private bool isPlayingLogoAnimation;

	private double lastReactorDisplayUpdate;

	private StringBuilder cachedStringBuilder = new StringBuilder(256);

	private bool nextRefreshLeaderboardSafety;

	private StringBuilder leaderboardDisplay = new StringBuilder(1024);

	[Serializable]
	public class WarningPres
	{
		public int time;

		public AbilitySound sound;
	}

	public enum State
	{
		WaitingForConnect,
		WaitingForShiftStart,
		WaitingForFirstShiftStart,
		ReadyForShift,
		ShiftActive,
		PostShift,
		PreparingToDrill,
		Drilling
	}
}
