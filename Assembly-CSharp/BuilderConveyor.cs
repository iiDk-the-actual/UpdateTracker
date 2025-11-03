using System;
using System.Collections.Generic;
using GorillaTagScripts;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class BuilderConveyor : MonoBehaviour
{
	private void Start()
	{
		this.InitIfNeeded();
	}

	public void Setup()
	{
		this.InitIfNeeded();
	}

	private void InitIfNeeded()
	{
		if (this.initialized)
		{
			return;
		}
		this.nextPieceToSpawn = 0;
		this.grabbedPieceTypes = new Queue<int>(10);
		this.grabbedPieceMaterials = new Queue<int>(10);
		this.setSelector.Setup(this._includeCategories);
		this.currentDisplayGroup = this.setSelector.GetSelectedGroup();
		this.piecesInSet.Clear();
		foreach (BuilderPieceSet.BuilderPieceSubset builderPieceSubset in this.currentDisplayGroup.pieceSubsets)
		{
			if (this._includeCategories.Contains(builderPieceSubset.pieceCategory))
			{
				this.piecesInSet.AddRange(builderPieceSubset.pieceInfos);
			}
		}
		double timeAsDouble = Time.timeAsDouble;
		this.nextSpawnTime = timeAsDouble + (double)this.spawnDelay;
		this.setSelector.OnSelectedGroup.AddListener(new UnityAction<int>(this.OnSelectedSetChange));
		this.initialized = true;
		this.splineLength = this.spline.Splines[0].GetLength();
		this.maxItemsOnSpline = Mathf.RoundToInt(this.splineLength / (this.conveyorMoveSpeed * this.spawnDelay)) + 5;
		this.nativeSpline = new NativeSpline(this.spline.Splines[0], this.spline.transform.localToWorldMatrix, Allocator.Persistent);
	}

	public int GetMaxItemsOnConveyor()
	{
		return Mathf.RoundToInt(this.splineLength / (this.conveyorMoveSpeed * this.spawnDelay)) + 5;
	}

	public float GetFrameMovement()
	{
		return this.conveyorMoveSpeed / this.splineLength;
	}

	private void OnDestroy()
	{
		if (this.setSelector != null)
		{
			this.setSelector.OnSelectedGroup.RemoveListener(new UnityAction<int>(this.OnSelectedSetChange));
		}
		this.nativeSpline.Dispose();
	}

	public void OnSelectedSetChange(int displayGroupID)
	{
		if (this.table.GetTableState() != BuilderTable.TableState.Ready)
		{
			return;
		}
		this.table.RequestShelfSelection(this.shelfID, displayGroupID, true);
	}

	public void SetSelection(int displayGroupID)
	{
		this.setSelector.SetSelection(displayGroupID);
		this.currentDisplayGroup = this.setSelector.GetSelectedGroup();
		this.piecesInSet.Clear();
		foreach (BuilderPieceSet.BuilderPieceSubset builderPieceSubset in this.currentDisplayGroup.pieceSubsets)
		{
			if (this._includeCategories.Contains(builderPieceSubset.pieceCategory))
			{
				this.piecesInSet.AddRange(builderPieceSubset.pieceInfos);
			}
		}
		this.nextPieceToSpawn = 0;
		this.loopCount = 0;
	}

	public int GetSelectedDisplayGroupID()
	{
		return this.setSelector.GetSelectedGroup().GetDisplayGroupIdentifier();
	}

	public void UpdateConveyor()
	{
		if (!this.initialized)
		{
			this.Setup();
		}
		for (int i = this.piecesOnConveyor.Count - 1; i >= 0; i--)
		{
			BuilderPiece builderPiece = this.piecesOnConveyor[i];
			if (builderPiece.state != BuilderPiece.State.OnConveyor)
			{
				if (PhotonNetwork.LocalPlayer.IsMasterClient && builderPiece.state != BuilderPiece.State.None)
				{
					this.grabbedPieceTypes.Enqueue(builderPiece.pieceType);
					this.grabbedPieceMaterials.Enqueue(builderPiece.materialType);
				}
				builderPiece.shelfOwner = -1;
				this.piecesOnConveyor.RemoveAt(i);
				this.table.conveyorManager.RemovePieceFromJob(builderPiece);
			}
		}
	}

	public void RemovePieceFromConveyor(Transform pieceTransform)
	{
		foreach (BuilderPiece builderPiece in this.piecesOnConveyor)
		{
			if (builderPiece.transform == pieceTransform)
			{
				this.piecesOnConveyor.Remove(builderPiece);
				builderPiece.shelfOwner = -1;
				this.table.RequestRecyclePiece(builderPiece, false, -1);
				break;
			}
		}
	}

	private Vector3 EvaluateSpline(float t)
	{
		float num;
		this._evaluateCurve = this.nativeSpline.GetCurve(this.nativeSpline.SplineToCurveT(t, out num));
		return CurveUtility.EvaluatePosition(this._evaluateCurve, num);
	}

	public void UpdateShelfSliced()
	{
		if (!PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			return;
		}
		if (this.shouldVerifySetSelection)
		{
			BuilderPieceSet.BuilderDisplayGroup selectedGroup = this.setSelector.GetSelectedGroup();
			if (selectedGroup == null || !BuilderSetManager.instance.DoesAnyPlayerInRoomOwnPieceSet(selectedGroup.setID))
			{
				int defaultGroupID = this.setSelector.GetDefaultGroupID();
				if (defaultGroupID != -1)
				{
					this.OnSelectedSetChange(defaultGroupID);
				}
			}
			this.shouldVerifySetSelection = false;
		}
		if (this.waitForResourceChange)
		{
			return;
		}
		double timeAsDouble = Time.timeAsDouble;
		if (timeAsDouble >= this.nextSpawnTime)
		{
			this.SpawnNextPiece();
			this.nextSpawnTime = timeAsDouble + (double)this.spawnDelay;
		}
	}

	public void VerifySetSelection()
	{
		this.shouldVerifySetSelection = true;
	}

	public void OnAvailableResourcesChange()
	{
		this.waitForResourceChange = false;
	}

	public Transform GetSpawnTransform()
	{
		return this.spawnTransform;
	}

	public void OnShelfPieceCreated(BuilderPiece piece, float timeOffset)
	{
		float num = timeOffset * this.conveyorMoveSpeed / this.splineLength;
		if (num > 1f)
		{
			Debug.LogWarningFormat("Piece {0} add to shelf time {1}", new object[] { piece.pieceId, num });
		}
		int count = this.piecesOnConveyor.Count;
		this.piecesOnConveyor.Add(piece);
		float num2 = Mathf.Clamp(num, 0f, 1f);
		Vector3 vector = this.EvaluateSpline(num2);
		Quaternion quaternion = this.spawnTransform.rotation * Quaternion.Euler(piece.desiredShelfRotationOffset);
		Vector3 vector2 = vector + this.spawnTransform.rotation * piece.desiredShelfOffset;
		piece.transform.SetPositionAndRotation(vector2, quaternion);
		if (num <= 1f)
		{
			this.table.conveyorManager.AddPieceToJob(piece, num2, this.shelfID);
		}
	}

	public void OnShelfPieceRecycled(BuilderPiece piece)
	{
		this.piecesOnConveyor.Remove(piece);
		if (piece != null)
		{
			this.table.conveyorManager.RemovePieceFromJob(piece);
		}
	}

	public void OnClearTable()
	{
		this.piecesOnConveyor.Clear();
		this.grabbedPieceTypes.Clear();
		this.grabbedPieceMaterials.Clear();
	}

	public void ResetConveyorState()
	{
		for (int i = this.piecesOnConveyor.Count - 1; i >= 0; i--)
		{
			BuilderPiece builderPiece = this.piecesOnConveyor[i];
			if (!(builderPiece == null))
			{
				BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
				{
					type = BuilderTable.BuilderCommandType.Recycle,
					pieceId = builderPiece.pieceId,
					localPosition = builderPiece.transform.position,
					localRotation = builderPiece.transform.rotation,
					player = NetworkSystem.Instance.LocalPlayer,
					isLeft = false,
					parentPieceId = -1
				};
				this.table.ExecutePieceRecycled(builderCommand);
			}
		}
		this.OnClearTable();
	}

	private void SpawnNextPiece()
	{
		int num;
		int num2;
		this.FindNextAffordablePieceType(out num, out num2);
		if (num == -1)
		{
			return;
		}
		this.table.RequestCreateConveyorPiece(num, num2, this.shelfID);
	}

	private void FindNextAffordablePieceType(out int pieceType, out int materialType)
	{
		if (this.grabbedPieceTypes.Count > 0)
		{
			pieceType = this.grabbedPieceTypes.Dequeue();
			materialType = this.grabbedPieceMaterials.Dequeue();
			return;
		}
		pieceType = -1;
		materialType = -1;
		if (this.piecesInSet.Count <= 0)
		{
			return;
		}
		for (int i = this.nextPieceToSpawn; i < this.piecesInSet.Count; i++)
		{
			BuilderPiece piecePrefab = this.piecesInSet[i].piecePrefab;
			if (this.table.HasEnoughResources(piecePrefab))
			{
				if (i + 1 >= this.piecesInSet.Count)
				{
					this.loopCount++;
					this.loopCount = Mathf.Max(0, this.loopCount);
				}
				this.nextPieceToSpawn = (i + 1) % this.piecesInSet.Count;
				pieceType = piecePrefab.name.GetStaticHash();
				materialType = this.GetMaterialType(this.piecesInSet[i]);
				return;
			}
		}
		this.loopCount++;
		this.loopCount = Mathf.Max(0, this.loopCount);
		for (int j = 0; j < this.nextPieceToSpawn; j++)
		{
			BuilderPiece piecePrefab2 = this.piecesInSet[j].piecePrefab;
			if (this.table.HasEnoughResources(piecePrefab2))
			{
				this.nextPieceToSpawn = (j + 1) % this.piecesInSet.Count;
				pieceType = piecePrefab2.name.GetStaticHash();
				materialType = this.GetMaterialType(this.piecesInSet[j]);
				return;
			}
		}
		this.waitForResourceChange = true;
	}

	private int GetMaterialType(BuilderPieceSet.PieceInfo info)
	{
		if (info.piecePrefab.materialOptions != null && info.overrideSetMaterial && info.pieceMaterialTypes.Length != 0)
		{
			int num = this.loopCount % info.pieceMaterialTypes.Length;
			string text = info.pieceMaterialTypes[num];
			if (string.IsNullOrEmpty(text))
			{
				Debug.LogErrorFormat("Empty Material Override for piece {0} in set {1}", new object[]
				{
					info.piecePrefab.name,
					this.currentDisplayGroup.displayName
				});
				return -1;
			}
			return text.GetHashCode();
		}
		else
		{
			if (string.IsNullOrEmpty(this.currentDisplayGroup.defaultMaterial))
			{
				return -1;
			}
			return this.currentDisplayGroup.defaultMaterial.GetHashCode();
		}
	}

	[Header("Set Selection")]
	[SerializeField]
	private BuilderSetSelector setSelector;

	public List<BuilderPieceSet.BuilderPieceCategory> _includeCategories;

	[HideInInspector]
	public BuilderTable table;

	public int shelfID = -1;

	[Header("Conveyor Properties")]
	[SerializeField]
	private Transform spawnTransform;

	[SerializeField]
	private SplineContainer spline;

	private float conveyorMoveSpeed = 0.2f;

	private float spawnDelay = 1.5f;

	private double nextSpawnTime;

	private int nextPieceToSpawn;

	private BuilderPieceSet.BuilderDisplayGroup currentDisplayGroup;

	private int loopCount;

	private List<BuilderPieceSet.PieceInfo> piecesInSet = new List<BuilderPieceSet.PieceInfo>(10);

	private Queue<int> grabbedPieceTypes;

	private Queue<int> grabbedPieceMaterials;

	private List<BuilderPiece> piecesOnConveyor = new List<BuilderPiece>(10);

	private Vector3 moveDirection;

	private bool waitForResourceChange;

	private bool initialized;

	private float splineLength = 1f;

	private int maxItemsOnSpline;

	private global::UnityEngine.Splines.BezierCurve _evaluateCurve;

	public NativeSpline nativeSpline;

	private bool shouldVerifySetSelection;
}
