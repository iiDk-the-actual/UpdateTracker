using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	public class CustomRopeSimulation : MonoBehaviour
	{
		private void Start()
		{
			Vector3 position = base.transform.position;
			for (int i = 0; i < this.nodeCount; i++)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.ropeNodePrefab);
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = position;
				this.nodes.Add(gameObject.transform);
				position.y -= this.nodeDistance;
			}
			this.nodes[this.nodes.Count - 1].GetComponentInChildren<Renderer>().enabled = false;
			this.burstNodes = new NativeArray<BurstRopeNode>(this.nodes.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		}

		private void OnDestroy()
		{
			this.burstNodes.Dispose();
		}

		private void Update()
		{
			new SolveRopeJob
			{
				fixedDeltaTime = Time.deltaTime,
				gravity = this.gravity,
				nodes = this.burstNodes,
				nodeDistance = this.nodeDistance,
				rootPos = base.transform.position
			}.Run<SolveRopeJob>();
			for (int i = 0; i < this.burstNodes.Length; i++)
			{
				this.nodes[i].position = this.burstNodes[i].curPos;
				if (i > 0)
				{
					Vector3 vector = this.burstNodes[i - 1].curPos - this.burstNodes[i].curPos;
					this.nodes[i].up = -vector;
				}
			}
		}

		private List<Transform> nodes = new List<Transform>();

		[SerializeField]
		private GameObject ropeNodePrefab;

		[SerializeField]
		private int nodeCount = 10;

		[SerializeField]
		private float nodeDistance = 0.4f;

		[SerializeField]
		private Vector3 gravity = new Vector3(0f, -9.81f, 0f);

		private NativeArray<BurstRopeNode> burstNodes;
	}
}
