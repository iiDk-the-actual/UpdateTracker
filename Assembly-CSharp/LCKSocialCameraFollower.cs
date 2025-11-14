using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag;
using Liv.Lck.GorillaTag;
using UnityEngine;
using UnityEngine.Serialization;

public class LCKSocialCameraFollower : MonoBehaviour, ITickSystemTick
{
	public Transform ScaleTransform
	{
		get
		{
			return this._scaleTransform;
		}
	}

	public GameObject CameraVisualsRoot
	{
		get
		{
			return this._cameraVisualsRoot;
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
		this._initialScale = base.transform.localScale;
		this.m_gtCameraVisuals = this._cameraVisualsRoot.GetComponent<IGtCameraVisuals>();
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
		if (!this.isParentedToRig)
		{
			base.transform.parent = null;
		}
	}

	public void SetParentToRig()
	{
		this.isParentedToRig = true;
		base.transform.parent = this.m_rigContainer.transform;
		base.transform.localPosition = new Vector3(0f, -0.2f, 0.132f);
		base.transform.localRotation = Quaternion.identity;
		base.transform.localScale = this._initialScale * 0.3f;
	}

	public void SetParentNull()
	{
		this.isParentedToRig = false;
		base.transform.parent = null;
		base.transform.localScale = this._initialScale;
	}

	private void PostRigEnable(RigContainer _)
	{
		base.gameObject.SetActive(true);
		this.m_gtCameraVisuals.SetNetworkedVisualsActive(false);
		this.m_gtCameraVisuals.SetRecordingState(false);
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
		if (!this.isParentedToRig)
		{
			base.transform.position = this.m_transformToFollow.position;
			base.transform.root.rotation = this.m_transformToFollow.rotation;
		}
	}

	[SerializeField]
	private Transform _scaleTransform;

	[FormerlySerializedAs("_coconutCamera")]
	[SerializeField]
	private GameObject _cameraVisualsRoot;

	[SerializeField]
	private List<GameObject> _visualObjects;

	[SerializeField]
	private RigContainer m_rigContainer;

	private Transform m_transformToFollow;

	private LckSocialCamera m_networkController;

	private IGtCameraVisuals m_gtCameraVisuals;

	private Vector3 _initialScale = Vector3.one;

	private bool isParentedToRig;
}
