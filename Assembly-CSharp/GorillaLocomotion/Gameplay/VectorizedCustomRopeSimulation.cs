using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	public class VectorizedCustomRopeSimulation : MonoBehaviour
	{
		private void Awake()
		{
			VectorizedCustomRopeSimulation.instance = this;
		}

		public static void Register(GorillaRopeSwing rope)
		{
			VectorizedCustomRopeSimulation.registerQueue.Add(rope);
		}

		public static void Unregister(GorillaRopeSwing rope)
		{
			VectorizedCustomRopeSimulation.deregisterQueue.Add(rope);
		}

		private void RegenerateData()
		{
			this.Dispose();
			foreach (GorillaRopeSwing gorillaRopeSwing in VectorizedCustomRopeSimulation.registerQueue)
			{
				this.ropes.Add(gorillaRopeSwing);
			}
			foreach (GorillaRopeSwing gorillaRopeSwing2 in VectorizedCustomRopeSimulation.deregisterQueue)
			{
				this.ropes.Remove(gorillaRopeSwing2);
			}
			VectorizedCustomRopeSimulation.registerQueue.Clear();
			VectorizedCustomRopeSimulation.deregisterQueue.Clear();
			int num = this.ropes.Count;
			while (num % 4 != 0)
			{
				num++;
			}
			int num2 = num * 32 / 4;
			this.burstData = new VectorizedBurstRopeData
			{
				posX = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				posY = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				posZ = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				validNodes = new NativeArray<int4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				lastPosX = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				lastPosY = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				lastPosZ = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				ropeRoots = new NativeArray<float3>(num, Allocator.Persistent, NativeArrayOptions.ClearMemory),
				nodeMass = new NativeArray<float4>(num2, Allocator.Persistent, NativeArrayOptions.ClearMemory)
			};
			for (int i = 0; i < this.ropes.Count; i += 4)
			{
				int num3 = 0;
				while (num3 < 4 && this.ropes.Count > i + num3)
				{
					this.ropes[i + num3].ropeDataStartIndex = 32 * i / 4;
					this.ropes[i + num3].ropeDataIndexOffset = num3;
					num3++;
				}
			}
			int num4 = 0;
			for (int j = 0; j < num2; j++)
			{
				float4 @float = this.burstData.posX[j];
				float4 float2 = this.burstData.posY[j];
				float4 float3 = this.burstData.posZ[j];
				int4 @int = this.burstData.validNodes[j];
				for (int k = 0; k < 4; k++)
				{
					int num5 = num4 * 4 + k;
					int num6 = j - num4 * 32;
					if (this.ropes.Count > num5 && this.ropes[num5].nodes.Length > num6)
					{
						Vector3 localPosition = this.ropes[num5].nodes[num6].localPosition;
						@float[k] = localPosition.x;
						float2[k] = localPosition.y;
						float3[k] = localPosition.z;
						@int[k] = 1;
					}
					else
					{
						@float[k] = 0f;
						float2[k] = 0f;
						float3[k] = 0f;
						@int[k] = 0;
					}
				}
				if (j > 0 && (j + 1) % 32 == 0)
				{
					num4++;
				}
				this.burstData.posX[j] = @float;
				this.burstData.posY[j] = float2;
				this.burstData.posZ[j] = float3;
				this.burstData.lastPosX[j] = @float;
				this.burstData.lastPosY[j] = float2;
				this.burstData.lastPosZ[j] = float3;
				this.burstData.validNodes[j] = @int;
				this.burstData.nodeMass[j] = math.float4(1f, 1f, 1f, 1f);
			}
			for (int l = 0; l < this.ropes.Count; l++)
			{
				this.burstData.ropeRoots[l] = float3.zero;
			}
		}

		private void Dispose()
		{
			if (!this.burstData.posX.IsCreated)
			{
				return;
			}
			this.burstData.posX.Dispose();
			this.burstData.posY.Dispose();
			this.burstData.posZ.Dispose();
			this.burstData.validNodes.Dispose();
			this.burstData.lastPosX.Dispose();
			this.burstData.lastPosY.Dispose();
			this.burstData.lastPosZ.Dispose();
			this.burstData.ropeRoots.Dispose();
			this.burstData.nodeMass.Dispose();
		}

		private void OnDestroy()
		{
			this.Dispose();
		}

		public void SetRopePos(GorillaRopeSwing ropeTarget, Vector3[] positions, bool setCurPos, bool setLastPos, int onlySetIndex = -1)
		{
			if (!this.ropes.Contains(ropeTarget))
			{
				return;
			}
			int ropeDataIndexOffset = ropeTarget.ropeDataIndexOffset;
			for (int i = 0; i < positions.Length; i++)
			{
				if (onlySetIndex < 0 || i == onlySetIndex)
				{
					int num = ropeTarget.ropeDataStartIndex + i;
					if (setCurPos)
					{
						float4 @float = this.burstData.posX[num];
						float4 float2 = this.burstData.posY[num];
						float4 float3 = this.burstData.posZ[num];
						@float[ropeDataIndexOffset] = positions[i].x;
						float2[ropeDataIndexOffset] = positions[i].y;
						float3[ropeDataIndexOffset] = positions[i].z;
						this.burstData.posX[num] = @float;
						this.burstData.posY[num] = float2;
						this.burstData.posZ[num] = float3;
					}
					if (setLastPos)
					{
						float4 float4 = this.burstData.lastPosX[num];
						float4 float5 = this.burstData.lastPosY[num];
						float4 float6 = this.burstData.lastPosZ[num];
						float4[ropeDataIndexOffset] = positions[i].x;
						float5[ropeDataIndexOffset] = positions[i].y;
						float6[ropeDataIndexOffset] = positions[i].z;
						this.burstData.lastPosX[num] = float4;
						this.burstData.lastPosY[num] = float5;
						this.burstData.lastPosZ[num] = float6;
					}
				}
			}
		}

		public void SetVelocity(GorillaRopeSwing ropeTarget, Vector3 velocity, bool wholeRope, int boneIndex = 1)
		{
			List<Vector3> list = new List<Vector3>();
			float num = math.min(velocity.magnitude, 15f);
			int ropeDataStartIndex = ropeTarget.ropeDataStartIndex;
			int ropeDataIndexOffset = ropeTarget.ropeDataIndexOffset;
			if (ropeTarget.SupportsMovingAtRuntime)
			{
				velocity = Quaternion.Inverse(ropeTarget.transform.rotation) * velocity;
			}
			for (int i = 0; i < ropeTarget.nodes.Length; i++)
			{
				Vector3 vector = new Vector3(this.burstData.lastPosX[ropeDataStartIndex + i][ropeDataIndexOffset], this.burstData.lastPosY[ropeDataStartIndex + i][ropeDataIndexOffset], this.burstData.lastPosZ[ropeDataStartIndex + i][ropeDataIndexOffset]);
				if ((wholeRope || boneIndex == i) && boneIndex > 0)
				{
					Vector3 vector2 = velocity / (float)boneIndex * (float)i;
					vector2 = Vector3.ClampMagnitude(vector2, num);
					list.Add(vector += vector2 * this.lastDelta);
				}
				else
				{
					list.Add(vector);
				}
			}
			int num2 = -1;
			if (!wholeRope && boneIndex > 0)
			{
				num2 = boneIndex;
			}
			this.SetRopePos(ropeTarget, list.ToArray(), true, false, num2);
		}

		public Vector3 GetNodeVelocity(GorillaRopeSwing ropeTarget, int nodeIndex)
		{
			int num = ropeTarget.ropeDataStartIndex + nodeIndex;
			int ropeDataIndexOffset = ropeTarget.ropeDataIndexOffset;
			Vector3 vector = new Vector3(this.burstData.posX[num][ropeDataIndexOffset], this.burstData.posY[num][ropeDataIndexOffset], this.burstData.posZ[num][ropeDataIndexOffset]);
			Vector3 vector2 = new Vector3(this.burstData.lastPosX[num][ropeDataIndexOffset], this.burstData.lastPosY[num][ropeDataIndexOffset], this.burstData.lastPosZ[num][ropeDataIndexOffset]);
			Vector3 vector3 = (vector - vector2) / this.lastDelta;
			if (ropeTarget.SupportsMovingAtRuntime)
			{
				vector3 = ropeTarget.transform.rotation * vector3;
			}
			return vector3;
		}

		public void SetMassForPlayers(GorillaRopeSwing ropeTarget, bool hasPlayers, int furthestBoneIndex = 0)
		{
			if (!this.ropes.Contains(ropeTarget))
			{
				return;
			}
			int ropeDataIndexOffset = ropeTarget.ropeDataIndexOffset;
			for (int i = 0; i < 32; i++)
			{
				int num = ropeTarget.ropeDataStartIndex + i;
				float4 @float = this.burstData.nodeMass[num];
				if (hasPlayers && i == furthestBoneIndex + 1)
				{
					@float[ropeDataIndexOffset] = 0.1f;
				}
				else
				{
					@float[ropeDataIndexOffset] = 1f;
				}
				this.burstData.nodeMass[num] = @float;
			}
		}

		private void Update()
		{
			if (VectorizedCustomRopeSimulation.registerQueue.Count > 0 || VectorizedCustomRopeSimulation.deregisterQueue.Count > 0)
			{
				this.RegenerateData();
			}
			if (this.ropes.Count <= 0)
			{
				return;
			}
			float num = math.min(Time.deltaTime, 0.05f);
			VectorizedSolveRopeJob vectorizedSolveRopeJob = new VectorizedSolveRopeJob
			{
				applyConstraintIterations = this.applyConstraintIterations,
				finalPassIterations = this.finalPassIterations,
				lastDeltaTime = this.lastDelta,
				deltaTime = num,
				gravity = this.gravity,
				data = this.burstData,
				nodeDistance = this.nodeDistance,
				ropeCount = this.ropes.Count
			};
			vectorizedSolveRopeJob.Schedule(default(JobHandle)).Complete();
			for (int i = 0; i < this.ropes.Count; i++)
			{
				GorillaRopeSwing gorillaRopeSwing = this.ropes[i];
				if (!gorillaRopeSwing.isIdle || !gorillaRopeSwing.isFullyIdle)
				{
					int ropeDataIndexOffset = gorillaRopeSwing.ropeDataIndexOffset;
					for (int j = 0; j < gorillaRopeSwing.nodes.Length; j++)
					{
						int num2 = gorillaRopeSwing.ropeDataStartIndex + j;
						gorillaRopeSwing.nodes[j].localPosition = new Vector3(vectorizedSolveRopeJob.data.posX[num2][ropeDataIndexOffset], vectorizedSolveRopeJob.data.posY[num2][ropeDataIndexOffset], vectorizedSolveRopeJob.data.posZ[num2][ropeDataIndexOffset]);
					}
					if (gorillaRopeSwing.SupportsMovingAtRuntime)
					{
						for (int k = 0; k < gorillaRopeSwing.nodes.Length - 1; k++)
						{
							Transform transform = gorillaRopeSwing.nodes[k];
							int ropeDataStartIndex = gorillaRopeSwing.ropeDataStartIndex;
							transform.up = gorillaRopeSwing.transform.rotation * -(gorillaRopeSwing.nodes[k + 1].localPosition - transform.localPosition);
						}
					}
					else
					{
						for (int l = 0; l < gorillaRopeSwing.nodes.Length - 1; l++)
						{
							Transform transform2 = gorillaRopeSwing.nodes[l];
							int ropeDataStartIndex2 = gorillaRopeSwing.ropeDataStartIndex;
							transform2.up = -(gorillaRopeSwing.nodes[l + 1].localPosition - transform2.localPosition);
						}
					}
				}
			}
			this.lastDelta = num;
		}

		public static VectorizedCustomRopeSimulation instance;

		public const int MAX_NODE_COUNT = 32;

		public const float MAX_ROPE_SPEED = 15f;

		private List<Transform> nodes = new List<Transform>();

		[SerializeField]
		private float nodeDistance = 1f;

		[SerializeField]
		private int applyConstraintIterations = 20;

		[SerializeField]
		private int finalPassIterations = 1;

		[SerializeField]
		private float gravity = -0.15f;

		private VectorizedBurstRopeData burstData;

		private float lastDelta = 0.02f;

		private List<GorillaRopeSwing> ropes = new List<GorillaRopeSwing>();

		private static List<GorillaRopeSwing> registerQueue = new List<GorillaRopeSwing>();

		private static List<GorillaRopeSwing> deregisterQueue = new List<GorillaRopeSwing>();
	}
}
