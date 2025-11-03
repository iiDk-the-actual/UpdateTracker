using System;
using GorillaLocomotion.Swimming;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

public class StickyProjectile : MonoBehaviour, IProjectile, ITickSystemTick
{
	private void Awake()
	{
		this.stickyPart.GetLocalPositionAndRotation(out this.stickyPartLocalPosition, out this.stickyPartLocalRotation);
		this.stickyPartLocalScale = this.stickyPart.localScale;
		this.headZoneInversePosition = this.INVERSE_HEAD_ROTATION * this.headZonePosition;
		this.headZoneInverseLocalPosition = this.INVERSE_HEAD_ROTATION * this.localHeadZonePosition;
		this.rb = base.GetComponent<Rigidbody>();
		this.rbwi = base.GetComponent<RigidbodyWaterInteraction>();
		this.collider = base.GetComponent<Collider>();
		this.pcc = base.GetComponent<PlayerColoredCosmetic>();
		this.triggerLayer = LayerMask.NameToLayer("Gorilla Tag Collider");
		UnityEvent onReset = this.OnReset;
		if (onReset == null)
		{
			return;
		}
		onReset.Invoke();
	}

	public void Launch(Vector3 startPosition, Quaternion startRotation, Vector3 velocity, float chargeFrac, VRRig ownerRig, int progress)
	{
		UnityEvent onLaunch = this.OnLaunch;
		if (onLaunch != null)
		{
			onLaunch.Invoke();
		}
		this.stickyPart.SetParent(base.transform, false);
		this.stickyPart.SetLocalPositionAndRotation(this.stickyPartLocalPosition, this.stickyPartLocalRotation);
		this.stickyPart.localScale = this.stickyPartLocalScale;
		base.transform.SetPositionAndRotation(startPosition, startRotation);
		base.transform.localScale = Vector3.one * ownerRig.scaleFactor;
		this.rb.isKinematic = false;
		this.rb.position = startPosition;
		this.rb.rotation = startRotation;
		this.rb.linearVelocity = velocity;
		if (this.faceVelocityWhileAirborne)
		{
			TickSystem<object>.AddTickCallback(this);
			this.rb.angularVelocity = Vector3.zero;
		}
		else
		{
			this.rb.angularVelocity = Random.onUnitSphere * Random.Range(this.launchRandomSpinSpeedMinMax.x, this.launchRandomSpinSpeedMinMax.y);
		}
		this.rbwi.enabled = true;
		this.collider.enabled = true;
		if (this.pcc != null)
		{
			this.pcc.UpdateColor(ownerRig.playerColor);
		}
	}

	private void StickTo(Transform otherTransform, Vector3 position, Quaternion rotation)
	{
		this.stickyPart.parent = otherTransform;
		this.stickyPart.SetPositionAndRotation(position + rotation * this.stickyPartLocalPosition, rotation * this.stickyPartLocalRotation);
		this.rb.isKinematic = true;
		this.rbwi.enabled = false;
		this.collider.enabled = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		TickSystem<object>.RemoveTickCallback(this);
		ContactPoint contact = collision.GetContact(0);
		this.StickTo(collision.transform, contact.point, this.alignToHitNormal ? Quaternion.LookRotation(contact.normal, Random.onUnitSphere) : base.transform.rotation);
		this.stickEvents.InvokeAll(StickyProjectile.StickFlags.Wall);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != this.triggerLayer)
		{
			return;
		}
		TickSystem<object>.RemoveTickCallback(this);
		Vector3 vector = Time.fixedDeltaTime * 2f * this.rb.linearVelocity;
		Vector3 vector2 = base.transform.position - vector;
		Vector3 vector3;
		Quaternion quaternion;
		if (this.alignToHitNormal)
		{
			float magnitude = vector.magnitude;
			RaycastHit raycastHit;
			other.Raycast(new Ray(vector2, vector / magnitude), out raycastHit, 2f * magnitude);
			vector3 = raycastHit.point;
			quaternion = Quaternion.LookRotation(raycastHit.normal, Random.onUnitSphere);
		}
		else
		{
			vector3 = other.ClosestPoint(vector2);
			quaternion = base.transform.rotation;
		}
		VRRig componentInParent = other.GetComponentInParent<VRRig>();
		if (componentInParent != null)
		{
			if (this.headZoneRadius > 0f && string.Equals(other.name, "SpeakerHeadCollider"))
			{
				Vector3 vector4;
				Quaternion quaternion2;
				other.transform.GetPositionAndRotation(out vector4, out quaternion2);
				Vector3 vector5 = quaternion2 * this.headZoneInversePosition + vector4;
				if ((vector3 - vector5).magnitude <= this.headZoneRadius * componentInParent.scaleFactor)
				{
					if (componentInParent.isOfflineVRRig)
					{
						this.StickTo(other.transform, quaternion2 * this.headZoneInverseLocalPosition + vector4, quaternion2 * this.INVERSE_HEAD_ROTATION);
						this.stickyPart.localScale *= this.scaleOnLocalHeadZone;
						this.stickEvents.InvokeAll(StickyProjectile.StickFlags.LocalHeadZone);
						return;
					}
					this.StickTo(other.transform, vector5, quaternion2 * this.INVERSE_HEAD_ROTATION);
					this.stickEvents.InvokeAll(StickyProjectile.StickFlags.RemoteHeadZone);
					return;
				}
				else if (componentInParent.isOfflineVRRig)
				{
					this.stickyPart.localScale *= this.scaleOnLocalHead;
				}
			}
			this.stickEvents.InvokeAll(componentInParent.isOfflineVRRig ? StickyProjectile.StickFlags.LocalPlayer : StickyProjectile.StickFlags.RemotePlayer);
		}
		else
		{
			this.stickEvents.InvokeAll(StickyProjectile.StickFlags.Wall);
		}
		this.StickTo(other.transform, vector3, quaternion);
	}

	private void OnEnable()
	{
		this.stickyPart.gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		this.stickyPart.gameObject.SetActive(false);
		UnityEvent onReset = this.OnReset;
		if (onReset == null)
		{
			return;
		}
		onReset.Invoke();
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		this.rb.rotation = Quaternion.LookRotation(this.rb.linearVelocity);
	}

	[SerializeField]
	private Transform stickyPart;

	[Tooltip("Align the positive Z direction of this object to the rigidbody's velocity.")]
	[SerializeField]
	private bool faceVelocityWhileAirborne;

	[Tooltip("Set the rigidbody's angular velocity to a random unit Vector3, multiplied by a random value in this range.")]
	[SerializeField]
	private Vector2 launchRandomSpinSpeedMinMax = new Vector2(90f, 360f);

	[Tooltip("When enabled, the positive Z direction will face away from whatever surface the projectile hit. When disabled, it will keep its original rotation.")]
	[SerializeField]
	private bool alignToHitNormal = true;

	[Space]
	[SerializeField]
	public UnityEvent OnReset;

	[SerializeField]
	public UnityEvent OnLaunch;

	[Tooltip("Scale the 'Sticky Part' by this value when hitting the local player's head. Usually used to prevent things from obscuring your vision too much.")]
	[SerializeField]
	private float scaleOnLocalHead = 0.7f;

	[Tooltip("The radius of the head zone. Can be set to 0 to disable head zone functionality.")]
	[SerializeField]
	private float headZoneRadius = 0.15f;

	[Tooltip("The local origin of the head zone, relative to the player rig's head transform. When a shot hits inside the zone, the 'Sticky Part' will be moved to this position relative to the hit player's head.")]
	[SerializeField]
	private Vector3 headZonePosition = new Vector3(0f, 0.02f, 0.17f);

	[Tooltip("Scale the 'Sticky Part' by this value when hitting the local player's head zone. Can override 'Scale On Local Head' in case you want it to appear larger for emphasis.")]
	[SerializeField]
	private float scaleOnLocalHeadZone = 1f;

	[Tooltip("When a shot hits inside a remote player's head zone, it will be moved to the 'Head Zone Relative Position'. For the local player, it will instead be moved here. This DOES NOT AFFECT the actual origin of the head zone for hit-detection purposes, it is purely visual after-the-fact.")]
	[SerializeField]
	private Vector3 localHeadZonePosition = new Vector3(0f, 0.05f, 0.2f);

	[SerializeField]
	private FlagEvents<StickyProjectile.StickFlags> stickEvents;

	private readonly Quaternion INVERSE_HEAD_ROTATION = Quaternion.Inverse(Quaternion.Euler(0f, 270f, 252.3229f));

	private Vector3 headZoneInversePosition;

	private Vector3 headZoneInverseLocalPosition;

	private Vector3 stickyPartLocalPosition;

	private Quaternion stickyPartLocalRotation;

	private Vector3 stickyPartLocalScale;

	private Rigidbody rb;

	private RigidbodyWaterInteraction rbwi;

	private Collider collider;

	private PlayerColoredCosmetic pcc;

	private int triggerLayer;

	[Flags]
	public enum StickFlags
	{
		Wall = 1,
		LocalPlayer = 2,
		RemotePlayer = 4,
		LocalHeadZone = 8,
		RemoteHeadZone = 16
	}
}
