using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public sealed class GorillaHuntManager : GorillaGameManager
{
	public override GameModeType GameType()
	{
		return GameModeType.HuntDown;
	}

	public override string GameModeName()
	{
		return "HUNTDOWN";
	}

	public override string GameModeNameRoomLabel()
	{
		string text;
		if (!LocalisationManager.TryGetKeyForCurrentLocale("GAME_MODE_HUNT_ROOM_LABEL", out text, "(HUNTDOWN GAME)"))
		{
			Debug.LogError("[LOCALIZATION::GORILLA_GAME_MANAGER] Failed to get key for Game Mode [GAME_MODE_HUNT_ROOM_LABEL]");
		}
		return text;
	}

	public override void AddFusionDataBehaviour(NetworkObject behaviour)
	{
		behaviour.AddBehaviour<HuntGameModeData>();
	}

	public override void StartPlaying()
	{
		base.StartPlaying();
		GorillaTagger.Instance.offlineVRRig.huntComputer.SetActive(true);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			for (int i = 0; i < this.currentHunted.Count; i++)
			{
				this.tempPlayer = this.currentHunted[i];
				if (this.tempPlayer == null || !this.tempPlayer.InRoom())
				{
					this.currentHunted.RemoveAt(i);
					i--;
				}
			}
			for (int i = 0; i < this.currentTarget.Count; i++)
			{
				this.tempPlayer = this.currentTarget[i];
				if (this.tempPlayer == null || !this.tempPlayer.InRoom())
				{
					this.currentTarget.RemoveAt(i);
					i--;
				}
			}
			this.UpdateState();
		}
	}

	public override void StopPlaying()
	{
		base.StopPlaying();
		GorillaTagger.Instance.offlineVRRig.huntComputer.SetActive(false);
		base.StopAllCoroutines();
	}

	public override void ResetGame()
	{
		base.ResetGame();
		this.currentHunted.Clear();
		this.currentTarget.Clear();
		for (int i = 0; i < this.currentHuntedArray.Length; i++)
		{
			this.currentHuntedArray[i] = -1;
			this.currentTargetArray[i] = -1;
		}
		this.huntStarted = false;
		this.waitingToStartNextHuntGame = false;
		this.inStartCountdown = false;
		this.timeHuntGameEnded = 0.0;
		this.countDownTime = 0;
		this.timeLastSlowTagged = 0f;
	}

	public void UpdateState()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (NetworkSystem.Instance.RoomPlayerCount <= 3)
			{
				this.CleanUpHunt();
				this.huntStarted = false;
				this.waitingToStartNextHuntGame = false;
				this.iterator1 = 0;
				while (this.iterator1 < RoomSystem.PlayersInRoom.Count)
				{
					RoomSystem.SendSoundEffectToPlayer(0, 0.25f, RoomSystem.PlayersInRoom[this.iterator1], false);
					this.iterator1++;
				}
				return;
			}
			if (NetworkSystem.Instance.RoomPlayerCount > 3 && !this.huntStarted && !this.waitingToStartNextHuntGame && !this.inStartCountdown)
			{
				Utils.Log("<color=red> there are enough players</color>", this);
				base.StartCoroutine(this.StartHuntCountdown());
				return;
			}
			this.UpdateHuntState();
		}
	}

	public void CleanUpHunt()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.currentHunted.Clear();
			this.currentTarget.Clear();
		}
	}

	public IEnumerator StartHuntCountdown()
	{
		if (NetworkSystem.Instance.IsMasterClient && !this.inStartCountdown)
		{
			this.inStartCountdown = true;
			this.countDownTime = 5;
			this.CleanUpHunt();
			while (this.countDownTime > 0)
			{
				yield return new WaitForSeconds(1f);
				this.countDownTime--;
			}
			this.StartHunt();
		}
		yield return null;
		yield break;
	}

	public void StartHunt()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.huntStarted = true;
			this.waitingToStartNextHuntGame = false;
			this.countDownTime = 0;
			this.inStartCountdown = false;
			this.CleanUpHunt();
			this.iterator1 = 0;
			while (this.iterator1 < NetworkSystem.Instance.AllNetPlayers.Count<NetPlayer>())
			{
				if (this.currentTarget.Count < 10)
				{
					this.currentTarget.Add(NetworkSystem.Instance.AllNetPlayers[this.iterator1]);
					RoomSystem.SendSoundEffectToPlayer(0, 0.25f, NetworkSystem.Instance.AllNetPlayers[this.iterator1], false);
				}
				this.iterator1++;
			}
			this.RandomizePlayerList(ref this.currentTarget);
		}
	}

	public void RandomizePlayerList(ref List<NetPlayer> listToRandomize)
	{
		for (int i = 0; i < listToRandomize.Count - 1; i++)
		{
			this.tempRandIndex = Random.Range(i, listToRandomize.Count);
			this.tempRandPlayer = listToRandomize[i];
			listToRandomize[i] = listToRandomize[this.tempRandIndex];
			listToRandomize[this.tempRandIndex] = this.tempRandPlayer;
		}
	}

	public IEnumerator HuntEnd()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			while ((double)Time.time < this.timeHuntGameEnded + (double)this.tagCoolDown)
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (this.waitingToStartNextHuntGame)
			{
				base.StartCoroutine(this.StartHuntCountdown());
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	public void UpdateHuntState()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.notHuntedCount = 0;
			foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
			{
				if (this.currentTarget.Contains(netPlayer) && !this.currentHunted.Contains(netPlayer))
				{
					this.notHuntedCount++;
				}
			}
			if (this.notHuntedCount <= 2 && this.huntStarted)
			{
				this.EndHuntGame();
			}
		}
	}

	private void EndHuntGame()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
			{
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, netPlayer);
				RoomSystem.SendSoundEffectToPlayer(2, 0.25f, netPlayer, false);
			}
			this.huntStarted = false;
			this.timeHuntGameEnded = (double)Time.time;
			this.waitingToStartNextHuntGame = true;
			base.StartCoroutine(this.HuntEnd());
		}
	}

	public override bool LocalCanTag(NetPlayer myPlayer, NetPlayer otherPlayer)
	{
		if (this.waitingToStartNextHuntGame || this.countDownTime > 0 || GorillaTagger.Instance.currentStatus == GorillaTagger.StatusEffect.Frozen)
		{
			return false;
		}
		if (this.currentHunted.Contains(myPlayer) && !this.currentHunted.Contains(otherPlayer) && Time.time > this.timeLastSlowTagged + 1f)
		{
			this.timeLastSlowTagged = Time.time;
			return true;
		}
		return this.IsTargetOf(myPlayer, otherPlayer);
	}

	public override bool LocalIsTagged(NetPlayer player)
	{
		return !this.waitingToStartNextHuntGame && this.countDownTime <= 0 && this.currentHunted.Contains(player);
	}

	public override void ReportTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		if (NetworkSystem.Instance.IsMasterClient && !this.waitingToStartNextHuntGame)
		{
			if ((this.currentHunted.Contains(taggingPlayer) || !this.currentTarget.Contains(taggingPlayer)) && !this.currentHunted.Contains(taggedPlayer) && this.currentTarget.Contains(taggedPlayer))
			{
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.SetSlowedTime, taggedPlayer);
				RoomSystem.SendSoundEffectOnOther(5, 0.125f, taggedPlayer, false);
				return;
			}
			if (this.IsTargetOf(taggingPlayer, taggedPlayer))
			{
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, taggedPlayer);
				RoomSystem.SendSoundEffectOnOther(0, 0.25f, taggedPlayer, false);
				this.currentHunted.Add(taggedPlayer);
				this.UpdateHuntState();
			}
		}
	}

	public bool IsTargetOf(NetPlayer huntingPlayer, NetPlayer huntedPlayer)
	{
		return !this.currentHunted.Contains(huntingPlayer) && !this.currentHunted.Contains(huntedPlayer) && this.currentTarget.Contains(huntingPlayer) && this.currentTarget.Contains(huntedPlayer) && huntedPlayer == this.GetTargetOf(huntingPlayer);
	}

	public NetPlayer GetTargetOf(NetPlayer netPlayer)
	{
		if (this.currentHunted.Contains(netPlayer) || !this.currentTarget.Contains(netPlayer))
		{
			return null;
		}
		this.tempTargetIndex = this.currentTarget.IndexOf(netPlayer);
		for (int num = (this.tempTargetIndex + 1) % this.currentTarget.Count; num != this.tempTargetIndex; num = (num + 1) % this.currentTarget.Count)
		{
			if (this.currentTarget[num] == netPlayer)
			{
				return null;
			}
			if (!this.currentHunted.Contains(this.currentTarget[num]) && this.currentTarget[num] != null)
			{
				return this.currentTarget[num];
			}
		}
		return null;
	}

	public override void HitPlayer(NetPlayer taggedPlayer)
	{
		if (NetworkSystem.Instance.IsMasterClient && !this.waitingToStartNextHuntGame && !this.currentHunted.Contains(taggedPlayer) && this.currentTarget.Contains(taggedPlayer))
		{
			RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, taggedPlayer);
			RoomSystem.SendSoundEffectOnOther(0, 0.25f, taggedPlayer, false);
			this.currentHunted.Add(taggedPlayer);
			this.UpdateHuntState();
		}
	}

	public override bool CanAffectPlayer(NetPlayer player, bool thisFrame)
	{
		return !this.waitingToStartNextHuntGame && !this.currentHunted.Contains(player) && this.currentTarget.Contains(player);
	}

	public override void OnPlayerEnteredRoom(NetPlayer newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		bool isMasterClient = NetworkSystem.Instance.IsMasterClient;
	}

	public override void NewVRRig(NetPlayer player, int vrrigPhotonViewID, bool didTutorial)
	{
		base.NewVRRig(player, vrrigPhotonViewID, didTutorial);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (!this.waitingToStartNextHuntGame && this.huntStarted)
			{
				this.currentHunted.Add(player);
			}
			this.UpdateState();
		}
	}

	public override void OnPlayerLeftRoom(NetPlayer otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (this.currentTarget.Contains(otherPlayer))
			{
				this.currentTarget.Remove(otherPlayer);
			}
			if (this.currentHunted.Contains(otherPlayer))
			{
				this.currentHunted.Remove(otherPlayer);
			}
			this.UpdateState();
		}
	}

	private void CopyHuntDataListToArray()
	{
		this.copyListToArrayIndex = 0;
		while (this.copyListToArrayIndex < 10)
		{
			this.currentHuntedArray[this.copyListToArrayIndex] = -1;
			this.currentTargetArray[this.copyListToArrayIndex] = -1;
			this.copyListToArrayIndex++;
		}
		this.copyListToArrayIndex = this.currentHunted.Count - 1;
		while (this.copyListToArrayIndex >= 0)
		{
			if (this.currentHunted[this.copyListToArrayIndex] == null)
			{
				this.currentHunted.RemoveAt(this.copyListToArrayIndex);
			}
			this.copyListToArrayIndex--;
		}
		this.copyListToArrayIndex = this.currentTarget.Count - 1;
		while (this.copyListToArrayIndex >= 0)
		{
			if (this.currentTarget[this.copyListToArrayIndex] == null)
			{
				this.currentTarget.RemoveAt(this.copyListToArrayIndex);
			}
			this.copyListToArrayIndex--;
		}
		this.copyListToArrayIndex = 0;
		while (this.copyListToArrayIndex < this.currentHunted.Count)
		{
			this.currentHuntedArray[this.copyListToArrayIndex] = this.currentHunted[this.copyListToArrayIndex].ActorNumber;
			this.copyListToArrayIndex++;
		}
		this.copyListToArrayIndex = 0;
		while (this.copyListToArrayIndex < this.currentTarget.Count)
		{
			this.currentTargetArray[this.copyListToArrayIndex] = this.currentTarget[this.copyListToArrayIndex].ActorNumber;
			this.copyListToArrayIndex++;
		}
	}

	private void CopyHuntDataArrayToList()
	{
		this.currentTarget.Clear();
		this.copyArrayToListIndex = 0;
		while (this.copyArrayToListIndex < this.currentTargetArray.Length)
		{
			if (this.currentTargetArray[this.copyArrayToListIndex] != -1)
			{
				this.tempPlayer = NetworkSystem.Instance.GetPlayer(this.currentTargetArray[this.copyArrayToListIndex]);
				if (this.tempPlayer != null)
				{
					this.currentTarget.Add(this.tempPlayer);
				}
			}
			this.copyArrayToListIndex++;
		}
		this.currentHunted.Clear();
		this.copyArrayToListIndex = 0;
		while (this.copyArrayToListIndex < this.currentHuntedArray.Length)
		{
			if (this.currentHuntedArray[this.copyArrayToListIndex] != -1)
			{
				this.tempPlayer = NetworkSystem.Instance.GetPlayer(this.currentHuntedArray[this.copyArrayToListIndex]);
				if (this.tempPlayer != null)
				{
					this.currentHunted.Add(this.tempPlayer);
				}
			}
			this.copyArrayToListIndex++;
		}
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
		this.waitingToStartNextHuntGame = false;
		this.UpdateHuntState();
	}

	public override void OnSerializeRead(object newData)
	{
		HuntData huntData = (HuntData)newData;
		huntData.currentHuntedArray.CopyTo(this.currentHuntedArray, true);
		huntData.currentTargetArray.CopyTo(this.currentTargetArray, true);
		this.huntStarted = huntData.huntStarted;
		this.waitingToStartNextHuntGame = huntData.waitingToStartNextHuntGame;
		this.countDownTime = huntData.countDownTime;
		this.CopyHuntDataArrayToList();
	}

	public override object OnSerializeWrite()
	{
		this.CopyHuntDataListToArray();
		HuntData huntData = default(HuntData);
		huntData.currentHuntedArray.CopyFrom(this.currentHuntedArray, 0, this.currentHuntedArray.Length);
		huntData.currentTargetArray.CopyFrom(this.currentTargetArray, 0, this.currentTargetArray.Length);
		huntData.huntStarted = this.huntStarted;
		huntData.waitingToStartNextHuntGame = this.waitingToStartNextHuntGame;
		huntData.countDownTime = this.countDownTime;
		return huntData;
	}

	public override void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		this.CopyHuntDataListToArray();
		stream.SendNext(this.currentHuntedArray[0]);
		stream.SendNext(this.currentHuntedArray[1]);
		stream.SendNext(this.currentHuntedArray[2]);
		stream.SendNext(this.currentHuntedArray[3]);
		stream.SendNext(this.currentHuntedArray[4]);
		stream.SendNext(this.currentHuntedArray[5]);
		stream.SendNext(this.currentHuntedArray[6]);
		stream.SendNext(this.currentHuntedArray[7]);
		stream.SendNext(this.currentHuntedArray[8]);
		stream.SendNext(this.currentHuntedArray[9]);
		stream.SendNext(this.currentTargetArray[0]);
		stream.SendNext(this.currentTargetArray[1]);
		stream.SendNext(this.currentTargetArray[2]);
		stream.SendNext(this.currentTargetArray[3]);
		stream.SendNext(this.currentTargetArray[4]);
		stream.SendNext(this.currentTargetArray[5]);
		stream.SendNext(this.currentTargetArray[6]);
		stream.SendNext(this.currentTargetArray[7]);
		stream.SendNext(this.currentTargetArray[8]);
		stream.SendNext(this.currentTargetArray[9]);
		stream.SendNext(this.huntStarted);
		stream.SendNext(this.waitingToStartNextHuntGame);
		stream.SendNext(this.countDownTime);
	}

	public override void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		this.currentHuntedArray[0] = (int)stream.ReceiveNext();
		this.currentHuntedArray[1] = (int)stream.ReceiveNext();
		this.currentHuntedArray[2] = (int)stream.ReceiveNext();
		this.currentHuntedArray[3] = (int)stream.ReceiveNext();
		this.currentHuntedArray[4] = (int)stream.ReceiveNext();
		this.currentHuntedArray[5] = (int)stream.ReceiveNext();
		this.currentHuntedArray[6] = (int)stream.ReceiveNext();
		this.currentHuntedArray[7] = (int)stream.ReceiveNext();
		this.currentHuntedArray[8] = (int)stream.ReceiveNext();
		this.currentHuntedArray[9] = (int)stream.ReceiveNext();
		this.currentTargetArray[0] = (int)stream.ReceiveNext();
		this.currentTargetArray[1] = (int)stream.ReceiveNext();
		this.currentTargetArray[2] = (int)stream.ReceiveNext();
		this.currentTargetArray[3] = (int)stream.ReceiveNext();
		this.currentTargetArray[4] = (int)stream.ReceiveNext();
		this.currentTargetArray[5] = (int)stream.ReceiveNext();
		this.currentTargetArray[6] = (int)stream.ReceiveNext();
		this.currentTargetArray[7] = (int)stream.ReceiveNext();
		this.currentTargetArray[8] = (int)stream.ReceiveNext();
		this.currentTargetArray[9] = (int)stream.ReceiveNext();
		this.huntStarted = (bool)stream.ReceiveNext();
		this.waitingToStartNextHuntGame = (bool)stream.ReceiveNext();
		this.countDownTime = (int)stream.ReceiveNext();
		this.CopyHuntDataArrayToList();
	}

	public override int MyMatIndex(NetPlayer forPlayer)
	{
		NetPlayer targetOf = this.GetTargetOf(forPlayer);
		if (this.currentHunted.Contains(forPlayer) || (this.huntStarted && targetOf == null))
		{
			return 3;
		}
		return 0;
	}

	public override float[] LocalPlayerSpeed()
	{
		if (this.currentHunted.Contains(NetworkSystem.Instance.LocalPlayer) || (this.huntStarted && this.GetTargetOf(NetworkSystem.Instance.LocalPlayer) == null))
		{
			return new float[] { 8.5f, 1.3f };
		}
		if (GorillaTagger.Instance.currentStatus == GorillaTagger.StatusEffect.Slowed)
		{
			return new float[] { 5.5f, 0.9f };
		}
		return new float[] { 6.5f, 1.1f };
	}

	public override void InfrequentUpdate()
	{
		base.InfrequentUpdate();
	}

	public float tagCoolDown = 5f;

	public int[] currentHuntedArray = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

	public List<NetPlayer> currentHunted = new List<NetPlayer>(10);

	public int[] currentTargetArray = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

	public List<NetPlayer> currentTarget = new List<NetPlayer>(10);

	public bool huntStarted;

	public bool waitingToStartNextHuntGame;

	public bool inStartCountdown;

	public int countDownTime;

	public double timeHuntGameEnded;

	public float timeLastSlowTagged;

	public object objRef;

	private int iterator1;

	private NetPlayer tempRandPlayer;

	private int tempRandIndex;

	private int notHuntedCount;

	private int tempTargetIndex;

	private NetPlayer tempPlayer;

	private int copyListToArrayIndex;

	private int copyArrayToListIndex;
}
