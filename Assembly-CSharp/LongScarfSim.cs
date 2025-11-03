using System;
using GorillaExtensions;
using UnityEngine;

public class LongScarfSim : MonoBehaviour
{
	private void Start()
	{
		this.clampToPlane.Normalize();
		this.velocityEstimator = base.GetComponent<GorillaVelocityEstimator>();
		this.baseLocalRotations = new Quaternion[this.gameObjects.Length];
		for (int i = 0; i < this.gameObjects.Length; i++)
		{
			this.baseLocalRotations[i] = this.gameObjects[i].transform.localRotation;
		}
	}

	private void LateUpdate()
	{
		this.velocity *= this.drag;
		this.velocity.y = this.velocity.y - this.gravityStrength * Time.deltaTime;
		Vector3 position = base.transform.position;
		Vector3 vector = this.lastCenterPos + this.velocity * Time.deltaTime;
		Vector3 vector2 = position + (vector - position).normalized * this.centerOfMassLength;
		Vector3 vector3 = base.transform.InverseTransformPoint(vector2);
		float num = Vector3.Dot(vector3, this.clampToPlane);
		if (num < 0f)
		{
			vector3 -= this.clampToPlane * num;
			vector2 = base.transform.TransformPoint(vector3);
		}
		Vector3 vector4 = vector2;
		this.velocity = (vector4 - this.lastCenterPos) / Time.deltaTime;
		this.lastCenterPos = vector4;
		float num2 = (float)(this.velocityEstimator.linearVelocity.IsLongerThan(this.speedThreshold) ? 1 : 0);
		this.currentBlend = Mathf.MoveTowards(this.currentBlend, num2, this.blendAmountPerSecond * Time.deltaTime);
		Quaternion quaternion = Quaternion.LookRotation(vector4 - position);
		for (int i = 0; i < this.gameObjects.Length; i++)
		{
			Quaternion quaternion2 = this.gameObjects[i].transform.parent.rotation * this.baseLocalRotations[i];
			this.gameObjects[i].transform.rotation = Quaternion.Lerp(quaternion2, quaternion, this.currentBlend);
		}
	}

	[SerializeField]
	private GameObject[] gameObjects;

	[SerializeField]
	private float speedThreshold = 1f;

	[SerializeField]
	private float blendAmountPerSecond = 1f;

	private GorillaVelocityEstimator velocityEstimator;

	private Quaternion[] baseLocalRotations;

	private float currentBlend;

	[SerializeField]
	private float centerOfMassLength;

	[SerializeField]
	private float gravityStrength;

	[SerializeField]
	private float drag;

	[SerializeField]
	private Vector3 clampToPlane;

	private Vector3 lastCenterPos;

	private Vector3 velocity;
}
