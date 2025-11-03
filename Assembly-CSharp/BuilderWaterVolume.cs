using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BuilderWaterVolume : MonoBehaviour, IBuilderPieceComponent
{
	public void OnPieceCreate(int pieceType, int pieceId)
	{
	}

	public void OnPieceDestroy()
	{
	}

	public void OnPiecePlacementDeserialized()
	{
		bool flag = (double)Vector3.Dot(Vector3.up, base.transform.up) > 0.5 && !this.piece.IsPieceMoving();
		this.waterVolume.SetActive(flag);
		this.waterMesh.SetActive(flag);
		if (this.floatingObjects != null)
		{
			this.floatingObjects.localPosition = (flag ? this.floating.localPosition : this.sunk.localPosition);
		}
	}

	public void OnPieceActivate()
	{
		bool flag = (double)Vector3.Dot(Vector3.up, base.transform.up) > 0.5 && !this.piece.IsPieceMoving();
		this.waterVolume.SetActive(flag);
		this.waterMesh.SetActive(flag);
		if (this.floatingObjects != null)
		{
			this.floatingObjects.localPosition = (flag ? this.floating.localPosition : this.sunk.localPosition);
		}
	}

	public void OnPieceDeactivate()
	{
		this.waterVolume.SetActive(false);
		this.waterMesh.SetActive(true);
		if (this.floatingObjects != null)
		{
			this.floatingObjects.localPosition = this.floating.localPosition;
		}
	}

	[SerializeField]
	private BuilderPiece piece;

	[SerializeField]
	private GameObject waterVolume;

	[SerializeField]
	private GameObject waterMesh;

	[FormerlySerializedAs("lillyPads")]
	[SerializeField]
	private Transform floatingObjects;

	[SerializeField]
	private Transform floating;

	[SerializeField]
	private Transform sunk;
}
