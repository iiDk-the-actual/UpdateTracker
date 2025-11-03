using System;
using UnityEngine;

public class GorillaTriggerColliderHandIndicator : MonoBehaviourTick
{
	public override void Tick()
	{
		this.currentVelocity = (base.transform.position - this.lastPosition) / Time.deltaTime;
		this.lastPosition = base.transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.throwableController != null)
		{
			this.throwableController.GrabbableObjectHover(this.isLeftHand);
		}
	}

	public Vector3 currentVelocity;

	public Vector3 lastPosition = Vector3.zero;

	public bool isLeftHand;

	public GorillaThrowableController throwableController;
}
