using System;
using UnityEngine;

public class GorillaWalkingGrab : MonoBehaviour
{
	private void Start()
	{
		this.thisRigidbody = base.gameObject.GetComponent<Rigidbody>();
		this.positionHistory = new Vector3[this.historySteps];
		this.historyIndex = 0;
	}

	private void FixedUpdate()
	{
		this.historyIndex++;
		if (this.historyIndex >= this.historySteps)
		{
			this.historyIndex = 0;
		}
		this.positionHistory[this.historyIndex] = this.handToStickTo.transform.position;
		this.thisRigidbody.MovePosition(this.handToStickTo.transform.position);
		base.transform.rotation = this.handToStickTo.transform.rotation;
	}

	private bool MakeJump()
	{
		return false;
	}

	private void OnCollisionStay(Collision collision)
	{
		if (!this.MakeJump())
		{
			Vector3 vector = Vector3.ProjectOnPlane(this.positionHistory[(this.historyIndex != 0) ? (this.historyIndex - 1) : (this.historySteps - 1)] - this.handToStickTo.transform.position, collision.GetContact(0).normal);
			Vector3 vector2 = this.thisRigidbody.transform.position - this.handToStickTo.transform.position;
			this.playspaceRigidbody.MovePosition(this.playspaceRigidbody.transform.position + vector - vector2);
		}
	}

	public GameObject handToStickTo;

	public float ratioToUse;

	public float forceMultiplier;

	public int historySteps;

	public Rigidbody playspaceRigidbody;

	private Rigidbody thisRigidbody;

	private Vector3 lastPosition;

	private Vector3 maybeLastPositionIDK;

	private Vector3[] positionHistory;

	private int historyIndex;
}
