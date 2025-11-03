using System;
using Fusion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(PhotonView), typeof(NetworkObject))]
[NetworkBehaviourWeaved(0)]
public class NetworkView : NetworkBehaviour, IStateAuthorityChanged, IPublicFacingInterface, IPunOwnershipCallbacks
{
	public bool IsMine
	{
		get
		{
			return this.punView != null && this.punView.IsMine;
		}
	}

	public bool IsValid
	{
		get
		{
			return this.punView != null;
		}
	}

	public bool HasView
	{
		get
		{
			return this.punView != null;
		}
	}

	public bool IsRoomView
	{
		get
		{
			return this.punView.IsRoomView;
		}
	}

	public PhotonView GetView
	{
		get
		{
			return this.punView;
		}
	}

	public NetPlayer Owner
	{
		get
		{
			return NetworkSystem.Instance.GetPlayer(this.punView.Owner);
		}
	}

	public int ViewID
	{
		get
		{
			return this.punView.ViewID;
		}
	}

	internal OwnershipOption OwnershipTransfer
	{
		get
		{
			return this.punView.OwnershipTransfer;
		}
		set
		{
			this.punView.OwnershipTransfer = value;
			if (this.reliableView != null)
			{
				this.reliableView.OwnershipTransfer = value;
			}
		}
	}

	public int OwnerActorNr
	{
		get
		{
			return this.punView.OwnerActorNr;
		}
		set
		{
			this.punView.OwnerActorNr = value;
			if (this.reliableView != null)
			{
				this.reliableView.OwnerActorNr = value;
			}
		}
	}

	public int ControllerActorNr
	{
		get
		{
			return this.punView.ControllerActorNr;
		}
		set
		{
			this.punView.ControllerActorNr = value;
			if (this.reliableView != null)
			{
				this.reliableView.ControllerActorNr = value;
			}
		}
	}

	private void GetViews()
	{
		PhotonView[] components = base.GetComponents<PhotonView>();
		if (components.Length > 1)
		{
			if (components[0].Synchronization == ViewSynchronization.UnreliableOnChange)
			{
				this.punView = components[0];
				this.reliableView = components[1];
			}
			else if (components[0].Synchronization == ViewSynchronization.ReliableDeltaCompressed)
			{
				this.reliableView = components[0];
				this.punView = components[1];
			}
		}
		else
		{
			this.punView = components[0];
		}
		if (this.punView == null)
		{
			this.punView = base.GetComponent<PhotonView>();
		}
		if (this.fusionView == null)
		{
			this.fusionView = base.GetComponent<NetworkObject>();
		}
	}

	protected virtual void Awake()
	{
		this.GetViews();
	}

	protected virtual void Start()
	{
		if (this._sceneObject)
		{
			NetworkSystem.Instance.RegisterSceneNetworkItem(base.gameObject);
		}
	}

	public void SendRPC(string method, NetPlayer targetPlayer, params object[] parameters)
	{
		Player playerRef = (targetPlayer as PunNetPlayer).PlayerRef;
		this.punView.RPC(method, playerRef, parameters);
	}

	public void SendRPC(string method, RpcTarget target, params object[] parameters)
	{
		this.punView.RPC(method, target, parameters);
	}

	public void SendRPC(string method, int target, params object[] parameters)
	{
		Room currentRoom = PhotonNetwork.CurrentRoom;
		if (currentRoom == null || !currentRoom.Players.ContainsKey(target))
		{
			return;
		}
		this.punView.RPC(method, currentRoom.Players[target], parameters);
	}

	public override void Spawned()
	{
		base.Spawned();
		this._spawned = true;
	}

	public void RequestOwnership()
	{
		this.GetView.RequestOwnership();
	}

	public void ReleaseOwnership()
	{
		this.changingStatAuth = true;
		base.Object.ReleaseStateAuthority();
	}

	public virtual void StateAuthorityChanged()
	{
		if (this.changingStatAuth)
		{
			this.changingStatAuth = false;
		}
	}

	public virtual void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
	}

	public virtual void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
	}

	public virtual void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
	{
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
	}

	[SerializeField]
	private PhotonView punView;

	[SerializeField]
	private PhotonView reliableView;

	[SerializeField]
	internal NetworkObject fusionView;

	[SerializeField]
	protected bool _sceneObject;

	private bool _spawned;

	private bool changingStatAuth;
}
