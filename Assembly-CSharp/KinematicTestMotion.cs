using System;
using UnityEngine;

public class KinematicTestMotion : MonoBehaviour
{
	private void FixedUpdate()
	{
		if (this.updateType != KinematicTestMotion.UpdateType.FixedUpdate)
		{
			return;
		}
		this.UpdatePosition(Time.time);
	}

	private void Update()
	{
		if (this.updateType != KinematicTestMotion.UpdateType.Update)
		{
			return;
		}
		this.UpdatePosition(Time.time);
	}

	private void LateUpdate()
	{
		if (this.updateType != KinematicTestMotion.UpdateType.LateUpdate)
		{
			return;
		}
		this.UpdatePosition(Time.time);
	}

	private void UpdatePosition(float time)
	{
		float num = Mathf.Sin(time * 2f * 3.1415927f * this.period) * 0.5f + 0.5f;
		Vector3 vector = Vector3.Lerp(this.start.position, this.end.position, num);
		if (this.moveType == KinematicTestMotion.MoveType.TransformPosition)
		{
			base.transform.position = vector;
			return;
		}
		if (this.moveType == KinematicTestMotion.MoveType.RigidbodyMovePosition)
		{
			this.rigidbody.MovePosition(vector);
		}
	}

	public Transform start;

	public Transform end;

	public Rigidbody rigidbody;

	public KinematicTestMotion.UpdateType updateType;

	public KinematicTestMotion.MoveType moveType = KinematicTestMotion.MoveType.RigidbodyMovePosition;

	public float period = 4f;

	public enum UpdateType
	{
		Update,
		LateUpdate,
		FixedUpdate
	}

	public enum MoveType
	{
		TransformPosition,
		RigidbodyMovePosition
	}
}
