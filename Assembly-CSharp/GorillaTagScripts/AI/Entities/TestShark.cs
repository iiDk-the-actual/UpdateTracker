using System;
using System.Runtime.CompilerServices;
using GorillaTagScripts.AI.States;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts.AI.Entities
{
	public class TestShark : AIEntity
	{
		private new void Awake()
		{
			base.Awake();
			this.chasingTimer = 0f;
			this._stateMachine = new StateMachine();
			this.circularPatrol = new CircularPatrol_State(this);
			this.patrol = new Patrol_State(this);
			this.chase = new Chase_State(this);
			this._stateMachine.AddTransition(this.patrol, this.chase, this.<Awake>g__ShouldChase|7_0());
			this._stateMachine.AddTransition(this.chase, this.patrol, this.<Awake>g__ShouldPatrol|7_1());
			this._stateMachine.SetState(this.patrol);
		}

		private void Update()
		{
			this._stateMachine.Tick();
			this.shouldChase = false;
			this.chasingTimer += Time.deltaTime;
			if (this.chasingTimer >= this.nextTimeToChasePlayer)
			{
				base.ChooseClosestTarget();
				if (this.followTarget != null)
				{
					this.chase.FollowTarget = this.followTarget;
					this.shouldChase = true;
				}
				this.chasingTimer = 0f;
			}
		}

		[CompilerGenerated]
		private Func<bool> <Awake>g__ShouldChase|7_0()
		{
			return () => this.shouldChase && PhotonNetwork.InRoom;
		}

		[CompilerGenerated]
		private Func<bool> <Awake>g__ShouldPatrol|7_1()
		{
			return () => this.chase.chaseOver;
		}

		public float nextTimeToChasePlayer = 30f;

		private float chasingTimer;

		private bool shouldChase;

		private StateMachine _stateMachine;

		private CircularPatrol_State circularPatrol;

		private Patrol_State patrol;

		private Chase_State chase;
	}
}
