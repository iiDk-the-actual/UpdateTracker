using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GorillaTagScripts.AI
{
	public class AIEntity : MonoBehaviour
	{
		protected void Awake()
		{
			this.navMeshAgent = base.gameObject.GetComponent<NavMeshAgent>();
			this.animator = base.gameObject.GetComponent<Animator>();
			if (this.waypointsContainer != null)
			{
				foreach (Transform transform in this.waypointsContainer.GetComponentsInChildren<Transform>())
				{
					this.waypoints.Add(transform);
				}
			}
		}

		protected void ChooseRandomTarget()
		{
			int randomTarget = Random.Range(0, GorillaParent.instance.vrrigs.Count);
			int num = GorillaParent.instance.vrrigs.FindIndex((VRRig x) => x.creator != null && x.creator == GorillaParent.instance.vrrigs[randomTarget].creator);
			if (num == -1)
			{
				num = Random.Range(0, GorillaParent.instance.vrrigs.Count);
			}
			if (num < GorillaParent.instance.vrrigs.Count)
			{
				this.targetPlayer = GorillaParent.instance.vrrigs[num].creator;
				this.followTarget = GorillaParent.instance.vrrigs[num].head.rigTarget;
				NavMeshHit navMeshHit;
				this.targetIsOnNavMesh = NavMesh.SamplePosition(this.followTarget.position, out navMeshHit, this.navMeshSampleRange, 1);
				return;
			}
			this.targetPlayer = null;
			this.followTarget = null;
		}

		protected void ChooseClosestTarget()
		{
			VRRig vrrig = null;
			float num = float.MaxValue;
			foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
			{
				if (vrrig2.head != null && !(vrrig2.head.rigTarget == null))
				{
					float sqrMagnitude = (base.transform.position - vrrig2.head.rigTarget.transform.position).sqrMagnitude;
					if (sqrMagnitude < this.minChaseRange * this.minChaseRange && sqrMagnitude < num)
					{
						num = sqrMagnitude;
						vrrig = vrrig2;
					}
				}
			}
			if (vrrig != null)
			{
				this.targetPlayer = vrrig.creator;
				this.followTarget = vrrig.head.rigTarget;
				NavMeshHit navMeshHit;
				this.targetIsOnNavMesh = NavMesh.SamplePosition(this.followTarget.position, out navMeshHit, this.navMeshSampleRange, 1);
				return;
			}
			this.targetPlayer = null;
			this.followTarget = null;
		}

		public GameObject waypointsContainer;

		public Transform circleCenter;

		public float circleRadius;

		public float angularSpeed;

		public float patrolSpeed;

		public float fleeSpeed;

		public NavMeshAgent navMeshAgent;

		public Animator animator;

		public float fleeRang;

		public float fleeSpeedMult;

		public float minChaseRange;

		public float attackDistance;

		public float navMeshSampleRange = 5f;

		internal readonly List<Transform> waypoints = new List<Transform>();

		internal float defaultSpeed;

		public Transform followTarget;

		public NetPlayer targetPlayer;

		public bool targetIsOnNavMesh;
	}
}
