using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GorillaIKHandTarget : MonoBehaviour
{
	private void Start()
	{
		this.thisRigidbody = base.gameObject.GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		this.thisRigidbody.MovePosition(this.handToStickTo.transform.position);
		base.transform.rotation = this.handToStickTo.transform.rotation;
	}

	private void OnCollisionEnter(Collision collision)
	{
	}

	public GameObject handToStickTo;

	public bool isLeftHand;

	public float hapticStrength;

	private Rigidbody thisRigidbody;

	private XRController controllerReference;
}
