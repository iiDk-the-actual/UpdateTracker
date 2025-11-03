using System;
using GorillaExtensions;
using UnityEngine;

public class BeeAvoiderTest : MonoBehaviour
{
	public void Update()
	{
		Vector3 position = this.patrolPoints[this.nextPatrolPoint].transform.position;
		Vector3 position2 = base.transform.position;
		Vector3 vector = (position - position2).normalized * this.speed;
		this.velocity = Vector3.MoveTowards(this.velocity * this.drag, vector, this.acceleration);
		if ((position2 - position).IsLongerThan(this.instabilityOffRadius))
		{
			this.velocity += Random.insideUnitSphere * this.instability * Time.deltaTime;
		}
		Vector3 vector2 = position2 + this.velocity * Time.deltaTime;
		GameObject[] array = this.avoidancePoints;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 position3 = array[i].transform.position;
			if ((vector2 - position3).IsShorterThan(this.avoidRadius))
			{
				Vector3 normalized = Vector3.Cross(position3 - vector2, position - vector2).normalized;
				Vector3 normalized2 = (position - position3).normalized;
				float num = Vector3.Dot(vector2 - position3, normalized);
				Vector3 vector3 = (this.avoidRadius - num) * normalized;
				vector2 += vector3;
				this.velocity += vector3;
			}
		}
		base.transform.position = vector2;
		base.transform.rotation = Quaternion.LookRotation(position - vector2);
		if ((vector2 - position).IsShorterThan(this.patrolArrivedRadius))
		{
			this.nextPatrolPoint = (this.nextPatrolPoint + 1) % this.patrolPoints.Length;
		}
	}

	public GameObject[] patrolPoints;

	public GameObject[] avoidancePoints;

	public float speed;

	public float acceleration;

	public float instability;

	public float instabilityOffRadius;

	public float drag;

	public float avoidRadius;

	public float patrolArrivedRadius;

	private int nextPatrolPoint;

	private Vector3 velocity;
}
