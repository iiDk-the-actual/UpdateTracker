using System;
using Fusion;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkSceneObject : SimulationBehaviour
{
	public bool IsMine
	{
		get
		{
			return this.photonView.IsMine;
		}
	}

	protected virtual void Start()
	{
		if (this.photonView == null)
		{
			this.photonView = base.GetComponent<PhotonView>();
		}
	}

	protected virtual void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
	}

	protected virtual void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
	}

	private void RegisterOnRunner()
	{
		NetworkRunner runner = (NetworkSystem.Instance as NetworkSystemFusion).runner;
		if (runner != null && runner.IsRunning)
		{
			runner.AddGlobal(this);
		}
	}

	private void RemoveFromRunner()
	{
		NetworkRunner runner = (NetworkSystem.Instance as NetworkSystemFusion).runner;
		if (runner != null && runner.IsRunning)
		{
			runner.RemoveGlobal(this);
		}
	}

	public PhotonView photonView;
}
