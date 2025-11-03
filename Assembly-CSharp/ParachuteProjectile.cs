using System;
using GorillaTag.Cosmetics;
using UnityEngine;

public class ParachuteProjectile : MonoBehaviour, IProjectile, ITickSystemTick
{
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		this.launched = false;
		this.landTime = 0f;
		this.launchedTime = 0f;
		this.peakTime = float.MaxValue;
		this.monkeMeshFilter.mesh = this.launchMesh;
		this.parachute.SetActive(false);
		if (!this.TickRunning)
		{
			TickSystem<object>.AddCallbackTarget(this);
		}
	}

	private void OnDisable()
	{
		this.launched = false;
		if (this.TickRunning)
		{
			TickSystem<object>.RemoveCallbackTarget(this);
		}
	}

	public void Launch(Vector3 startPosition, Quaternion startRotation, Vector3 velocity, float chargeFrac, VRRig ownerRig, int progress)
	{
		this.parachuteDeployed = false;
		this.landed = false;
		if (this.rb == null)
		{
			this.rb = base.GetComponent<Rigidbody>();
		}
		this.rb.position = startPosition;
		this.rb.rotation = startRotation;
		this.ChangeUp(Vector3.up);
		this.rb.freezeRotation = true;
		if (ownerRig == null)
		{
			base.transform.localScale = Vector3.one;
		}
		else
		{
			base.transform.localScale = Vector3.one * ownerRig.scaleFactor;
		}
		this.rb.isKinematic = false;
		this.rb.linearVelocity = velocity;
		this.rb.linearDamping = this.initialDrag;
		this.rb.angularDamping = this.initialAngularDrag;
		this.launchedTime = Time.time;
		this.monkeMeshFilter.mesh = this.launchMesh;
		this.parachute.SetActive(false);
		if (velocity.y > 0f)
		{
			this.peakTime = velocity.y / (-1f * Physics.gravity.y);
		}
		else
		{
			this.peakTime = 0f;
		}
		this.launched = true;
	}

	private void OnPeakReached()
	{
		this.parachuteDeployed = true;
		this.parachute.SetActive(true);
		this.monkeMeshFilter.mesh = this.parachutingMesh;
		this.ChangeUp(Vector3.up);
		this.rb.linearDamping = this.parachuteDrag;
		this.rb.angularDamping = this.parachuteAngularDrag;
	}

	private void OnLanded(Collision collision)
	{
		this.landTime = Time.time;
		this.landed = true;
		ContactPoint contact = collision.GetContact(0);
		this.rb.isKinematic = true;
		this.rb.position = contact.point + contact.normal * (this.groundOffset * base.transform.localScale.x);
		this.ChangeUp(contact.normal);
		this.monkeMeshFilter.mesh = this.landedMesh;
		this.parachute.SetActive(false);
	}

	private void ChangeUp(Vector3 newUp)
	{
		Vector3 vector = Vector3.Cross(this.rb.transform.right, newUp);
		if (vector.sqrMagnitude < 1E-45f)
		{
			vector = Vector3.Cross(Vector3.Cross(newUp, this.rb.transform.forward), newUp);
		}
		this.rb.rotation = Quaternion.LookRotation(vector, newUp);
	}

	private void PlayImpactEffects(Vector3 position, Vector3 normal)
	{
		if (this.impactEffect != null)
		{
			Vector3 vector = position + this.impactEffectOffset * normal;
			GameObject gameObject = ObjectPools.instance.Instantiate(this.impactEffect, vector, true);
			gameObject.transform.localScale = base.transform.localScale * this.impactEffectScaleMultiplier;
			gameObject.transform.up = normal;
		}
		ObjectPools.instance.Destroy(base.gameObject);
	}

	public void OnTriggerEvent(bool isLeft, Collider col)
	{
		if (this.parachuteDeployed)
		{
			this.PlayImpactEffects(base.transform.position, Vector3.up);
			GorillaTriggerColliderHandIndicator componentInParent = col.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
			if (componentInParent != null)
			{
				float num = GorillaTagger.Instance.tapHapticStrength / 2f;
				float fixedDeltaTime = Time.fixedDeltaTime;
				GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, num, fixedDeltaTime);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!this.launched || this.landed)
		{
			return;
		}
		ContactPoint contact = collision.GetContact(0);
		if (collision.collider.attachedRigidbody != null)
		{
			this.PlayImpactEffects(contact.point, contact.normal);
			return;
		}
		if (collision.collider.gameObject.IsOnLayer(UnityLayer.GorillaThrowable))
		{
			this.PlayImpactEffects(contact.point, contact.normal);
			return;
		}
		if (!this.parachuteDeployed)
		{
			this.PlayImpactEffects(contact.point, contact.normal);
			return;
		}
		if (Vector3.Angle(contact.normal, Vector3.up) < this.groudUpThreshold)
		{
			this.OnLanded(collision);
			return;
		}
		this.PlayImpactEffects(contact.point, contact.normal);
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		if (!this.parachuteDeployed && Time.time > this.launchedTime + this.parachuteDeployDelay && Time.time >= this.launchedTime + this.peakTime)
		{
			this.OnPeakReached();
		}
		if (this.landed && Time.time > this.landTime + this.destroyOnLandDelay)
		{
			this.PlayImpactEffects(base.transform.position, base.transform.up);
		}
	}

	[SerializeField]
	private MeshFilter monkeMeshFilter;

	[SerializeField]
	private GameObject parachute;

	[SerializeField]
	private Mesh launchMesh;

	[SerializeField]
	private Mesh parachutingMesh;

	[SerializeField]
	private Mesh landedMesh;

	[Tooltip("time to wait after launch before deploying the parachute")]
	[SerializeField]
	private float parachuteDeployDelay = 1f;

	[Tooltip("time to wait after landing before destroying")]
	[SerializeField]
	private float destroyOnLandDelay = 3f;

	[Tooltip("How far from the collision point should the projectile sit when landed")]
	[SerializeField]
	private float groundOffset;

	[Tooltip("Acceptable angle in degrees of surface from world up to be considered the ground")]
	[SerializeField]
	private float groudUpThreshold = 45f;

	[Tooltip("Drag before the parachute is deployed.")]
	[SerializeField]
	private float initialDrag;

	[Tooltip("Drag before the parachute is deployed.")]
	[SerializeField]
	private float initialAngularDrag = 0.05f;

	[Tooltip("Drag after the parachute is deployed.")]
	[SerializeField]
	private float parachuteDrag = 5f;

	[Tooltip("Drag after the parachute is deployed.")]
	[SerializeField]
	private float parachuteAngularDrag = 10f;

	[SerializeField]
	private GameObject impactEffect;

	[SerializeField]
	private float impactEffectScaleMultiplier = 1f;

	[Tooltip("Distance from the surface that the particle should spawn.")]
	[SerializeField]
	private float impactEffectOffset;

	private Rigidbody rb;

	private bool launched;

	private float launchedTime;

	private float landTime;

	private float peakTime = float.MaxValue;

	private bool parachuteDeployed;

	private bool landed;
}
