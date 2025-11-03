using System;
using GorillaTagScripts;
using UnityEngine;

public class BuilderArmShelf : MonoBehaviour
{
	private void Start()
	{
		this.ownerRig = base.GetComponentInParent<VRRig>();
	}

	public bool IsOwnedLocally()
	{
		return this.ownerRig != null && this.ownerRig.isLocal;
	}

	public bool CanAttachToArmPiece()
	{
		return this.ownerRig != null && this.ownerRig.scaleFactor >= 1f;
	}

	public void DropAttachedPieces()
	{
		if (this.ownerRig != null && this.piece != null)
		{
			Vector3 vector = Vector3.zero;
			if (this.piece.firstChildPiece == null)
			{
				return;
			}
			BuilderTable table = this.piece.GetTable();
			Vector3 vector2 = table.roomCenter.position - this.piece.transform.position;
			vector2.Normalize();
			Vector3 vector3 = Quaternion.Euler(0f, 180f, 0f) * vector2;
			vector = BuilderTable.DROP_ZONE_REPEL * vector3;
			BuilderPiece builderPiece = this.piece.firstChildPiece;
			while (builderPiece != null)
			{
				table.RequestDropPiece(builderPiece, builderPiece.transform.position + vector3 * 0.1f, builderPiece.transform.rotation, vector, Vector3.zero);
				builderPiece = builderPiece.nextSiblingPiece;
			}
		}
	}

	[HideInInspector]
	public BuilderPiece piece;

	public Transform pieceAnchor;

	private VRRig ownerRig;
}
