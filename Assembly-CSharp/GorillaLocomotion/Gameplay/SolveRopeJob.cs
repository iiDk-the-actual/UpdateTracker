using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	[BurstCompile]
	public struct SolveRopeJob : IJob
	{
		public void Execute()
		{
			this.Simulate();
			for (int i = 0; i < 20; i++)
			{
				this.ApplyConstraint();
			}
		}

		private void Simulate()
		{
			for (int i = 0; i < this.nodes.Length; i++)
			{
				BurstRopeNode burstRopeNode = this.nodes[i];
				Vector3 vector = burstRopeNode.curPos - burstRopeNode.lastPos;
				burstRopeNode.lastPos = burstRopeNode.curPos;
				Vector3 vector2 = burstRopeNode.curPos + vector;
				vector2 += this.gravity * this.fixedDeltaTime;
				burstRopeNode.curPos = vector2;
				this.nodes[i] = burstRopeNode;
			}
		}

		private void ApplyConstraint()
		{
			BurstRopeNode burstRopeNode = this.nodes[0];
			burstRopeNode.curPos = this.rootPos;
			this.nodes[0] = burstRopeNode;
			for (int i = 0; i < this.nodes.Length - 1; i++)
			{
				BurstRopeNode burstRopeNode2 = this.nodes[i];
				BurstRopeNode burstRopeNode3 = this.nodes[i + 1];
				float magnitude = (burstRopeNode2.curPos - burstRopeNode3.curPos).magnitude;
				float num = Mathf.Abs(magnitude - this.nodeDistance);
				Vector3 vector = Vector3.zero;
				if (magnitude > this.nodeDistance)
				{
					vector = (burstRopeNode2.curPos - burstRopeNode3.curPos).normalized;
				}
				else if (magnitude < this.nodeDistance)
				{
					vector = (burstRopeNode3.curPos - burstRopeNode2.curPos).normalized;
				}
				Vector3 vector2 = vector * num;
				burstRopeNode2.curPos -= vector2 * 0.5f;
				burstRopeNode3.curPos += vector2 * 0.5f;
				this.nodes[i] = burstRopeNode2;
				this.nodes[i + 1] = burstRopeNode3;
			}
		}

		[ReadOnly]
		public float fixedDeltaTime;

		[WriteOnly]
		public NativeArray<BurstRopeNode> nodes;

		[ReadOnly]
		public Vector3 gravity;

		[ReadOnly]
		public Vector3 rootPos;

		[ReadOnly]
		public float nodeDistance;
	}
}
