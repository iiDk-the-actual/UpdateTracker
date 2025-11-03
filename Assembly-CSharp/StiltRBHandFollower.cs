using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

public class StiltRBHandFollower : MonoBehaviour
{
	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.rb.maxAngularVelocity = this.angularSpeedLimit;
	}

	private void FixedUpdate()
	{
		Vector3 vector = this.targetHand.TransformPoint(this.handOffset);
		float num;
		Vector3 vector2;
		(this.targetHand.TransformRotation(this.handRotOffset) * Quaternion.Inverse(this.rb.transform.rotation)).ToAngleAxis(out num, out vector2);
		this.rb.linearVelocity = (vector - this.rb.transform.position) / Time.fixedDeltaTime;
		this.rb.angularVelocity = vector2 * num * 0.017453292f / Time.fixedDeltaTime;
	}

	private void OnCollisionEnter(Collision collision)
	{
		this.collisions[collision.collider] = collision.contacts[0].point;
	}

	private void OnCollisionStay(Collision collision)
	{
		this.collisions[collision.collider] = collision.contacts[0].point;
	}

	private void OnCollisionExit(Collision collision)
	{
		this.collisions.Remove(collision.collider);
	}

	private Rigidbody rb;

	[SerializeField]
	private Transform targetHand;

	[SerializeField]
	private Vector3 handOffset;

	[SerializeField]
	private Quaternion handRotOffset = Quaternion.identity;

	[SerializeField]
	private float angularSpeedLimit;

	private Dictionary<Collider, Vector3> collisions = new Dictionary<Collider, Vector3>();
}
