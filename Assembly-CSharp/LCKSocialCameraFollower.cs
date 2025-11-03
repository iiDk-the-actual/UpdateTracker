using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag;
using Liv.Lck.GorillaTag;
using UnityEngine;

public class LCKSocialCameraFollower : MonoBehaviour, ITickSystemTick
{
	public Transform ScaleTransform
	{
		get
		{
			return this._scaleTransform;
		}
	}

	public CoconutCamera CoconutCamera
	{
		get
		{
			return this._coconutCamera;
		}
	}

	public List<GameObject> VisualObjects
	{
		get
		{
			return this._visualObjects;
		}
	}

	private void Awake()
	{
		if (this.m_rigContainer.Rig.isOfflineVRRig)
		{
			base.gameObject.SetActive(false);
			return;
		}
		ListProcessor<Action<RigContainer>> disableEvent = this.m_rigContainer.RigEvents.disableEvent;
		Action<RigContainer> action = new Action<RigContainer>(this.PreRigDisable);
		disableEvent.Add(in action);
		ListProcessor<Action<RigContainer>> enableEvent = this.m_rigContainer.RigEvents.enableEvent;
		action = new Action<RigContainer>(this.PostRigEnable);
		enableEvent.Add(in action);
	}

	private void Start()
	{
		base.transform.parent = null;
	}

	private void PostRigEnable(RigContainer _)
	{
		base.gameObject.SetActive(true);
		this._coconutCamera.SetVisualsActive(false);
		this._coconutCamera.SetRecordingState(false);
	}

	private void PreRigDisable(RigContainer _)
	{
		base.gameObject.SetActive(false);
	}

	public void SetNetworkController(LckSocialCamera networkController)
	{
		if (this.m_networkController.IsNotNull() && this.m_networkController != networkController)
		{
			this.m_networkController.TurnOff();
		}
		this.m_networkController = networkController;
		this.m_transformToFollow = this.m_networkController.transform;
		TickSystem<object>.AddTickCallback(this);
	}

	public void RemoveNetworkController(LckSocialCamera networkController)
	{
		if (this.m_networkController != networkController)
		{
			return;
		}
		this.m_transformToFollow = null;
		this.m_networkController = null;
		TickSystem<object>.RemoveCallbackTarget(this);
	}

	bool ITickSystemTick.TickRunning { get; set; }

	void ITickSystemTick.Tick()
	{
		base.transform.position = this.m_transformToFollow.position;
		base.transform.root.rotation = this.m_transformToFollow.rotation;
	}

	[SerializeField]
	private Transform _scaleTransform;

	[SerializeField]
	private CoconutCamera _coconutCamera;

	[SerializeField]
	private List<GameObject> _visualObjects;

	[SerializeField]
	private RigContainer m_rigContainer;

	private Transform m_transformToFollow;

	private LckSocialCamera m_networkController;
}
