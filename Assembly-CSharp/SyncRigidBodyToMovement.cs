using System;
using BoingKit;
using UnityEngine;

public class SyncRigidBodyToMovement : MonoBehaviour
{
	private void Awake()
	{
		this.targetParent = this.targetRigidbody.transform.parent;
		this.targetRigidbody.transform.parent = null;
		this.targetRigidbody.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		this.targetRigidbody.gameObject.SetActive(true);
		this.targetRigidbody.transform.position = base.transform.position;
		this.targetRigidbody.transform.rotation = base.transform.rotation;
	}

	private void OnDisable()
	{
		this.targetRigidbody.gameObject.SetActive(false);
	}

	private void FixedUpdate()
	{
		this.targetRigidbody.linearVelocity = (base.transform.position - this.targetRigidbody.position) / Time.fixedDeltaTime;
		this.targetRigidbody.angularVelocity = QuaternionUtil.ToAngularVector(Quaternion.Inverse(this.targetRigidbody.rotation) * base.transform.rotation) / Time.fixedDeltaTime;
	}

	[SerializeField]
	private Rigidbody targetRigidbody;

	private Transform targetParent;
}
