using System;
using ExitGames.Client.Photon;
using Fusion;
using Photon.Pun;
using Photon.Realtime;

[NetworkBehaviourWeaved(0)]
public abstract class NetworkComponent : NetworkView, IPunObservable, IStateAuthorityChanged, IPublicFacingInterface, IOnPhotonViewOwnerChange, IPhotonViewCallback, IInRoomCallbacks, IPunInstantiateMagicCallback
{
	internal virtual void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		this.AddToNetwork();
	}

	internal virtual void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	protected override void Start()
	{
		base.Start();
		this.AddToNetwork();
	}

	private void AddToNetwork()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void Spawned()
	{
		if (NetworkSystem.Instance.InRoom)
		{
			this.OnSpawned();
		}
	}

	public override void FixedUpdateNetwork()
	{
		this.WriteDataFusion();
	}

	public override void Render()
	{
		if (!base.HasStateAuthority)
		{
			this.ReadDataFusion();
		}
	}

	public abstract void WriteDataFusion();

	public abstract void ReadDataFusion();

	public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		this.OnSpawned();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			this.WriteDataPUN(stream, info);
			return;
		}
		if (stream.IsReading)
		{
			this.ReadDataPUN(stream, info);
		}
	}

	protected abstract void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info);

	protected abstract void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info);

	public virtual void OnSpawned()
	{
	}

	protected virtual void OnOwnerSwitched(NetPlayer newOwningPlayer)
	{
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
		this.OnOwnerSwitched(NetworkSystem.Instance.GetPlayer(newMasterClient));
	}

	public override void StateAuthorityChanged()
	{
		base.StateAuthorityChanged();
		if (base.Object == null)
		{
			return;
		}
		if (base.Object.StateAuthority == default(PlayerRef))
		{
			return;
		}
		if (NetworkSystem.Instance.InRoom)
		{
			this.OnOwnerSwitched(NetworkSystem.Instance.GetPlayer(base.Object.StateAuthority));
			return;
		}
		this.OnOwnerSwitched(NetworkSystem.Instance.LocalPlayer);
	}

	public void OnMasterClientSwitch(NetPlayer newMaster)
	{
		this.StateAuthorityChanged();
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public virtual void OnOwnerChange(Player newOwner, Player previousOwner)
	{
	}

	public bool IsLocallyOwned
	{
		get
		{
			return base.IsMine;
		}
	}

	public bool ShouldWriteObjectData
	{
		get
		{
			return NetworkSystem.Instance.ShouldWriteObjectData(base.gameObject);
		}
	}

	public bool ShouldUpdateobject
	{
		get
		{
			return NetworkSystem.Instance.ShouldUpdateObject(base.gameObject);
		}
	}

	public int OwnerID
	{
		get
		{
			return NetworkSystem.Instance.GetOwningPlayerID(base.gameObject);
		}
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
}
