using System;
using GorillaLocomotion.Gameplay;
using Liv.Lck.GorillaTag;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class LckDirectGrabbable : MonoBehaviour, IGorillaGrabable
{
	public event Action onGrabbed;

	public event Action onReleased;

	public GorillaGrabber grabber
	{
		get
		{
			return this._grabber;
		}
	}

	public bool isGrabbed
	{
		get
		{
			return this._grabber != null;
		}
	}

	public Vector3 GetLocalGrabbedPosition(GorillaGrabber grabber)
	{
		if (grabber == null)
		{
			return Vector3.zero;
		}
		return base.transform.InverseTransformPoint(grabber.transform.position);
	}

	public bool CanBeGrabbed(GorillaGrabber grabber)
	{
		return this._grabber == null || grabber == this._grabber;
	}

	public void OnGrabbed(GorillaGrabber grabber, out Transform grabbedTransform, out Vector3 localGrabbedPosition)
	{
		if (!base.isActiveAndEnabled)
		{
			this._grabber = null;
			grabbedTransform = grabber.transform;
			localGrabbedPosition = Vector3.zero;
			return;
		}
		if (this._grabber != null && this._grabber != grabber)
		{
			this.ForceRelease();
		}
		bool flag;
		bool flag2;
		if (this._precise && this.IsSlingshotHeldInHand(out flag, out flag2) && ((grabber.XrNode == XRNode.LeftHand && flag) || (grabber.XrNode == XRNode.RightHand && flag2)))
		{
			this._grabber = null;
			grabbedTransform = grabber.transform;
			localGrabbedPosition = Vector3.zero;
			return;
		}
		this._grabber = grabber;
		GtColliderTriggerProcessor.CurrentGrabbedHand = grabber.XrNode;
		GtColliderTriggerProcessor.IsGrabbingTablet = true;
		grabbedTransform = base.transform;
		localGrabbedPosition = this.GetLocalGrabbedPosition(this._grabber);
		this.target.SetParent(grabber.transform, true);
		Action action = this.onGrabbed;
		if (action != null)
		{
			action();
		}
		UnityEvent onTabletGrabbed = this.OnTabletGrabbed;
		if (onTabletGrabbed == null)
		{
			return;
		}
		onTabletGrabbed.Invoke();
	}

	public void OnGrabReleased(GorillaGrabber grabber)
	{
		this.target.transform.SetParent(this._originalTargetParent, true);
		this._grabber = null;
		GtColliderTriggerProcessor.IsGrabbingTablet = false;
		Action action = this.onReleased;
		if (action != null)
		{
			action();
		}
		UnityEvent onTabletReleased = this.OnTabletReleased;
		if (onTabletReleased == null)
		{
			return;
		}
		onTabletReleased.Invoke();
	}

	public void ForceGrab(GorillaGrabber grabber)
	{
		grabber.Inject(base.transform, this.GetLocalGrabbedPosition(grabber));
	}

	public void ForceRelease()
	{
		if (this._grabber == null)
		{
			return;
		}
		this._grabber.Inject(null, Vector3.zero);
	}

	private bool IsSlingshotHeldInHand(out bool leftHand, out bool rightHand)
	{
		VRRig rig = VRRigCache.Instance.localRig.Rig;
		if (rig == null || rig.projectileWeapon == null)
		{
			leftHand = false;
			rightHand = false;
			return false;
		}
		leftHand = rig.projectileWeapon.InLeftHand();
		rightHand = rig.projectileWeapon.InRightHand();
		return rig.projectileWeapon.InHand();
	}

	public void SetOriginalTargetParent(Transform parent)
	{
		this._originalTargetParent = parent;
	}

	public bool MomentaryGrabOnly()
	{
		return true;
	}

	string IGorillaGrabable.get_name()
	{
		return base.name;
	}

	public UnityEvent OnTabletGrabbed = new UnityEvent();

	public UnityEvent OnTabletReleased = new UnityEvent();

	[SerializeField]
	private Transform _originalTargetParent;

	public Transform target;

	[SerializeField]
	private bool _precise;

	private GorillaGrabber _grabber;
}
