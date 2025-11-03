using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GorillaTagManager : GorillaGameManager
{
	public override void Awake()
	{
		base.Awake();
		this.currentInfectedArray = new int[10];
		for (int i = 0; i < this.currentInfectedArray.Length; i++)
		{
			this.currentInfectedArray[i] = -1;
		}
	}

	public override void StartPlaying()
	{
		base.StartPlaying();
		if (NetworkSystem.Instance.IsMasterClient)
		{
			for (int i = 0; i < this.currentInfected.Count; i++)
			{
				this.tempPlayer = this.currentInfected[i];
				if (this.tempPlayer == null || !this.tempPlayer.InRoom())
				{
					this.currentInfected.RemoveAt(i);
					i--;
				}
			}
			if (this.currentIt != null && !this.currentIt.InRoom())
			{
				this.currentIt = null;
			}
			if (this.lastInfectedPlayer != null && !this.lastInfectedPlayer.InRoom())
			{
				this.lastInfectedPlayer = null;
			}
			this.UpdateState();
		}
	}

	public override void StopPlaying()
	{
		base.StopPlaying();
		base.StopAllCoroutines();
		this.lastTaggedActorNr.Clear();
	}

	public override void ResetGame()
	{
		base.ResetGame();
		for (int i = 0; i < this.currentInfectedArray.Length; i++)
		{
			this.currentInfectedArray[i] = -1;
		}
		this.currentInfected.Clear();
		this.lastTag = 0.0;
		this.timeInfectedGameEnded = 0.0;
		this.allInfected = false;
		this.isCurrentlyTag = false;
		this.waitingToStartNextInfectionGame = false;
		this.currentIt = null;
		this.lastInfectedPlayer = null;
	}

	public virtual void UpdateState()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (global::GorillaGameModes.GameMode.ParticipatingPlayers.Count < 1)
			{
				this.isCurrentlyTag = true;
				this.ClearInfectionState();
				this.lastInfectedPlayer = null;
				this.currentIt = null;
				return;
			}
			if (this.isCurrentlyTag && this.currentIt == null)
			{
				int num = Random.Range(0, global::GorillaGameModes.GameMode.ParticipatingPlayers.Count);
				this.ChangeCurrentIt(global::GorillaGameModes.GameMode.ParticipatingPlayers[num], false);
				return;
			}
			if (this.isCurrentlyTag && global::GorillaGameModes.GameMode.ParticipatingPlayers.Count >= this.infectedModeThreshold)
			{
				this.SetisCurrentlyTag(false);
				this.ClearInfectionState();
				int num2 = Random.Range(0, global::GorillaGameModes.GameMode.ParticipatingPlayers.Count);
				this.AddInfectedPlayer(global::GorillaGameModes.GameMode.ParticipatingPlayers[num2], true);
				this.lastInfectedPlayer = global::GorillaGameModes.GameMode.ParticipatingPlayers[num2];
				return;
			}
			if (!this.isCurrentlyTag && global::GorillaGameModes.GameMode.ParticipatingPlayers.Count < this.infectedModeThreshold)
			{
				this.ClearInfectionState();
				this.lastInfectedPlayer = null;
				this.SetisCurrentlyTag(true);
				int num3 = Random.Range(0, global::GorillaGameModes.GameMode.ParticipatingPlayers.Count);
				this.ChangeCurrentIt(global::GorillaGameModes.GameMode.ParticipatingPlayers[num3], false);
				return;
			}
			if (!this.isCurrentlyTag && this.currentInfected.Count == 0)
			{
				int num4 = Random.Range(0, global::GorillaGameModes.GameMode.ParticipatingPlayers.Count);
				this.AddInfectedPlayer(global::GorillaGameModes.GameMode.ParticipatingPlayers[num4], true);
				return;
			}
			if (!this.isCurrentlyTag)
			{
				this.UpdateInfectionState();
			}
		}
	}

	public override void InfrequentUpdate()
	{
		base.InfrequentUpdate();
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.UpdateState();
		}
		this.inspectorLocalPlayerSpeed = this.LocalPlayerSpeed();
	}

	protected virtual IEnumerator InfectionRoundEndingCoroutine()
	{
		while ((double)Time.time < this.timeInfectedGameEnded + (double)this.tagCoolDown)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!this.isCurrentlyTag && this.waitingToStartNextInfectionGame)
		{
			this.InfectionRoundStart();
		}
		yield return null;
		yield break;
	}

	protected virtual void InfectionRoundStart()
	{
		this.ClearInfectionState();
		global::GorillaGameModes.GameMode.RefreshPlayers();
		List<NetPlayer> participatingPlayers = global::GorillaGameModes.GameMode.ParticipatingPlayers;
		if (participatingPlayers.Count > 0)
		{
			int num = Random.Range(0, participatingPlayers.Count);
			int num2 = 0;
			while (num2 < 10 && participatingPlayers[num] == this.lastInfectedPlayer)
			{
				num = Random.Range(0, participatingPlayers.Count);
				num2++;
			}
			this.AddInfectedPlayer(participatingPlayers[num], true);
			this.lastInfectedPlayer = participatingPlayers[num];
			this.lastTag = (double)Time.time;
		}
	}

	public virtual void UpdateInfectionState()
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		this.allInfected = true;
		foreach (NetPlayer netPlayer in global::GorillaGameModes.GameMode.ParticipatingPlayers)
		{
			if (!this.currentInfected.Contains(netPlayer))
			{
				this.allInfected = false;
				break;
			}
		}
		if (!this.isCurrentlyTag && !this.waitingToStartNextInfectionGame && this.allInfected)
		{
			this.InfectionRoundEnd();
		}
	}

	public void UpdateTagState(bool withTagFreeze = true)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		foreach (NetPlayer netPlayer in global::GorillaGameModes.GameMode.ParticipatingPlayers)
		{
			if (this.currentIt == netPlayer)
			{
				if (withTagFreeze)
				{
					RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, netPlayer);
				}
				else
				{
					RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.JoinedTaggedTime, netPlayer);
				}
				RoomSystem.SendSoundEffectOnOther(0, 0.25f, netPlayer, false);
				break;
			}
		}
	}

	protected virtual void InfectionRoundEnd()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			foreach (NetPlayer netPlayer in global::GorillaGameModes.GameMode.ParticipatingPlayers)
			{
				RoomSystem.SendSoundEffectToPlayer(2, 0.25f, netPlayer, true);
			}
			PlayerGameEvents.GameModeCompleteRound();
			global::GorillaGameModes.GameMode.BroadcastRoundComplete();
			this.lastTaggedActorNr.Clear();
			this.waitingToStartNextInfectionGame = true;
			this.timeInfectedGameEnded = (double)Time.time;
			base.StartCoroutine(this.InfectionRoundEndingCoroutine());
		}
	}

	public override bool LocalCanTag(NetPlayer myPlayer, NetPlayer otherPlayer)
	{
		if (this.isCurrentlyTag)
		{
			return myPlayer == this.currentIt && myPlayer != otherPlayer;
		}
		return this.currentInfected.Contains(myPlayer) && !this.currentInfected.Contains(otherPlayer);
	}

	public override bool LocalIsTagged(NetPlayer player)
	{
		if (this.isCurrentlyTag)
		{
			return this.currentIt == player;
		}
		return this.currentInfected.Contains(player);
	}

	public override void LocalTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer, bool bodyHit, bool leftHand)
	{
		if (this.LocalCanTag(NetworkSystem.Instance.LocalPlayer, taggedPlayer) && (double)Time.time > this.lastQuestTagTime + (double)this.tagCoolDown)
		{
			PlayerGameEvents.MiscEvent("GameModeTag", 1);
			this.lastQuestTagTime = (double)Time.time;
			if (!this.isCurrentlyTag)
			{
				PlayerGameEvents.GameModeObjectiveTriggered();
			}
		}
	}

	protected float InterpolatedInfectedJumpMultiplier(int infectedCount)
	{
		if (global::GorillaGameModes.GameMode.ParticipatingPlayers.Count < 2)
		{
			return this.fastJumpMultiplier;
		}
		return (this.fastJumpMultiplier - this.slowJumpMultiplier) / (float)(global::GorillaGameModes.GameMode.ParticipatingPlayers.Count - 1) * (float)(global::GorillaGameModes.GameMode.ParticipatingPlayers.Count - infectedCount) + this.slowJumpMultiplier;
	}

	protected float InterpolatedInfectedJumpSpeed(int infectedCount)
	{
		if (global::GorillaGameModes.GameMode.ParticipatingPlayers.Count < 2)
		{
			return this.fastJumpLimit;
		}
		return (this.fastJumpLimit - this.slowJumpLimit) / (float)(global::GorillaGameModes.GameMode.ParticipatingPlayers.Count - 1) * (float)(global::GorillaGameModes.GameMode.ParticipatingPlayers.Count - infectedCount) + this.slowJumpLimit;
	}

	protected float InterpolatedNoobJumpMultiplier(int infectedCount)
	{
		if (global::GorillaGameModes.GameMode.ParticipatingPlayers.Count < 2)
		{
			return this.slowJumpMultiplier;
		}
		return (this.fastJumpMultiplier - this.slowJumpMultiplier) / (float)(global::GorillaGameModes.GameMode.ParticipatingPlayers.Count - 1) * (float)(infectedCount - 1) * 0.9f + this.slowJumpMultiplier;
	}

	protected float InterpolatedNoobJumpSpeed(int infectedCount)
	{
		if (global::GorillaGameModes.GameMode.ParticipatingPlayers.Count < 2)
		{
			return this.slowJumpLimit;
		}
		return (this.fastJumpLimit - this.fastJumpLimit) / (float)(global::GorillaGameModes.GameMode.ParticipatingPlayers.Count - 1) * (float)(infectedCount - 1) * 0.9f + this.slowJumpLimit;
	}

	public override void ReportTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.taggingRig = this.FindPlayerVRRig(taggingPlayer);
			this.taggedRig = this.FindPlayerVRRig(taggedPlayer);
			if (this.taggingRig == null || this.taggedRig == null)
			{
				return;
			}
			this.taggedRig.SetTaggedBy(this.taggingRig);
			if (this.isCurrentlyTag)
			{
				if (taggingPlayer == this.currentIt && taggingPlayer != taggedPlayer && (double)Time.time > this.lastTag + (double)this.tagCoolDown)
				{
					base.AddLastTagged(taggedPlayer, taggingPlayer);
					this.ChangeCurrentIt(taggedPlayer, true);
					this.lastTag = (double)Time.time;
					this.HandleTagBroadcast(taggedPlayer, taggingPlayer);
					global::GorillaGameModes.GameMode.BroadcastTag(taggedPlayer, taggingPlayer);
					return;
				}
			}
			else if (this.currentInfected.Contains(taggingPlayer) && !this.currentInfected.Contains(taggedPlayer) && (double)Time.time > this.lastTag + (double)this.tagCoolDown)
			{
				if (!this.taggingRig.IsPositionInRange(this.taggedRig.transform.position, 6f) && !this.taggingRig.CheckTagDistanceRollback(this.taggedRig, 6f, 0.2f))
				{
					GorillaNot.instance.SendReport("extremely far tag", taggingPlayer.UserId, taggingPlayer.NickName);
					return;
				}
				this.HandleTagBroadcast(taggedPlayer, taggingPlayer);
				global::GorillaGameModes.GameMode.BroadcastTag(taggedPlayer, taggingPlayer);
				base.AddLastTagged(taggedPlayer, taggingPlayer);
				this.AddInfectedPlayer(taggedPlayer, true);
				int count = this.currentInfected.Count;
			}
		}
	}

	public override void HitPlayer(NetPlayer taggedPlayer)
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.taggedRig = this.FindPlayerVRRig(taggedPlayer);
			if (this.taggedRig == null || this.waitingToStartNextInfectionGame || (double)Time.time < this.timeInfectedGameEnded + (double)(2f * this.tagCoolDown))
			{
				return;
			}
			if (this.isCurrentlyTag)
			{
				base.AddLastTagged(taggedPlayer, taggedPlayer);
				this.ChangeCurrentIt(taggedPlayer, false);
				return;
			}
			if (!this.currentInfected.Contains(taggedPlayer))
			{
				base.AddLastTagged(taggedPlayer, taggedPlayer);
				this.AddInfectedPlayer(taggedPlayer, false);
				int count = this.currentInfected.Count;
			}
		}
	}

	public override bool CanAffectPlayer(NetPlayer player, bool thisFrame)
	{
		if (this.isCurrentlyTag)
		{
			return this.currentIt != player && thisFrame;
		}
		return !this.waitingToStartNextInfectionGame && (double)Time.time >= this.timeInfectedGameEnded + (double)(2f * this.tagCoolDown) && !this.currentInfected.Contains(player);
	}

	public bool IsInfected(NetPlayer player)
	{
		if (this.isCurrentlyTag)
		{
			return this.currentIt == player;
		}
		return this.currentInfected.Contains(player);
	}

	public override void NewVRRig(NetPlayer player, int vrrigPhotonViewID, bool didTutorial)
	{
		base.NewVRRig(player, vrrigPhotonViewID, didTutorial);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			bool flag = this.isCurrentlyTag;
			this.UpdateState();
			if (!flag && !this.isCurrentlyTag)
			{
				if (didTutorial)
				{
					this.AddInfectedPlayer(player, false);
				}
				this.UpdateInfectionState();
			}
		}
	}

	public override void OnPlayerLeftRoom(NetPlayer otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			while (this.currentInfected.Contains(otherPlayer))
			{
				this.currentInfected.Remove(otherPlayer);
			}
			if (this.isCurrentlyTag && ((otherPlayer != null && otherPlayer == this.currentIt) || this.currentIt.ActorNumber == otherPlayer.ActorNumber))
			{
				if (global::GorillaGameModes.GameMode.ParticipatingPlayers.Count > 0)
				{
					int num = Random.Range(0, global::GorillaGameModes.GameMode.ParticipatingPlayers.Count);
					this.ChangeCurrentIt(global::GorillaGameModes.GameMode.ParticipatingPlayers[num], false);
				}
			}
			else if (!this.isCurrentlyTag && global::GorillaGameModes.GameMode.ParticipatingPlayers.Count >= this.infectedModeThreshold)
			{
				this.UpdateInfectionState();
			}
			this.UpdateState();
		}
	}

	private void CopyInfectedListToArray()
	{
		this.iterator1 = 0;
		while (this.iterator1 < this.currentInfectedArray.Length)
		{
			this.currentInfectedArray[this.iterator1] = -1;
			this.iterator1++;
		}
		this.iterator1 = this.currentInfected.Count - 1;
		while (this.iterator1 >= 0)
		{
			if (this.currentInfected[this.iterator1] == null)
			{
				this.currentInfected.RemoveAt(this.iterator1);
			}
			this.iterator1--;
		}
		this.iterator1 = 0;
		while (this.iterator1 < this.currentInfected.Count)
		{
			this.currentInfectedArray[this.iterator1] = this.currentInfected[this.iterator1].ActorNumber;
			this.iterator1++;
		}
	}

	private void CopyInfectedArrayToList()
	{
		this.currentInfected.Clear();
		this.iterator1 = 0;
		while (this.iterator1 < this.currentInfectedArray.Length)
		{
			if (this.currentInfectedArray[this.iterator1] != -1)
			{
				this.tempPlayer = NetworkSystem.Instance.GetPlayer(this.currentInfectedArray[this.iterator1]);
				if (this.tempPlayer != null)
				{
					this.currentInfected.Add(this.tempPlayer);
				}
			}
			this.iterator1++;
		}
	}

	protected virtual void ChangeCurrentIt(NetPlayer newCurrentIt, bool withTagFreeze = true)
	{
		this.lastTag = (double)Time.time;
		this.currentIt = newCurrentIt;
		this.UpdateTagState(withTagFreeze);
	}

	public void SetisCurrentlyTag(bool newTagSetting)
	{
		if (newTagSetting)
		{
			this.isCurrentlyTag = true;
		}
		else
		{
			this.isCurrentlyTag = false;
		}
		RoomSystem.SendSoundEffectAll(2, 0.25f, false);
	}

	public virtual void AddInfectedPlayer(NetPlayer infectedPlayer, bool withTagStop = true)
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.currentInfected.Add(infectedPlayer);
			if (!withTagStop)
			{
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.JoinedTaggedTime, infectedPlayer);
			}
			else
			{
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, infectedPlayer);
			}
			RoomSystem.SendSoundEffectOnOther(0, 0.25f, infectedPlayer, false);
			this.UpdateInfectionState();
		}
	}

	public void ClearInfectionState()
	{
		this.currentInfected.Clear();
		this.waitingToStartNextInfectionGame = false;
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		base.OnMasterClientSwitched(newMasterClient);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.CopyRoomDataToLocalData();
			this.UpdateState();
		}
	}

	public void CopyRoomDataToLocalData()
	{
		this.lastTag = 0.0;
		this.timeInfectedGameEnded = 0.0;
		this.waitingToStartNextInfectionGame = false;
		if (this.isCurrentlyTag)
		{
			this.UpdateTagState(true);
			return;
		}
		this.UpdateInfectionState();
	}

	public override void OnSerializeRead(object newData)
	{
		TagData tagData = (TagData)newData;
		this.isCurrentlyTag = tagData.isCurrentlyTag;
		this.tempItInt = tagData.currentItID;
		this.currentIt = ((this.tempItInt != -1) ? NetworkSystem.Instance.GetPlayer(this.tempItInt) : null);
		tagData.infectedPlayerList.CopyTo(this.currentInfectedArray, true);
		this.CopyInfectedArrayToList();
	}

	public override object OnSerializeWrite()
	{
		this.CopyInfectedListToArray();
		TagData tagData = default(TagData);
		tagData.isCurrentlyTag = this.isCurrentlyTag;
		tagData.currentItID = ((this.currentIt != null) ? this.currentIt.ActorNumber : (-1));
		tagData.infectedPlayerList.CopyFrom(this.currentInfectedArray, 0, this.currentInfectedArray.Length);
		return tagData;
	}

	public override void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		this.CopyInfectedListToArray();
		stream.SendNext(this.isCurrentlyTag);
		stream.SendNext((this.currentIt != null) ? this.currentIt.ActorNumber : (-1));
		stream.SendNext(this.currentInfectedArray[0]);
		stream.SendNext(this.currentInfectedArray[1]);
		stream.SendNext(this.currentInfectedArray[2]);
		stream.SendNext(this.currentInfectedArray[3]);
		stream.SendNext(this.currentInfectedArray[4]);
		stream.SendNext(this.currentInfectedArray[5]);
		stream.SendNext(this.currentInfectedArray[6]);
		stream.SendNext(this.currentInfectedArray[7]);
		stream.SendNext(this.currentInfectedArray[8]);
		stream.SendNext(this.currentInfectedArray[9]);
		base.WriteLastTagged(stream);
	}

	public override void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		NetworkSystem.Instance.GetPlayer(info.Sender);
		bool flag = this.currentIt == NetworkSystem.Instance.LocalPlayer;
		bool flag2 = this.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer);
		this.isCurrentlyTag = (bool)stream.ReceiveNext();
		this.tempItInt = (int)stream.ReceiveNext();
		this.currentIt = ((this.tempItInt != -1) ? NetworkSystem.Instance.GetPlayer(this.tempItInt) : null);
		this.currentInfectedArray[0] = (int)stream.ReceiveNext();
		this.currentInfectedArray[1] = (int)stream.ReceiveNext();
		this.currentInfectedArray[2] = (int)stream.ReceiveNext();
		this.currentInfectedArray[3] = (int)stream.ReceiveNext();
		this.currentInfectedArray[4] = (int)stream.ReceiveNext();
		this.currentInfectedArray[5] = (int)stream.ReceiveNext();
		this.currentInfectedArray[6] = (int)stream.ReceiveNext();
		this.currentInfectedArray[7] = (int)stream.ReceiveNext();
		this.currentInfectedArray[8] = (int)stream.ReceiveNext();
		this.currentInfectedArray[9] = (int)stream.ReceiveNext();
		base.ReadLastTagged(stream);
		this.CopyInfectedArrayToList();
		if (this.isCurrentlyTag)
		{
			if (!flag && this.currentIt == NetworkSystem.Instance.LocalPlayer)
			{
				this.lastQuestTagTime = (double)Time.time;
				return;
			}
		}
		else if (!flag2 && this.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
		{
			this.lastQuestTagTime = (double)Time.time;
		}
	}

	public override GameModeType GameType()
	{
		return GameModeType.Infection;
	}

	public override string GameModeName()
	{
		return "INFECTION";
	}

	public override string GameModeNameRoomLabel()
	{
		string text;
		if (!LocalisationManager.TryGetKeyForCurrentLocale("GAME_MODE_INFECTION_ROOM_LABEL", out text, "(INFECTION GAME)"))
		{
			Debug.LogError("[LOCALIZATION::GORILLA_GAME_MANAGER] Failed to get key for Game Mode [GAME_MODE_INFECTION_ROOM_LABEL]");
		}
		return text;
	}

	public override void AddFusionDataBehaviour(NetworkObject netObject)
	{
		netObject.AddBehaviour<TagGameModeData>();
	}

	public override int MyMatIndex(NetPlayer forPlayer)
	{
		if (this.isCurrentlyTag && forPlayer == this.currentIt)
		{
			return 1;
		}
		if (this.currentInfected.Contains(forPlayer))
		{
			return 2;
		}
		return 0;
	}

	public override float[] LocalPlayerSpeed()
	{
		if (this.isCurrentlyTag)
		{
			if (NetworkSystem.Instance.LocalPlayer == this.currentIt)
			{
				this.playerSpeed[0] = this.fastJumpLimit;
				this.playerSpeed[1] = this.fastJumpMultiplier;
				return this.playerSpeed;
			}
			this.playerSpeed[0] = this.slowJumpLimit;
			this.playerSpeed[1] = this.slowJumpMultiplier;
			return this.playerSpeed;
		}
		else
		{
			if (this.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
			{
				this.playerSpeed[0] = this.InterpolatedInfectedJumpSpeed(this.currentInfected.Count);
				this.playerSpeed[1] = this.InterpolatedInfectedJumpMultiplier(this.currentInfected.Count);
				return this.playerSpeed;
			}
			this.playerSpeed[0] = this.InterpolatedNoobJumpSpeed(this.currentInfected.Count);
			this.playerSpeed[1] = this.InterpolatedNoobJumpMultiplier(this.currentInfected.Count);
			return this.playerSpeed;
		}
	}

	public new const int k_defaultMatIndex = 0;

	public const int k_itMatIndex = 1;

	public const int k_infectedMatIndex = 2;

	public float tagCoolDown = 5f;

	public int infectedModeThreshold = 4;

	public const byte ReportTagEvent = 1;

	public const byte ReportInfectionTagEvent = 2;

	[NonSerialized]
	public List<NetPlayer> currentInfected = new List<NetPlayer>(10);

	[NonSerialized]
	public int[] currentInfectedArray;

	[NonSerialized]
	public NetPlayer currentIt;

	[NonSerialized]
	public NetPlayer lastInfectedPlayer;

	public double lastTag;

	public double timeInfectedGameEnded;

	public bool waitingToStartNextInfectionGame;

	public bool isCurrentlyTag;

	private int tempItInt;

	private int iterator1;

	private NetPlayer tempPlayer;

	private bool allInfected;

	public float[] inspectorLocalPlayerSpeed;

	private protected VRRig taggingRig;

	private protected VRRig taggedRig;

	private NetPlayer lastTaggedPlayer;

	private double lastQuestTagTime;
}
