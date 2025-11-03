using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace GorillaTagScripts
{
	[BurstCompile]
	internal struct FindNearbyPiecesJob : IJobParallelForTransform
	{
		public void Execute(int index, TransformAccess transform)
		{
			if (!transform.isValid)
			{
				return;
			}
			this.CheckGridPlane(index, this.leftPieceInHandIndex, transform, this.leftHandPos, true, this.leftHandGridPlanes);
			this.CheckGridPlane(index, this.rightPieceInHandIndex, transform, this.rightHandPos, false, this.rightHandGridPlanes);
		}

		private void CheckGridPlane(int gridPlaneIndex, int handPieceIndex, TransformAccess transform, Vector3 handPos, bool isLeft, NativeList<BuilderGridPlaneData>.ParallelWriter checkGridPlanes)
		{
			if (handPieceIndex < 0)
			{
				return;
			}
			if ((transform.position - handPos).sqrMagnitude > this.distanceThreshSq)
			{
				return;
			}
			BuilderGridPlaneData builderGridPlaneData = this.gridPlaneData[gridPlaneIndex];
			int pieceIndex = builderGridPlaneData.pieceIndex;
			int rootPieceIndex = this.GetRootPieceIndex(pieceIndex);
			if (rootPieceIndex == handPieceIndex)
			{
				return;
			}
			if (!this.CanPiecesPotentiallySnap(this.localPlayerActorNumber, handPieceIndex, pieceIndex, rootPieceIndex, this.pieceData[pieceIndex].requestedParentPieceIndex, isLeft))
			{
				return;
			}
			transform.GetPositionAndRotation(out builderGridPlaneData.position, out builderGridPlaneData.rotation);
			checkGridPlanes.AddNoResize(builderGridPlaneData);
		}

		public bool CanPiecesPotentiallySnap(int localActorNumber, int pieceInHandIndex, int attachToPieceIndex, int attachToPieceRootIndex, int requestedParentPieceIndex, bool isLeft)
		{
			return this.CanPlayerAttachToRootPiece(localActorNumber, attachToPieceRootIndex, isLeft) && (requestedParentPieceIndex == -1 || pieceInHandIndex != this.GetRootPieceIndex(requestedParentPieceIndex));
		}

		public bool CanPlayerAttachToRootPiece(int playerActorNumber, int attachToPieceRootIndex, bool isLeft)
		{
			BuilderPieceData builderPieceData = this.pieceData[attachToPieceRootIndex];
			if (builderPieceData.state != BuilderPiece.State.AttachedAndPlaced && builderPieceData.privatePlotIndex < 0 && builderPieceData.state != BuilderPiece.State.AttachedToArm)
			{
				return true;
			}
			int attachedBuiltInPiece = this.GetAttachedBuiltInPiece(attachToPieceRootIndex);
			if (attachedBuiltInPiece == -1)
			{
				return true;
			}
			BuilderPieceData builderPieceData2 = this.pieceData[attachedBuiltInPiece];
			if (builderPieceData2.privatePlotIndex < 0 && !builderPieceData2.isArmPiece)
			{
				return true;
			}
			if (builderPieceData2.isArmPiece)
			{
				if (builderPieceData2.heldByActorNumber == playerActorNumber)
				{
					int playerIndex = this.GetPlayerIndex(playerActorNumber);
					return playerIndex >= 0 && this.playerData[playerIndex].scale >= 1f;
				}
				return false;
			}
			else
			{
				if (builderPieceData2.privatePlotIndex < 0)
				{
					return true;
				}
				if (!this.CanPlayerAttachToPlot(builderPieceData2.privatePlotIndex, playerActorNumber))
				{
					return false;
				}
				if (!isLeft)
				{
					return this.privatePlotData[builderPieceData2.privatePlotIndex].isUnderCapacityRight;
				}
				return this.privatePlotData[builderPieceData2.privatePlotIndex].isUnderCapacityLeft;
			}
		}

		public bool CanPlayerAttachToPlot(int privatePlotIndex, int actorNumber)
		{
			BuilderPrivatePlotData builderPrivatePlotData = this.privatePlotData[privatePlotIndex];
			return (builderPrivatePlotData.plotState == BuilderPiecePrivatePlot.PlotState.Occupied && builderPrivatePlotData.ownerActorNumber == actorNumber) || (builderPrivatePlotData.plotState == BuilderPiecePrivatePlot.PlotState.Vacant && this.localPlayerPlotIndex < 0);
		}

		private int GetPlayerIndex(int playerActorNumber)
		{
			for (int i = 0; i < this.playerData.Length; i++)
			{
				if (this.playerData[i].playerActorNumber == playerActorNumber)
				{
					return i;
				}
			}
			return -1;
		}

		public int GetAttachedBuiltInPiece(int pieceIndex)
		{
			BuilderPieceData builderPieceData = this.pieceData[pieceIndex];
			if (builderPieceData.isBuiltIntoTable)
			{
				return pieceIndex;
			}
			if (builderPieceData.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return -1;
			}
			int num = this.GetRootPieceIndex(pieceIndex);
			int parentPieceIndex = this.pieceData[num].parentPieceIndex;
			if (parentPieceIndex != -1)
			{
				num = parentPieceIndex;
			}
			if (this.pieceData[num].isBuiltIntoTable)
			{
				return num;
			}
			return -1;
		}

		private int GetRootPieceIndex(int pieceIndex)
		{
			int num = pieceIndex;
			while (num != -1 && this.pieceData[num].parentPieceIndex != -1 && !this.pieceData[this.pieceData[num].parentPieceIndex].isBuiltIntoTable)
			{
				num = this.pieceData[num].parentPieceIndex;
			}
			return num;
		}

		[ReadOnly]
		public float distanceThreshSq;

		[ReadOnly]
		public Vector3 leftHandPos;

		[ReadOnly]
		public int leftPieceInHandIndex;

		[ReadOnly]
		public Vector3 rightHandPos;

		[ReadOnly]
		public int rightPieceInHandIndex;

		[ReadOnly]
		public int localPlayerPlotIndex;

		[ReadOnly]
		public int localPlayerActorNumber;

		[ReadOnly]
		public NativeArray<BuilderPieceData> pieceData;

		[ReadOnly]
		public NativeArray<BuilderGridPlaneData> gridPlaneData;

		[ReadOnly]
		public NativeArray<BuilderPrivatePlotData> privatePlotData;

		[ReadOnly]
		public NativeArray<BuilderPlayerData> playerData;

		public NativeList<BuilderGridPlaneData>.ParallelWriter leftHandGridPlanes;

		public NativeList<BuilderGridPlaneData>.ParallelWriter rightHandGridPlanes;
	}
}
