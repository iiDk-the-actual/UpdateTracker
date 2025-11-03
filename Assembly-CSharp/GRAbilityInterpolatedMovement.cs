using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GRAbilityInterpolatedMovement
{
	public void Setup(Transform root)
	{
		this.root = root;
		this.rb = root.gameObject.GetComponent<Rigidbody>();
		this.walkableArea = NavMesh.GetAreaFromName("walkable");
	}

	public void InitFromVelocityAndDuration(Vector3 velocity, float duration)
	{
		this.velocity = velocity;
		this.duration = duration;
		float magnitude = velocity.magnitude;
	}

	public void Start()
	{
		this.startPos = this.root.position;
		this.endPos = this.startPos + this.velocity * this.duration;
		this.endTime = Time.timeAsDouble + (double)this.duration;
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(this.endPos, out navMeshHit, 5f, this.walkableArea))
		{
			this.endPos = navMeshHit.position;
		}
	}

	public void Stop()
	{
	}

	public bool IsDone()
	{
		return Time.timeAsDouble >= this.endTime;
	}

	public void Update(float dt)
	{
		Vector3 position = this.root.position;
		float num = Mathf.Clamp01(1f - (float)((this.endTime - Time.timeAsDouble) / (double)this.duration));
		GRAbilityInterpolatedMovement.InterpType interpType = this.interpolationType;
		Vector3 vector;
		if (interpType != GRAbilityInterpolatedMovement.InterpType.Linear && interpType == GRAbilityInterpolatedMovement.InterpType.EaseOut)
		{
			vector = Vector3.Lerp(this.startPos, this.endPos, AbilityHelperFunctions.EaseOutPower(num, 2.5f));
		}
		else
		{
			vector = Vector3.Lerp(this.startPos, this.endPos, num);
		}
		vector.y = Mathf.Lerp(this.startPos.y, this.endPos.y, num * num);
		NavMeshHit navMeshHit;
		if (NavMesh.Raycast(position, vector, out navMeshHit, this.walkableArea))
		{
			vector = navMeshHit.position;
		}
		this.root.position = vector;
		if (this.rb != null)
		{
			this.rb.position = vector;
		}
	}

	public Vector3 velocity = Vector3.zero;

	private Vector3 startPos;

	private Vector3 endPos;

	public float duration;

	public double endTime;

	public float maxVelocityMagnitude = 2f;

	private Transform root;

	private Rigidbody rb;

	public GRAbilityInterpolatedMovement.InterpType interpolationType;

	private int walkableArea = -1;

	public enum InterpType
	{
		Linear,
		EaseOut
	}
}
