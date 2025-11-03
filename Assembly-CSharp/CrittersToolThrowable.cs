using System;
using System.Diagnostics;
using Unity.XR.CoreUtils;
using UnityEngine;

public class CrittersToolThrowable : CrittersActor
{
	public override void Initialize()
	{
		base.Initialize();
		this.hasBeenGrabbedByPlayer = false;
		this.shouldDisable = false;
		this.hasTriggeredSinceLastGrab = false;
		this._sqrActivationSpeed = this.requiredActivationSpeed * this.requiredActivationSpeed;
	}

	public override void GrabbedBy(CrittersActor grabbingActor, bool positionOverride = false, Quaternion localRotation = default(Quaternion), Vector3 localOffset = default(Vector3), bool disableGrabbing = false)
	{
		base.GrabbedBy(grabbingActor, positionOverride, localRotation, localOffset, disableGrabbing);
		this.hasBeenGrabbedByPlayer = true;
		this.hasTriggeredSinceLastGrab = false;
		this.OnPickedUp();
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (CrittersManager.instance.containerLayer.Contains(collision.gameObject.layer))
		{
			return;
		}
		if (this.requiresPlayerGrabBeforeActivate && !this.hasBeenGrabbedByPlayer)
		{
			return;
		}
		if (this._sqrActivationSpeed > 0f && collision.relativeVelocity.sqrMagnitude < this._sqrActivationSpeed)
		{
			return;
		}
		if (this.onlyTriggerOncePerGrab && this.hasTriggeredSinceLastGrab)
		{
			return;
		}
		if (this.onlyTriggerOnDirectCritterHit)
		{
			CrittersPawn component = collision.gameObject.GetComponent<CrittersPawn>();
			if (component != null && component.isActiveAndEnabled)
			{
				this.hasTriggeredSinceLastGrab = true;
				this.OnImpactCritter(component);
			}
		}
		else
		{
			Vector3 point = collision.contacts[0].point;
			Vector3 normal = collision.contacts[0].normal;
			this.hasTriggeredSinceLastGrab = true;
			this.OnImpact(point, normal);
		}
		if (this.destroyOnImpact)
		{
			this.shouldDisable = true;
		}
	}

	protected virtual void OnImpact(Vector3 hitPosition, Vector3 hitNormal)
	{
	}

	protected virtual void OnImpactCritter(CrittersPawn impactedCritter)
	{
	}

	protected virtual void OnPickedUp()
	{
	}

	[Conditional("DRAW_DEBUG")]
	protected void ShowDebugVisualization(Vector3 position, float scale, float duration = 0f)
	{
		if (!this.debugImpactPrefab)
		{
			return;
		}
		DelayedDestroyObject delayedDestroyObject = Object.Instantiate<DelayedDestroyObject>(this.debugImpactPrefab, position, Quaternion.identity);
		delayedDestroyObject.transform.localScale *= scale;
		if (duration != 0f)
		{
			delayedDestroyObject.lifetime = duration;
		}
	}

	public override bool ProcessLocal()
	{
		bool flag = base.ProcessLocal();
		if (this.shouldDisable)
		{
			base.gameObject.SetActive(false);
			return true;
		}
		return flag;
	}

	public override void TogglePhysics(bool enable)
	{
		if (enable)
		{
			this.rb.isKinematic = false;
			this.rb.interpolation = RigidbodyInterpolation.Interpolate;
			this.rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			return;
		}
		this.rb.isKinematic = true;
		this.rb.interpolation = RigidbodyInterpolation.None;
		this.rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
	}

	[Header("Throwable")]
	public bool requiresPlayerGrabBeforeActivate = true;

	public float requiredActivationSpeed = 2f;

	public bool onlyTriggerOnDirectCritterHit;

	public bool destroyOnImpact = true;

	public bool onlyTriggerOncePerGrab = true;

	[Header("Debug")]
	[SerializeField]
	private DelayedDestroyObject debugImpactPrefab;

	private bool hasBeenGrabbedByPlayer;

	protected bool shouldDisable;

	private bool hasTriggeredSinceLastGrab;

	private float _sqrActivationSpeed;
}
