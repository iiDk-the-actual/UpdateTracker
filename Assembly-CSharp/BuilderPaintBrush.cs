using System;
using GorillaLocomotion.Climbing;
using Photon.Pun;
using UnityEngine;

public class BuilderPaintBrush : HoldableObject
{
	private void Awake()
	{
		this.pieceLayers |= 1 << LayerMask.NameToLayer("Gorilla Object");
		this.pieceLayers |= 1 << LayerMask.NameToLayer("BuilderProp");
		this.pieceLayers |= 1 << LayerMask.NameToLayer("Prop");
		this.paintDistance = Vector3.SqrMagnitude(this.paintVolumeHalfExtents);
		this.rb = base.GetComponent<Rigidbody>();
	}

	public override void DropItemCleanup()
	{
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		this.holdingHand = grabbingHand;
		this.handVelocity = grabbingHand.GetComponent<GorillaVelocityTracker>();
		if (this.handVelocity == null)
		{
			Debug.Log("No Velocity Estimator");
		}
		this.inLeftHand = grabbingHand == EquipmentInteractor.instance.leftHand;
		BodyDockPositions myBodyDockPositions = GorillaTagger.Instance.offlineVRRig.myBodyDockPositions;
		this.rb.isKinematic = true;
		this.rb.useGravity = false;
		if (this.inLeftHand)
		{
			base.transform.SetParent(myBodyDockPositions.leftHandTransform, true);
		}
		else
		{
			base.transform.SetParent(myBodyDockPositions.rightHandTransform, true);
		}
		base.transform.localScale = Vector3.one;
		EquipmentInteractor.instance.UpdateHandEquipment(this, this.inLeftHand);
		GorillaTagger.Instance.StartVibration(this.inLeftHand, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
		this.brushState = BuilderPaintBrush.PaintBrushState.Held;
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (base.OnRelease(zoneReleased, releasingHand))
		{
			this.holdingHand = null;
			EquipmentInteractor.instance.UpdateHandEquipment(null, this.inLeftHand);
			this.inLeftHand = false;
			this.handVelocity = null;
			this.ClearHoveredPiece();
			base.transform.parent = null;
			base.transform.localScale = Vector3.one;
			this.rb.isKinematic = false;
			this.rb.linearVelocity = Vector3.zero;
			this.rb.angularVelocity = Vector3.zero;
			this.rb.useGravity = true;
			return true;
		}
		return false;
	}

	private void LateUpdate()
	{
		if (this.brushState == BuilderPaintBrush.PaintBrushState.Inactive)
		{
			return;
		}
		if (this.holdingHand == null || this.materialType == -1)
		{
			this.brushState = BuilderPaintBrush.PaintBrushState.Inactive;
			return;
		}
		this.FindPieceToPaint();
	}

	private void FindPieceToPaint()
	{
		switch (this.brushState)
		{
		case BuilderPaintBrush.PaintBrushState.Held:
		{
			if (this.materialType == -1)
			{
				return;
			}
			Array.Clear(this.hitColliders, 0, this.hitColliders.Length);
			int num = Physics.OverlapBoxNonAlloc(this.brushSurface.transform.position - this.brushSurface.up * this.paintVolumeHalfExtents.y, this.paintVolumeHalfExtents, this.hitColliders, this.brushSurface.transform.rotation, this.pieceLayers, QueryTriggerInteraction.Ignore);
			BuilderPieceCollider builderPieceCollider = null;
			Collider collider = null;
			float num2 = float.MaxValue;
			for (int i = 0; i < num; i++)
			{
				BuilderPieceCollider component = this.hitColliders[i].GetComponent<BuilderPieceCollider>();
				if (component != null && component.piece.materialType != this.materialType && component.piece.materialType != -1)
				{
					float sqrMagnitude = (this.brushSurface.transform.position - component.transform.position).sqrMagnitude;
					if (sqrMagnitude < num2 && component.piece.CanPlayerGrabPiece(PhotonNetwork.LocalPlayer.ActorNumber, component.piece.transform.position))
					{
						num2 = sqrMagnitude;
						builderPieceCollider = component;
						collider = this.hitColliders[i];
					}
				}
			}
			if (builderPieceCollider != null)
			{
				this.ClearHoveredPiece();
				this.hoveredPiece = builderPieceCollider.piece;
				this.hoveredPieceCollider = collider;
				this.hoveredPiece.PaintingTint(true);
				GorillaTagger.Instance.StartVibration(this.inLeftHand, GorillaTagger.Instance.tapHapticStrength / 4f, GorillaTagger.Instance.tapHapticDuration);
				this.positionDelta = 0f;
				this.lastPosition = this.brushSurface.transform.position;
				this.brushState = BuilderPaintBrush.PaintBrushState.Hover;
				return;
			}
			break;
		}
		case BuilderPaintBrush.PaintBrushState.Hover:
		{
			if (this.hoveredPiece == null || this.hoveredPieceCollider == null)
			{
				this.ClearHoveredPiece();
				return;
			}
			float sqrMagnitude2 = this.handVelocity.GetLatestVelocity(false).sqrMagnitude;
			float sqrMagnitude3 = this.handVelocity.GetAverageVelocity(false, 0.15f, false).sqrMagnitude;
			if (this.handVelocity != null && (sqrMagnitude2 > this.maxPaintVelocitySqrMag || sqrMagnitude3 > this.maxPaintVelocitySqrMag))
			{
				this.ClearHoveredPiece();
				return;
			}
			Vector3 vector = this.brushSurface.position - this.brushSurface.up * this.paintVolumeHalfExtents.y;
			Vector3 vector2 = this.hoveredPieceCollider.ClosestPointOnBounds(vector);
			if (Vector3.SqrMagnitude(vector - vector2) > this.paintDistance)
			{
				this.ClearHoveredPiece();
				return;
			}
			GorillaTagger.Instance.StartVibration(this.inLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, Time.deltaTime);
			float num3 = Vector3.Distance(this.lastPosition, this.brushSurface.position);
			if (num3 < this.minimumWiggleFrameDistance)
			{
				this.lastPosition = this.brushSurface.position;
				return;
			}
			this.positionDelta += Math.Min(num3, this.maximumWiggleFrameDistance);
			this.lastPosition = this.brushSurface.position;
			if (this.positionDelta >= this.wiggleDistanceRequirement)
			{
				this.positionDelta = 0f;
				this.audioSource.clip = this.paintSound;
				this.audioSource.GTPlay();
				this.PaintPiece();
				this.brushState = BuilderPaintBrush.PaintBrushState.JustPainted;
				return;
			}
			break;
		}
		case BuilderPaintBrush.PaintBrushState.JustPainted:
			if (this.paintTimeElapsed > this.paintDelay)
			{
				this.paintTimeElapsed = 0f;
				this.brushState = BuilderPaintBrush.PaintBrushState.Held;
				return;
			}
			this.paintTimeElapsed += Time.deltaTime;
			break;
		default:
			return;
		}
	}

	private void PaintPiece()
	{
		this.hoveredPiece.GetTable().RequestPaintPiece(this.hoveredPiece.pieceId, this.materialType);
		this.hoveredPiece.PaintingTint(false);
		this.hoveredPiece = null;
		this.hoveredPieceCollider = null;
		this.paintTimeElapsed = 0f;
		GorillaTagger.Instance.StartVibration(this.inLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
	}

	private void ClearHoveredPiece()
	{
		if (this.hoveredPiece != null)
		{
			this.hoveredPiece.PaintingTint(false);
		}
		this.hoveredPiece = null;
		this.hoveredPieceCollider = null;
		this.positionDelta = 0f;
		this.brushState = ((this.holdingHand == null || this.materialType == -1) ? BuilderPaintBrush.PaintBrushState.Inactive : BuilderPaintBrush.PaintBrushState.Held);
	}

	public void SetBrushMaterial(int inMaterialType)
	{
		this.materialType = inMaterialType;
		this.audioSource.clip = this.paintSound;
		this.audioSource.GTPlay();
		if (this.holdingHand != null)
		{
			GorillaTagger.Instance.StartVibration(this.inLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		}
		if (this.materialType == -1)
		{
			this.ClearHoveredPiece();
		}
		else if (this.brushState == BuilderPaintBrush.PaintBrushState.Inactive && this.holdingHand != null)
		{
			this.brushState = BuilderPaintBrush.PaintBrushState.Held;
		}
		if (this.paintBrushMaterialOptions != null && this.brushRenderer != null)
		{
			Material material;
			int num;
			this.paintBrushMaterialOptions.GetMaterialFromType(this.materialType, out material, out num);
			if (material != null)
			{
				this.brushRenderer.material = material;
			}
		}
	}

	[SerializeField]
	private Transform brushSurface;

	[SerializeField]
	private Vector3 paintVolumeHalfExtents;

	[SerializeField]
	private BuilderMaterialOptions paintBrushMaterialOptions;

	[SerializeField]
	private MeshRenderer brushRenderer;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip paintSound;

	[SerializeField]
	private AudioClip brushStrokeSound;

	private GameObject holdingHand;

	private bool inLeftHand;

	private GorillaVelocityTracker handVelocity;

	private BuilderPiece hoveredPiece;

	private Collider hoveredPieceCollider;

	private Collider[] hitColliders = new Collider[16];

	private LayerMask pieceLayers = 0;

	private Vector3 lastPosition = Vector3.zero;

	private float positionDelta;

	private float wiggleDistanceRequirement = 0.08f;

	private float minimumWiggleFrameDistance = 0.005f;

	private float maximumWiggleFrameDistance = 0.04f;

	private float maxPaintVelocitySqrMag = 0.5f;

	private float paintDelay = 0.2f;

	private float paintTimeElapsed = -1f;

	private float paintDistance;

	private int materialType = -1;

	private BuilderPaintBrush.PaintBrushState brushState;

	private Rigidbody rb;

	public enum PaintBrushState
	{
		Inactive,
		HeldRemote,
		Held,
		Hover,
		JustPainted
	}
}
