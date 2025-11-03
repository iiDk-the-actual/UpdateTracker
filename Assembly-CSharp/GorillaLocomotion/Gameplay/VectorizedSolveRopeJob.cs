using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GorillaLocomotion.Gameplay
{
	[BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
	public struct VectorizedSolveRopeJob : IJob
	{
		public void Execute()
		{
			this.Simulate();
			for (int i = 0; i < this.applyConstraintIterations; i++)
			{
				this.ApplyConstraint();
			}
			for (int j = 0; j < this.finalPassIterations; j++)
			{
				this.FinalPass();
			}
		}

		private void Simulate()
		{
			for (int i = 0; i < this.data.posX.Length; i++)
			{
				float4 @float = (this.data.posX[i] - this.data.lastPosX[i]) / this.lastDeltaTime;
				float4 float2 = (this.data.posY[i] - this.data.lastPosY[i]) / this.lastDeltaTime;
				float4 float3 = (this.data.posZ[i] - this.data.lastPosZ[i]) / this.lastDeltaTime;
				this.data.lastPosX[i] = this.data.posX[i];
				this.data.lastPosY[i] = this.data.posY[i];
				this.data.lastPosZ[i] = this.data.posZ[i];
				float4 float4 = this.data.lastPosX[i] + @float * this.deltaTime * 0.996f;
				float4 float5 = this.data.lastPosY[i] + float2 * this.deltaTime;
				float4 float6 = this.data.lastPosZ[i] + float3 * this.deltaTime * 0.996f;
				float5 += this.gravity * this.deltaTime;
				this.data.posX[i] = float4 * this.data.validNodes[i];
				this.data.posY[i] = float5 * this.data.validNodes[i];
				this.data.posZ[i] = float6 * this.data.validNodes[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void dot4(ref float4 ax, ref float4 ay, ref float4 az, ref float4 bx, ref float4 by, ref float4 bz, ref float4 output)
		{
			output = ax * bx + ay * by + az * bz;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void length4(ref float4 xVals, ref float4 yVals, ref float4 zVals, ref float4 output)
		{
			float4 @float = float4.zero;
			VectorizedSolveRopeJob.dot4(ref xVals, ref yVals, ref zVals, ref xVals, ref yVals, ref zVals, ref @float);
			@float = math.abs(@float);
			output = math.sqrt(@float);
		}

		private void ConstrainRoots()
		{
			int num = 0;
			for (int i = 0; i < this.data.posX.Length; i += 32)
			{
				for (int j = 0; j < 4; j++)
				{
					float4 @float = this.data.posX[i];
					float4 float2 = this.data.posY[i];
					float4 float3 = this.data.posZ[i];
					@float[j] = this.data.ropeRoots[num].x;
					float2[j] = this.data.ropeRoots[num].y;
					float3[j] = this.data.ropeRoots[num].z;
					this.data.posX[i] = @float;
					this.data.posY[i] = float2;
					this.data.posZ[i] = float3;
					num++;
				}
			}
		}

		private void ApplyConstraint()
		{
			this.ConstrainRoots();
			float4 @float = math.int4(-1, -1, -1, -1);
			for (int i = 0; i < this.ropeCount; i += 4)
			{
				for (int j = 0; j < 31; j++)
				{
					int num = i / 4 * 32 + j;
					float4 float2 = this.data.validNodes[num];
					float4 float3 = this.data.validNodes[num + 1];
					if (math.lengthsq(float3) >= 0.1f)
					{
						float4 float4 = float4.zero;
						float4 float5 = this.data.posX[num] - this.data.posX[num + 1];
						float4 float6 = this.data.posY[num] - this.data.posY[num + 1];
						float4 float7 = this.data.posZ[num] - this.data.posZ[num + 1];
						VectorizedSolveRopeJob.length4(ref float5, ref float6, ref float7, ref float4);
						float4 float8 = math.abs(float4 - this.nodeDistance);
						float4 float9 = math.sign(float4 - this.nodeDistance);
						float4 += float2 - @float;
						float4 += 0.01f;
						float4 float10 = float5 / float4;
						float4 float11 = float6 / float4;
						float4 float12 = float7 / float4;
						float4 float13 = float9 * float10 * float8;
						float4 float14 = float9 * float11 * float8;
						float4 float15 = float9 * float12 * float8;
						float4 float16 = this.data.nodeMass[num] / (this.data.nodeMass[num] + this.data.nodeMass[num + 1]);
						float4 float17 = this.data.nodeMass[num + 1] / (this.data.nodeMass[num] + this.data.nodeMass[num + 1]);
						ref NativeArray<float4> ptr = ref this.data.posX;
						int num2 = num;
						ptr[num2] -= float13 * float3 * float16;
						ptr = ref this.data.posY;
						num2 = num;
						ptr[num2] -= float14 * float3 * float16;
						ptr = ref this.data.posZ;
						num2 = num;
						ptr[num2] -= float15 * float3 * float16;
						ptr = ref this.data.posX;
						num2 = num + 1;
						ptr[num2] += float13 * float3 * float17;
						ptr = ref this.data.posY;
						num2 = num + 1;
						ptr[num2] += float14 * float3 * float17;
						ptr = ref this.data.posZ;
						num2 = num + 1;
						ptr[num2] += float15 * float3 * float17;
					}
				}
			}
		}

		private void FinalPass()
		{
			this.ConstrainRoots();
			float4 @float = math.int4(-1, -1, -1, -1);
			for (int i = 0; i < this.ropeCount; i += 4)
			{
				for (int j = 0; j < 31; j++)
				{
					int num = i / 4 * 32 + j;
					this.data.validNodes[num];
					float4 float2 = this.data.validNodes[num + 1];
					float4 float3 = float4.zero;
					float4 float4 = this.data.posX[num] - this.data.posX[num + 1];
					float4 float5 = this.data.posY[num] - this.data.posY[num + 1];
					float4 float6 = this.data.posZ[num] - this.data.posZ[num + 1];
					VectorizedSolveRopeJob.length4(ref float4, ref float5, ref float6, ref float3);
					float4 float7 = math.abs(float3 - this.nodeDistance);
					float4 float8 = math.sign(float3 - this.nodeDistance);
					float3 += this.data.validNodes[num] - @float;
					float3 += 0.01f;
					float4 float9 = float4 / float3;
					float4 float10 = float5 / float3;
					float4 float11 = float6 / float3;
					float4 float12 = float8 * float9 * float7;
					float4 float13 = float8 * float10 * float7;
					float4 float14 = float8 * float11 * float7;
					ref NativeArray<float4> ptr = ref this.data.posX;
					int num2 = num + 1;
					ptr[num2] += float12 * float2;
					ptr = ref this.data.posY;
					num2 = num + 1;
					ptr[num2] += float13 * float2;
					ptr = ref this.data.posZ;
					num2 = num + 1;
					ptr[num2] += float14 * float2;
				}
			}
		}

		[ReadOnly]
		public int applyConstraintIterations;

		[ReadOnly]
		public int finalPassIterations;

		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public float lastDeltaTime;

		[ReadOnly]
		public int ropeCount;

		public VectorizedBurstRopeData data;

		[ReadOnly]
		public float gravity;

		[ReadOnly]
		public float nodeDistance;
	}
}
