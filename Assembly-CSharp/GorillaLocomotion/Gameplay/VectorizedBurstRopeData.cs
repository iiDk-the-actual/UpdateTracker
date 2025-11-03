using System;
using Unity.Collections;
using Unity.Mathematics;

namespace GorillaLocomotion.Gameplay
{
	public struct VectorizedBurstRopeData
	{
		public NativeArray<float4> posX;

		public NativeArray<float4> posY;

		public NativeArray<float4> posZ;

		public NativeArray<int4> validNodes;

		public NativeArray<float4> lastPosX;

		public NativeArray<float4> lastPosY;

		public NativeArray<float4> lastPosZ;

		public NativeArray<float3> ropeRoots;

		public NativeArray<float4> nodeMass;
	}
}
