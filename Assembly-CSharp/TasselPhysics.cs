using System;
using UnityEngine;

public class TasselPhysics : MonoBehaviour
{
	private void Awake()
	{
		this.centerOfMassLength = this.localCenterOfMass.magnitude;
		if (this.LockXAxis)
		{
			this.rotCorrection = Quaternion.Inverse(Quaternion.LookRotation(Vector3.right, this.localCenterOfMass));
			return;
		}
		this.rotCorrection = Quaternion.Inverse(Quaternion.LookRotation(this.localCenterOfMass));
	}

	private void Update()
	{
		float y = base.transform.lossyScale.y;
		this.velocity *= this.drag;
		this.velocity.y = this.velocity.y - this.gravityStrength * y * Time.deltaTime;
		Vector3 position = base.transform.position;
		Vector3 vector = this.lastCenterPos + this.velocity * Time.deltaTime;
		Vector3 vector2 = position + (vector - position).normalized * this.centerOfMassLength * y;
		this.velocity = (vector2 - this.lastCenterPos) / Time.deltaTime;
		this.lastCenterPos = vector2;
		if (this.LockXAxis)
		{
			foreach (GameObject gameObject in this.tasselInstances)
			{
				gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.right, vector2 - position) * this.rotCorrection;
			}
			return;
		}
		foreach (GameObject gameObject2 in this.tasselInstances)
		{
			gameObject2.transform.rotation = Quaternion.LookRotation(vector2 - position, gameObject2.transform.position - position) * this.rotCorrection;
		}
	}

	[SerializeField]
	private GameObject[] tasselInstances;

	[SerializeField]
	private Vector3 localCenterOfMass;

	[SerializeField]
	private float gravityStrength;

	[SerializeField]
	private float drag;

	[SerializeField]
	private bool LockXAxis;

	private Vector3 lastCenterPos;

	private Vector3 velocity;

	private float centerOfMassLength;

	private Quaternion rotCorrection;
}
