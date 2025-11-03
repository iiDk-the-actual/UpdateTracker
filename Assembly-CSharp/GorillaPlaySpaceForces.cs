using System;
using UnityEngine;

public class GorillaPlaySpaceForces : MonoBehaviour
{
	private void Start()
	{
		this.playspaceRigidbody = base.GetComponent<Rigidbody>();
		this.leftHandRigidbody = this.leftHand.GetComponent<Rigidbody>();
		this.leftHandCollider = this.leftHand.GetComponent<Collider>();
		this.rightHandRigidbody = this.rightHand.GetComponent<Rigidbody>();
		this.rightHandCollider = this.rightHand.GetComponent<Collider>();
	}

	private void FixedUpdate()
	{
		if (Time.time >= 0.1f)
		{
			this.bodyCollider.transform.position = this.headsetTransform.position + this.bodyColliderOffset;
		}
	}

	public GameObject rightHand;

	public GameObject leftHand;

	public Collider bodyCollider;

	private Collider leftHandCollider;

	private Collider rightHandCollider;

	public Transform rightHandTransform;

	public Transform leftHandTransform;

	private Rigidbody leftHandRigidbody;

	private Rigidbody rightHandRigidbody;

	public Vector3 bodyColliderOffset;

	public float forceConstant;

	private Vector3 lastLeftHandPosition;

	private Vector3 lastRightHandPosition;

	private Rigidbody playspaceRigidbody;

	public Transform headsetTransform;
}
