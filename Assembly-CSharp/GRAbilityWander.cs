using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GRAbilityWander : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.moveAbility.Setup(agent, anim, audioSource, root, head, lineOfSight);
	}

	public override void Start()
	{
		base.Start();
		this.moveAbility.Start();
		Vector3 vector = this.PickRandomDestination();
		this.moveAbility.SetTargetPos(vector);
	}

	public override void Stop()
	{
		this.moveAbility.Stop();
	}

	public override bool IsDone()
	{
		return false;
	}

	public override void Think(float dt)
	{
		if (this.moveAbility.IsDone())
		{
			Vector3 vector = this.PickRandomDestination();
			this.moveAbility.SetTargetPos(vector);
		}
	}

	private Vector3 PickRandomDestination()
	{
		Vector3 vector = this.agent.transform.position;
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(vector, out navMeshHit, 1f, this.walkableArea))
		{
			Vector3 position = navMeshHit.position;
			Vector3 forward = this.agent.transform.forward;
			float num = 0f;
			Vector3 vector2 = position;
			for (int i = 0; i < GRAbilityWander.rotations.Length; i++)
			{
				Vector3 vector3 = GRAbilityWander.rotations[i] * forward;
				float num2 = 8f;
				if (NavMesh.Raycast(position, position + vector3 * num2, out navMeshHit, this.walkableArea))
				{
					num2 = navMeshHit.distance * 0.95f;
				}
				float num3 = num2 * GRAbilityWander.rotationWeight[i];
				if (num3 > num)
				{
					num = num3;
					vector2 = position + vector3 * num2;
				}
			}
			if (NavMesh.SamplePosition(vector2, out navMeshHit, 1f, this.walkableArea))
			{
				vector = navMeshHit.position;
			}
		}
		return vector;
	}

	protected override void UpdateShared(float dt)
	{
		this.moveAbility.Update(dt);
	}

	public GRAbilityMoveToTarget moveAbility;

	private static Quaternion[] rotations = new Quaternion[]
	{
		Quaternion.Euler(0f, 0f, 0f),
		Quaternion.Euler(0f, 45f, 0f),
		Quaternion.Euler(0f, -45f, 0f),
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, -90f, 0f),
		Quaternion.Euler(0f, 135f, 0f),
		Quaternion.Euler(0f, -135f, 0f),
		Quaternion.Euler(0f, 180f, 0f)
	};

	private static float[] rotationWeight = new float[] { 1f, 0.75f, 0.75f, 0.5f, 0.5f, 0.2f, 0.2f, 0.2f };
}
