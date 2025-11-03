using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public class GRElevatorManager : NetworkComponent, ITickSystemTick
{
	public bool InPrivateRoom
	{
		get
		{
			return NetworkSystem.Instance.SessionIsPrivate;
		}
	}

	public bool TickRunning { get; set; }

	protected override void Awake()
	{
		base.Awake();
		if (GRElevatorManager._instance != null)
		{
			Debug.LogError("Multiple elevator managers! This should never happen!");
			return;
		}
		GRElevatorManager._instance = this;
		this.currentState = GRElevatorManager.ElevatorSystemState.InLocation;
		this.currentLocation = GRElevatorManager.ElevatorLocation.Stump;
		this.destination = GRElevatorManager.ElevatorLocation.Stump;
		this.elevatorByLocation = new Dictionary<GRElevatorManager.ElevatorLocation, GRElevator>();
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			this.elevatorByLocation[this.allElevators[i].location] = this.allElevators[i];
		}
		this.actorIds = new List<int>();
		this.mainStagingShuttle.specificFloor = -1;
		this.mainDrillShuttle.specificFloor = 0;
		this.allShuttles = new List<GRShuttle>(64);
		for (int j = 0; j < this.shuttleGroups.Count; j++)
		{
			GRElevatorManager.GRShuttleGroup grshuttleGroup = this.shuttleGroups[j];
			for (int k = 0; k < grshuttleGroup.ghostReactorStagingShuttles.Count; k++)
			{
				this.allShuttles.Add(grshuttleGroup.ghostReactorStagingShuttles[k]);
				grshuttleGroup.ghostReactorStagingShuttles[k].SetLocation(grshuttleGroup.location);
			}
		}
		this.allShuttles.Add(this.mainStagingShuttle);
		this.allShuttles.Add(this.mainDrillShuttle);
		for (int l = 0; l < this.allShuttles.Count; l++)
		{
			this.allShuttles[l].Init(l);
		}
	}

	protected override void Start()
	{
		base.Start();
		NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnLeftRoom;
		NetworkSystem.Instance.OnPlayerJoined += this.OnPlayerAdded;
		NetworkSystem.Instance.OnPlayerLeft += this.OnPlayerRemoved;
	}

	protected void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnLeftRoom;
		NetworkSystem.Instance.OnPlayerJoined -= this.OnPlayerAdded;
		NetworkSystem.Instance.OnPlayerLeft -= this.OnPlayerRemoved;
	}

	private new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
		TickSystem<object>.AddTickCallback(this);
	}

	private new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void Tick()
	{
		if (!this.cosmeticsInitialized)
		{
			this.CheckInitializationState();
			return;
		}
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			this.allElevators[i].PhysicalElevatorUpdate();
		}
		this.ProcessElevatorSystemState();
		if (this.justTeleported)
		{
			this.justTeleported = false;
			GTPlayer.Instance.disableMovement = false;
		}
	}

	private void CheckInitializationState()
	{
		this.cosmeticsInitialized = true;
		if (GRElevatorManager.InControlOfElevator())
		{
			this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.InLocation, GRElevatorManager.ElevatorLocation.Stump);
		}
	}

	public void ProcessElevatorSystemState()
	{
		switch (this.currentState)
		{
		case GRElevatorManager.ElevatorSystemState.Dormant:
			break;
		case GRElevatorManager.ElevatorSystemState.InLocation:
			if (this.currentLocation == this.destination && this.waitForZoneLoadFallbackTimer >= 0f && this.elevatorByLocation[this.currentLocation].DoorIsClosing())
			{
				this.waitForZoneLoadFallbackTimer += Time.deltaTime;
				if (this.waitForZoneLoadFallbackTimer >= this.waitForZoneLoadFallbackMaxTime)
				{
					this.OnReachedDestination();
				}
			}
			break;
		case GRElevatorManager.ElevatorSystemState.DestinationPressed:
		{
			if (!GRElevatorManager.InControlOfElevator())
			{
				return;
			}
			double time = this.GetTime();
			if (this.elevatorByLocation[this.currentLocation].DoorsFullyClosed() && time >= this.doorsFullyClosedTime + (double)this.doorsFullyClosedDelay)
			{
				this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.WaitingToTeleport, GRElevatorManager.ElevatorLocation.None);
				return;
			}
			if (time >= this.destinationButtonLastPressedTime + (double)this.destinationButtonlastPressedDelay && !this.elevatorByLocation[this.currentLocation].DoorIsClosing())
			{
				this.destinationButtonLastPressedTime = time;
				this.CloseAllElevators();
				return;
			}
			break;
		}
		case GRElevatorManager.ElevatorSystemState.WaitingToTeleport:
			if (!GRElevatorManager.InControlOfElevator())
			{
				return;
			}
			if (this.GetTime() >= this.doorsFullyClosedTime + (double)this.doorsFullyClosedDelay && !this.waitingForRemoteTeleport)
			{
				this.ActivateElevating();
				return;
			}
			break;
		default:
			return;
		}
	}

	public void ActivateElevating()
	{
		if (PhotonNetwork.InRoom)
		{
			this.photonView.RPC("RemoteActivateTeleport", RpcTarget.All, new object[]
			{
				(int)this.currentLocation,
				(int)this.destination,
				GRElevatorManager.LowestActorNumberInElevator()
			});
			return;
		}
		this.ActivateTeleport(this.currentLocation, this.destination, -1, this.GetTime());
	}

	public void LeadElevatorJoin()
	{
		GRElevatorManager.LeadElevatorJoin(this.elevatorByLocation[this.currentLocation].friendCollider, this.elevatorByLocation[this.destination].friendCollider, this.elevatorByLocation[this.destination].joinTrigger);
	}

	public static void LeadElevatorJoin(GorillaFriendCollider sourceFriendCollider, GorillaFriendCollider destinationFriendCollider, GorillaNetworkJoinTrigger destinationJoinTrigger)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			sourceFriendCollider.RefreshPlayersInSphere();
			destinationFriendCollider.RefreshPlayersInSphere();
			PhotonNetworkController.Instance.FriendIDList = new List<string>(sourceFriendCollider.playerIDsCurrentlyTouching);
			PhotonNetworkController.Instance.FriendIDList.AddRange(destinationFriendCollider.playerIDsCurrentlyTouching);
			foreach (string text in PhotonNetworkController.Instance.FriendIDList)
			{
			}
			PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			RoomSystem.SendElevatorFollowCommand(PhotonNetworkController.Instance.shuffler, PhotonNetworkController.Instance.keyStr, sourceFriendCollider, destinationFriendCollider);
			PhotonNetwork.SendAllOutgoingCommands();
			PhotonNetworkController.Instance.AttemptToJoinPublicRoom(destinationJoinTrigger, JoinType.JoinWithElevator, null);
		}
		GRElevatorManager.JoinPublicRoom();
	}

	public static void LeadShuttleJoin(GorillaFriendCollider sourceFriendCollider, GorillaFriendCollider destinationFriendCollider, GorillaNetworkJoinTrigger destinationJoinTrigger, int targetLevel)
	{
		sourceFriendCollider.RefreshPlayersInSphere();
		destinationFriendCollider.RefreshPlayersInSphere();
		GorillaComputer.instance.friendJoinCollider = destinationFriendCollider;
		GorillaComputer.instance.UpdateScreen();
		if (NetworkSystem.Instance.InRoom)
		{
			PhotonNetworkController.Instance.FriendIDList = new List<string>(sourceFriendCollider.playerIDsCurrentlyTouching);
			PhotonNetworkController.Instance.FriendIDList.AddRange(destinationFriendCollider.playerIDsCurrentlyTouching);
			foreach (string text in PhotonNetworkController.Instance.FriendIDList)
			{
			}
			PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			Debug.Log("Send Shuttle Join");
			RoomSystem.SendShuttleFollowCommand(PhotonNetworkController.Instance.shuffler, PhotonNetworkController.Instance.keyStr, sourceFriendCollider, destinationFriendCollider);
			PhotonNetwork.SendAllOutgoingCommands();
			List<ValueTuple<string, string>> list = null;
			if (targetLevel >= 0)
			{
				int joinDepthSectionFromLevel = GhostReactor.GetJoinDepthSectionFromLevel(targetLevel);
				list = new List<ValueTuple<string, string>>
				{
					new ValueTuple<string, string>("ghostReactorDepth", joinDepthSectionFromLevel.ToString())
				};
				Debug.LogFormat("GR Room Param Join {0} {1}", new object[]
				{
					list[0].Item1,
					list[0].Item2
				});
			}
			PhotonNetworkController.Instance.AttemptToJoinPublicRoom(destinationJoinTrigger, JoinType.JoinWithElevator, list);
		}
		PhotonNetworkController.Instance.AttemptToJoinPublicRoom(destinationJoinTrigger, JoinType.Solo, null);
	}

	public void UpdateElevatorState(GRElevatorManager.ElevatorSystemState newState, GRElevatorManager.ElevatorLocation location = GRElevatorManager.ElevatorLocation.None)
	{
		switch (this.currentState)
		{
		case GRElevatorManager.ElevatorSystemState.Dormant:
			switch (newState)
			{
			case GRElevatorManager.ElevatorSystemState.InLocation:
				this.elevatorByLocation[this.currentLocation].PlayDing();
				this.OpenElevator(this.destination);
				break;
			case GRElevatorManager.ElevatorSystemState.DestinationPressed:
			case GRElevatorManager.ElevatorSystemState.WaitingToTeleport:
				this.maxDoorClosingTime = this.GetTime();
				this.destinationButtonLastPressedTime = this.GetTime();
				this.doorsFullyClosedTime = this.GetTime();
				if (this.destination != this.currentLocation)
				{
					this.destination = location;
				}
				this.elevatorByLocation[this.currentLocation].PlayElevatorMoving();
				this.elevatorByLocation[this.destination].PlayElevatorMoving();
				break;
			}
			break;
		case GRElevatorManager.ElevatorSystemState.InLocation:
			switch (newState)
			{
			case GRElevatorManager.ElevatorSystemState.Dormant:
				this.CloseAllElevators();
				break;
			case GRElevatorManager.ElevatorSystemState.InLocation:
				if (location == this.currentLocation)
				{
					this.OpenElevator(this.currentLocation);
				}
				else
				{
					this.CloseAllElevators();
				}
				break;
			case GRElevatorManager.ElevatorSystemState.DestinationPressed:
				if (location != this.currentLocation)
				{
					this.destination = location;
					this.destinationButtonLastPressedTime = this.GetTime();
					this.maxDoorClosingTime = this.GetTime();
				}
				else
				{
					if (this.elevatorByLocation[this.destination].DoorIsClosing())
					{
						this.OpenElevator(this.currentLocation);
					}
					newState = this.currentState;
				}
				if (this.currentLocation != GRElevatorManager.ElevatorLocation.None)
				{
					this.elevatorByLocation[this.currentLocation].PlayElevatorMoving();
				}
				this.elevatorByLocation[this.destination].PlayElevatorMoving();
				break;
			case GRElevatorManager.ElevatorSystemState.WaitingToTeleport:
				if (this.currentLocation != GRElevatorManager.ElevatorLocation.None)
				{
					this.elevatorByLocation[this.currentLocation].PlayElevatorMoving();
				}
				this.elevatorByLocation[this.destination].PlayElevatorMoving();
				break;
			}
			break;
		case GRElevatorManager.ElevatorSystemState.DestinationPressed:
			switch (newState)
			{
			case GRElevatorManager.ElevatorSystemState.Dormant:
				this.CloseAllElevators();
				break;
			case GRElevatorManager.ElevatorSystemState.InLocation:
				this.OpenElevator(location);
				this.elevatorByLocation[this.currentLocation].PlayDing();
				break;
			case GRElevatorManager.ElevatorSystemState.DestinationPressed:
				if (location != this.currentLocation)
				{
					this.destination = location;
				}
				break;
			case GRElevatorManager.ElevatorSystemState.WaitingToTeleport:
				this.doorsFullyClosedTime = this.GetTime();
				if (this.currentLocation != GRElevatorManager.ElevatorLocation.None)
				{
					this.elevatorByLocation[this.currentLocation].PlayElevatorMoving();
					this.elevatorByLocation[this.currentLocation].PlayElevatorMusic(0f);
				}
				this.elevatorByLocation[this.destination].PlayElevatorMoving();
				break;
			}
			break;
		case GRElevatorManager.ElevatorSystemState.WaitingToTeleport:
			switch (newState)
			{
			case GRElevatorManager.ElevatorSystemState.Dormant:
				this.CloseAllElevators();
				this.elevatorByLocation[this.currentLocation].PlayElevatorStopped();
				this.elevatorByLocation[this.destination].PlayElevatorStopped();
				break;
			case GRElevatorManager.ElevatorSystemState.InLocation:
			{
				ZoneManagement instance = ZoneManagement.instance;
				instance.OnSceneLoadsCompleted = (Action)Delegate.Combine(instance.OnSceneLoadsCompleted, new Action(this.OnReachedDestination));
				this.waitForZoneLoadFallbackTimer = 0.01f;
				this.elevatorByLocation[this.currentLocation].PlayElevatorStopped();
				this.currentLocation = location;
				break;
			}
			case GRElevatorManager.ElevatorSystemState.DestinationPressed:
			case GRElevatorManager.ElevatorSystemState.WaitingToTeleport:
				if (location != this.currentLocation)
				{
					this.destination = location;
				}
				else
				{
					this.OpenElevator(location);
					newState = GRElevatorManager.ElevatorSystemState.InLocation;
				}
				break;
			}
			break;
		}
		this.currentState = newState;
		this.UpdateUI();
	}

	public void UpdateUI()
	{
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			this.allElevators[i].outerText.text = "ELEVATOR LOCATION:\n" + this.currentLocation.ToString().ToUpper();
			GRElevatorManager.ElevatorSystemState elevatorSystemState = this.currentState;
			if (elevatorSystemState > GRElevatorManager.ElevatorSystemState.InLocation)
			{
				if (elevatorSystemState - GRElevatorManager.ElevatorSystemState.DestinationPressed <= 1)
				{
					if (this.destination != this.currentLocation)
					{
						this.allElevators[i].innerText.text = "NEXT STOP:\n" + this.destination.ToString().ToUpper();
					}
					else
					{
						this.allElevators[i].innerText.text = "CHOOSE DESTINATION";
					}
				}
			}
			else
			{
				this.allElevators[i].innerText.text = "CHOOSE DESTINATION";
			}
		}
	}

	public static void RegisterElevator(GRElevator elevator)
	{
		if (GRElevatorManager._instance == null)
		{
			return;
		}
		GRElevatorManager._instance.elevatorByLocation[elevator.location] = elevator;
	}

	public static void DeregisterElevator(GRElevator elevator)
	{
		if (GRElevatorManager._instance == null)
		{
			return;
		}
		GRElevatorManager._instance.elevatorByLocation[elevator.location] = null;
	}

	public static void ElevatorButtonPressed(GRElevator.ButtonType type, GRElevatorManager.ElevatorLocation location)
	{
		if (GRElevatorManager._instance != null)
		{
			GRElevatorManager._instance.ElevatorButtonPressedInternal(type, location);
			if (!GRElevatorManager._instance.IsMine && NetworkSystem.Instance.InRoom)
			{
				GRElevatorManager._instance.photonView.RPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new object[]
				{
					(int)type,
					(int)location
				});
			}
		}
	}

	private void ElevatorButtonPressedInternal(GRElevator.ButtonType type, GRElevatorManager.ElevatorLocation location)
	{
		this.elevatorByLocation[location].PressButtonVisuals(type);
		this.elevatorByLocation[location].PlayButtonPress();
		if (base.IsMine)
		{
			this.ProcessElevatorButtonPress(type, location);
		}
	}

	public void ProcessElevatorButtonPress(GRElevator.ButtonType type, GRElevatorManager.ElevatorLocation location)
	{
		switch (type)
		{
		case GRElevator.ButtonType.Stump:
			if (this.currentState != GRElevatorManager.ElevatorSystemState.WaitingToTeleport)
			{
				this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.DestinationPressed, GRElevatorManager.ElevatorLocation.Stump);
				return;
			}
			break;
		case GRElevator.ButtonType.City:
			if (this.currentState != GRElevatorManager.ElevatorSystemState.WaitingToTeleport)
			{
				this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.DestinationPressed, GRElevatorManager.ElevatorLocation.City);
				return;
			}
			break;
		case GRElevator.ButtonType.GhostReactor:
			if (this.currentState != GRElevatorManager.ElevatorSystemState.WaitingToTeleport)
			{
				this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.DestinationPressed, GRElevatorManager.ElevatorLocation.GhostReactor);
				return;
			}
			break;
		case GRElevator.ButtonType.Open:
			if (this.currentState != GRElevatorManager.ElevatorSystemState.WaitingToTeleport)
			{
				if (this.currentState == GRElevatorManager.ElevatorSystemState.DestinationPressed)
				{
					if (this.GetTime() >= this.maxDoorClosingTime + (double)this.doorMaxClosingDelay)
					{
						break;
					}
					this.destinationButtonLastPressedTime = this.GetTime();
					this.doorsFullyClosedTime = this.GetTime();
				}
				this.OpenElevator(location);
				return;
			}
			break;
		case GRElevator.ButtonType.Close:
			this.CloseAllElevators();
			break;
		case GRElevator.ButtonType.Summon:
			if (this.currentState != GRElevatorManager.ElevatorSystemState.WaitingToTeleport && this.currentState != GRElevatorManager.ElevatorSystemState.DestinationPressed)
			{
				this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.DestinationPressed, location);
				return;
			}
			break;
		case GRElevator.ButtonType.MonkeBlocks:
			if (this.currentState != GRElevatorManager.ElevatorSystemState.WaitingToTeleport)
			{
				this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.DestinationPressed, GRElevatorManager.ElevatorLocation.MonkeBlocks);
				return;
			}
			break;
		default:
			return;
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(this.doorsFullyClosedTime);
		stream.SendNext(this.destinationButtonLastPressedTime);
		stream.SendNext(this.maxDoorClosingTime);
		stream.SendNext((int)this.currentLocation);
		stream.SendNext((int)this.destination);
		stream.SendNext((int)this.currentState);
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			stream.SendNext((int)this.allElevators[i].state);
		}
		for (int j = 0; j < this.allShuttles.Count; j++)
		{
			stream.SendNext((byte)this.allShuttles[j].GetState());
			bool flag = this.allShuttles[j].specificDestinationShuttle == null;
			NetPlayer owner = this.allShuttles[j].GetOwner();
			int num = ((!flag || owner == null) ? (-1) : owner.ActorNumber);
			stream.SendNext(num);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		double num = (double)stream.ReceiveNext();
		if (!double.IsNaN(num) && !double.IsInfinity(num))
		{
			this.doorsFullyClosedTime = num;
		}
		num = (double)stream.ReceiveNext();
		if (!double.IsNaN(num) && !double.IsInfinity(num))
		{
			this.destinationButtonLastPressedTime = num;
		}
		num = (double)stream.ReceiveNext();
		if (!double.IsNaN(num) && !double.IsInfinity(num))
		{
			this.maxDoorClosingTime = num;
		}
		GRElevatorManager.ElevatorLocation elevatorLocation = this.currentLocation;
		int num2 = (int)stream.ReceiveNext();
		if (num2 >= 0 && num2 <= 4)
		{
			this.currentLocation = (GRElevatorManager.ElevatorLocation)num2;
		}
		GRElevatorManager.ElevatorLocation elevatorLocation2 = this.destination;
		num2 = (int)stream.ReceiveNext();
		if (num2 >= 0 && num2 <= 4)
		{
			this.destination = (GRElevatorManager.ElevatorLocation)num2;
		}
		num2 = (int)stream.ReceiveNext();
		if (num2 >= 0 && num2 < 5)
		{
			this.currentState = (GRElevatorManager.ElevatorSystemState)num2;
		}
		this.UpdateUI();
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			num2 = (int)stream.ReceiveNext();
			if (num2 >= 0 && num2 < 8)
			{
				this.allElevators[i].UpdateRemoteState((GRElevator.ElevatorState)num2);
			}
		}
		for (int j = 0; j < this.allShuttles.Count; j++)
		{
			byte b = (byte)stream.ReceiveNext();
			int num3 = (int)stream.ReceiveNext();
			if (b >= 0 && b < 7)
			{
				this.allShuttles[j].SetState((GRShuttleState)b, false);
			}
			if (this.allShuttles[j].specificDestinationShuttle == null && num3 != -1)
			{
				NetPlayer netPlayer = NetPlayer.Get(num3);
				this.allShuttles[j].SetOwner(netPlayer);
			}
		}
	}

	[PunRPC]
	public void RemoteElevatorButtonPress(int elevatorButtonPressed, int elevatorLocation, PhotonMessageInfo info)
	{
		if (!base.IsMine || this.m_RpcSpamChecks.IsSpamming(GRElevatorManager.RPC.RemoteElevatorButtonPress))
		{
			return;
		}
		if (elevatorLocation < 0 || elevatorLocation >= 4)
		{
			return;
		}
		if (elevatorButtonPressed < 0 || elevatorButtonPressed >= 8)
		{
			return;
		}
		this.ElevatorButtonPressedInternal((GRElevator.ButtonType)elevatorButtonPressed, (GRElevatorManager.ElevatorLocation)elevatorLocation);
	}

	[PunRPC]
	public void RemoteActivateTeleport(int elevatorStartLocation, int elevatorDestinationLocation, int lowestActorNumber, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient || this.m_RpcSpamChecks.IsSpamming(GRElevatorManager.RPC.RemoteActivateTeleport))
		{
			return;
		}
		if (elevatorStartLocation < 0 || elevatorStartLocation >= 4 || elevatorDestinationLocation < 0 || elevatorDestinationLocation >= 4)
		{
			return;
		}
		if (!this.waitingForRemoteTeleport)
		{
			base.StartCoroutine(this.TeleportDelay((GRElevatorManager.ElevatorLocation)elevatorStartLocation, (GRElevatorManager.ElevatorLocation)elevatorDestinationLocation, lowestActorNumber, info.SentServerTime));
		}
	}

	private IEnumerator TeleportDelay(GRElevatorManager.ElevatorLocation start, GRElevatorManager.ElevatorLocation destination, int lowestActorNumber, double sentServerTime)
	{
		this.timeLastTeleported = (double)Time.time;
		this.waitingForRemoteTeleport = true;
		this.lastTeleportSource = start;
		yield return new WaitForSeconds((float)(PhotonNetwork.Time - (sentServerTime + 0.75)));
		this.RefreshTeleportingPlayersJoinTime();
		yield return new WaitForSeconds(0.25f);
		this.waitingForRemoteTeleport = false;
		this.ActivateTeleport(start, destination, lowestActorNumber, sentServerTime);
		yield break;
	}

	public void ActivateTeleport(GRElevatorManager.ElevatorLocation start, GRElevatorManager.ElevatorLocation destination, int lowestActorNumber, double photonServerTime)
	{
		GRElevator grelevator = this.elevatorByLocation[start];
		GRElevator grelevator2 = this.elevatorByLocation[destination];
		if (grelevator == null || grelevator2 == null)
		{
			return;
		}
		grelevator.friendCollider.RefreshPlayersInSphere();
		if (!PhotonNetwork.InRoom)
		{
			this.RefreshTeleportingPlayersJoinTime();
		}
		if (!grelevator.friendCollider.playerIDsCurrentlyTouching.Contains(NetworkSystem.Instance.LocalPlayer.UserId))
		{
			this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.InLocation, destination);
			return;
		}
		this.elevatorByLocation[destination].collidersAndVisuals.SetActive(true);
		float num = grelevator2.transform.rotation.eulerAngles.y - grelevator.transform.rotation.eulerAngles.y;
		GTPlayer instance = GTPlayer.Instance;
		VRRig localRig = VRRig.LocalRig;
		Vector3 vector = localRig.transform.position - instance.transform.position;
		Vector3 vector2 = instance.headCollider.transform.position - instance.transform.position;
		Vector3 vector3 = grelevator2.transform.TransformPoint(grelevator.transform.InverseTransformPoint(instance.transform.position));
		Vector3 vector4 = localRig.transform.position - grelevator.transform.position;
		vector4.x *= 0.8f;
		vector4.z *= 0.8f;
		vector3 = grelevator2.transform.position + (Quaternion.Euler(0f, num, 0f) * vector4 - vector) + localRig.headConstraint.rotation * localRig.head.trackingPositionOffset;
		Vector3 vector5 = Vector3.zero;
		Vector3 vector6 = grelevator2.transform.position + (Quaternion.Euler(0f, num, 0f) * vector4 - vector) + vector2 - grelevator2.transform.position;
		float magnitude = vector6.magnitude;
		vector6 = vector6.normalized;
		if (Physics.SphereCastNonAlloc(grelevator2.transform.position, instance.headCollider.radius * 1.5f, vector6, this.correctionRaycastHit, magnitude * 1.05f, this.correctionRaycastMask) > 0)
		{
			vector5 = vector6 * instance.headCollider.radius * -1.5f;
		}
		instance.TeleportTo(vector3 + vector5, instance.transform.rotation, false, false);
		instance.turnParent.transform.RotateAround(instance.headCollider.transform.position, base.transform.up, num);
		localRig.transform.position = instance.transform.position + vector;
		instance.InitializeValues();
		this.justTeleported = true;
		instance.disableMovement = true;
		GorillaComputer.instance.allowedMapsToJoin = this.elevatorByLocation[destination].joinTrigger.myCollider.myAllowedMapsToJoin;
		this.lastTeleportSource = start;
		this.lastLowestActorNr = lowestActorNumber;
		if (!this.InPrivateRoom && lowestActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.LeadElevatorJoin();
		}
		this.UpdateElevatorState(GRElevatorManager.ElevatorSystemState.InLocation, destination);
		grelevator2.PlayElevatorMusic(grelevator.musicAudio.time);
	}

	public void CloseAllElevators()
	{
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			if (!this.allElevators[i].DoorIsClosing())
			{
				this.allElevators[i].UpdateLocalState(GRElevator.ElevatorState.DoorBeginClosing);
			}
		}
	}

	public void OpenElevator(GRElevatorManager.ElevatorLocation location)
	{
		for (int i = 0; i < this.allElevators.Count; i++)
		{
			this.allElevators[i].UpdateLocalState((this.allElevators[i].location == location) ? GRElevator.ElevatorState.DoorBeginOpening : GRElevator.ElevatorState.DoorBeginClosing);
		}
	}

	public double GetTime()
	{
		double num = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		if (this.doorsFullyClosedTime > num || this.destinationButtonLastPressedTime > num || this.maxDoorClosingTime > num || num - this.doorsFullyClosedTime > 10.0 || num - this.destinationButtonLastPressedTime > 10.0 || num - this.maxDoorClosingTime > 20.0)
		{
			this.doorsFullyClosedTime = num;
			this.destinationButtonLastPressedTime = num;
			this.maxDoorClosingTime = num;
		}
		return num;
	}

	public static bool ValidElevatorNetworking(int actorNr)
	{
		if (GRElevatorManager._instance == null)
		{
			return false;
		}
		if (RoomSystem.WasRoomPrivate)
		{
			return false;
		}
		if (actorNr == GRElevatorManager._instance.lastLowestActorNr)
		{
			return true;
		}
		if (GRElevatorManager._instance.lastTeleportSource == GRElevatorManager.ElevatorLocation.None)
		{
			return false;
		}
		GorillaFriendCollider friendCollider = GRElevatorManager._instance.elevatorByLocation[GRElevatorManager._instance.destination].friendCollider;
		GorillaFriendCollider friendCollider2 = GRElevatorManager._instance.elevatorByLocation[GRElevatorManager._instance.lastTeleportSource].friendCollider;
		if ((double)Time.time < GRElevatorManager._instance.timeLastTeleported + 3.0)
		{
			friendCollider.RefreshPlayersInSphere();
			friendCollider2.RefreshPlayersInSphere();
		}
		NetPlayer netPlayer = NetPlayer.Get(actorNr);
		return netPlayer != null && (friendCollider.playerIDsCurrentlyTouching.Contains(netPlayer.UserId) || friendCollider2.playerIDsCurrentlyTouching.Contains(netPlayer.UserId));
	}

	public static bool ValidShuttleNetworking(int actorNr)
	{
		if (GRElevatorManager._instance == null)
		{
			return false;
		}
		if (RoomSystem.WasRoomPrivate)
		{
			return false;
		}
		GRPlayer grplayer = GRPlayer.Get(actorNr);
		if (grplayer == null)
		{
			return false;
		}
		GRShuttle shuttle = GRElevatorManager.GetShuttle(grplayer.shuttleData.currShuttleId);
		GRShuttle grshuttle = GRElevatorManager.GetShuttle(grplayer.shuttleData.targetShuttleId);
		if (shuttle == null)
		{
			return false;
		}
		if (grshuttle == null)
		{
			grshuttle = GRElevatorManager.GetShuttle(GRShuttle.CalcTargetShuttleId(grplayer.shuttleData.currShuttleId, grplayer.shuttleData.ownerUserId));
			if (grshuttle == null)
			{
				return false;
			}
		}
		NetPlayer netPlayer = NetPlayer.Get(actorNr);
		if (netPlayer == null)
		{
			return false;
		}
		if (netPlayer == shuttle.GetOwner())
		{
			return true;
		}
		GorillaFriendCollider friendCollider = grshuttle.friendCollider;
		GorillaFriendCollider friendCollider2 = shuttle.friendCollider;
		friendCollider.RefreshPlayersInSphere();
		friendCollider2.RefreshPlayersInSphere();
		return friendCollider.playerIDsCurrentlyTouching.Contains(netPlayer.UserId) || friendCollider2.playerIDsCurrentlyTouching.Contains(netPlayer.UserId);
	}

	public static bool IsPlayerInShuttle(int actorNr, GRShuttle currShuttle, GRShuttle targetShuttle)
	{
		if (GRElevatorManager._instance == null)
		{
			return false;
		}
		NetPlayer netPlayer = NetPlayer.Get(actorNr);
		if (netPlayer == null)
		{
			return false;
		}
		bool flag = false;
		if (currShuttle != null)
		{
			GorillaFriendCollider friendCollider = currShuttle.friendCollider;
			if (friendCollider != null)
			{
				friendCollider.RefreshPlayersInSphere();
			}
			flag = friendCollider.playerIDsCurrentlyTouching.Contains(netPlayer.UserId);
		}
		bool flag2 = false;
		if (targetShuttle != null)
		{
			GorillaFriendCollider friendCollider2 = targetShuttle.friendCollider;
			if (friendCollider2 != null)
			{
				friendCollider2.RefreshPlayersInSphere();
			}
			friendCollider2.playerIDsCurrentlyTouching.Contains(netPlayer.UserId);
		}
		return flag || flag2;
	}

	public static int LowestActorNumberInElevator()
	{
		GorillaFriendCollider friendCollider = GRElevatorManager._instance.elevatorByLocation[GRElevatorManager._instance.currentLocation].friendCollider;
		GorillaFriendCollider friendCollider2 = GRElevatorManager._instance.elevatorByLocation[GRElevatorManager._instance.destination].friendCollider;
		friendCollider.RefreshPlayersInSphere();
		friendCollider2.RefreshPlayersInSphere();
		int num = int.MaxValue;
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		for (int i = 0; i < allNetPlayers.Length; i++)
		{
			if (num > allNetPlayers[i].ActorNumber && (friendCollider.playerIDsCurrentlyTouching.Contains(allNetPlayers[i].UserId) || friendCollider2.playerIDsCurrentlyTouching.Contains(allNetPlayers[i].UserId)))
			{
				num = allNetPlayers[i].ActorNumber;
			}
		}
		return num;
	}

	public static int LowestActorNumberInElevator(GorillaFriendCollider sourceFriendCollider, GorillaFriendCollider destinationFriendCollider)
	{
		sourceFriendCollider.RefreshPlayersInSphere();
		destinationFriendCollider.RefreshPlayersInSphere();
		int num = int.MaxValue;
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		for (int i = 0; i < allNetPlayers.Length; i++)
		{
			if (num > allNetPlayers[i].ActorNumber && (sourceFriendCollider.playerIDsCurrentlyTouching.Contains(allNetPlayers[i].UserId) || destinationFriendCollider.playerIDsCurrentlyTouching.Contains(allNetPlayers[i].UserId)))
			{
				num = allNetPlayers[i].ActorNumber;
			}
		}
		return num;
	}

	private void RefreshTeleportingPlayersJoinTime()
	{
		GorillaFriendCollider friendCollider = GRElevatorManager._instance.elevatorByLocation[GRElevatorManager._instance.currentLocation].friendCollider;
		this.actorIds.Clear();
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		for (int i = 0; i < allNetPlayers.Length; i++)
		{
			RigContainer rigContainer;
			if (friendCollider.playerIDsCurrentlyTouching.Contains(allNetPlayers[i].UserId) && VRRigCache.Instance.TryGetVrrig(allNetPlayers[i], out rigContainer))
			{
				rigContainer.Rig.ResetTimeSpawned();
			}
		}
	}

	public static bool InControlOfElevator()
	{
		return !NetworkSystem.Instance.InRoom || GRElevatorManager._instance.IsMine;
	}

	public static void JoinPublicRoom()
	{
		PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GRElevatorManager._instance.elevatorByLocation[GRElevatorManager._instance.destination].joinTrigger, JoinType.Solo, null);
	}

	public void OnReachedDestination()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.OnSceneLoadsCompleted = (Action)Delegate.Remove(instance.OnSceneLoadsCompleted, new Action(this.OnReachedDestination));
		this.elevatorByLocation[this.destination].PlayElevatorStopped();
		if (this.currentLocation == this.destination)
		{
			this.OpenElevator(this.currentLocation);
			this.elevatorByLocation[this.currentLocation].PlayDing();
		}
		this.waitForZoneLoadFallbackTimer = -1f;
	}

	public static GRShuttle GetShuttle(int shuttleId)
	{
		if (GRElevatorManager._instance == null)
		{
			return null;
		}
		return GRElevatorManager._instance.GetShuttleById(shuttleId);
	}

	public void InitShuttles(GhostReactor reactor)
	{
		for (int i = 0; i < this.allShuttles.Count; i++)
		{
			this.allShuttles[i].SetReactor(reactor);
		}
	}

	public GRShuttle GetPlayerShuttle(GRShuttleGroupLoc shuttleGroupLoc, int shuttleIndex)
	{
		int i = 0;
		while (i < this.shuttleGroups.Count)
		{
			if (this.shuttleGroups[i].location == shuttleGroupLoc)
			{
				if (shuttleIndex < 0 || shuttleIndex >= this.shuttleGroups[i].ghostReactorStagingShuttles.Count)
				{
					Debug.LogErrorFormat("Invalid Shuttle Index {0} of {1}", new object[]
					{
						shuttleIndex,
						this.shuttleGroups[i].ghostReactorStagingShuttles.Count
					});
					return null;
				}
				return this.shuttleGroups[i].ghostReactorStagingShuttles[shuttleIndex];
			}
			else
			{
				i++;
			}
		}
		return null;
	}

	public GRShuttle GetDrillShuttleForPlayer(int actorNumber)
	{
		return this.GetShuttleForPlayer(actorNumber, GRShuttleGroupLoc.Drill);
	}

	public GRShuttle GetStagingShuttleForPlayer(int actorNumber)
	{
		return this.GetShuttleForPlayer(actorNumber, GRShuttleGroupLoc.Staging);
	}

	public GRShuttle GetShuttleForPlayer(int actorNumber, GRShuttleGroupLoc shuttleGroupLoc)
	{
		for (int i = 0; i < this.shuttleGroups.Count; i++)
		{
			if (this.shuttleGroups[i].location == shuttleGroupLoc)
			{
				for (int j = 0; j < this.shuttleGroups[i].ghostReactorStagingShuttles.Count; j++)
				{
					GRShuttle grshuttle = this.shuttleGroups[i].ghostReactorStagingShuttles[j];
					if (!(grshuttle == null))
					{
						NetPlayer owner = grshuttle.GetOwner();
						if (owner != null && owner.ActorNumber == actorNumber)
						{
							return grshuttle;
						}
					}
				}
			}
		}
		return null;
	}

	public GRShuttle GetShuttleById(int shuttleId)
	{
		for (int i = 0; i < this.allShuttles.Count; i++)
		{
			if (this.allShuttles[i].shuttleId == shuttleId)
			{
				return this.allShuttles[i];
			}
		}
		return null;
	}

	private int AddPlayer(NetPlayer netPlayer)
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return -1;
		}
		int num = -1;
		List<GRShuttle> ghostReactorStagingShuttles = this.shuttleGroups[0].ghostReactorStagingShuttles;
		for (int i = 0; i < ghostReactorStagingShuttles.Count; i++)
		{
			if (ghostReactorStagingShuttles[i].GetOwner() == null)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			return -1;
		}
		for (int j = 0; j < this.shuttleGroups.Count; j++)
		{
			this.shuttleGroups[j].ghostReactorStagingShuttles[num].SetOwner(netPlayer);
		}
		return num;
	}

	private void RemovePlayer(NetPlayer netPlayer)
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		int num = -1;
		List<GRShuttle> ghostReactorStagingShuttles = this.shuttleGroups[0].ghostReactorStagingShuttles;
		for (int i = 0; i < ghostReactorStagingShuttles.Count; i++)
		{
			if (ghostReactorStagingShuttles[i].GetOwner() == netPlayer)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		for (int j = 0; j < this.shuttleGroups.Count; j++)
		{
			this.shuttleGroups[j].ghostReactorStagingShuttles[num].SetOwner(null);
		}
	}

	public void OnLeftRoom()
	{
		for (int i = 0; i < this.shuttleGroups.Count; i++)
		{
			for (int j = 0; j < this.shuttleGroups[i].ghostReactorStagingShuttles.Count; j++)
			{
				GRShuttle grshuttle = this.shuttleGroups[i].ghostReactorStagingShuttles[j];
				if (!(grshuttle == null))
				{
					grshuttle.SetOwner(null);
				}
			}
		}
	}

	public void OnPlayerAdded(NetPlayer player)
	{
		if (!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
		{
			return;
		}
		this.AddPlayer(player);
	}

	public void OnPlayerRemoved(NetPlayer player)
	{
		if (!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
		{
			return;
		}
		this.RemovePlayer(player);
	}

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
	}

	public PhotonView photonView;

	public static GRElevatorManager _instance;

	public Dictionary<GRElevatorManager.ElevatorLocation, GRElevator> elevatorByLocation;

	public List<GRElevator> allElevators;

	[SerializeField]
	private GRElevatorManager.ElevatorLocation destination;

	[SerializeField]
	private GRElevatorManager.ElevatorLocation currentLocation;

	private GRElevatorManager.ElevatorLocation lastTeleportSource = GRElevatorManager.ElevatorLocation.None;

	public GRElevatorManager.ElevatorSystemState currentState;

	private double timeLastTeleported;

	private bool cosmeticsInitialized;

	[SerializeField]
	private List<GRElevatorManager.GRShuttleGroup> shuttleGroups;

	public GRShuttle mainStagingShuttle;

	public GRShuttle mainDrillShuttle;

	private List<GRShuttle> allShuttles;

	public float destinationButtonlastPressedDelay = 3f;

	public float doorsFullyClosedDelay = 3f;

	public float doorMaxClosingDelay = 12f;

	public double destinationButtonLastPressedTime;

	public double doorsFullyClosedTime;

	public double maxDoorClosingTime;

	private List<int> actorIds;

	public CallLimitersList<CallLimiter, GRElevatorManager.RPC> m_RpcSpamChecks = new CallLimitersList<CallLimiter, GRElevatorManager.RPC>();

	private bool justTeleported;

	private bool waitingForRemoteTeleport;

	private int lastLowestActorNr;

	private RaycastHit[] correctionRaycastHit = new RaycastHit[1];

	public LayerMask correctionRaycastMask;

	private float waitForZoneLoadFallbackTimer;

	public float waitForZoneLoadFallbackMaxTime = 5f;

	[Serializable]
	public class GRShuttleGroup
	{
		public GRShuttleGroupLoc location;

		public List<GRShuttle> ghostReactorStagingShuttles;
	}

	public enum ElevatorSystemState
	{
		Dormant,
		InLocation,
		DestinationPressed,
		WaitingToTeleport,
		Teleporting,
		None
	}

	public enum RPC
	{
		RemoteElevatorButtonPress,
		RemoteActivateTeleport
	}

	public enum ElevatorLocation
	{
		Stump,
		City,
		GhostReactor,
		MonkeBlocks,
		None
	}
}
