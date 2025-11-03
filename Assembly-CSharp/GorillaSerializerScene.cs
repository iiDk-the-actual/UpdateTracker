using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

internal class GorillaSerializerScene : GorillaSerializer, IOnPhotonViewPreNetDestroy, IPhotonViewCallback
{
	internal bool HasAuthority
	{
		get
		{
			return this.photonView.IsMine;
		}
	}

	protected virtual void Start()
	{
		if (!this.targetComponent.IsNull())
		{
			IGorillaSerializeableScene gorillaSerializeableScene = this.targetComponent as IGorillaSerializeableScene;
			if (gorillaSerializeableScene != null)
			{
				gorillaSerializeableScene.OnSceneLinking(this);
				this.serializeTarget = gorillaSerializeableScene;
				this.sceneSerializeTarget = gorillaSerializeableScene;
				this.successfullInstantiate = true;
				this.photonView.AddCallbackTarget(this);
				return;
			}
		}
		Debug.LogError("GorillaSerializerscene: missing target component or invalid target", base.gameObject);
		base.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		if (!this.successfullInstantiate)
		{
			return;
		}
		if (!this.validDisable)
		{
			this.validDisable = true;
			return;
		}
		this.OnValidEnable();
	}

	protected virtual void OnValidEnable()
	{
		this.sceneSerializeTarget.OnNetworkObjectEnable();
	}

	private void OnDisable()
	{
		if (!this.successfullInstantiate || !this.validDisable)
		{
			return;
		}
		this.OnValidDisable();
	}

	protected virtual void OnValidDisable()
	{
		this.sceneSerializeTarget.OnNetworkObjectDisable();
	}

	public override void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		GorillaNot.instance.SendReport("bad net obj creation", info.Sender.UserId, info.Sender.NickName);
		if (info.photonView.IsMine)
		{
			PhotonNetwork.Destroy(info.photonView);
			return;
		}
		Object.Destroy(base.gameObject);
	}

	void IOnPhotonViewPreNetDestroy.OnPreNetDestroy(PhotonView rootView)
	{
		this.validDisable = false;
	}

	protected override bool ValidOnSerialize(PhotonStream stream, in PhotonMessageInfo info)
	{
		if (!this.transferrable)
		{
			return info.Sender == PhotonNetwork.MasterClient;
		}
		return base.ValidOnSerialize(stream, in info);
	}

	[SerializeField]
	private bool transferrable;

	[SerializeField]
	private MonoBehaviour targetComponent;

	private IGorillaSerializeableScene sceneSerializeTarget;

	protected bool validDisable = true;
}
