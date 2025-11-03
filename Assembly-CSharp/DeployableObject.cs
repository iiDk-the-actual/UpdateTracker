using System;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using GorillaTag;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class DeployableObject : TransferrableObject
{
	protected override void Awake()
	{
		this._deploySignal.OnSignal += this.DeployRPC;
		base.Awake();
	}

	internal override void OnEnable()
	{
		this._deploySignal.Enable();
		VRRig componentInParent = base.GetComponentInParent<VRRig>();
		for (int i = 0; i < this._rigAwareObjects.Length; i++)
		{
			IRigAware rigAware = this._rigAwareObjects[i] as IRigAware;
			if (rigAware != null)
			{
				rigAware.SetRig(componentInParent);
			}
		}
		this.m_VRRig = componentInParent;
		ListProcessor<Action<RigContainer>> disableEvent = this.m_VRRig.rigContainer.RigEvents.disableEvent;
		Action<RigContainer> action = new Action<RigContainer>(this.OnRigPreDisable);
		disableEvent.Add(in action);
		base.OnEnable();
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		this.itemState &= (TransferrableObject.ItemStates)(-2);
	}

	internal override void OnDisable()
	{
		this.m_VRRig = null;
		this._deploySignal.Disable();
		if (this._objectToDeploy.activeSelf)
		{
			this.ReturnChild();
		}
		base.OnDisable();
	}

	private void OnRigPreDisable(RigContainer rc)
	{
		this.m_spamChecker.Reset();
		ListProcessor<Action<RigContainer>> disableEvent = rc.RigEvents.disableEvent;
		Action<RigContainer> action = new Action<RigContainer>(this.OnRigPreDisable);
		disableEvent.Remove(in action);
	}

	protected override void OnDestroy()
	{
		this._deploySignal.Dispose();
		base.OnDestroy();
	}

	protected override void LateUpdateReplicated()
	{
		base.LateUpdateReplicated();
		if (this.itemState.HasFlag(TransferrableObject.ItemStates.State0))
		{
			if (!this._objectToDeploy.activeSelf)
			{
				this.DeployChild();
				return;
			}
		}
		else if (this._objectToDeploy.activeSelf)
		{
			this.ReturnChild();
		}
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		if (VRRig.LocalRig != this.ownerRig)
		{
			return false;
		}
		bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
		GorillaVelocityTracker interactPointVelocityTracker = GTPlayer.Instance.GetInteractPointVelocityTracker(flag);
		Transform transform = base.transform;
		Vector3 vector = transform.TransformPoint(Vector3.zero);
		Quaternion rotation = transform.rotation;
		Vector3 averageVelocity = interactPointVelocityTracker.GetAverageVelocity(true, 0.15f, false);
		this.DeployLocal(vector, rotation, averageVelocity, false);
		this._deploySignal.Raise(ReceiverGroup.Others, BitPackUtils.PackWorldPosForNetwork(vector), BitPackUtils.PackQuaternionForNetwork(rotation), BitPackUtils.PackWorldPosForNetwork(averageVelocity * 100f));
		return true;
	}

	protected virtual void DeployLocal(Vector3 launchPos, Quaternion launchRot, Vector3 releaseVel, bool isRemote = false)
	{
		this.DisableWhileDeployed(true);
		this._child.Deploy(this, launchPos, launchRot, releaseVel, isRemote);
	}

	private void DeployRPC(long packedPos, int packedRot, long packedVel, PhotonSignalInfo info)
	{
		if (info.sender != base.OwningPlayer())
		{
			return;
		}
		GorillaNot.IncrementRPCCall(info, "DeployRPC");
		if (!this.m_spamChecker.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(packedPos);
		Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(packedRot);
		Vector3 vector2 = BitPackUtils.UnpackWorldPosFromNetwork(packedVel) / 100f;
		float num = 10000f;
		if (!(in vector).IsValid(in num) || !(in quaternion).IsValid() || !this.m_VRRig.IsPositionInRange(vector, this._maxDeployDistance))
		{
			return;
		}
		this.DeployLocal(vector, quaternion, this.m_VRRig.ClampVelocityRelativeToPlayerSafe(vector2, this._maxThrowVelocity, 100f), true);
	}

	private void DisableWhileDeployed(bool active)
	{
		if (this._disabledWhileDeployed.IsNullOrEmpty<GameObject>())
		{
			return;
		}
		for (int i = 0; i < this._disabledWhileDeployed.Length; i++)
		{
			this._disabledWhileDeployed[i].SetActive(!active);
		}
	}

	public void DeployChild()
	{
		this.itemState |= TransferrableObject.ItemStates.State0;
		this._objectToDeploy.SetActive(true);
		this.DisableWhileDeployed(true);
		UnityEvent onDeploy = this._onDeploy;
		if (onDeploy == null)
		{
			return;
		}
		onDeploy.Invoke();
	}

	public void ReturnChild()
	{
		this.itemState &= (TransferrableObject.ItemStates)(-2);
		this._objectToDeploy.SetActive(false);
		this.DisableWhileDeployed(false);
		UnityEvent onReturn = this._onReturn;
		if (onReturn == null)
		{
			return;
		}
		onReturn.Invoke();
	}

	[SerializeField]
	private GameObject _objectToDeploy;

	[SerializeField]
	private DeployedChild _child;

	[SerializeField]
	private GameObject[] _disabledWhileDeployed = new GameObject[0];

	[SerializeField]
	private SoundBankPlayer deploySound;

	[SerializeField]
	private PhotonSignal<long, int, long> _deploySignal = "_deploySignal";

	[SerializeField]
	private float _maxDeployDistance = 4f;

	[SerializeField]
	private float _maxThrowVelocity = 50f;

	[SerializeField]
	private UnityEvent _onDeploy;

	[SerializeField]
	private UnityEvent _onReturn;

	[SerializeField]
	private Component[] _rigAwareObjects = new Component[0];

	[SerializeField]
	private CallLimiter m_spamChecker = new CallLimiter(2, 1f, 0.5f);

	private VRRig m_VRRig;
}
