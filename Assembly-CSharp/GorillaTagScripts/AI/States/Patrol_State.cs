using System;
using UnityEngine;
using UnityEngine.AI;

namespace GorillaTagScripts.AI.States
{
	public class Patrol_State : IState
	{
		public Patrol_State(AIEntity entity)
		{
			this.entity = entity;
			this.agent = this.entity.navMeshAgent;
		}

		public void Tick()
		{
			if (this.agent.remainingDistance <= this.agent.stoppingDistance)
			{
				Vector3 position = this.entity.waypoints[Random.Range(0, this.entity.waypoints.Count - 1)].transform.position;
				this.agent.SetDestination(position);
			}
		}

		public void OnEnter()
		{
			string text = "Current State: ";
			Type typeFromHandle = typeof(Patrol_State);
			Debug.Log(text + ((typeFromHandle != null) ? typeFromHandle.ToString() : null));
			if (this.entity.waypoints.Count > 0)
			{
				this.agent.SetDestination(this.entity.waypoints[0].transform.position);
			}
		}

		public void OnExit()
		{
		}

		private AIEntity entity;

		private NavMeshAgent agent;
	}
}
