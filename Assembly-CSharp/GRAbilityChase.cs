using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GRAbilityChase : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.targetPlayer = null;
		this.lastSeenTargetTime = 0.0;
		this.lastSeenTargetPosition = Vector3.zero;
		if (GRAbilityChase.targetOffsets == null)
		{
			int num = 8;
			GRAbilityChase.targetOffsets = new List<Vector3>(num);
			float num2 = 1f;
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = new Vector3(num2, 0f, 0f);
				vector = Quaternion.Euler(0f, (float)i / (float)num * 360f, 0f) * vector;
				GRAbilityChase.targetOffsets.Add(vector);
			}
			Random random = new Random();
			List<Vector3> list = GRAbilityChase.targetOffsets.OrderBy((Vector3 x) => random.Next()).ToList<Vector3>();
			GRAbilityChase.targetOffsets.Clear();
			GRAbilityChase.targetOffsets.AddRange(list);
		}
		if (this.attributes && this.chaseSpeed == 0f)
		{
			this.chaseSpeed = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.ChaseSpeed);
		}
	}

	public override void Start()
	{
		base.Start();
		this.PlayAnim(this.animName, 0.1f, this.animSpeed);
		this.agent.navAgent.speed = this.chaseSpeed;
		this.lastSeenTargetTime = Time.timeAsDouble;
		this.movementSound.Play(null);
	}

	public override void Stop()
	{
		base.Stop();
	}

	public override bool IsDone()
	{
		return this.targetPlayer == null || Time.timeAsDouble - this.lastSeenTargetTime >= (double)this.giveUpDelay;
	}

	public override void Think(float dt)
	{
		GRPlayer grplayer = GRPlayer.Get(this.targetPlayer);
		if (grplayer != null && grplayer.State == GRPlayer.GRPlayerState.Alive)
		{
			Vector3 vector = grplayer.transform.position;
			vector += GRAbilityChase.GetMoveTargetOffset(vector, this.entity);
			if (this.lineOfSight.HasLineOfSight(this.head.position, vector))
			{
				this.lastSeenTargetTime = Time.timeAsDouble;
			}
			if ((float)(Time.timeAsDouble - this.lastSeenTargetTime) <= this.loseVisibilityDelay)
			{
				this.lastSeenTargetPosition = vector;
			}
		}
		this.agent.RequestDestination(this.lastSeenTargetPosition);
	}

	protected override void UpdateShared(float dt)
	{
		GameAgent.UpdateFacing(this.root, this.agent.navAgent, this.targetPlayer, this.maxTurnSpeed);
	}

	public void SetTargetPlayer(NetPlayer targetPlayer)
	{
		this.targetPlayer = targetPlayer;
	}

	public static Vector3 GetMoveTargetOffset(Vector3 targetPos, GameEntity attackingEntity)
	{
		int num = attackingEntity.id.index % GRAbilityChase.targetOffsets.Count;
		return GRAbilityChase.targetOffsets[num];
	}

	public float chaseSpeed;

	public string animName;

	public float animSpeed;

	public float maxTurnSpeed;

	public float loseVisibilityDelay;

	public float giveUpDelay;

	public AbilitySound movementSound;

	private NetPlayer targetPlayer;

	private double lastSeenTargetTime;

	private Vector3 lastSeenTargetPosition;

	private static List<Vector3> targetOffsets;
}
