using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class HeadlessHead : HoldableObject
	{
		protected void Awake()
		{
			this.ownerRig = base.GetComponentInParent<VRRig>();
			if (this.ownerRig == null)
			{
				this.ownerRig = GorillaTagger.Instance.offlineVRRig;
			}
			this.isLocal = this.ownerRig.isOfflineVRRig;
			this.stateBitsWriteInfo = VRRig.WearablePackedStatesBitWriteInfos[(int)this.wearablePackedStateSlot];
			this.baseLocalPosition = base.transform.localPosition;
			this.hasFirstPersonRenderer = this.firstPersonRenderer != null;
		}

		protected void OnEnable()
		{
			if (this.ownerRig == null)
			{
				Debug.LogError("HeadlessHead \"" + base.transform.GetPath() + "\": Deactivating because ownerRig is null.", this);
				base.gameObject.SetActive(false);
				return;
			}
			this.ownerRig.bodyRenderer.SetCosmeticBodyType(GorillaBodyType.NoHead);
		}

		private void OnDisable()
		{
			this.ownerRig.bodyRenderer.SetCosmeticBodyType(GorillaBodyType.Default);
		}

		protected virtual void LateUpdate()
		{
			if (this.isLocal)
			{
				this.LateUpdateLocal();
			}
			else
			{
				this.LateUpdateReplicated();
			}
			this.LateUpdateShared();
		}

		protected virtual void LateUpdateLocal()
		{
			this.ownerRig.WearablePackedStates = GTBitOps.WriteBits(this.ownerRig.WearablePackedStates, this.stateBitsWriteInfo, (this.isHeld ? 1 : 0) + (this.isHeldLeftHand ? 2 : 0));
		}

		protected virtual void LateUpdateReplicated()
		{
			int num = GTBitOps.ReadBits(this.ownerRig.WearablePackedStates, this.stateBitsWriteInfo.index, this.stateBitsWriteInfo.valueMask);
			this.isHeld = num != 0;
			this.isHeldLeftHand = (num & 2) != 0;
		}

		protected virtual void LateUpdateShared()
		{
			if (this.isHeld != this.wasHeld || this.isHeldLeftHand != this.wasHeldLeftHand)
			{
				this.blendingFromPosition = base.transform.position;
				this.blendingFromRotation = base.transform.rotation;
				this.blendFraction = 0f;
			}
			Quaternion quaternion;
			Vector3 vector;
			if (this.isHeldLeftHand)
			{
				quaternion = this.ownerRig.leftHandTransform.rotation * this.rotationFromLeftHand;
				vector = this.ownerRig.leftHandTransform.TransformPoint(this.offsetFromLeftHand) - quaternion * this.holdAnchorPoint.transform.localPosition;
			}
			else if (this.isHeld)
			{
				quaternion = this.ownerRig.rightHandTransform.rotation * this.rotationFromRightHand;
				vector = this.ownerRig.rightHandTransform.TransformPoint(this.offsetFromRightHand) - quaternion * this.holdAnchorPoint.transform.localPosition;
			}
			else
			{
				quaternion = base.transform.parent.rotation;
				vector = base.transform.parent.TransformPoint(this.baseLocalPosition);
			}
			if (this.blendFraction < 1f)
			{
				this.blendFraction += Time.deltaTime / this.blendDuration;
				quaternion = Quaternion.Lerp(this.blendingFromRotation, quaternion, this.blendFraction);
				vector = Vector3.Lerp(this.blendingFromPosition, vector, this.blendFraction);
			}
			base.transform.rotation = quaternion;
			base.transform.position = vector;
			if (this.hasFirstPersonRenderer)
			{
				float x = base.transform.lossyScale.x;
				this.firstPersonRenderer.enabled = (this.firstPersonHideCenter.transform.position - GTPlayer.Instance.headCollider.transform.position).IsLongerThan(this.firstPersonHiddenRadius * x);
			}
			this.wasHeld = this.isHeld;
			this.wasHeldLeftHand = this.isHeldLeftHand;
		}

		public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
		{
		}

		public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
		{
			this.isHeld = true;
			this.isHeldLeftHand = grabbingHand == EquipmentInteractor.instance.leftHand;
			EquipmentInteractor.instance.UpdateHandEquipment(this, this.isHeldLeftHand);
		}

		public override void DropItemCleanup()
		{
			this.isHeld = false;
			this.isHeldLeftHand = false;
		}

		public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
		{
			if (EquipmentInteractor.instance.rightHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.rightHand)
			{
				return false;
			}
			if (EquipmentInteractor.instance.leftHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.leftHand)
			{
				return false;
			}
			EquipmentInteractor.instance.UpdateHandEquipment(null, this.isHeldLeftHand);
			this.isHeld = false;
			this.isHeldLeftHand = false;
			return true;
		}

		[Tooltip("The slot this cosmetic resides.")]
		public VRRig.WearablePackedStateSlots wearablePackedStateSlot = VRRig.WearablePackedStateSlots.Face;

		[SerializeField]
		private Vector3 offsetFromLeftHand = new Vector3(0f, 0.0208f, 0.171f);

		[SerializeField]
		private Vector3 offsetFromRightHand = new Vector3(0f, 0.0208f, 0.171f);

		[SerializeField]
		private Quaternion rotationFromLeftHand = Quaternion.Euler(14.063973f, 52.56744f, 10.067408f);

		[SerializeField]
		private Quaternion rotationFromRightHand = Quaternion.Euler(14.063973f, 52.56744f, 10.067408f);

		private Vector3 baseLocalPosition;

		private VRRig ownerRig;

		private bool isLocal;

		private bool isHeld;

		private bool isHeldLeftHand;

		private GTBitOps.BitWriteInfo stateBitsWriteInfo;

		[SerializeField]
		private MeshRenderer firstPersonRenderer;

		[SerializeField]
		private float firstPersonHiddenRadius;

		[SerializeField]
		private Transform firstPersonHideCenter;

		[SerializeField]
		private Transform holdAnchorPoint;

		private bool hasFirstPersonRenderer;

		private Vector3 blendingFromPosition;

		private Quaternion blendingFromRotation;

		private float blendFraction;

		private bool wasHeld;

		private bool wasHeldLeftHand;

		[SerializeField]
		private float blendDuration = 0.3f;
	}
}
