using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaLocomotion.Gameplay;
using GT_CustomMapSupportRuntime;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class HandHold : MonoBehaviour, IGorillaGrabable
{
	public static event HandHold.HandHoldPositionEvent HandPositionRequestOverride;

	public static event HandHold.HandHoldEvent HandPositionReleaseOverride;

	public void OnDisable()
	{
		for (int i = 0; i < this.currentGrabbers.Count; i++)
		{
			if (this.currentGrabbers[i].IsNotNull())
			{
				this.currentGrabbers[i].Ungrab(this);
			}
		}
	}

	private void Initialize()
	{
		if (this.initialized)
		{
			return;
		}
		this.myTappable = base.GetComponent<Tappable>();
		this.myCollider = base.GetComponent<Collider>();
		this.initialized = true;
	}

	public virtual bool CanBeGrabbed(GorillaGrabber grabber)
	{
		return true;
	}

	void IGorillaGrabable.OnGrabbed(GorillaGrabber g, out Transform grabbedTransform, out Vector3 localGrabbedPosition)
	{
		this.Initialize();
		grabbedTransform = base.transform;
		Vector3 position = g.transform.position;
		localGrabbedPosition = base.transform.InverseTransformPoint(position);
		Vector3 vector;
		g.Player.AddHandHold(base.transform, localGrabbedPosition, g, g.IsLeftHand, this.rotatePlayerWhenHeld, out vector);
		this.currentGrabbers.AddIfNew(g);
		if (this.handSnapMethod != HandHold.HandSnapMethod.None && HandHold.HandPositionRequestOverride != null)
		{
			HandHold.HandPositionRequestOverride(this, g.IsLeftHand, this.CalculateOffset(position));
		}
		UnityEvent<Vector3> onGrab = this.OnGrab;
		if (onGrab != null)
		{
			onGrab.Invoke(vector);
		}
		UnityEvent<HandHold> onGrabHandHold = this.OnGrabHandHold;
		if (onGrabHandHold != null)
		{
			onGrabHandHold.Invoke(this);
		}
		UnityEvent<bool> onGrabHanded = this.OnGrabHanded;
		if (onGrabHanded != null)
		{
			onGrabHanded.Invoke(g.IsLeftHand);
		}
		if (this.myTappable != null)
		{
			this.myTappable.OnGrab();
		}
	}

	void IGorillaGrabable.OnGrabReleased(GorillaGrabber g)
	{
		this.Initialize();
		g.Player.RemoveHandHold(g, g.IsLeftHand);
		this.currentGrabbers.Remove(g);
		if (this.handSnapMethod != HandHold.HandSnapMethod.None && HandHold.HandPositionReleaseOverride != null)
		{
			HandHold.HandPositionReleaseOverride(this, g.IsLeftHand);
		}
		UnityEvent onRelease = this.OnRelease;
		if (onRelease != null)
		{
			onRelease.Invoke();
		}
		UnityEvent<HandHold> onReleaseHandHold = this.OnReleaseHandHold;
		if (onReleaseHandHold != null)
		{
			onReleaseHandHold.Invoke(this);
		}
		if (this.myTappable != null)
		{
			this.myTappable.OnRelease();
		}
	}

	private Vector3 CalculateOffset(Vector3 position)
	{
		switch (this.handSnapMethod)
		{
		case HandHold.HandSnapMethod.SnapToNearestEdge:
			if (this.myCollider == null)
			{
				this.myCollider = base.GetComponent<Collider>();
				if (this.myCollider is MeshCollider && !(this.myCollider as MeshCollider).convex)
				{
					this.handSnapMethod = HandHold.HandSnapMethod.None;
					return Vector3.zero;
				}
			}
			return base.transform.position - this.myCollider.ClosestPoint(position);
		case HandHold.HandSnapMethod.SnapToXAxisPoint:
			return base.transform.position - base.transform.TransformPoint(Vector3.right * base.transform.InverseTransformPoint(position).x);
		case HandHold.HandSnapMethod.SnapToYAxisPoint:
			return base.transform.position - base.transform.TransformPoint(Vector3.up * base.transform.InverseTransformPoint(position).y);
		case HandHold.HandSnapMethod.SnapToZAxisPoint:
			return base.transform.position - base.transform.TransformPoint(Vector3.forward * base.transform.InverseTransformPoint(position).z);
		default:
			return Vector3.zero;
		}
	}

	public bool MomentaryGrabOnly()
	{
		return this.forceMomentary;
	}

	public void CopyProperties(HandHoldSettings handHoldSettings)
	{
		this.handSnapMethod = (HandHold.HandSnapMethod)handHoldSettings.handSnapMethod;
		this.rotatePlayerWhenHeld = handHoldSettings.rotatePlayerWhenHeld;
		this.forceMomentary = !handHoldSettings.allowPreGrab;
	}

	string IGorillaGrabable.get_name()
	{
		return base.name;
	}

	private Dictionary<Transform, Transform> attached = new Dictionary<Transform, Transform>();

	[SerializeField]
	private HandHold.HandSnapMethod handSnapMethod;

	[SerializeField]
	private bool rotatePlayerWhenHeld;

	[SerializeField]
	private UnityEvent<Vector3> OnGrab;

	[SerializeField]
	private UnityEvent<HandHold> OnGrabHandHold;

	[SerializeField]
	private UnityEvent<bool> OnGrabHanded;

	[SerializeField]
	private UnityEvent OnRelease;

	[SerializeField]
	private UnityEvent<HandHold> OnReleaseHandHold;

	private bool initialized;

	private Collider myCollider;

	private Tappable myTappable;

	[Tooltip("Turning this on disables \"pregrabbing\". Use pregrabbing to allow players to catch a handhold even if they have squeezed the trigger too soon. Useful if you're anticipating jumping players needed to grab while airborne")]
	[SerializeField]
	private bool forceMomentary = true;

	private List<GorillaGrabber> currentGrabbers = new List<GorillaGrabber>();

	private enum HandSnapMethod
	{
		None,
		SnapToCenterPoint,
		SnapToNearestEdge,
		SnapToXAxisPoint,
		SnapToYAxisPoint,
		SnapToZAxisPoint
	}

	public delegate void HandHoldPositionEvent(HandHold hh, bool lh, Vector3 pos);

	public delegate void HandHoldEvent(HandHold hh, bool lh);
}
