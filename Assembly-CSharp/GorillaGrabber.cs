using System;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using UnityEngine;
using UnityEngine.XR;

public class GorillaGrabber : MonoBehaviour
{
	public bool isGrabbing
	{
		get
		{
			return this.currentGrabbable != null;
		}
	}

	public XRNode XrNode
	{
		get
		{
			return this.xrNode;
		}
	}

	public bool IsLeftHand
	{
		get
		{
			return this.XrNode == XRNode.LeftHand;
		}
	}

	public bool IsRightHand
	{
		get
		{
			return this.XrNode == XRNode.RightHand;
		}
	}

	public GTPlayer Player
	{
		get
		{
			return this.player;
		}
	}

	private void Start()
	{
		this.hapticStrengthActual = this.hapticStrength;
		this.audioSource = base.GetComponent<AudioSource>();
		this.player = base.GetComponentInParent<GTPlayer>();
		if (!this.player)
		{
			Debug.LogWarning("Gorilla Grabber Component has no player in hierarchy. Disabling this Gorilla Grabber");
			base.GetComponent<GorillaGrabber>().enabled = false;
		}
	}

	public void CheckGrabber(bool initiateGrab)
	{
		bool grabMomentary = ControllerInputPoller.GetGrabMomentary(this.xrNode);
		bool grabRelease = ControllerInputPoller.GetGrabRelease(this.xrNode);
		if (this.currentGrabbable != null && (grabRelease || this.GrabDistanceOverCheck()))
		{
			this.Ungrab(null);
		}
		if (grabMomentary)
		{
			this.grabTimeStamp = Time.time;
		}
		if (initiateGrab && this.currentGrabbable == null)
		{
			this.currentGrabbable = this.TryGrab(Time.time - this.grabTimeStamp < this.coyoteTimeDuration);
		}
		if (this.currentGrabbable != null && this.hapticStrengthActual > 0f)
		{
			GorillaTagger.Instance.DoVibration(this.xrNode, this.hapticStrengthActual, Time.deltaTime);
			this.hapticStrengthActual -= this.hapticDecay * Time.deltaTime;
		}
	}

	private bool GrabDistanceOverCheck()
	{
		return this.currentGrabbedTransform == null || Vector3.Distance(base.transform.position, this.currentGrabbedTransform.TransformPoint(this.localGrabbedPosition)) > this.breakDistance;
	}

	internal void Ungrab(IGorillaGrabable specificGrabbable = null)
	{
		if (specificGrabbable != null && specificGrabbable != this.currentGrabbable)
		{
			return;
		}
		this.currentGrabbable.OnGrabReleased(this);
		PlayerGameEvents.DroppedObject(this.currentGrabbable.name);
		this.currentGrabbable = null;
		this.gripEffects.Stop();
		this.hapticStrengthActual = this.hapticStrength;
	}

	private IGorillaGrabable TryGrab(bool momentary)
	{
		IGorillaGrabable gorillaGrabable = null;
		Debug.DrawRay(base.transform.position, base.transform.forward * (this.grabRadius * this.player.scale), Color.blue, 1f);
		int num = Physics.OverlapSphereNonAlloc(base.transform.position, this.grabRadius * this.player.scale, this.grabCastResults);
		float num2 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			IGorillaGrabable gorillaGrabable2;
			if (this.grabCastResults[i].TryGetComponent<IGorillaGrabable>(out gorillaGrabable2))
			{
				float num3 = Vector3.Distance(base.transform.position, this.FindClosestPoint(this.grabCastResults[i], base.transform.position));
				if (num3 < num2)
				{
					num2 = num3;
					gorillaGrabable = gorillaGrabable2;
				}
			}
		}
		if (gorillaGrabable != null && (!gorillaGrabable.MomentaryGrabOnly() || momentary) && gorillaGrabable.CanBeGrabbed(this))
		{
			gorillaGrabable.OnGrabbed(this, out this.currentGrabbedTransform, out this.localGrabbedPosition);
			PlayerGameEvents.GrabbedObject(gorillaGrabable.name);
		}
		if (gorillaGrabable != null && !gorillaGrabable.CanBeGrabbed(this))
		{
			gorillaGrabable = null;
		}
		return gorillaGrabable;
	}

	private Vector3 FindClosestPoint(Collider collider, Vector3 position)
	{
		if (collider is MeshCollider && !(collider as MeshCollider).convex)
		{
			return position;
		}
		return collider.ClosestPoint(position);
	}

	public void Inject(Transform currentGrabbableTransform, Vector3 localGrabbedPosition)
	{
		if (this.currentGrabbable != null)
		{
			this.Ungrab(null);
		}
		if (currentGrabbableTransform != null)
		{
			this.currentGrabbable = currentGrabbableTransform.GetComponent<IGorillaGrabable>();
			this.currentGrabbedTransform = currentGrabbableTransform;
			this.localGrabbedPosition = localGrabbedPosition;
			this.currentGrabbable.OnGrabbed(this, out this.currentGrabbedTransform, out localGrabbedPosition);
		}
	}

	private GTPlayer player;

	[SerializeField]
	private XRNode xrNode = XRNode.LeftHand;

	private AudioSource audioSource;

	private Transform currentGrabbedTransform;

	private Vector3 localGrabbedPosition;

	private IGorillaGrabable currentGrabbable;

	[SerializeField]
	private float grabRadius = 0.015f;

	[SerializeField]
	private float breakDistance = 0.3f;

	[SerializeField]
	private float hapticStrength = 0.2f;

	private float hapticStrengthActual = 0.2f;

	[SerializeField]
	private float hapticDecay;

	[SerializeField]
	private ParticleSystem gripEffects;

	private Collider[] grabCastResults = new Collider[32];

	private float grabTimeStamp;

	[SerializeField]
	private float coyoteTimeDuration = 0.25f;
}
