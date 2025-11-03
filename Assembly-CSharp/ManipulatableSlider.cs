using System;
using UnityEngine;

public class ManipulatableSlider : ManipulatableObject
{
	private void Awake()
	{
		this.localSpace = base.transform.worldToLocalMatrix;
		this.startingPos = base.transform.localPosition;
	}

	protected override void OnStartManipulation(GameObject grabbingHand)
	{
	}

	protected override void OnStopManipulation(GameObject releasingHand, Vector3 releaseVelocity)
	{
		if (this.applyReleaseVelocity)
		{
			this.velocity = this.localSpace.MultiplyVector(releaseVelocity);
		}
	}

	protected override bool ShouldHandDetach(GameObject hand)
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = hand.transform.position;
		return Vector3.SqrMagnitude(position - position2) > this.breakDistance * this.breakDistance;
	}

	protected override void OnHeldUpdate(GameObject hand)
	{
		Vector3 vector = this.localSpace.MultiplyPoint3x4(hand.transform.position);
		vector.x = Mathf.Clamp(vector.x, this.minXOffset, this.maxXOffset);
		vector.y = Mathf.Clamp(vector.y, this.minYOffset, this.maxYOffset);
		vector.z = Mathf.Clamp(vector.z, this.minZOffset, this.maxZOffset);
		vector += this.startingPos;
		base.transform.localPosition = vector;
	}

	protected override void OnReleasedUpdate()
	{
		if (this.velocity != Vector3.zero)
		{
			Vector3 vector = this.localSpace.MultiplyPoint(base.transform.position);
			vector += this.velocity * Time.deltaTime;
			if (vector.x < this.minXOffset)
			{
				vector.x = this.minXOffset;
				this.velocity.x = 0f;
			}
			else if (vector.x > this.maxXOffset)
			{
				vector.x = this.maxXOffset;
				this.velocity.x = 0f;
			}
			if (vector.y < this.minYOffset)
			{
				vector.y = this.minYOffset;
				this.velocity.y = 0f;
			}
			else if (vector.y > this.maxYOffset)
			{
				vector.y = this.maxYOffset;
				this.velocity.y = 0f;
			}
			if (vector.z < this.minZOffset)
			{
				vector.z = this.minZOffset;
				this.velocity.z = 0f;
			}
			else if (vector.z > this.maxZOffset)
			{
				vector.z = this.maxZOffset;
				this.velocity.z = 0f;
			}
			vector += this.startingPos;
			base.transform.localPosition = vector;
			this.velocity *= 1f - this.releaseDrag * Time.deltaTime;
			if (this.velocity.sqrMagnitude < 0.001f)
			{
				this.velocity = Vector3.zero;
			}
		}
	}

	public void SetProgress(float x, float y, float z)
	{
		x = Mathf.Clamp(x, 0f, 1f);
		y = Mathf.Clamp(y, 0f, 1f);
		z = Mathf.Clamp(z, 0f, 1f);
		Vector3 vector = this.startingPos;
		vector.x += Mathf.Lerp(this.minXOffset, this.maxXOffset, x);
		vector.y += Mathf.Lerp(this.minYOffset, this.maxYOffset, y);
		vector.z += Mathf.Lerp(this.minZOffset, this.maxZOffset, z);
		base.transform.localPosition = vector;
	}

	public float GetProgressX()
	{
		return ((base.transform.localPosition - this.startingPos).x - this.minXOffset) / (this.maxXOffset - this.minXOffset);
	}

	public float GetProgressY()
	{
		return ((base.transform.localPosition - this.startingPos).y - this.minYOffset) / (this.maxYOffset - this.minYOffset);
	}

	public float GetProgressZ()
	{
		return ((base.transform.localPosition - this.startingPos).z - this.minZOffset) / (this.maxZOffset - this.minZOffset);
	}

	public float breakDistance = 0.2f;

	public float maxXOffset;

	public float minXOffset;

	public float maxYOffset;

	public float minYOffset;

	public float maxZOffset;

	public float minZOffset;

	public bool applyReleaseVelocity;

	public float releaseDrag = 1f;

	private Matrix4x4 localSpace;

	private Vector3 startingPos;

	private Vector3 velocity;
}
