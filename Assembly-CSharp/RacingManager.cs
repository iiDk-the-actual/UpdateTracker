using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RacingManager : NetworkSceneObject, ITickSystemTick
{
	public static RacingManager instance { get; private set; }

	public bool TickRunning { get; set; }

	private void Awake()
	{
		RacingManager.instance = this;
		HashSet<int> hashSet = new HashSet<int>();
		this.races = new RacingManager.Race[this.raceSetups.Length];
		for (int i = 0; i < this.raceSetups.Length; i++)
		{
			this.races[i] = new RacingManager.Race(i, this.raceSetups[i], hashSet, this.photonView);
		}
		RoomSystem.JoinedRoomEvent += new Action(this.OnRoomJoin);
		RoomSystem.PlayerJoinedEvent += new Action<NetPlayer>(this.OnPlayerJoined);
	}

	protected override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		TickSystem<object>.AddTickCallback(this);
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		TickSystem<object>.RemoveTickCallback(this);
		base.OnDisable();
	}

	private void OnRoomJoin()
	{
		for (int i = 0; i < this.races.Length; i++)
		{
			this.races[i].Clear();
		}
	}

	private void OnPlayerJoined(NetPlayer player)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		for (int i = 0; i < this.races.Length; i++)
		{
			this.races[i].SendStateToNewPlayer(player);
		}
	}

	public void RegisterVisual(RaceVisual visual)
	{
		int raceId = visual.raceId;
		if (raceId >= 0 && raceId < this.races.Length)
		{
			this.races[raceId].RegisterVisual(visual);
		}
	}

	public void Button_StartRace(int raceId, int laps)
	{
		if (raceId >= 0 && raceId < this.races.Length)
		{
			this.races[raceId].Button_StartRace(laps);
		}
	}

	[PunRPC]
	private void RequestRaceStart_RPC(int raceId, int laps, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RequestRaceStart_RPC");
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (laps != 1 && laps != 3 && laps != 5)
		{
			return;
		}
		if (raceId >= 0 && raceId < this.races.Length)
		{
			this.races[raceId].Host_RequestRaceStart(laps, info.Sender.ActorNumber);
		}
	}

	[PunRPC]
	private void RaceBeginCountdown_RPC(byte raceId, byte laps, double startTime, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RaceBeginCountdown_RPC");
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		if (laps != 1 && laps != 3 && laps != 5)
		{
			return;
		}
		if (!double.IsFinite(startTime))
		{
			return;
		}
		if (startTime < PhotonNetwork.Time || startTime > PhotonNetwork.Time + 4.0)
		{
			return;
		}
		if (raceId >= 0 && (int)raceId < this.races.Length)
		{
			this.races[(int)raceId].BeginCountdown(startTime, (int)laps);
		}
	}

	[PunRPC]
	private void RaceLockInParticipants_RPC(byte raceId, int[] participantActorNumbers, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RaceLockInParticipants_RPC");
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		if (participantActorNumbers.Length > 10)
		{
			return;
		}
		for (int i = 1; i < participantActorNumbers.Length; i++)
		{
			if (participantActorNumbers[i] <= participantActorNumbers[i - 1])
			{
				return;
			}
		}
		if (raceId >= 0 && (int)raceId < this.races.Length)
		{
			this.races[(int)raceId].LockInParticipants(participantActorNumbers, false);
		}
	}

	public void OnCheckpointPassed(int raceId, int checkpointIndex)
	{
		this.photonView.RPC("PassCheckpoint_RPC", RpcTarget.All, new object[]
		{
			(byte)raceId,
			(byte)checkpointIndex
		});
	}

	[PunRPC]
	private void PassCheckpoint_RPC(byte raceId, byte checkpointIndex, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "PassCheckpoint_RPC");
		if (raceId >= 0 && (int)raceId < this.races.Length)
		{
			this.races[(int)raceId].PassCheckpoint(info.Sender, (int)checkpointIndex, info.SentServerTime);
		}
	}

	[PunRPC]
	private void RaceEnded_RPC(byte raceId, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RaceEnded_RPC");
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		if (raceId >= 0 && (int)raceId < this.races.Length)
		{
			this.races[(int)raceId].RaceEnded();
		}
	}

	void ITickSystemTick.Tick()
	{
		for (int i = 0; i < this.races.Length; i++)
		{
			this.races[i].Tick();
		}
	}

	public bool IsActorLockedIntoAnyRace(int actorNumber)
	{
		for (int i = 0; i < this.races.Length; i++)
		{
			if (this.races[i].IsActorLockedIntoRace(actorNumber))
			{
				return true;
			}
		}
		return false;
	}

	[SerializeField]
	private RacingManager.RaceSetup[] raceSetups;

	private const int MinPlayersInRace = 1;

	private const float ResultsDuration = 10f;

	private RacingManager.Race[] races;

	[Serializable]
	private struct RaceSetup
	{
		public BoxCollider startVolume;

		public int numCheckpoints;

		public float dqBaseDuration;

		public float dqInterval;
	}

	private struct RacerData
	{
		public int actorNumber;

		public string playerName;

		public int numCheckpointsPassed;

		public double latestCheckpointTime;

		public bool isDisqualified;
	}

	private class RacerComparer : IComparer<RacingManager.RacerData>
	{
		public int Compare(RacingManager.RacerData a, RacingManager.RacerData b)
		{
			int num = a.isDisqualified.CompareTo(b.isDisqualified);
			if (num != 0)
			{
				return num;
			}
			int num2 = a.numCheckpointsPassed.CompareTo(b.numCheckpointsPassed);
			if (num2 != 0)
			{
				return -num2;
			}
			if (a.numCheckpointsPassed > 0)
			{
				return a.latestCheckpointTime.CompareTo(b.latestCheckpointTime);
			}
			return a.actorNumber.CompareTo(b.actorNumber);
		}

		public static RacingManager.RacerComparer instance = new RacingManager.RacerComparer();
	}

	public enum RacingState
	{
		Inactive,
		Countdown,
		InProgress,
		Results
	}

	private class Race
	{
		public Race(int raceIndex, RacingManager.RaceSetup setup, HashSet<int> actorsInAnyRace, PhotonView photonView)
		{
			this.raceIndex = raceIndex;
			this.numCheckpoints = setup.numCheckpoints;
			this.raceStartZone = setup.startVolume;
			this.dqBaseDuration = setup.dqBaseDuration;
			this.dqInterval = setup.dqInterval;
			this.photonView = photonView;
		}

		public RacingManager.RacingState racingState { get; private set; }

		public void RegisterVisual(RaceVisual visual)
		{
			this.raceVisual = visual;
		}

		public void Clear()
		{
			this.hasLockedInParticipants = false;
			this.racers.Clear();
			this.playerLookup.Clear();
			this.racingState = RacingManager.RacingState.Inactive;
		}

		public bool IsActorLockedIntoRace(int actorNumber)
		{
			if (this.racingState != RacingManager.RacingState.InProgress || !this.hasLockedInParticipants)
			{
				return false;
			}
			for (int i = 0; i < this.racers.Count; i++)
			{
				if (this.racers[i].actorNumber == actorNumber)
				{
					return true;
				}
			}
			return false;
		}

		public void SendStateToNewPlayer(NetPlayer newPlayer)
		{
			switch (this.racingState)
			{
			case RacingManager.RacingState.Inactive:
			case RacingManager.RacingState.Results:
				return;
			case RacingManager.RacingState.Countdown:
				this.photonView.RPC("RaceBeginCountdown_RPC", RpcTarget.All, new object[]
				{
					(byte)this.raceIndex,
					(byte)this.numLapsSelected,
					this.raceStartTime
				});
				return;
			case RacingManager.RacingState.InProgress:
				return;
			default:
				return;
			}
		}

		public void Tick()
		{
			if (Time.time >= this.nextTickTimestamp)
			{
				this.nextTickTimestamp = Time.time + this.TickWithNextDelay();
			}
		}

		public float TickWithNextDelay()
		{
			bool flag = this.raceVisual != null;
			if (flag)
			{
				this.raceVisual.ActivateStartingWall(this.racingState == RacingManager.RacingState.Countdown);
			}
			switch (this.racingState)
			{
			case RacingManager.RacingState.Inactive:
				if (flag)
				{
					this.RefreshStartingPlayerList();
				}
				return 1f;
			case RacingManager.RacingState.Countdown:
				if (this.raceStartTime > PhotonNetwork.Time)
				{
					if (flag)
					{
						this.RefreshStartingPlayerList();
						this.raceVisual.UpdateCountdown(Mathf.CeilToInt((float)(this.raceStartTime - PhotonNetwork.Time)));
					}
				}
				else
				{
					this.RaceCountdownEnds();
				}
				return 0.1f;
			case RacingManager.RacingState.InProgress:
				if (PhotonNetwork.IsMasterClient)
				{
					if (PhotonNetwork.Time > this.abortRaceAtTimestamp)
					{
						this.photonView.RPC("RaceEnded_RPC", RpcTarget.All, new object[] { (byte)this.raceIndex });
					}
					else
					{
						int num = 0;
						for (int i = 0; i < this.racers.Count; i++)
						{
							if (this.racers[i].numCheckpointsPassed < this.numCheckpointsToWin)
							{
								num++;
							}
						}
						if (num == 0)
						{
							this.photonView.RPC("RaceEnded_RPC", RpcTarget.All, new object[] { (byte)this.raceIndex });
						}
					}
				}
				return 1f;
			case RacingManager.RacingState.Results:
				if (Time.time >= this.resultsEndTimestamp)
				{
					if (flag)
					{
						this.raceVisual.OnRaceReset();
					}
					this.racingState = RacingManager.RacingState.Inactive;
				}
				return 1f;
			default:
				return 1f;
			}
		}

		public void RaceEnded()
		{
			if (this.racingState != RacingManager.RacingState.InProgress)
			{
				return;
			}
			this.racingState = RacingManager.RacingState.Results;
			this.resultsEndTimestamp = Time.time + 10f;
			if (this.raceVisual != null)
			{
				this.raceVisual.OnRaceEnded();
			}
			for (int i = 0; i < this.racers.Count; i++)
			{
				RacingManager.RacerData racerData = this.racers[i];
				if (racerData.numCheckpointsPassed < this.numCheckpointsToWin)
				{
					racerData.isDisqualified = true;
					this.racers[i] = racerData;
				}
			}
			this.racers.Sort(RacingManager.RacerComparer.instance);
			this.OnRacerOrderChanged();
			for (int j = 0; j < this.racers.Count; j++)
			{
				if (this.racers[j].actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					VRRig.LocalRig.hoverboardVisual.SetRaceDisplay("");
					VRRig.LocalRig.hoverboardVisual.SetRaceLapsDisplay("");
					return;
				}
			}
		}

		private void RefreshStartingPlayerList()
		{
			if (this.raceVisual != null && this.UpdateActorsInStartZone())
			{
				RacingManager.Race.stringBuilder.Clear();
				RacingManager.Race.stringBuilder.AppendLine("NEXT RACE LINEUP");
				for (int i = 0; i < this.actorsInStartZone.Count; i++)
				{
					RacingManager.Race.stringBuilder.Append("    ");
					RacingManager.Race.stringBuilder.AppendLine(this.playerNamesInStartZone[this.actorsInStartZone[i]]);
				}
				this.raceVisual.SetRaceStartScoreboardText(RacingManager.Race.stringBuilder.ToString(), "");
			}
		}

		public void Button_StartRace(int laps)
		{
			if (this.racingState != RacingManager.RacingState.Inactive)
			{
				return;
			}
			this.photonView.RPC("RequestRaceStart_RPC", RpcTarget.MasterClient, new object[] { this.raceIndex, laps });
		}

		public void Host_RequestRaceStart(int laps, int requestedByActorNumber)
		{
			if (this.racingState != RacingManager.RacingState.Inactive)
			{
				return;
			}
			this.UpdateActorsInStartZone();
			if (this.actorsInStartZone.Contains(requestedByActorNumber))
			{
				this.photonView.RPC("RaceBeginCountdown_RPC", RpcTarget.All, new object[]
				{
					(byte)this.raceIndex,
					(byte)laps,
					PhotonNetwork.Time + 4.0
				});
			}
		}

		public void BeginCountdown(double startTime, int laps)
		{
			if (this.racingState != RacingManager.RacingState.Inactive)
			{
				return;
			}
			this.racingState = RacingManager.RacingState.Countdown;
			this.raceStartTime = startTime;
			this.abortRaceAtTimestamp = startTime + (double)this.dqBaseDuration;
			this.numLapsSelected = laps;
			this.numCheckpointsToWin = this.numCheckpoints * laps + 1;
			this.hasLockedInParticipants = false;
			if (this.raceVisual != null)
			{
				this.raceVisual.OnCountdownStart(laps, (float)(startTime - PhotonNetwork.Time));
			}
		}

		public void RaceCountdownEnds()
		{
			if (this.racingState != RacingManager.RacingState.Countdown)
			{
				return;
			}
			this.racingState = RacingManager.RacingState.InProgress;
			if (this.raceVisual != null)
			{
				this.raceVisual.OnRaceStart();
			}
			this.UpdateActorsInStartZone();
			if (PhotonNetwork.IsMasterClient)
			{
				this.photonView.RPC("RaceLockInParticipants_RPC", RpcTarget.All, new object[]
				{
					(byte)this.raceIndex,
					this.actorsInStartZone.ToArray()
				});
				return;
			}
			if (this.actorsInStartZone.Count >= 1)
			{
				this.LockInParticipants(this.actorsInStartZone.ToArray(), true);
			}
		}

		public void LockInParticipants(int[] participantActorNumbers, bool isProvisional = false)
		{
			if (this.hasLockedInParticipants)
			{
				return;
			}
			if (!isProvisional && participantActorNumbers.Length < 1)
			{
				this.racingState = RacingManager.RacingState.Inactive;
				return;
			}
			this.racers.Clear();
			if (participantActorNumbers.Length != 0)
			{
				foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
				{
					int actorNumber = vrrig.OwningNetPlayer.ActorNumber;
					if (participantActorNumbers.BinarySearch(actorNumber) >= 0 && !RacingManager.instance.IsActorLockedIntoAnyRace(actorNumber))
					{
						this.racers.Add(new RacingManager.RacerData
						{
							actorNumber = actorNumber,
							playerName = vrrig.OwningNetPlayer.SanitizedNickName,
							latestCheckpointTime = this.raceStartTime
						});
					}
				}
			}
			if (!isProvisional)
			{
				if (this.racers.Count < 1)
				{
					this.racingState = RacingManager.RacingState.Inactive;
					return;
				}
				this.hasLockedInParticipants = true;
			}
			this.racers.Sort(RacingManager.RacerComparer.instance);
			this.OnRacerOrderChanged();
		}

		public void PassCheckpoint(Player player, int checkpointIndex, double time)
		{
			if (this.racingState == RacingManager.RacingState.Inactive)
			{
				return;
			}
			if (time < this.raceStartTime || time < PhotonNetwork.Time - 5.0 || time > PhotonNetwork.Time + 0.10000000149011612)
			{
				return;
			}
			if (this.abortRaceAtTimestamp < time + (double)this.dqInterval)
			{
				this.abortRaceAtTimestamp = time + (double)this.dqInterval;
			}
			RacingManager.RacerData racerData = default(RacingManager.RacerData);
			int i = 0;
			while (i < this.racers.Count)
			{
				racerData = this.racers[i];
				if (racerData.actorNumber == player.ActorNumber)
				{
					if (racerData.numCheckpointsPassed >= this.numCheckpointsToWin || racerData.isDisqualified)
					{
						return;
					}
					if (checkpointIndex != racerData.numCheckpointsPassed % this.numCheckpoints)
					{
						return;
					}
					RigContainer rigContainer;
					if (this.raceVisual != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer) && !this.raceVisual.IsPlayerNearCheckpoint(rigContainer.Rig, checkpointIndex))
					{
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (racerData.actorNumber != player.ActorNumber)
			{
				return;
			}
			racerData.numCheckpointsPassed++;
			racerData.latestCheckpointTime = time;
			this.racers[i] = racerData;
			if (racerData.numCheckpointsPassed >= this.numCheckpointsToWin || (i > 0 && RacingManager.RacerComparer.instance.Compare(this.racers[i - 1], racerData) > 0))
			{
				this.racers.Sort(RacingManager.RacerComparer.instance);
				this.OnRacerOrderChanged();
			}
			if (player.IsLocal)
			{
				if (checkpointIndex == this.numCheckpoints - 1)
				{
					int num = racerData.numCheckpointsPassed / this.numCheckpoints + 1;
					if (num > this.numLapsSelected)
					{
						this.raceVisual.ShowFinishLineText("FINISH");
						this.raceVisual.EnableRaceEndSound();
						return;
					}
					if (num == this.numLapsSelected)
					{
						this.raceVisual.ShowFinishLineText("FINAL LAP");
						return;
					}
					this.raceVisual.ShowFinishLineText("NEXT LAP");
					return;
				}
				else if (checkpointIndex == 0)
				{
					int num2 = racerData.numCheckpointsPassed / this.numCheckpoints + 1;
					if (num2 > this.numLapsSelected)
					{
						VRRig.LocalRig.hoverboardVisual.SetRaceLapsDisplay("");
						return;
					}
					VRRig.LocalRig.hoverboardVisual.SetRaceLapsDisplay(string.Format("LAP {0}/{1}", num2, this.numLapsSelected));
				}
			}
		}

		private void OnRacerOrderChanged()
		{
			if (this.raceVisual != null)
			{
				RacingManager.Race.stringBuilder.Clear();
				RacingManager.Race.timesStringBuilder.Clear();
				RacingManager.Race.timesStringBuilder.AppendLine("");
				bool flag = false;
				switch (this.racingState)
				{
				case RacingManager.RacingState.Inactive:
					return;
				case RacingManager.RacingState.Countdown:
					RacingManager.Race.stringBuilder.AppendLine("STARTING LINEUP");
					flag = true;
					break;
				case RacingManager.RacingState.InProgress:
					RacingManager.Race.stringBuilder.AppendLine("RACE LEADERBOARD");
					break;
				case RacingManager.RacingState.Results:
					RacingManager.Race.stringBuilder.AppendLine("RACE RESULTS");
					break;
				}
				for (int i = 0; i < this.racers.Count; i++)
				{
					RacingManager.RacerData racerData = this.racers[i];
					if (racerData.actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
					{
						VRRig.LocalRig.hoverboardVisual.SetRaceDisplay(racerData.isDisqualified ? "DQ" : (i + 1).ToString());
					}
					string text = (racerData.isDisqualified ? "DQ. " : (flag ? "    " : ((i + 1).ToString() + ". ")));
					RacingManager.Race.stringBuilder.Append(text);
					if (text.Length <= 3)
					{
						RacingManager.Race.stringBuilder.Append(" ");
					}
					RacingManager.Race.stringBuilder.AppendLine(racerData.playerName);
					if (racerData.isDisqualified)
					{
						RacingManager.Race.timesStringBuilder.AppendLine("--.--");
					}
					else if (racerData.numCheckpointsPassed < this.numCheckpointsToWin)
					{
						RacingManager.Race.timesStringBuilder.AppendLine("");
					}
					else
					{
						RacingManager.Race.timesStringBuilder.AppendLine(string.Format("{0:0.00}", racerData.latestCheckpointTime - this.raceStartTime));
					}
				}
				string text2 = RacingManager.Race.stringBuilder.ToString();
				string text3 = RacingManager.Race.timesStringBuilder.ToString();
				this.raceVisual.SetScoreboardText(text2, text3);
				this.raceVisual.SetRaceStartScoreboardText(text2, text3);
			}
		}

		private bool UpdateActorsInStartZone()
		{
			if (Time.time < this.nextStartZoneUpdateTimestamp)
			{
				return false;
			}
			this.nextStartZoneUpdateTimestamp = Time.time + 0.1f;
			List<int> list = this.actorsInStartZone2;
			List<int> list2 = this.actorsInStartZone;
			this.actorsInStartZone = list;
			this.actorsInStartZone2 = list2;
			this.actorsInStartZone.Clear();
			this.playerNamesInStartZone.Clear();
			int num = Physics.OverlapBoxNonAlloc(this.raceStartZone.transform.position, this.raceStartZone.size / 2f, RacingManager.Race.overlapColliders, this.raceStartZone.transform.rotation, RacingManager.Race.playerLayerMask);
			num = Mathf.Min(num, RacingManager.Race.overlapColliders.Length);
			for (int i = 0; i < num; i++)
			{
				Collider collider = RacingManager.Race.overlapColliders[i];
				if (!(collider == null))
				{
					VRRig component = collider.attachedRigidbody.gameObject.GetComponent<VRRig>();
					int count = this.actorsInStartZone.Count;
					if (!(component == null))
					{
						if (component.isLocal)
						{
							if (NetworkSystem.Instance.LocalPlayer == null)
							{
								RacingManager.Race.overlapColliders[i] = null;
								goto IL_01E2;
							}
							if (RacingManager.instance.IsActorLockedIntoAnyRace(NetworkSystem.Instance.LocalPlayer.ActorNumber))
							{
								goto IL_01E2;
							}
							this.actorsInStartZone.AddSortedUnique(NetworkSystem.Instance.LocalPlayer.ActorNumber);
							if (this.actorsInStartZone.Count > count)
							{
								this.playerNamesInStartZone.Add(NetworkSystem.Instance.LocalPlayer.ActorNumber, component.playerNameVisible);
							}
						}
						else
						{
							if (RacingManager.instance.IsActorLockedIntoAnyRace(component.OwningNetPlayer.ActorNumber))
							{
								goto IL_01E2;
							}
							this.actorsInStartZone.AddSortedUnique(component.OwningNetPlayer.ActorNumber);
							if (this.actorsInStartZone.Count > count)
							{
								this.playerNamesInStartZone.Add(component.OwningNetPlayer.ActorNumber, component.playerNameVisible);
							}
						}
						RacingManager.Race.overlapColliders[i] = null;
					}
				}
				IL_01E2:;
			}
			if (this.actorsInStartZone2.Count != this.actorsInStartZone.Count)
			{
				return true;
			}
			for (int j = 0; j < this.actorsInStartZone.Count; j++)
			{
				if (this.actorsInStartZone[j] != this.actorsInStartZone2[j])
				{
					return true;
				}
			}
			return false;
		}

		private int raceIndex;

		private int numCheckpoints;

		private float dqBaseDuration;

		private float dqInterval;

		private BoxCollider raceStartZone;

		private PhotonView photonView;

		private List<RacingManager.RacerData> racers = new List<RacingManager.RacerData>(10);

		private Dictionary<NetPlayer, int> playerLookup = new Dictionary<NetPlayer, int>();

		private List<int> actorsInStartZone = new List<int>();

		private List<int> actorsInStartZone2 = new List<int>();

		private Dictionary<int, string> playerNamesInStartZone = new Dictionary<int, string>();

		private int numLapsSelected = 1;

		private double raceStartTime;

		private double abortRaceAtTimestamp;

		private float resultsEndTimestamp;

		private bool isInstanceLoaded;

		private int numCheckpointsToWin;

		private RaceVisual raceVisual;

		private bool hasLockedInParticipants;

		private float nextTickTimestamp;

		private static StringBuilder stringBuilder = new StringBuilder();

		private static StringBuilder timesStringBuilder = new StringBuilder();

		private static Collider[] overlapColliders = new Collider[20];

		private static int playerLayerMask = UnityLayer.GorillaBodyCollider.ToLayerMask() | UnityLayer.GorillaTagCollider.ToLayerMask();

		private float nextStartZoneUpdateTimestamp;
	}
}
