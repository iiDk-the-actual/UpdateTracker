using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class BuilderItem : TransferrableObject
	{
		public override bool ShouldBeKinematic()
		{
			return this.itemState == TransferrableObject.ItemStates.State2 || this.itemState == TransferrableObject.ItemStates.State4 || base.ShouldBeKinematic();
		}

		protected override void Awake()
		{
			base.Awake();
			this.parent = base.transform.parent;
			this.currTable = null;
			this.initialPosition = base.transform.position;
			this.initialRotation = base.transform.rotation;
			this.initialGrabInteractorScale = this.gripInteractor.transform.localScale;
		}

		internal override void OnEnable()
		{
			base.OnEnable();
		}

		internal override void OnDisable()
		{
			base.OnDisable();
		}

		protected override void Start()
		{
			base.Start();
			this.itemState = TransferrableObject.ItemStates.State4;
			this.currentState = TransferrableObject.PositionState.Dropped;
		}

		public void AttachPiece(BuilderPiece piece)
		{
			base.transform.SetPositionAndRotation(piece.transform.position, piece.transform.rotation);
			piece.transform.localScale = Vector3.one;
			piece.transform.SetParent(this.itemRoot.transform);
			Debug.LogFormat(piece.gameObject, "Attach Piece {0} to container {1}", new object[]
			{
				piece.gameObject.GetInstanceID(),
				base.gameObject.GetInstanceID()
			});
			this.attachedPiece = piece;
		}

		public void DetachPiece(BuilderPiece piece)
		{
			if (piece != this.attachedPiece)
			{
				Debug.LogErrorFormat("Trying to detach piece {0} from a container containing {1}", new object[]
				{
					piece.pieceId,
					this.attachedPiece.pieceId
				});
				return;
			}
			piece.transform.SetParent(null);
			Debug.LogFormat(this.attachedPiece.gameObject, "Detach Piece {0} from container {1}", new object[]
			{
				this.attachedPiece.gameObject.GetInstanceID(),
				base.gameObject.GetInstanceID()
			});
			this.attachedPiece = null;
		}

		private new void OnStateChanged()
		{
			if (this.itemState == TransferrableObject.ItemStates.State2)
			{
				this.enableCollidersWhenReady = true;
				this.gripInteractor.transform.localScale = this.initialGrabInteractorScale * 2f;
				this.handsFreeOfCollidersTime = 0f;
				return;
			}
			this.enableCollidersWhenReady = false;
			this.gripInteractor.transform.localScale = this.initialGrabInteractorScale;
			this.handsFreeOfCollidersTime = 0f;
		}

		public override Matrix4x4 GetDefaultTransformationMatrix()
		{
			if (this.reliableState.dirty)
			{
				base.SetupHandMatrix(this.reliableState.leftHandAttachPos, this.reliableState.leftHandAttachRot, this.reliableState.rightHandAttachPos, this.reliableState.rightHandAttachRot);
				this.reliableState.dirty = false;
			}
			return base.GetDefaultTransformationMatrix();
		}

		protected override void LateUpdateShared()
		{
			base.LateUpdateShared();
			if (base.InHand())
			{
				this.itemState = TransferrableObject.ItemStates.State0;
			}
			BuilderItem.BuilderItemState itemState = (BuilderItem.BuilderItemState)this.itemState;
			if (itemState != this.previousItemState)
			{
				this.OnStateChanged();
			}
			this.previousItemState = itemState;
			if (this.enableCollidersWhenReady)
			{
				bool flag = this.IsOverlapping(EquipmentInteractor.instance.overlapInteractionPointsRight) || this.IsOverlapping(EquipmentInteractor.instance.overlapInteractionPointsLeft);
				this.handsFreeOfCollidersTime += (flag ? 0f : Time.deltaTime);
				if (this.handsFreeOfCollidersTime > 0.1f)
				{
					this.gripInteractor.transform.localScale = this.initialGrabInteractorScale;
					this.enableCollidersWhenReady = false;
				}
			}
		}

		private bool IsOverlapping(List<InteractionPoint> interactionPoints)
		{
			if (interactionPoints == null)
			{
				return false;
			}
			for (int i = 0; i < interactionPoints.Count; i++)
			{
				if (interactionPoints[i] == this.gripInteractor)
				{
					return true;
				}
			}
			return false;
		}

		protected override void LateUpdateLocal()
		{
			base.LateUpdateLocal();
		}

		public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
		{
			if (GorillaTagger.Instance.offlineVRRig.scaleFactor < 1f)
			{
				return;
			}
			base.OnGrab(pointGrabbed, grabbingHand);
			this.itemState = TransferrableObject.ItemStates.State0;
		}

		public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
		{
			if (!base.OnRelease(zoneReleased, releasingHand))
			{
				return false;
			}
			this.itemState = TransferrableObject.ItemStates.State1;
			this.Reparent(null);
			this.parentItem = null;
			this.gripInteractor.transform.localScale = this.initialGrabInteractorScale;
			return true;
		}

		public void OnHoverOverTableStart(BuilderTable table)
		{
			this.currTable = table;
		}

		public void OnHoverOverTableEnd(BuilderTable table)
		{
			this.currTable = null;
		}

		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();
		}

		public override void OnLeftRoom()
		{
			base.OnLeftRoom();
			base.transform.position = this.initialPosition;
			base.transform.rotation = this.initialRotation;
			if (this.worldShareableInstance != null)
			{
				this.worldShareableInstance.transform.position = this.initialPosition;
				this.worldShareableInstance.transform.rotation = this.initialRotation;
			}
			this.itemState = TransferrableObject.ItemStates.State4;
			this.currentState = TransferrableObject.PositionState.Dropped;
		}

		private void PlayVFX(GameObject vfx)
		{
			ObjectPools.instance.Instantiate(vfx, base.transform.position, true);
		}

		private bool Reparent(Transform _transform)
		{
			if (!this.allowReparenting)
			{
				return false;
			}
			if (this.parent)
			{
				this.parent.SetParent(_transform);
				base.transform.SetParent(this.parent);
				return true;
			}
			return false;
		}

		private bool ShouldPlayFX()
		{
			return this.previousItemState == BuilderItem.BuilderItemState.isHeld || this.previousItemState == BuilderItem.BuilderItemState.dropped;
		}

		public static GameObject BuildEnvItem(int prefabHash, Vector3 position, Quaternion rotation)
		{
			GameObject gameObject = ObjectPools.instance.Instantiate(prefabHash, true);
			gameObject.transform.SetPositionAndRotation(position, rotation);
			return gameObject;
		}

		protected override void OnHandMatrixUpdate(Vector3 localPosition, Quaternion localRotation, bool leftHand)
		{
			if (leftHand)
			{
				this.reliableState.leftHandAttachPos = localPosition;
				this.reliableState.leftHandAttachRot = localRotation;
			}
			else
			{
				this.reliableState.rightHandAttachPos = localPosition;
				this.reliableState.rightHandAttachRot = localRotation;
			}
			this.reliableState.dirty = true;
		}

		public int GetPhotonViewId()
		{
			if (this.worldShareableInstance == null)
			{
				return -1;
			}
			return this.worldShareableInstance.ViewID;
		}

		public BuilderItemReliableState reliableState;

		public string builtItemPath;

		public GameObject itemRoot;

		private bool enableCollidersWhenReady;

		private float handsFreeOfCollidersTime;

		[NonSerialized]
		public BuilderPiece attachedPiece;

		public List<Behaviour> onlyWhenPlacedBehaviours;

		[NonSerialized]
		public BuilderItem parentItem;

		public List<BuilderAttachGridPlane> gridPlanes;

		public List<BuilderAttachEdge> edges;

		private List<Collider> colliders;

		private Transform parent;

		private Vector3 initialPosition;

		private Quaternion initialRotation;

		private Vector3 initialGrabInteractorScale;

		private BuilderTable currTable;

		[SerializeField]
		private AudioSource audioSource;

		public AudioClip snapAudio;

		public AudioClip placeAudio;

		public GameObject placeVFX;

		private new BuilderItem.BuilderItemState previousItemState = BuilderItem.BuilderItemState.dropped;

		private enum BuilderItemState
		{
			isHeld = 1,
			dropped,
			placed = 4,
			unused0 = 8,
			none = 16
		}
	}
}
