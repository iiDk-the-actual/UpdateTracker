using System;
using UnityEngine;

public class PinwheelAnimator : MonoBehaviour
{
	protected void OnEnable()
	{
		this.oldPos = this.spinnerTransform.position;
		this.spinSpeed = 0f;
	}

	protected void LateUpdate()
	{
		Vector3 position = this.spinnerTransform.position;
		Vector3 forward = base.transform.forward;
		Vector3 vector = position - this.oldPos;
		float num = Mathf.Clamp(vector.magnitude / Time.deltaTime * Vector3.Dot(vector.normalized, forward) * this.spinSpeedMultiplier, -this.maxSpinSpeed, this.maxSpinSpeed);
		this.spinSpeed = Mathf.Lerp(this.spinSpeed, num, Time.deltaTime * this.damping);
		this.spinnerTransform.Rotate(Vector3.forward, this.spinSpeed * 360f * Time.deltaTime);
		this.oldPos = position;
	}

	public Transform spinnerTransform;

	[Tooltip("In revolutions per second.")]
	public float maxSpinSpeed = 4f;

	public float spinSpeedMultiplier = 5f;

	public float damping = 0.5f;

	private Vector3 oldPos;

	private float spinSpeed;
}
