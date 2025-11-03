using System;
using GorillaExtensions;
using Unity.Cinemachine;
using UnityEngine;

public class GragerHoldable : MonoBehaviour
{
	private void Start()
	{
		this.LocalRotationAxis = this.LocalRotationAxis.normalized;
		this.lastWorldPosition = base.transform.TransformPoint(this.LocalCenterOfMass);
		this.lastClackParentLocalPosition = base.transform.parent.InverseTransformPoint(this.lastWorldPosition);
		this.centerOfMassRadius = this.LocalCenterOfMass.magnitude;
		this.RotationCorrection = Quaternion.Euler(this.RotationCorrectionEuler);
	}

	private void Update()
	{
		Vector3 vector = base.transform.TransformPoint(this.LocalCenterOfMass);
		Vector3 vector2 = this.lastWorldPosition + this.velocity * Time.deltaTime * this.drag;
		Vector3 vector3 = base.transform.parent.TransformDirection(this.LocalRotationAxis);
		Vector3 vector4 = base.transform.position + (vector2 - base.transform.position).ProjectOntoPlane(vector3).normalized * this.centerOfMassRadius;
		vector4 = Vector3.MoveTowards(vector4, vector, this.localFriction * Time.deltaTime);
		this.velocity = (vector4 - this.lastWorldPosition) / Time.deltaTime;
		this.velocity += Vector3.down * this.gravity * Time.deltaTime;
		this.lastWorldPosition = vector4;
		base.transform.rotation = Quaternion.LookRotation(vector4 - base.transform.position, vector3) * this.RotationCorrection;
		Vector3 vector5 = base.transform.parent.InverseTransformPoint(base.transform.TransformPoint(this.LocalCenterOfMass));
		if ((vector5 - this.lastClackParentLocalPosition).IsLongerThan(this.distancePerClack))
		{
			this.clackAudio.GTPlayOneShot(this.allClacks[Random.Range(0, this.allClacks.Length)], 1f);
			this.lastClackParentLocalPosition = vector5;
		}
	}

	[SerializeField]
	private Vector3 LocalCenterOfMass;

	[SerializeField]
	private Vector3 LocalRotationAxis;

	[SerializeField]
	private Vector3 RotationCorrectionEuler;

	[SerializeField]
	private float drag;

	[SerializeField]
	private float gravity;

	[SerializeField]
	private float localFriction;

	[SerializeField]
	private float distancePerClack;

	[SerializeField]
	private AudioSource clackAudio;

	[SerializeField]
	private AudioClip[] allClacks;

	private float centerOfMassRadius;

	private Vector3 velocity;

	private Vector3 lastWorldPosition;

	private Vector3 lastClackParentLocalPosition;

	private Quaternion RotationCorrection;
}
