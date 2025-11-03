using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaTagScripts;
using UnityEngine;
using UnityEngine.Events;

public class BuilderResizeWatch : MonoBehaviour
{
	public int SizeLayerMaskGrow
	{
		get
		{
			int num = 0;
			if (this.growSettings.affectLayerA)
			{
				num |= 1;
			}
			if (this.growSettings.affectLayerB)
			{
				num |= 2;
			}
			if (this.growSettings.affectLayerC)
			{
				num |= 4;
			}
			if (this.growSettings.affectLayerD)
			{
				num |= 8;
			}
			return num;
		}
	}

	public int SizeLayerMaskShrink
	{
		get
		{
			int num = 0;
			if (this.shrinkSettings.affectLayerA)
			{
				num |= 1;
			}
			if (this.shrinkSettings.affectLayerB)
			{
				num |= 2;
			}
			if (this.shrinkSettings.affectLayerC)
			{
				num |= 4;
			}
			if (this.shrinkSettings.affectLayerD)
			{
				num |= 8;
			}
			return num;
		}
	}

	private void Start()
	{
		if (this.enlargeButton != null)
		{
			this.enlargeButton.onPressButton.AddListener(new UnityAction(this.OnEnlargeButtonPressed));
		}
		if (this.shrinkButton != null)
		{
			this.shrinkButton.onPressButton.AddListener(new UnityAction(this.OnShrinkButtonPressed));
		}
		this.ownerRig = base.GetComponentInParent<VRRig>();
		this.enableDist = GTPlayer.Instance.bodyCollider.height;
		this.enableDistSq = this.enableDist * this.enableDist;
	}

	private void OnDestroy()
	{
		if (this.enlargeButton != null)
		{
			this.enlargeButton.onPressButton.RemoveListener(new UnityAction(this.OnEnlargeButtonPressed));
		}
		if (this.shrinkButton != null)
		{
			this.shrinkButton.onPressButton.RemoveListener(new UnityAction(this.OnShrinkButtonPressed));
		}
	}

	private void OnEnlargeButtonPressed()
	{
		if (this.sizeManager == null)
		{
			if (this.ownerRig == null)
			{
				Debug.LogWarning("Builder resize watch has no owner rig");
				return;
			}
			this.sizeManager = this.ownerRig.sizeManager;
		}
		if (this.sizeManager != null && this.sizeManager.currentSizeLayerMaskValue != this.SizeLayerMaskGrow && !this.updateCollision)
		{
			this.DisableCollisionWithPieces();
			this.sizeManager.currentSizeLayerMaskValue = this.SizeLayerMaskGrow;
			if (this.fxForLayerChange != null)
			{
				ObjectPools.instance.Instantiate(this.fxForLayerChange, this.ownerRig.transform.position, true);
			}
			this.timeToCheckCollision = (double)(Time.time + this.growDelay);
			this.updateCollision = true;
		}
	}

	private void DisableCollisionWithPieces()
	{
		BuilderTable builderTable;
		if (!BuilderTable.TryGetBuilderTableForZone(this.ownerRig.zoneEntity.currentZone, out builderTable))
		{
			return;
		}
		int num = Physics.OverlapSphereNonAlloc(GTPlayer.Instance.headCollider.transform.position, 1f, this.tempDisableColliders, builderTable.allPiecesMask);
		for (int i = 0; i < num; i++)
		{
			BuilderPiece builderPieceFromCollider = BuilderPiece.GetBuilderPieceFromCollider(this.tempDisableColliders[i]);
			if (builderPieceFromCollider != null && builderPieceFromCollider.state == BuilderPiece.State.AttachedAndPlaced && !builderPieceFromCollider.isBuiltIntoTable && !this.collisionDisabledPieces.Contains(builderPieceFromCollider))
			{
				foreach (Collider collider in builderPieceFromCollider.colliders)
				{
					collider.enabled = false;
				}
				foreach (Collider collider2 in builderPieceFromCollider.placedOnlyColliders)
				{
					collider2.enabled = false;
				}
				this.collisionDisabledPieces.Add(builderPieceFromCollider);
			}
		}
	}

	private void EnableCollisionWithPieces()
	{
		for (int i = this.collisionDisabledPieces.Count - 1; i >= 0; i--)
		{
			BuilderPiece builderPiece = this.collisionDisabledPieces[i];
			if (builderPiece == null)
			{
				this.collisionDisabledPieces.RemoveAt(i);
			}
			else if (Vector3.SqrMagnitude(GTPlayer.Instance.bodyCollider.transform.position - builderPiece.transform.position) >= this.enableDistSq)
			{
				this.EnableCollisionWithPiece(builderPiece);
				this.collisionDisabledPieces.RemoveAt(i);
			}
		}
	}

	private void EnableCollisionWithPiece(BuilderPiece piece)
	{
		foreach (Collider collider in piece.colliders)
		{
			collider.enabled = piece.state != BuilderPiece.State.None && piece.state != BuilderPiece.State.Displayed;
		}
		foreach (Collider collider2 in piece.placedOnlyColliders)
		{
			collider2.enabled = piece.state == BuilderPiece.State.AttachedAndPlaced;
		}
	}

	private void Update()
	{
		if (this.updateCollision && (double)Time.time >= this.timeToCheckCollision)
		{
			this.EnableCollisionWithPieces();
			if (this.collisionDisabledPieces.Count <= 0)
			{
				this.updateCollision = false;
			}
		}
	}

	private void OnShrinkButtonPressed()
	{
		if (this.sizeManager == null)
		{
			if (this.ownerRig == null)
			{
				Debug.LogWarning("Builder resize watch has no owner rig");
			}
			this.sizeManager = this.ownerRig.sizeManager;
		}
		if (this.sizeManager != null && this.sizeManager.currentSizeLayerMaskValue != this.SizeLayerMaskShrink)
		{
			this.sizeManager.currentSizeLayerMaskValue = this.SizeLayerMaskShrink;
		}
	}

	[SerializeField]
	private HeldButton enlargeButton;

	[SerializeField]
	private HeldButton shrinkButton;

	[SerializeField]
	private GameObject fxForLayerChange;

	private VRRig ownerRig;

	private SizeManager sizeManager;

	[HideInInspector]
	public Collider[] tempDisableColliders = new Collider[128];

	[HideInInspector]
	public List<BuilderPiece> collisionDisabledPieces = new List<BuilderPiece>();

	private float enableDist = 1f;

	private float enableDistSq = 1f;

	private bool updateCollision;

	private float growDelay = 1f;

	private double timeToCheckCollision;

	public BuilderResizeWatch.BuilderSizeChangeSettings growSettings;

	public BuilderResizeWatch.BuilderSizeChangeSettings shrinkSettings;

	[Serializable]
	public struct BuilderSizeChangeSettings
	{
		public bool affectLayerA;

		public bool affectLayerB;

		public bool affectLayerC;

		public bool affectLayerD;
	}
}
