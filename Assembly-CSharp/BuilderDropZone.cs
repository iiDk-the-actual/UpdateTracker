using System;
using System.Collections;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;

public class BuilderDropZone : MonoBehaviour
{
	private void Awake()
	{
		this.repelDirectionWorld = base.transform.TransformDirection(this.repelDirectionLocal.normalized);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!this.onEnter)
		{
			return;
		}
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		BuilderPieceCollider component = other.GetComponent<BuilderPieceCollider>();
		if (component != null)
		{
			BuilderPiece piece = component.piece;
			if (this.table != null && this.table.builderNetworking != null)
			{
				if (piece == null)
				{
					return;
				}
				if (this.dropType == BuilderDropZone.DropType.Recycle)
				{
					bool flag = piece.state != BuilderPiece.State.Displayed && piece.state != BuilderPiece.State.OnShelf && piece.state > BuilderPiece.State.AttachedAndPlaced;
					if (!piece.isBuiltIntoTable && flag)
					{
						this.table.builderNetworking.RequestRecyclePiece(piece.pieceId, piece.transform.position, piece.transform.rotation, true, -1);
						return;
					}
				}
				else
				{
					this.table.builderNetworking.PieceEnteredDropZone(piece, this.dropType, this.dropZoneID);
				}
			}
		}
	}

	public Vector3 GetRepelDirectionWorld()
	{
		return this.repelDirectionWorld;
	}

	public void PlayEffect()
	{
		if (this.vfxRoot != null && !this.playingEffect)
		{
			this.vfxRoot.SetActive(true);
			this.playingEffect = true;
			if (this.sfxPrefab != null)
			{
				ObjectPools.instance.Instantiate(this.sfxPrefab, base.transform.position, base.transform.rotation, true);
			}
			base.StartCoroutine(this.DelayedStopEffect());
		}
	}

	private IEnumerator DelayedStopEffect()
	{
		yield return new WaitForSeconds(this.effectDuration);
		this.vfxRoot.SetActive(false);
		this.playingEffect = false;
		yield break;
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.onEnter)
		{
			return;
		}
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		BuilderPieceCollider component = other.GetComponent<BuilderPieceCollider>();
		if (component != null)
		{
			BuilderPiece piece = component.piece;
			if (this.table != null && this.table.builderNetworking != null)
			{
				if (piece == null)
				{
					return;
				}
				if (this.dropType == BuilderDropZone.DropType.Recycle)
				{
					bool flag = piece.state != BuilderPiece.State.Displayed && piece.state != BuilderPiece.State.OnShelf && piece.state > BuilderPiece.State.AttachedAndPlaced;
					if (!piece.isBuiltIntoTable && flag)
					{
						this.table.builderNetworking.RequestRecyclePiece(piece.pieceId, piece.transform.position, piece.transform.rotation, true, -1);
						return;
					}
				}
				else
				{
					this.table.builderNetworking.PieceEnteredDropZone(piece, this.dropType, this.dropZoneID);
				}
			}
		}
	}

	[SerializeField]
	private BuilderDropZone.DropType dropType;

	[SerializeField]
	private bool onEnter = true;

	[SerializeField]
	private GameObject vfxRoot;

	[SerializeField]
	private GameObject sfxPrefab;

	public float effectDuration = 1f;

	private bool playingEffect;

	public bool overrideDirection;

	[SerializeField]
	private Vector3 repelDirectionLocal = Vector3.up;

	private Vector3 repelDirectionWorld = Vector3.up;

	[HideInInspector]
	public int dropZoneID = -1;

	internal BuilderTable table;

	public enum DropType
	{
		Invalid = -1,
		Repel,
		ReturnToShelf,
		BreakApart,
		Recycle
	}
}
