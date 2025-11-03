using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Sockets;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;

internal class RequestableOwnershipGaurdHandler : IPunOwnershipCallbacks, IInRoomCallbacks, INetworkRunnerCallbacks, IPublicFacingInterface
{
	static RequestableOwnershipGaurdHandler()
	{
		PhotonNetwork.AddCallbackTarget(RequestableOwnershipGaurdHandler.callbackInstance);
	}

	internal static void RegisterView(NetworkView view, RequestableOwnershipGuard guard)
	{
		if (view == null || RequestableOwnershipGaurdHandler.gaurdedViews.Contains(view))
		{
			return;
		}
		RequestableOwnershipGaurdHandler.gaurdedViews.Add(view);
		RequestableOwnershipGaurdHandler.guardingLookup.Add(view, guard);
	}

	internal static void RemoveView(NetworkView view)
	{
		if (view == null)
		{
			return;
		}
		RequestableOwnershipGaurdHandler.gaurdedViews.Remove(view);
		RequestableOwnershipGaurdHandler.guardingLookup.Remove(view);
	}

	internal static void RegisterViews(NetworkView[] views, RequestableOwnershipGuard guard)
	{
		for (int i = 0; i < views.Length; i++)
		{
			RequestableOwnershipGaurdHandler.RegisterView(views[i], guard);
		}
	}

	public static void RemoveViews(NetworkView[] views, RequestableOwnershipGuard guard)
	{
		for (int i = 0; i < views.Length; i++)
		{
			RequestableOwnershipGaurdHandler.RemoveView(views[i]);
		}
	}

	void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
		NetworkView networkView = RequestableOwnershipGaurdHandler.gaurdedViews.FirstOrDefault((NetworkView p) => p.GetView == targetView);
		RequestableOwnershipGuard requestableOwnershipGuard;
		if (networkView.IsNull() || !RequestableOwnershipGaurdHandler.guardingLookup.TryGetValue(networkView, out requestableOwnershipGuard) || requestableOwnershipGuard.IsNull())
		{
			return;
		}
		NetPlayer currentOwner = requestableOwnershipGuard.currentOwner;
		Player player = ((currentOwner != null) ? currentOwner.GetPlayerRef() : null);
		int num = ((player != null) ? player.ActorNumber : 0);
		if (num == 0 || previousOwner != player)
		{
			GTDev.LogWarning<string>("Ownership transferred but the previous owner didn't initiate the request, Switching back", null);
			targetView.OwnerActorNr = num;
			targetView.ControllerActorNr = num;
		}
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
		this.OnHostChangedShared();
	}

	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
		this.OnHostChangedShared();
	}

	private void OnHostChangedShared()
	{
		foreach (NetworkView networkView in RequestableOwnershipGaurdHandler.gaurdedViews)
		{
			RequestableOwnershipGuard requestableOwnershipGuard;
			if (!RequestableOwnershipGaurdHandler.guardingLookup.TryGetValue(networkView, out requestableOwnershipGuard))
			{
				break;
			}
			if (networkView.Owner != null && requestableOwnershipGuard.currentOwner != null && !object.Equals(networkView.Owner, requestableOwnershipGuard.currentOwner))
			{
				networkView.OwnerActorNr = requestableOwnershipGuard.currentOwner.ActorNumber;
				networkView.ControllerActorNr = requestableOwnershipGuard.currentOwner.ActorNumber;
			}
		}
	}

	void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
	}

	void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
	{
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
	}

	public void OnConnectedToServer(NetworkRunner runner)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
	}

	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
	}

	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
	}

	public void OnSceneLoadStart(NetworkRunner runner)
	{
	}

	private static HashSet<NetworkView> gaurdedViews = new HashSet<NetworkView>();

	private static readonly RequestableOwnershipGaurdHandler callbackInstance = new RequestableOwnershipGaurdHandler();

	private static Dictionary<NetworkView, RequestableOwnershipGuard> guardingLookup = new Dictionary<NetworkView, RequestableOwnershipGuard>();
}
