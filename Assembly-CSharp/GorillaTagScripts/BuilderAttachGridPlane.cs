using System;
using GorillaTagScripts.Builder;
using UnityEngine;

namespace GorillaTagScripts
{
	public class BuilderAttachGridPlane : MonoBehaviour
	{
		private void Awake()
		{
			if (this.center == null)
			{
				this.center = base.transform;
			}
		}

		public void Setup(BuilderPiece piece, int attachIndex, float gridSize)
		{
			this.piece = piece;
			this.attachIndex = attachIndex;
			this.pieceToGridPosition = piece.transform.InverseTransformPoint(base.transform.position);
			this.pieceToGridRotation = Quaternion.Inverse(piece.transform.rotation) * base.transform.rotation;
			float num = (float)(this.width + 2) * gridSize;
			float num2 = (float)(this.length + 2) * gridSize;
			this.boundingRadius = Mathf.Sqrt(num * num + num2 * num2);
			this.connected = new bool[this.width * this.length];
			this.widthOffset = ((this.width % 2 == 0) ? (gridSize / 2f) : 0f);
			this.lengthOffset = ((this.length % 2 == 0) ? (gridSize / 2f) : 0f);
			this.gridPlaneDataIndex = -1;
			this.childPieceCount = 0;
		}

		public void OnReturnToPool(BuilderPool pool)
		{
			SnapOverlap nextOverlap = this.firstOverlap;
			while (nextOverlap != null)
			{
				SnapOverlap snapOverlap = nextOverlap;
				nextOverlap = nextOverlap.nextOverlap;
				if (snapOverlap.otherPlane != null)
				{
					snapOverlap.otherPlane.RemoveSnapsWithPiece(this.piece, pool);
				}
				this.SetConnected(snapOverlap.bounds, false);
				pool.DestroySnapOverlap(snapOverlap);
			}
			int num = this.width * this.length;
			for (int i = 0; i < num; i++)
			{
				this.connected[i] = false;
			}
			this.childPieceCount = 0;
		}

		public Vector3 GetGridPosition(int x, int z, float gridSize)
		{
			float num = ((this.width % 2 == 0) ? (gridSize / 2f) : 0f);
			float num2 = ((this.length % 2 == 0) ? (gridSize / 2f) : 0f);
			return this.center.position + this.center.rotation * new Vector3((float)x * gridSize - num, (this.male ? 0.002f : (-0.002f)) * gridSize, (float)z * gridSize - num2);
		}

		public int GetChildCount()
		{
			return this.childPieceCount;
		}

		public void ChangeChildPieceCount(int delta)
		{
			this.childPieceCount += delta;
			if (this.piece.parentPiece == null)
			{
				return;
			}
			if (this.piece.parentAttachIndex < 0 || this.piece.parentAttachIndex >= this.piece.parentPiece.gridPlanes.Count)
			{
				return;
			}
			this.piece.parentPiece.gridPlanes[this.piece.parentAttachIndex].ChangeChildPieceCount(delta);
		}

		public void AddSnapOverlap(SnapOverlap newOverlap)
		{
			if (this.firstOverlap == null)
			{
				this.firstOverlap = newOverlap;
			}
			else
			{
				newOverlap.nextOverlap = this.firstOverlap;
				this.firstOverlap = newOverlap;
			}
			this.SetConnected(newOverlap.bounds, true);
		}

		public void RemoveSnapsWithDifferentRoot(BuilderPiece root, BuilderPool pool)
		{
			if (this.firstOverlap == null)
			{
				return;
			}
			if (pool == null)
			{
				return;
			}
			SnapOverlap snapOverlap = null;
			SnapOverlap snapOverlap2 = this.firstOverlap;
			while (snapOverlap2 != null)
			{
				if (snapOverlap2.otherPlane == null || snapOverlap2.otherPlane.piece == null)
				{
					SnapOverlap snapOverlap3 = snapOverlap2;
					if (snapOverlap == null)
					{
						this.firstOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = this.firstOverlap;
					}
					else
					{
						snapOverlap.nextOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = snapOverlap.nextOverlap;
					}
					this.SetConnected(snapOverlap3.bounds, false);
					pool.DestroySnapOverlap(snapOverlap3);
				}
				else if (root == null || snapOverlap2.otherPlane.piece.GetRootPiece() != root)
				{
					SnapOverlap snapOverlap4 = snapOverlap2;
					if (snapOverlap == null)
					{
						this.firstOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = this.firstOverlap;
					}
					else
					{
						snapOverlap.nextOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = snapOverlap.nextOverlap;
					}
					this.SetConnected(snapOverlap4.bounds, false);
					snapOverlap4.otherPlane.RemoveSnapsWithPiece(this.piece, pool);
					pool.DestroySnapOverlap(snapOverlap4);
				}
				else
				{
					snapOverlap = snapOverlap2;
					snapOverlap2 = snapOverlap2.nextOverlap;
				}
			}
		}

		public void RemoveSnapsWithPiece(BuilderPiece piece, BuilderPool pool)
		{
			if (this.firstOverlap == null)
			{
				return;
			}
			if (piece == null || pool == null)
			{
				return;
			}
			SnapOverlap snapOverlap = null;
			SnapOverlap snapOverlap2 = this.firstOverlap;
			while (snapOverlap2 != null)
			{
				if (snapOverlap2.otherPlane == null || snapOverlap2.otherPlane.piece == null)
				{
					SnapOverlap snapOverlap3 = snapOverlap2;
					if (snapOverlap == null)
					{
						this.firstOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = this.firstOverlap;
					}
					else
					{
						snapOverlap.nextOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = snapOverlap.nextOverlap;
					}
					this.SetConnected(snapOverlap3.bounds, false);
					pool.DestroySnapOverlap(snapOverlap3);
				}
				else if (snapOverlap2.otherPlane.piece == piece)
				{
					SnapOverlap snapOverlap4 = snapOverlap2;
					if (snapOverlap == null)
					{
						this.firstOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = this.firstOverlap;
					}
					else
					{
						snapOverlap.nextOverlap = snapOverlap2.nextOverlap;
						snapOverlap2 = snapOverlap.nextOverlap;
					}
					this.SetConnected(snapOverlap4.bounds, false);
					pool.DestroySnapOverlap(snapOverlap4);
				}
				else
				{
					snapOverlap = snapOverlap2;
					snapOverlap2 = snapOverlap2.nextOverlap;
				}
			}
		}

		private void SetConnected(SnapBounds bounds, bool connect)
		{
			int num = this.width / 2 - ((this.width % 2 == 0) ? 1 : 0);
			int num2 = this.length / 2 - ((this.length % 2 == 0) ? 1 : 0);
			int num3 = this.connected.Length;
			for (int i = bounds.min.x; i <= bounds.max.x; i++)
			{
				for (int j = bounds.min.y; j <= bounds.max.y; j++)
				{
					int num4 = (num + i) * this.length + (j + num2);
					if (num4 >= num3 || num4 < 0)
					{
						if (this.piece != null)
						{
							int pieceId = this.piece.pieceId;
						}
						return;
					}
					this.connected[num4] = connect;
				}
			}
		}

		public bool IsConnected(SnapBounds bounds)
		{
			int num = this.width / 2 - ((this.width % 2 == 0) ? 1 : 0);
			int num2 = this.length / 2 - ((this.length % 2 == 0) ? 1 : 0);
			int num3 = this.connected.Length;
			for (int i = bounds.min.x; i <= bounds.max.x; i++)
			{
				for (int j = bounds.min.y; j <= bounds.max.y; j++)
				{
					int num4 = (num + i) * this.length + (j + num2);
					if (num4 < 0 || num4 >= num3)
					{
						if (this.piece != null)
						{
							int pieceId = this.piece.pieceId;
						}
						return false;
					}
					if (this.connected[num4])
					{
						return true;
					}
				}
			}
			return false;
		}

		public void CalcGridOverlap(BuilderAttachGridPlane otherGridPlane, Vector3 otherPieceLocalPos, Quaternion otherPieceLocalRot, float gridSize, out Vector2Int min, out Vector2Int max)
		{
			int num = otherGridPlane.width;
			int num2 = otherGridPlane.length;
			Quaternion quaternion = otherPieceLocalRot * otherGridPlane.pieceToGridRotation;
			Vector3 lossyScale = base.transform.lossyScale;
			otherPieceLocalPos.Scale(base.transform.lossyScale);
			Vector3 vector = otherPieceLocalPos + otherPieceLocalRot * otherGridPlane.pieceToGridPosition;
			if (Mathf.Abs(Vector3.Dot(quaternion * Vector3.forward, Vector3.forward)) < 0.707f)
			{
				num = otherGridPlane.length;
				num2 = otherGridPlane.width;
			}
			float num3 = ((num % 2 == 0) ? (gridSize / 2f) : 0f);
			float num4 = ((num2 % 2 == 0) ? (gridSize / 2f) : 0f);
			float num5 = ((this.width % 2 == 0) ? (gridSize / 2f) : 0f);
			float num6 = ((this.length % 2 == 0) ? (gridSize / 2f) : 0f);
			float num7 = num3 - num5;
			float num8 = num4 - num6;
			int num9 = Mathf.RoundToInt((vector.x - num7) / gridSize);
			int num10 = Mathf.RoundToInt((vector.z - num8) / gridSize);
			int num11 = num9 + Mathf.FloorToInt((float)num / 2f);
			int num12 = num10 + Mathf.FloorToInt((float)num2 / 2f);
			int num13 = num11 - (num - 1);
			int num14 = num12 - (num2 - 1);
			int num15 = Mathf.FloorToInt((float)this.width / 2f);
			int num16 = Mathf.FloorToInt((float)this.length / 2f);
			int num17 = num15 - (this.width - 1);
			int num18 = num16 - (this.length - 1);
			min = new Vector2Int(Mathf.Max(num13, num17), Mathf.Max(num14, num18));
			max = new Vector2Int(Mathf.Min(num11, num15), Mathf.Min(num12, num16));
		}

		public bool IsAttachedToMovingGrid()
		{
			return this.piece.state == BuilderPiece.State.AttachedAndPlaced && !this.piece.isBuiltIntoTable && (this.isMoving || (!(this.piece.parentPiece == null) && this.piece.parentAttachIndex >= 0 && this.piece.parentAttachIndex < this.piece.parentPiece.gridPlanes.Count && this.piece.parentPiece.gridPlanes[this.piece.parentAttachIndex].IsAttachedToMovingGrid()));
		}

		public BuilderAttachGridPlane GetMovingParentGrid()
		{
			if (this.piece.isBuiltIntoTable)
			{
				return null;
			}
			if (this.movesOnPlace && this.movingPart != null && !this.movingPart.IsAnchoredToTable())
			{
				return this;
			}
			if (this.piece.parentPiece == null)
			{
				return null;
			}
			if (this.piece.parentAttachIndex < 0 || this.piece.parentAttachIndex >= this.piece.parentPiece.gridPlanes.Count)
			{
				return null;
			}
			return this.piece.parentPiece.gridPlanes[this.piece.parentAttachIndex].GetMovingParentGrid();
		}

		[Tooltip("Are the snap points in this grid \"outies\"")]
		public bool male;

		[Tooltip("(Optional) midpoint of the grid")]
		public Transform center;

		[Tooltip("number of snap points wide (local X-axis)")]
		public int width;

		[Tooltip("number of snap points long (local z-axis)")]
		public int length;

		[NonSerialized]
		public int gridPlaneDataIndex;

		[NonSerialized]
		public BuilderItem item;

		[NonSerialized]
		public BuilderPiece piece;

		[NonSerialized]
		public int attachIndex;

		[NonSerialized]
		public float boundingRadius;

		[NonSerialized]
		public Vector3 pieceToGridPosition;

		[NonSerialized]
		public Quaternion pieceToGridRotation;

		[NonSerialized]
		public bool[] connected;

		[NonSerialized]
		public SnapOverlap firstOverlap;

		[NonSerialized]
		public float widthOffset;

		[NonSerialized]
		public float lengthOffset;

		private int childPieceCount;

		[HideInInspector]
		public bool isMoving;

		[HideInInspector]
		public bool movesOnPlace;

		[HideInInspector]
		public BuilderMovingPart movingPart;
	}
}
