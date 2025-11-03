using System;
using BoingKit;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GorillaTagScripts
{
	[BurstCompile]
	public struct BuilderFindPotentialSnaps : IJobParallelFor
	{
		public void Execute(int index)
		{
			BuilderGridPlaneData builderGridPlaneData = this.gridPlanes[index];
			for (int i = 0; i < this.checkGridPlanes.Length; i++)
			{
				BuilderGridPlaneData builderGridPlaneData2 = this.checkGridPlanes[i];
				BuilderPotentialPlacementData builderPotentialPlacementData = default(BuilderPotentialPlacementData);
				if (this.TryPlaceGridPlaneOnGridPlane(ref builderGridPlaneData, ref builderGridPlaneData2, ref builderPotentialPlacementData))
				{
					this.potentialPlacements.Enqueue(builderPotentialPlacementData);
				}
			}
		}

		public bool TryPlaceGridPlaneOnGridPlane(ref BuilderGridPlaneData gridPlane, ref BuilderGridPlaneData checkGridPlane, ref BuilderPotentialPlacementData potentialPlacement)
		{
			if (checkGridPlane.male == gridPlane.male)
			{
				return false;
			}
			if (checkGridPlane.pieceId == gridPlane.pieceId)
			{
				return false;
			}
			Vector3 vector = gridPlane.position;
			Quaternion quaternion = gridPlane.rotation;
			Vector3 vector2 = this.worldToLocalRot * (vector + this.worldToLocalPos);
			Quaternion quaternion2 = this.worldToLocalRot * quaternion;
			vector = this.localToWorldPos + this.localToWorldRot * vector2;
			quaternion = this.localToWorldRot * quaternion2;
			Vector3 position = checkGridPlane.position;
			float sqrMagnitude = (position - vector).sqrMagnitude;
			float num = checkGridPlane.boundingRadius + gridPlane.boundingRadius;
			if (sqrMagnitude > num * num)
			{
				return false;
			}
			Quaternion rotation = checkGridPlane.rotation;
			Quaternion quaternion3 = Quaternion.Inverse(rotation);
			Quaternion quaternion4 = quaternion3 * quaternion;
			float num2 = Vector3.Dot(Vector3.up, quaternion4 * Vector3.up);
			if (num2 < this.currSnapParams.maxUpDotProduct)
			{
				return false;
			}
			Vector3 vector3 = quaternion3 * (vector - position);
			float y = vector3.y;
			if (Mathf.Abs(y) > 1f)
			{
				return false;
			}
			if ((gridPlane.male && y > this.currSnapParams.minOffsetY) || (!gridPlane.male && y < -this.currSnapParams.minOffsetY))
			{
				return false;
			}
			if (Mathf.Abs(y) > this.currSnapParams.maxOffsetY)
			{
				return false;
			}
			Quaternion identity = Quaternion.identity;
			Vector3 vector4 = new Vector3(quaternion4.x, quaternion4.y, quaternion4.z);
			if (vector4.sqrMagnitude > MathUtil.Epsilon)
			{
				Quaternion quaternion5;
				QuaternionUtil.DecomposeSwingTwist(quaternion4, Vector3.up, out quaternion5, out identity);
			}
			float maxTwistDotProduct = this.currSnapParams.maxTwistDotProduct;
			Vector3 vector5 = identity * Vector3.forward;
			float num3 = Vector3.Dot(vector5, Vector3.forward);
			float num4 = Vector3.Dot(vector5, Vector3.right);
			bool flag = Mathf.Abs(num3) > maxTwistDotProduct;
			bool flag2 = Mathf.Abs(num4) > maxTwistDotProduct;
			if (!flag && !flag2)
			{
				return false;
			}
			float num5;
			uint num6;
			if (flag)
			{
				num5 = ((num3 > 0f) ? 0f : 180f);
				num6 = ((num3 > 0f) ? 0U : 2U);
			}
			else
			{
				num5 = ((num4 > 0f) ? 90f : 270f);
				num6 = ((num4 > 0f) ? 1U : 3U);
			}
			int num7 = (flag2 ? gridPlane.width : gridPlane.length);
			int num8 = (flag2 ? gridPlane.length : gridPlane.width);
			float num9 = ((num8 % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num10 = ((num7 % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num11 = ((checkGridPlane.width % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num12 = ((checkGridPlane.length % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num13 = num9 - num11;
			float num14 = num10 - num12;
			int num15 = Mathf.RoundToInt((vector3.x - num13) / this.gridSize);
			int num16 = Mathf.RoundToInt((vector3.z - num14) / this.gridSize);
			int num17 = num15 + Mathf.FloorToInt((float)num8 / 2f);
			int num18 = num16 + Mathf.FloorToInt((float)num7 / 2f);
			int num19 = num17 - (num8 - 1);
			int num20 = num18 - (num7 - 1);
			int num21 = Mathf.FloorToInt((float)checkGridPlane.width / 2f);
			int num22 = Mathf.FloorToInt((float)checkGridPlane.length / 2f);
			int num23 = num21 - (checkGridPlane.width - 1);
			int num24 = num22 - (checkGridPlane.length - 1);
			if (num19 > num21 || num17 < num23 || num20 > num22 || num18 < num24)
			{
				return false;
			}
			Quaternion quaternion6 = Quaternion.Euler(0f, num5, 0f);
			Quaternion quaternion7 = rotation * quaternion6;
			float num25 = (float)num15 * this.gridSize + num13;
			float num26 = (float)num16 * this.gridSize + num14;
			Vector3 vector6 = new Vector3(num25, 0f, num26);
			Vector3 vector7 = position + rotation * vector6;
			Quaternion quaternion8 = quaternion7 * Quaternion.Inverse(gridPlane.localRotation);
			Vector3 vector8 = vector7 - quaternion8 * gridPlane.localPosition;
			potentialPlacement.localPosition = vector8;
			potentialPlacement.localRotation = quaternion8;
			float num27 = 0.025f;
			float num28 = -Mathf.Abs(y) + num2 * num27;
			potentialPlacement.score = num28;
			potentialPlacement.pieceId = gridPlane.pieceId;
			potentialPlacement.attachIndex = gridPlane.attachIndex;
			potentialPlacement.parentPieceId = checkGridPlane.pieceId;
			potentialPlacement.parentAttachIndex = checkGridPlane.attachIndex;
			potentialPlacement.attachDistance = Mathf.Abs(y);
			potentialPlacement.attachPlaneNormal = Vector3.up;
			if (!checkGridPlane.male)
			{
				potentialPlacement.attachPlaneNormal *= -1f;
			}
			potentialPlacement.parentAttachBounds.min.x = Mathf.Max(num23, num19);
			potentialPlacement.parentAttachBounds.min.y = Mathf.Max(num24, num20);
			potentialPlacement.parentAttachBounds.max.x = Mathf.Min(num21, num17);
			potentialPlacement.parentAttachBounds.max.y = Mathf.Min(num22, num18);
			potentialPlacement.twist = (byte)num6;
			potentialPlacement.bumpOffsetX = (sbyte)num15;
			potentialPlacement.bumpOffsetZ = (sbyte)num16;
			Vector2Int vector2Int = Vector2Int.zero;
			Vector2Int vector2Int2 = Vector2Int.zero;
			vector2Int.x = potentialPlacement.parentAttachBounds.min.x - num15;
			vector2Int2.x = potentialPlacement.parentAttachBounds.max.x - num15;
			vector2Int.y = potentialPlacement.parentAttachBounds.min.y - num16;
			vector2Int2.y = potentialPlacement.parentAttachBounds.max.y - num16;
			int num29 = ((num8 % 2 == 0) ? 1 : 0);
			int num30 = ((num7 % 2 == 0) ? 1 : 0);
			if (flag && num3 <= 0f)
			{
				vector2Int = this.Rotate180(vector2Int, num29, num30);
				vector2Int2 = this.Rotate180(vector2Int2, num29, num30);
			}
			else if (flag2 && num4 <= 0f)
			{
				vector2Int = this.Rotate270(vector2Int, num29, num30);
				vector2Int2 = this.Rotate270(vector2Int2, num29, num30);
			}
			else if (flag2 && num4 >= 0f)
			{
				vector2Int = this.Rotate90(vector2Int, num29, num30);
				vector2Int2 = this.Rotate90(vector2Int2, num29, num30);
			}
			potentialPlacement.attachBounds.min.x = Mathf.Min(vector2Int.x, vector2Int2.x);
			potentialPlacement.attachBounds.min.y = Mathf.Min(vector2Int.y, vector2Int2.y);
			potentialPlacement.attachBounds.max.x = Mathf.Max(vector2Int.x, vector2Int2.x);
			potentialPlacement.attachBounds.max.y = Mathf.Max(vector2Int.y, vector2Int2.y);
			return true;
		}

		private Vector2Int Rotate90(Vector2Int v, int offsetX, int offsetY)
		{
			return new Vector2Int(v.y * -1 + offsetY, v.x);
		}

		private Vector2Int Rotate270(Vector2Int v, int offsetX, int offsetY)
		{
			return new Vector2Int(v.y, v.x * -1 + offsetX);
		}

		private Vector2Int Rotate180(Vector2Int v, int offsetX, int offsetY)
		{
			return new Vector2Int(v.x * -1 + offsetX, v.y * -1 + offsetY);
		}

		[ReadOnly]
		public float gridSize;

		[ReadOnly]
		public BuilderTable.SnapParams currSnapParams;

		[ReadOnly]
		public NativeList<BuilderGridPlaneData> gridPlanes;

		[ReadOnly]
		public NativeList<BuilderGridPlaneData> checkGridPlanes;

		[ReadOnly]
		public Vector3 worldToLocalPos;

		[ReadOnly]
		public Quaternion worldToLocalRot;

		[ReadOnly]
		public Vector3 localToWorldPos;

		[ReadOnly]
		public Quaternion localToWorldRot;

		public NativeQueue<BuilderPotentialPlacementData>.ParallelWriter potentialPlacements;
	}
}
