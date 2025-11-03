using System;
using GorillaExtensions;
using Unity.Cinemachine;
using UnityEngine;

public class FakeWheelDriver : MonoBehaviour
{
	public bool hasCollision { get; private set; }

	public void SetThrust(Vector3 thrust)
	{
		this.thrust = thrust;
	}

	private void OnCollisionStay(Collision collision)
	{
		int num = 0;
		Vector3 vector = Vector3.zero;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (contactPoint.thisCollider == this.wheelCollider)
			{
				vector += contactPoint.point;
				num++;
			}
		}
		if (num > 0)
		{
			this.collisionNormal = collision.contacts[0].normal;
			this.collisionPoint = vector / (float)num;
			this.hasCollision = true;
		}
	}

	private void FixedUpdate()
	{
		if (this.hasCollision)
		{
			Vector3 vector = base.transform.rotation * this.thrust;
			if (this.myRigidBody.linearVelocity.IsShorterThan(this.maxSpeed))
			{
				vector = vector.ProjectOntoPlane(this.collisionNormal).normalized * this.thrust.magnitude;
				this.myRigidBody.AddForceAtPosition(vector, this.collisionPoint);
			}
			Vector3 vector2 = this.myRigidBody.linearVelocity.ProjectOntoPlane(this.collisionNormal).ProjectOntoPlane(vector.normalized);
			if (vector2.IsLongerThan(this.lateralFrictionForce))
			{
				this.myRigidBody.AddForceAtPosition(-vector2.normalized * this.lateralFrictionForce, this.collisionPoint);
			}
			else
			{
				this.myRigidBody.AddForceAtPosition(-vector2, this.collisionPoint);
			}
		}
		this.hasCollision = false;
	}

	[SerializeField]
	private Rigidbody myRigidBody;

	[SerializeField]
	private Vector3 thrust;

	[SerializeField]
	private Collider wheelCollider;

	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private float lateralFrictionForce;

	private Vector3 collisionPoint;

	private Vector3 collisionNormal;
}
