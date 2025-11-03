using System;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

public class MouthReactorCosmetic : MonoBehaviour, ITickSystemTick
{
	private void ResetReactorTransform()
	{
		if (this.reactorTransform == null)
		{
			this.reactorTransform = base.transform;
		}
	}

	private void ResetRadius()
	{
		this.reactorRadius = 0.1666667f;
	}

	private bool IsRadiusChanged
	{
		get
		{
			return this.reactorRadius != 0.1666667f;
		}
	}

	private void ResetOffset()
	{
		this.mouthOffset = MouthReactorCosmetic.DEFAULT_OFFSET;
	}

	private bool IsOffsetChanged
	{
		get
		{
			return this.mouthOffset != MouthReactorCosmetic.DEFAULT_OFFSET;
		}
	}

	private void OnEnable()
	{
		if (this.myRig == null)
		{
			this.myRig = base.GetComponentInParent<VRRig>();
		}
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		Vector3 vector = this.myRig.head.rigTarget.TransformPoint(this.mouthOffset);
		float sqrMagnitude = (this.reactorTransform.TransformPoint(this.reactorOffset) - vector).sqrMagnitude;
		if (sqrMagnitude < this.reactorRadius * this.reactorRadius)
		{
			if ((!this.mustExitBeforeRefire || !this.wasInside) && Time.time - this.lastInsideTime >= this.eventRefireDelay)
			{
				UnityEvent unityEvent = this.onInsideMouth;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
				this.lastInsideTime = Time.time;
			}
			this.wasInside = true;
		}
		else
		{
			this.wasInside = false;
		}
		if (this.continuousProperties.Count > 0)
		{
			this.continuousProperties.ApplyAll(Mathf.Min(0f, Mathf.Sqrt(sqrMagnitude) - this.reactorRadius));
		}
	}

	private static readonly Vector3 DEFAULT_OFFSET = new Vector3(0f, 0.0208f, 0.171f);

	private const float DEFAULT_RADIUS = 0.1666667f;

	[Tooltip("The transform to check against the mouth's position. Defaults to the transform this script is attached to.")]
	public Transform reactorTransform;

	[Tooltip("Offset the relative position of the reactor transform.")]
	public Vector3 reactorOffset = Vector3.zero;

	[Tooltip("How close the reactor needs to be to the mouth to trigger the event.")]
	public float reactorRadius = 0.1666667f;

	[Tooltip("The continuous value is the distance to the mouth. When inside the mouth radius, the value will always be 0.")]
	public ContinuousPropertyArray continuousProperties;

	[Tooltip("After the event fires, it must wait this many seconds before it fires again.")]
	public float eventRefireDelay = 0.6f;

	[Tooltip("After the event fires, prevent firing again until the reactor transform is moved outside the mouth and then back in.")]
	public bool mustExitBeforeRefire = true;

	public UnityEvent onInsideMouth;

	public Vector3 mouthOffset = MouthReactorCosmetic.DEFAULT_OFFSET;

	private VRRig myRig;

	private float lastInsideTime;

	private bool wasInside;
}
