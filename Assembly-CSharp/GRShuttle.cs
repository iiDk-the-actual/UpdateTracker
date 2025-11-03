using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

public class GRShuttle : MonoBehaviour, IGorillaSliceableSimple
{
	public void Awake()
	{
		this.shuttleUI.Setup(null, null);
		if (this.entryCardScanner != null)
		{
			this.entryCardScanner.requireSpecificPlayer = true;
			this.entryCardScanner.restrictToPlayer = null;
		}
		if (this.departCardScanner != null)
		{
			this.departCardScanner.requireSpecificPlayer = true;
			this.departCardScanner.restrictToPlayer = null;
		}
		this.state = GRShuttleState.Docked;
	}

	public void OnEnable()
	{
		Debug.LogFormat("Shuttle Slice Register {0}", new object[] { this.shuttleId });
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		Debug.LogFormat("Shuttle Slice Unregister {0}", new object[] { this.shuttleId });
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void Init(int shuttleId)
	{
		this.shuttleId = shuttleId;
		this.StopMoveFx();
	}

	public void SetBay(GRBay bay)
	{
		this.shuttleBay = bay;
	}

	public void SetReactor(GhostReactor reactor)
	{
		this.reactor = reactor;
	}

	public void SetLocation(GRShuttleGroupLoc location)
	{
		this.location = location;
		this.targetSection = this.ClampTargetSection(this.targetSection);
	}

	public void Setup(GhostReactor reactor, GRShuttleGroupLoc location, int employeeIndex)
	{
		this.reactor = reactor;
		this.location = location;
		this.employeeIndex = employeeIndex;
		this.SetOwner(null);
		this.targetSection = this.ClampTargetSection(this.targetSection);
	}

	public int GetTargetFloor()
	{
		if (this.specificDestinationShuttle != null)
		{
			return this.specificDestinationShuttle.specificFloor;
		}
		if (this.targetSection < 0 || this.targetSection >= GRShuttle.sectionFloors.Length)
		{
			return 0;
		}
		return GRShuttle.sectionFloors[this.targetSection];
	}

	public GRShuttleState GetState()
	{
		return this.state;
	}

	public NetPlayer GetOwner()
	{
		return this.shuttleOwner;
	}

	public void SetOwner(NetPlayer player)
	{
		this.shuttleOwner = player;
		this.shuttleUI.Setup(this.reactor, player);
		this.entryCardScanner.restrictToPlayer = player;
		this.departCardScanner.restrictToPlayer = player;
		if (this.shuttleBay != null)
		{
			this.shuttleBay.Refresh();
		}
	}

	public void SliceUpdate()
	{
		this.UpdateState();
	}

	public void Refresh()
	{
		this.shuttleUI.RefreshUI();
	}

	public void JoinShuttleRoomLocalPlayer(GRShuttle sourceShuttle, GRShuttle destShuttle)
	{
	}

	public static void TeleportLocalPlayer(GRShuttle sourceShuttle, GRShuttle destShuttle)
	{
		sourceShuttle.friendCollider.RefreshPlayersInSphere();
		if (!sourceShuttle.friendCollider.playerIDsCurrentlyTouching.Contains(NetworkSystem.Instance.LocalPlayer.UserId))
		{
			return;
		}
		GTPlayer instance = GTPlayer.Instance;
		VRRig localRig = VRRig.LocalRig;
		float num = destShuttle.transform.rotation.eulerAngles.y - sourceShuttle.transform.rotation.eulerAngles.y;
		Vector3 vector = localRig.transform.position - instance.transform.position;
		Vector3 vector2 = sourceShuttle.transform.InverseTransformPoint(instance.transform.position);
		vector2.x *= 0.8f;
		vector2.z *= 0.8f;
		Vector3 vector3 = destShuttle.transform.TransformPoint(vector2);
		instance.TeleportTo(vector3, instance.transform.rotation, false, false);
		instance.turnParent.transform.RotateAround(instance.headCollider.transform.position, sourceShuttle.transform.up, num);
		localRig.transform.position = instance.transform.position + vector;
		instance.InitializeValues();
	}

	public void SetState(GRShuttleState newState, bool force = false)
	{
		if (this.state == newState && !force)
		{
			return;
		}
		switch (this.state)
		{
		case GRShuttleState.Docked:
			if (this.shuttleBay != null)
			{
				this.shuttleBay.Refresh();
			}
			break;
		case GRShuttleState.PostMove:
			if (this.specificDestinationShuttle != null)
			{
				this.OpenDoorLocal();
			}
			else
			{
				this.CloseDoorLocal();
			}
			break;
		case GRShuttleState.PostArrive:
			this.OpenDoorLocal();
			break;
		}
		this.state = newState;
		this.stateStartTime = Time.timeAsDouble;
		switch (this.state)
		{
		case GRShuttleState.Docked:
			if (this.shuttleBay != null)
			{
				this.shuttleBay.Refresh();
			}
			this.StopMoveFx();
			break;
		case GRShuttleState.PreMove:
			this.CloseDoorLocal();
			this.takeOffSound.Play(null);
			if (this.specificDestinationShuttle != null)
			{
				GRPlayer grplayer = GRPlayer.Get(GRElevatorManager.LowestActorNumberInElevator(this.friendCollider, this.specificDestinationShuttle.friendCollider));
				this.shuttleOwner = grplayer.gamePlayer.rig.OwningNetPlayer;
			}
			GRShuttle.TryStartLocalPlayerShuttleMove(this.shuttleId, this.shuttleOwner);
			this.StartMoveFx();
			return;
		case GRShuttleState.Moving:
			this.moveSound.Play(null);
			return;
		case GRShuttleState.PostMove:
			break;
		case GRShuttleState.Arriving:
			this.CloseDoorLocal();
			this.moveSound.Play(null);
			return;
		case GRShuttleState.PostArrive:
			this.landSound.Play(null);
			return;
		default:
			return;
		}
	}

	private void UpdateState()
	{
		double timeAsDouble = Time.timeAsDouble;
		switch (this.state)
		{
		case GRShuttleState.PreMove:
			if (timeAsDouble > this.stateStartTime + 1.0)
			{
				this.SetState(GRShuttleState.Moving, false);
				return;
			}
			break;
		case GRShuttleState.Moving:
			if (timeAsDouble > this.stateStartTime + 5.0)
			{
				this.SetState(GRShuttleState.PostMove, false);
				return;
			}
			break;
		case GRShuttleState.PostMove:
			if (timeAsDouble > this.stateStartTime + 1.0)
			{
				this.SetState(GRShuttleState.Docked, false);
				return;
			}
			break;
		case GRShuttleState.Arriving:
			if (timeAsDouble > this.stateStartTime + 2.0)
			{
				this.SetState(GRShuttleState.PostArrive, false);
				return;
			}
			break;
		case GRShuttleState.PostArrive:
			if (timeAsDouble > this.stateStartTime + 1.0)
			{
				this.SetState(GRShuttleState.Docked, false);
			}
			break;
		default:
			return;
		}
	}

	public void RequestArrival()
	{
		this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleArrive, this.shuttleId);
	}

	private void StartMoveFx()
	{
		if (this.windowFx != null)
		{
			this.windowFx.Play();
		}
		for (int i = 0; i < this.hideOnMove.Count; i++)
		{
			this.hideOnMove[i].SetActive(false);
		}
		for (int j = 0; j < this.showOnMove.Count; j++)
		{
			this.showOnMove[j].SetActive(true);
		}
	}

	private void StopMoveFx()
	{
		if (this.windowFx != null)
		{
			this.windowFx.Stop();
		}
		for (int i = 0; i < this.hideOnMove.Count; i++)
		{
			this.hideOnMove[i].SetActive(true);
		}
		for (int j = 0; j < this.showOnMove.Count; j++)
		{
			this.showOnMove[j].SetActive(false);
		}
	}

	public bool IsPodUnlocked()
	{
		if (this.specificDestinationShuttle != null)
		{
			return true;
		}
		if (this.shuttleOwner == null)
		{
			return false;
		}
		GRPlayer grplayer = GRPlayer.Get(this.shuttleOwner);
		return !(grplayer == null) && grplayer.IsDropPodUnlocked();
	}

	public int GetMaxDropFloor()
	{
		if (this.shuttleOwner == null)
		{
			return 0;
		}
		GRPlayer grplayer = GRPlayer.Get(this.shuttleOwner);
		if (grplayer == null)
		{
			return 0;
		}
		return grplayer.GetMaxDropFloor();
	}

	public void OnShuttleMove()
	{
		if (this.state != GRShuttleState.Docked)
		{
			return;
		}
		this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleLaunch, this.shuttleId);
	}

	public void OnShuttleMoveActorNr(int actorNr)
	{
		if (this.state != GRShuttleState.Docked || actorNr != this.shuttleOwner.ActorNumber || this.GetTargetFloor() > this.GetMaxDropFloor())
		{
			this.departCardScanner.onFailed.Invoke();
			return;
		}
		this.departCardScanner.onSucceeded.Invoke();
		this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleLaunch, this.shuttleId);
	}

	public void TargetLevelUp()
	{
		if (this.state != GRShuttleState.Docked)
		{
			return;
		}
		this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleTargetLevelUp, this.shuttleId);
	}

	public void TargetLevelDown()
	{
		if (this.state != GRShuttleState.Docked)
		{
			return;
		}
		this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleTargetLevelDown, this.shuttleId);
	}

	private GRShuttle GetTargetShuttle()
	{
		if (this.specificDestinationShuttle != null)
		{
			return this.specificDestinationShuttle;
		}
		if (this.shuttleOwner == null)
		{
			return null;
		}
		GRShuttle drillShuttleForPlayer = GRElevatorManager._instance.GetDrillShuttleForPlayer(this.shuttleOwner.ActorNumber);
		GRShuttle stagingShuttleForPlayer = GRElevatorManager._instance.GetStagingShuttleForPlayer(this.shuttleOwner.ActorNumber);
		if (this.location != GRShuttleGroupLoc.Drill)
		{
			return drillShuttleForPlayer;
		}
		return stagingShuttleForPlayer;
	}

	public bool IsPlayerOwner(GRPlayer player)
	{
		return GRPlayer.Get(this.GetOwner()) == player;
	}

	public bool IsShuttleInteractableByPlayer(GRPlayer player, bool ignoreOwnership)
	{
		if (!ignoreOwnership && !this.IsPlayerOwner(player) && this.specificDestinationShuttle == null)
		{
			return false;
		}
		if (this.entryCardScanner == null)
		{
			return true;
		}
		if (this.departCardScanner == null)
		{
			return true;
		}
		bool flag = GameEntityManager.IsPlayerHandNearPosition(player.gamePlayer, this.entryCardScanner.transform.position, false, true, 16f);
		bool flag2 = GameEntityManager.IsPlayerHandNearPosition(player.gamePlayer, this.departCardScanner.transform.position, false, true, 16f);
		return flag || flag2;
	}

	public bool IsPlayerOwner(NetPlayer player)
	{
		return this.GetOwner() == player;
	}

	public void ToggleDoor()
	{
		if (this.state != GRShuttleState.Docked)
		{
			return;
		}
		if (this.entryDoor.doorState == GRDoor.DoorState.Closed)
		{
			this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleOpen, this.shuttleId);
			return;
		}
		if (this.entryDoor.doorState == GRDoor.DoorState.Open)
		{
			double timeAsDouble = Time.timeAsDouble;
			if (timeAsDouble > this.lastCloseTime + 5.0)
			{
				this.lastCloseTime = timeAsDouble;
				this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleClose, this.shuttleId);
			}
		}
	}

	public void ToggleDoorActorNr(int actorNr)
	{
		if (this.state == GRShuttleState.Docked && this.GetOwner() != null && this.GetOwner().ActorNumber == actorNr && GRPlayer.Get(this.shuttleOwner).IsDropPodUnlocked())
		{
			IDCardScanner idcardScanner = this.entryCardScanner;
			if (idcardScanner != null)
			{
				idcardScanner.onSucceeded.Invoke();
			}
			this.ToggleDoor();
			return;
		}
		IDCardScanner idcardScanner2 = this.entryCardScanner;
		if (idcardScanner2 == null)
		{
			return;
		}
		idcardScanner2.onFailed.Invoke();
	}

	public void EmergencyOpenDoor()
	{
		if (this.state == GRShuttleState.Docked)
		{
			if (PhotonNetwork.InRoom)
			{
				this.reactor.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.ShuttleOpen, this.shuttleId);
				return;
			}
			this.OpenDoorLocal();
		}
	}

	public void OnOpenDoor()
	{
		if (this.entryDoor.doorState == GRDoor.DoorState.Closed && this.entryCardScanner != null)
		{
			this.entryCardScanner.onSucceeded.Invoke();
		}
		this.OpenDoorLocal();
	}

	public void OpenDoorLocal()
	{
		if (this.entryDoor != null && this.entryDoor.doorState == GRDoor.DoorState.Closed)
		{
			this.entryDoor.SetDoorState(GRDoor.DoorState.Open);
		}
		if (this.shuttleBay != null)
		{
			this.shuttleBay.SetOpen(true);
		}
	}

	public void CloseDoorLocal()
	{
		if (this.entryDoor != null && this.entryDoor.doorState == GRDoor.DoorState.Open)
		{
			this.entryDoor.SetDoorState(GRDoor.DoorState.Closed);
		}
	}

	public void OnCloseDoor()
	{
		if (this.entryDoor.doorState == GRDoor.DoorState.Open && this.entryCardScanner != null)
		{
			this.entryCardScanner.onSucceeded.Invoke();
		}
		this.CloseDoorLocal();
	}

	public void OnLaunch()
	{
		if (this.GetTargetFloor() > this.GetMaxDropFloor())
		{
			return;
		}
		this.SetState(GRShuttleState.PreMove, false);
		if (this.departCardScanner != null)
		{
			this.departCardScanner.onSucceeded.Invoke();
		}
	}

	public void OnArrive()
	{
		this.SetState(GRShuttleState.Arriving, false);
	}

	public void OnTargetLevelUp()
	{
		this.targetSection = this.ClampTargetSection(this.targetSection - 1);
		if (this.shuttleUI != null)
		{
			this.shuttleUI.RefreshUI();
		}
	}

	public void OnTargetLevelDown()
	{
		this.targetSection = this.ClampTargetSection(this.targetSection + 1);
		if (this.shuttleUI != null)
		{
			this.shuttleUI.RefreshUI();
		}
	}

	private int ClampTargetSection(int newTargetSection)
	{
		if (this.location == GRShuttleGroupLoc.Staging)
		{
			newTargetSection = Mathf.Clamp(newTargetSection, 1, GRShuttle.sectionFloors.Length - 1);
		}
		else
		{
			newTargetSection = 0;
		}
		return newTargetSection;
	}

	public static void TryStartLocalPlayerShuttleMove(int currShuttleId, NetPlayer shuttleOwner)
	{
		GRPlayer local = GRPlayer.GetLocal();
		if (local == null)
		{
			return;
		}
		GRShuttle shuttle = GRElevatorManager.GetShuttle(currShuttleId);
		if (shuttle == null)
		{
			return;
		}
		if (!GRElevatorManager.IsPlayerInShuttle(local.gamePlayer.rig.OwningNetPlayer.ActorNumber, shuttle, null))
		{
			return;
		}
		if (shuttleOwner != null && shuttleOwner.GetPlayerRef() != null)
		{
			local.shuttleData.ownerUserId = shuttleOwner.UserId;
		}
		else
		{
			local.shuttleData.ownerUserId = VRRig.LocalRig.OwningNetPlayer.UserId;
		}
		local.shuttleData.currShuttleId = currShuttleId;
		local.shuttleData.targetShuttleId = -1;
		local.shuttleData.targetLevel = shuttle.GetTargetFloor();
		GRShuttle.SetPlayerShuttleState(local, GRPlayer.ShuttleState.Moving);
	}

	public static void UpdateGRPlayerShuttle(GRPlayer player)
	{
		if (player == null)
		{
			return;
		}
		GRPlayer.ShuttleData shuttleData = player.shuttleData;
		if (shuttleData == null || shuttleData.state == GRPlayer.ShuttleState.Idle)
		{
			return;
		}
		if (!player.gamePlayer.IsLocal())
		{
			return;
		}
		double timeAsDouble = Time.timeAsDouble;
		double num = shuttleData.stateStartTime;
		if (shuttleData.state != GRPlayer.ShuttleState.Idle && timeAsDouble > num + 10.0)
		{
			GRShuttle.CancelPlayerShuttle(player);
			return;
		}
		switch (shuttleData.state)
		{
		case GRPlayer.ShuttleState.Moving:
			if (timeAsDouble > num + 3.0)
			{
				GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.JoinRoom);
				return;
			}
			break;
		case GRPlayer.ShuttleState.WaitForLeaveRoom:
			if (!PhotonNetwork.InRoom)
			{
				GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.WaitForLeadPlayer);
				return;
			}
			break;
		case GRPlayer.ShuttleState.JoinRoom:
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.WaitForLeadPlayer);
				return;
			}
			GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.WaitForLeaveRoom);
			return;
		case GRPlayer.ShuttleState.WaitForLeadPlayer:
			player.shuttleData.targetShuttleId = -1;
			if (PhotonNetwork.InRoom)
			{
				player.shuttleData.targetShuttleId = GRShuttle.CalcTargetShuttleId(player.shuttleData.currShuttleId, player.shuttleData.ownerUserId);
			}
			if (player.shuttleData.targetShuttleId != -1)
			{
				GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.Teleport);
				return;
			}
			break;
		case GRPlayer.ShuttleState.Teleport:
		{
			GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(GRElevatorManager.GetShuttle(player.shuttleData.targetShuttleId).zone);
			if (timeAsDouble > num + 1.0 && (managerForZone == null || managerForZone.IsZoneActive()))
			{
				int num2 = GRShuttle.CalcTargetShuttleId(player.shuttleData.currShuttleId, player.shuttleData.ownerUserId);
				if (num2 == player.shuttleData.targetShuttleId)
				{
					GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.PostTeleport);
					return;
				}
				if (num2 != -1)
				{
					player.shuttleData.currShuttleId = player.shuttleData.targetShuttleId;
					player.shuttleData.targetShuttleId = num2;
					GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.TeleportToMyShuttleSafety);
					return;
				}
			}
			break;
		}
		case GRPlayer.ShuttleState.TeleportToMyShuttleSafety:
			GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.PostTeleport);
			return;
		case GRPlayer.ShuttleState.PostTeleport:
			if (timeAsDouble > num + 1.0)
			{
				GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.Idle);
			}
			break;
		default:
			return;
		}
	}

	public static int CalcTargetShuttleId(int currShuttleId, string ownerUserId)
	{
		GRShuttle shuttle = GRElevatorManager.GetShuttle(currShuttleId);
		if (shuttle.specificDestinationShuttle != null)
		{
			return shuttle.specificDestinationShuttle.shuttleId;
		}
		GRPlayer fromUserId = GRPlayer.GetFromUserId(ownerUserId);
		if (fromUserId != null)
		{
			bool flag = shuttle.GetTargetFloor() >= 0;
			GRShuttle assignedShuttle = fromUserId.GetAssignedShuttle(flag);
			if (assignedShuttle != null)
			{
				return assignedShuttle.shuttleId;
			}
		}
		return -1;
	}

	public static void CancelPlayerShuttle(GRPlayer player)
	{
		GRPlayer.ShuttleState shuttleState = player.shuttleData.state;
		if (shuttleState - GRPlayer.ShuttleState.Moving > 3)
		{
			if (shuttleState - GRPlayer.ShuttleState.Teleport <= 2)
			{
				GRShuttle shuttle = GRElevatorManager.GetShuttle(player.shuttleData.targetShuttleId);
				if (shuttle != null)
				{
					shuttle.OpenDoorLocal();
				}
			}
		}
		else
		{
			GRShuttle shuttle2 = GRElevatorManager.GetShuttle(player.shuttleData.currShuttleId);
			if (shuttle2 != null)
			{
				shuttle2.OpenDoorLocal();
			}
		}
		GRShuttle.SetPlayerShuttleState(player, GRPlayer.ShuttleState.Idle);
	}

	public static void SetPlayerShuttleState(GRPlayer player, GRPlayer.ShuttleState newState)
	{
		GRPlayer.ShuttleData shuttleData = player.shuttleData;
		GRPlayer.ShuttleState shuttleState = shuttleData.state;
		shuttleData.state = newState;
		shuttleData.stateStartTime = Time.timeAsDouble;
		switch (shuttleData.state)
		{
		case GRPlayer.ShuttleState.Moving:
		case GRPlayer.ShuttleState.WaitForLeaveRoom:
		case GRPlayer.ShuttleState.WaitForLeadPlayer:
			break;
		case GRPlayer.ShuttleState.JoinRoom:
		{
			GRShuttle shuttle = GRElevatorManager.GetShuttle(player.shuttleData.currShuttleId);
			GRShuttle targetShuttle = shuttle.GetTargetShuttle();
			if (targetShuttle != null && !NetworkSystem.Instance.SessionIsPrivate && shuttle.shuttleOwner.ActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				GRElevatorManager.LeadShuttleJoin(shuttle.friendCollider, targetShuttle.friendCollider, targetShuttle.joinTrigger, shuttle.GetTargetFloor());
				return;
			}
			break;
		}
		case GRPlayer.ShuttleState.Teleport:
		{
			GRShuttle shuttle2 = GRElevatorManager.GetShuttle(player.shuttleData.currShuttleId);
			GRShuttle shuttle3 = GRElevatorManager.GetShuttle(player.shuttleData.targetShuttleId);
			if (shuttle3 != null)
			{
				GRShuttle.TeleportLocalPlayer(shuttle2, shuttle3);
				shuttle3.CloseDoorLocal();
				return;
			}
			break;
		}
		case GRPlayer.ShuttleState.TeleportToMyShuttleSafety:
		{
			GRShuttle shuttle4 = GRElevatorManager.GetShuttle(player.shuttleData.currShuttleId);
			GRShuttle shuttle5 = GRElevatorManager.GetShuttle(player.shuttleData.targetShuttleId);
			if (shuttle5 != null)
			{
				GRShuttle.TeleportLocalPlayer(shuttle4, shuttle5);
				shuttle5.CloseDoorLocal();
				return;
			}
			break;
		}
		case GRPlayer.ShuttleState.PostTeleport:
		{
			GRShuttle shuttle6 = GRElevatorManager.GetShuttle(player.shuttleData.targetShuttleId);
			if (shuttle6 != null)
			{
				shuttle6.RequestArrival();
			}
			break;
		}
		default:
			return;
		}
	}

	public const int InvalidId = -1;

	private const int MAX_DEPTH = 29;

	public GTZone zone;

	public GRShuttleUI shuttleUI;

	public GRDoor entryDoor;

	private GRShuttleGroupLoc location;

	private int employeeIndex;

	public AbilitySound takeOffSound;

	public AbilitySound moveSound;

	public AbilitySound landSound;

	public GorillaFriendCollider friendCollider;

	public GorillaNetworkJoinTrigger joinTrigger;

	public GRShuttle specificDestinationShuttle;

	public int specificFloor = -1;

	public ParticleSystem windowFx;

	public List<GameObject> hideOnMove;

	public List<GameObject> showOnMove;

	public BoxCollider inShuttleVolume;

	public IDCardScanner entryCardScanner;

	public IDCardScanner departCardScanner;

	[NonSerialized]
	public int shuttleId;

	private GhostReactor reactor;

	private int targetSection;

	private GRShuttleState state;

	private double stateStartTime;

	private GRBay shuttleBay;

	private NetPlayer shuttleOwner;

	private double lastCloseTime;

	private static int[] sectionFloors = new int[] { -1, 0, 4, 9, 14, 19, 24, 29 };
}
