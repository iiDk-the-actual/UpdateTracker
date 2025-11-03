using System;
using System.Collections.Generic;
using Fusion;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTag;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public class WorldShareableItem : NetworkComponent, IRequestableOwnershipGuardCallbacks
{
	[DevInspectorShow]
	public TransferrableObject.PositionState transferableObjectState { get; set; }

	public TransferrableObject.ItemStates transferableObjectItemState { get; set; }

	public TransferrableObject.PositionState transferableObjectStateNetworked { get; set; }

	public TransferrableObject.ItemStates transferableObjectItemStateNetworked { get; set; }

	[DevInspectorShow]
	public WorldTargetItem target
	{
		get
		{
			return this._target;
		}
		set
		{
			this._target = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.guard = base.GetComponent<RequestableOwnershipGuard>();
		this.teleportSerializer = base.GetComponent<TransformViewTeleportSerializer>();
		NetworkSystem.Instance.RegisterSceneNetworkItem(base.gameObject);
	}

	internal override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		if (GTAppState.isQuitting)
		{
			return;
		}
		base.OnEnable();
		this.guard.AddCallbackTarget(this);
		WorldShareableItemManager.Register(this);
		NetworkSystem.Instance.RegisterSceneNetworkItem(base.gameObject);
	}

	internal override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		if (this.target == null || !this.target.transferrableObject.isSceneObject)
		{
			return;
		}
		PhotonView[] components = base.GetComponents<PhotonView>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].ViewID = 0;
		}
		this.transferableObjectState = TransferrableObject.PositionState.None;
		this.transferableObjectItemState = TransferrableObject.ItemStates.State0;
		this.guard.RemoveCallbackTarget(this);
		this.rpcCallBack = null;
		this.onOwnerChangeCb = null;
		WorldShareableItemManager.Unregister(this);
	}

	public void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		WorldShareableItemManager.Unregister(this);
	}

	public void SetupSharableViewIDs(NetPlayer player, int slotID)
	{
		PhotonView[] components = base.GetComponents<PhotonView>();
		PhotonView photonView = components[0];
		PhotonView photonView2 = components[1];
		int num = player.ActorNumber * 1000 + 990 + slotID * 2;
		this.guard.giveCreatorAbsoluteAuthority = true;
		if (num != photonView.ViewID)
		{
			photonView.ViewID = player.ActorNumber * 1000 + 990 + slotID * 2;
			photonView2.ViewID = player.ActorNumber * 1000 + 990 + slotID * 2 + 1;
			this.guard.SetCreator(player);
		}
	}

	public void ResetViews()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		PhotonView[] components = base.GetComponents<PhotonView>();
		PhotonView photonView = components[0];
		PhotonView photonView2 = components[1];
		photonView.ViewID = 0;
		photonView2.ViewID = 0;
	}

	public void SetupSharableObject(int itemIDx, NetPlayer owner, Transform targetXform)
	{
		if (this.target != null)
		{
			Debug.LogError("ERROR!!!  WorldShareableItem.SetupSharableObject: target is expected to be null before this call. In scene path = \"" + base.transform.GetPathQ() + "\"", this);
			return;
		}
		this.target = WorldTargetItem.GenerateTargetFromPlayerAndID(owner, itemIDx);
		if (this.target.targetObject != targetXform)
		{
			Debug.LogError(string.Format("The target object found a transform that does not match the target transform, this should never happen. owner: {0} itemIDx: {1} targetXformPath: {2}, target.targetObject: {3}", new object[]
			{
				owner,
				itemIDx,
				targetXform.GetPath(),
				this.target.targetObject.GetPath()
			}));
		}
		TransferrableObject component = this.target.targetObject.GetComponent<TransferrableObject>();
		this.validShareable = component.canDrop || component.shareable || component.allowWorldSharableInstance;
		if (!this.validShareable)
		{
			Debug.LogError(string.Format("tried to setup an invalid shareable {0} {1} {2}", owner, itemIDx, targetXform.GetPath()));
			base.gameObject.SetActive(false);
			this.Invalidate();
			return;
		}
		this.guard.AddCallbackTarget(component);
		this.guard.giveCreatorAbsoluteAuthority = true;
		component.SetWorldShareableItem(this);
	}

	public override void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		base.OnPhotonInstantiate(info);
	}

	public override void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		if (this.onOwnerChangeCb != null)
		{
			NetPlayer player = NetworkSystem.Instance.GetPlayer(newOwner);
			NetPlayer player2 = NetworkSystem.Instance.GetPlayer(previousOwner);
			this.onOwnerChangeCb(player, player2);
		}
	}

	[DevInspectorShow]
	public bool EnableRemoteSync
	{
		get
		{
			return this.enableRemoteSync;
		}
		set
		{
			this.enableRemoteSync = value;
		}
	}

	public void TriggeredUpdate()
	{
		if (!this.IsTargetValid())
		{
			return;
		}
		if (this.guard.isTrulyMine)
		{
			Vector3 vector;
			Quaternion quaternion;
			this.target.targetObject.GetPositionAndRotation(out vector, out quaternion);
			base.transform.SetPositionAndRotation(vector, quaternion);
			return;
		}
		if (!base.IsMine && this.EnableRemoteSync)
		{
			Vector3 vector2;
			Quaternion quaternion2;
			base.transform.GetPositionAndRotation(out vector2, out quaternion2);
			this.target.targetObject.SetPositionAndRotation(vector2, quaternion2);
		}
	}

	public void SyncToSceneObject(TransferrableObject transferrableObject)
	{
		this.target = WorldTargetItem.GenerateTargetFromWorldSharableItem(null, -2, transferrableObject.transform);
		base.transform.parent = null;
	}

	public void SetupSceneObjectOnNetwork(NetPlayer owner)
	{
		this.guard.SetOwnership(owner, false, false);
	}

	public bool IsTargetValid()
	{
		return this.target != null;
	}

	public void Invalidate()
	{
		this.target = null;
		this.transferableObjectState = TransferrableObject.PositionState.None;
		this.transferableObjectItemState = TransferrableObject.ItemStates.State0;
	}

	public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
		if (toPlayer == null)
		{
			return;
		}
		WorldShareableItem.CachedData cachedData;
		if (this.cachedDatas.TryGetValue(toPlayer, out cachedData))
		{
			this.transferableObjectState = cachedData.cachedTransferableObjectState;
			this.transferableObjectItemState = cachedData.cachedTransferableObjectItemState;
			this.cachedDatas.Remove(toPlayer);
		}
	}

	public override void WriteDataFusion()
	{
		this.transferableObjectItemStateNetworked = this.transferableObjectItemState;
		this.transferableObjectStateNetworked = this.transferableObjectState;
	}

	public override void ReadDataFusion()
	{
		this.transferableObjectItemState = this.transferableObjectItemStateNetworked;
		this.transferableObjectState = this.transferableObjectStateNetworked;
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(this.transferableObjectState);
		stream.SendNext(this.transferableObjectItemState);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (player != this.guard.actualOwner)
		{
			Debug.Log("Blocking info from non owner");
			this.cachedDatas.AddOrUpdate(player, new WorldShareableItem.CachedData
			{
				cachedTransferableObjectState = (TransferrableObject.PositionState)stream.ReceiveNext(),
				cachedTransferableObjectItemState = (TransferrableObject.ItemStates)stream.ReceiveNext()
			});
			return;
		}
		this.transferableObjectState = (TransferrableObject.PositionState)stream.ReceiveNext();
		this.transferableObjectItemState = (TransferrableObject.ItemStates)stream.ReceiveNext();
	}

	[PunRPC]
	internal void RPCWorldShareable(PhotonMessageInfo info)
	{
		NetworkSystem.Instance.GetPlayer(info.Sender);
		GorillaNot.IncrementRPCCall(info, "RPCWorldShareable");
		if (this.rpcCallBack == null)
		{
			return;
		}
		this.rpcCallBack();
	}

	public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		return true;
	}

	public void OnMyCreatorLeft()
	{
	}

	public bool OnOwnershipRequest(NetPlayer fromPlayer)
	{
		return true;
	}

	public void OnMyOwnerLeft()
	{
	}

	public void SetWillTeleport()
	{
		this.teleportSerializer.SetWillTeleport();
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

	private bool validShareable = true;

	public RequestableOwnershipGuard guard;

	private TransformViewTeleportSerializer teleportSerializer;

	[DevInspectorShow]
	[CanBeNull]
	private WorldTargetItem _target;

	public WorldShareableItem.OnOwnerChangeDelegate onOwnerChangeCb;

	public Action rpcCallBack;

	private bool enableRemoteSync = true;

	public Dictionary<NetPlayer, WorldShareableItem.CachedData> cachedDatas = new Dictionary<NetPlayer, WorldShareableItem.CachedData>();

	public delegate void Delegate();

	public delegate void OnOwnerChangeDelegate(NetPlayer newOwner, NetPlayer prevOwner);

	public struct CachedData
	{
		public TransferrableObject.PositionState cachedTransferableObjectState;

		public TransferrableObject.ItemStates cachedTransferableObjectItemState;
	}
}
