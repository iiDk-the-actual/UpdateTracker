using System;
using UnityEngine;

public class GRShieldCollider : MonoBehaviour
{
	public float KnockbackVelocity
	{
		get
		{
			return this.knockbackVelocity;
		}
	}

	public GRToolDirectionalShield ShieldTool
	{
		get
		{
			return this.shieldTool;
		}
	}

	public void OnEnemyBlocked(Vector3 enemyPosition)
	{
		if (this.shieldTool != null)
		{
			this.shieldTool.OnEnemyBlocked(enemyPosition);
		}
	}

	public void BlockHittable(Vector3 enemyPosition, Vector3 enemyAttackDirection, GameHittable hittable)
	{
		if (this.shieldTool != null)
		{
			this.shieldTool.BlockHittable(enemyPosition, enemyAttackDirection, hittable, this);
		}
	}

	[SerializeField]
	private float knockbackVelocity = 3f;

	[SerializeField]
	private GRToolDirectionalShield shieldTool;
}
