using System;
using System.Collections;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Modio.Mods;
using Photon.Pun;
using UnityEngine;

internal class VirtualStumpSerializer : GorillaSerializer
{
	internal bool HasAuthority
	{
		get
		{
			return this.photonView.IsMine;
		}
	}

	protected void Start()
	{
		NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
		NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnLeftRoom;
		NetworkSystem.Instance.OnPlayerLeft += this.OnPlayerLeftRoom;
	}

	private void OnPlayerLeftRoom(NetPlayer leavingPlayer)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		int driverID = CustomMapsTerminal.GetDriverID();
		if (leavingPlayer.ActorNumber == driverID)
		{
			CustomMapsTerminal.SetTerminalControlStatus(false, -2, true);
		}
	}

	private void OnJoinedRoom()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			VirtualStumpSerializer.roomInitialized = true;
			return;
		}
		Debug.Log("[VirtualStumpSerializer::OnJoinedRoom] Requesting Room Initialization...");
		VirtualStumpSerializer.waitingForRoomInitialization = true;
		base.SendRPC("RequestRoomInitialization_RPC", false, Array.Empty<object>());
	}

	private void OnLeftRoom()
	{
		Debug.Log("[VirtualStumpSerializer::OnLeftRoom]...");
		VirtualStumpSerializer.roomInitialized = false;
	}

	public static bool IsWaitingForRoomInit()
	{
		return VirtualStumpSerializer.waitingForRoomInitialization || !VirtualStumpSerializer.roomInitialized;
	}

	[PunRPC]
	private void RequestRoomInitialization_RPC(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RequestRoomInitialization_RPC");
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (player.CheckSingleCallRPC(NetPlayer.SingleCallRPC.CMS_RequestRoomInitialization))
		{
			return;
		}
		player.ReceivedSingleCallRPC(NetPlayer.SingleCallRPC.CMS_RequestRoomInitialization);
		long id = CustomMapManager.GetRoomMapId()._id;
		base.SendRPC("InitializeRoom_RPC", info.Sender, new object[]
		{
			CustomMapsTerminal.CurrentScreen,
			CustomMapsTerminal.GetDriverID(),
			CustomMapsTerminal.LocalModDetailsID,
			id
		});
	}

	[PunRPC]
	private void InitializeRoom_RPC(int currentScreen, int driverID, long modDetailsID, long loadedMapModID, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "InitializeRoom_RPC");
		if (!info.Sender.IsMasterClient || !VirtualStumpSerializer.waitingForRoomInitialization)
		{
			return;
		}
		if (driverID != -2 && NetworkSystem.Instance.GetPlayer(driverID) == null)
		{
			return;
		}
		CustomMapsTerminal.UpdateFromDriver(currentScreen, modDetailsID, driverID);
		if (loadedMapModID > 0L)
		{
			CustomMapManager.SetRoomMap(loadedMapModID);
		}
		VirtualStumpSerializer.roomInitialized = true;
		VirtualStumpSerializer.waitingForRoomInitialization = false;
		Debug.Log("[VStumpSerializer.InitializeRPC] Room initialization finished.");
	}

	public void LoadMapSynced(long modId)
	{
		CustomMapManager.SetRoomMap(modId);
		CustomMapManager.LoadMap(new ModId(modId));
		if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.SessionIsPrivate)
		{
			base.SendRPC("SetRoomMap_RPC", true, new object[] { modId });
		}
	}

	public void UnloadMapSynced()
	{
		CustomMapManager.UnloadMap(true);
		if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.SessionIsPrivate)
		{
			base.SendRPC("UnloadMap_RPC", true, Array.Empty<object>());
		}
	}

	[PunRPC]
	private void SetRoomMap_RPC(long modId, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "SetRoomMap_RPC");
		if (modId <= 0L)
		{
			return;
		}
		if (info.Sender.ActorNumber != this.photonView.OwnerActorNr && info.Sender.ActorNumber != CustomMapsTerminal.GetDriverID())
		{
			return;
		}
		if (modId != this.detailsScreen.currentMapMod.Id._id)
		{
			return;
		}
		CustomMapManager.SetRoomMap(modId);
	}

	[PunRPC]
	private void UnloadMap_RPC(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "UnloadMap_RPC");
		if (info.Sender.ActorNumber != CustomMapsTerminal.GetDriverID())
		{
			return;
		}
		if (!CustomMapManager.AreAllPlayersInVirtualStump())
		{
			return;
		}
		CustomMapManager.UnloadMap(true);
	}

	public void RequestTerminalControlStatusChange(bool lockedStatus)
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		base.SendRPC("RequestTerminalControlStatusChange_RPC", false, new object[] { lockedStatus });
	}

	[PunRPC]
	private void RequestTerminalControlStatusChange_RPC(bool lockedStatus, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RequestTerminalControlStatusChange_RPC");
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[19].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		if (!player.IsNull && CustomMapManager.IsRemotePlayerInVirtualStump(info.Sender.UserId))
		{
			CustomMapsTerminal.HandleTerminalControlStatusChangeRequest(lockedStatus, info.Sender.ActorNumber);
		}
	}

	public void SetTerminalControlStatus(bool locked, int playerID)
	{
		if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		base.SendRPC("SetTerminalControlStatus_RPC", true, new object[] { locked, playerID });
	}

	[PunRPC]
	private void SetTerminalControlStatus_RPC(bool locked, int driverID, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "SetTerminalControlStatus_RPC");
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		if (driverID != -2 && NetworkSystem.Instance.GetPlayer(driverID) == null)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[16].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		CustomMapsTerminal.SetTerminalControlStatus(locked, driverID, false);
	}

	public void SendTerminalStatus()
	{
		if (!NetworkSystem.Instance.InRoom || !CustomMapsTerminal.IsDriver)
		{
			return;
		}
		if (this.statusUpdateCoroutine != null)
		{
			base.StopCoroutine(this.statusUpdateCoroutine);
		}
		this.statusUpdateCoroutine = base.StartCoroutine(this.WaitToSendStatus());
	}

	private IEnumerator WaitToSendStatus()
	{
		yield return new WaitForSeconds(0.5f);
		base.SendRPC("UpdateScreen_RPC", true, new object[]
		{
			CustomMapsTerminal.CurrentScreen,
			CustomMapsTerminal.LocalModDetailsID,
			CustomMapsTerminal.GetDriverID()
		});
		yield break;
	}

	[PunRPC]
	private void UpdateScreen_RPC(int currentScreen, long modDetailsID, int driverID, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "UpdateScreen_RPC");
		if (info.Sender.ActorNumber != CustomMapsTerminal.GetDriverID() || !CustomMapManager.IsRemotePlayerInVirtualStump(info.Sender.UserId))
		{
			return;
		}
		if (currentScreen < -1 || currentScreen > 6)
		{
			return;
		}
		if (NetworkSystem.Instance.GetPlayer(driverID) == null)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[17].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		CustomMapsTerminal.UpdateFromDriver(currentScreen, modDetailsID, driverID);
	}

	public void RefreshDriverNickName()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		base.SendRPC("RefreshDriverNickName_RPC", true, Array.Empty<object>());
	}

	[PunRPC]
	private void RefreshDriverNickName_RPC(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RefreshDriverNickName_RPC");
		if (info.Sender.ActorNumber != CustomMapsTerminal.GetDriverID())
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[18].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		CustomMapsTerminal.RefreshDriverNickName();
	}

	[SerializeField]
	private VirtualStumpBarrierSFX barrierSFX;

	[SerializeField]
	private CustomMapsDisplayScreen detailsScreen;

	private static bool waitingForRoomInitialization;

	private static bool roomInitialized;

	private bool sendModList;

	private bool forceNewSearch;

	private bool waitToSendStatus;

	private bool sendNewStatus;

	private const float STATUS_UPDATE_INTERVAL = 0.5f;

	private Coroutine statusUpdateCoroutine;
}
