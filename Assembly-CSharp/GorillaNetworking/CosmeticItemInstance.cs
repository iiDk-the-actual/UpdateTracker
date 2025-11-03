using System;
using System.Collections.Generic;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

namespace GorillaNetworking
{
	public class CosmeticItemInstance
	{
		private void EnableItem(GameObject obj, bool enable)
		{
			try
			{
				obj.SetActive(enable);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("Exception while enabling cosmetic: {0}", ex));
			}
		}

		private void ApplyClippingOffsets(bool itemEnabled)
		{
			if (this._bodyDockPositions == null)
			{
				return;
			}
			if (this._anchorOverrides != null)
			{
				if (this.clippingOffsets.nameTag.enabled)
				{
					this._anchorOverrides.UpdateNameTagOffset(itemEnabled ? this.clippingOffsets.nameTag.offset : XformOffset.Identity, itemEnabled, this.activeSlot);
				}
				if (this.clippingOffsets.leftArm.enabled)
				{
					this._anchorOverrides.ApplyAntiClippingOffsets(TransferrableObject.PositionState.OnLeftArm, this.clippingOffsets.leftArm.offset, itemEnabled, this._bodyDockPositions.leftArmTransform);
				}
				if (this.clippingOffsets.rightArm.enabled)
				{
					this._anchorOverrides.ApplyAntiClippingOffsets(TransferrableObject.PositionState.OnRightArm, this.clippingOffsets.rightArm.offset, itemEnabled, this._bodyDockPositions.rightArmTransform);
				}
				if (this.clippingOffsets.chest.enabled)
				{
					this._anchorOverrides.ApplyAntiClippingOffsets(TransferrableObject.PositionState.OnChest, this.clippingOffsets.chest.offset, itemEnabled, this._anchorOverrides.chestDefaultTransform);
				}
				if (this.clippingOffsets.huntComputer.enabled)
				{
					this._anchorOverrides.UpdateHuntWatchOffset(this.clippingOffsets.huntComputer.offset, itemEnabled);
				}
				if (this.clippingOffsets.badge.enabled)
				{
					this._anchorOverrides.UpdateBadgeOffset(itemEnabled ? this.clippingOffsets.badge.offset : XformOffset.Identity, itemEnabled, this.activeSlot);
				}
				if (this.clippingOffsets.builderWatch.enabled)
				{
					this._anchorOverrides.UpdateBuilderWatchOffset(this.clippingOffsets.builderWatch.offset, itemEnabled);
				}
				if (this.clippingOffsets.friendshipBraceletLeft.enabled)
				{
					this._anchorOverrides.UpdateFriendshipBraceletOffset(this.clippingOffsets.friendshipBraceletLeft.offset, true, itemEnabled);
				}
				if (this.clippingOffsets.friendshipBraceletRight.enabled)
				{
					this._anchorOverrides.UpdateFriendshipBraceletOffset(this.clippingOffsets.friendshipBraceletRight.offset, false, itemEnabled);
				}
			}
		}

		public void DisableItem(CosmeticsController.CosmeticSlots cosmeticSlot)
		{
			bool flag = CosmeticsController.CosmeticSet.IsSlotLeftHanded(cosmeticSlot);
			bool flag2 = CosmeticsController.CosmeticSet.IsSlotRightHanded(cosmeticSlot);
			foreach (GameObject gameObject in this.objects)
			{
				this.EnableItem(gameObject, false);
			}
			if (flag)
			{
				foreach (GameObject gameObject2 in this.leftObjects)
				{
					this.EnableItem(gameObject2, false);
				}
			}
			if (flag2)
			{
				foreach (GameObject gameObject3 in this.rightObjects)
				{
					this.EnableItem(gameObject3, false);
				}
			}
			this.ApplyClippingOffsets(false);
		}

		public void EnableItem(CosmeticsController.CosmeticSlots cosmeticSlot, VRRig rig)
		{
			bool flag = CosmeticsController.CosmeticSet.IsSlotLeftHanded(cosmeticSlot);
			bool flag2 = CosmeticsController.CosmeticSet.IsSlotRightHanded(cosmeticSlot);
			this.activeSlot = cosmeticSlot;
			if (rig != null && this._anchorOverrides == null)
			{
				this._anchorOverrides = rig.gameObject.GetComponent<VRRigAnchorOverrides>();
				this._bodyDockPositions = rig.GetComponent<BodyDockPositions>();
			}
			foreach (GameObject gameObject in this.objects)
			{
				this.EnableItem(gameObject, true);
				if (cosmeticSlot == CosmeticsController.CosmeticSlots.Badge)
				{
					if (this.objects.Count > 1)
					{
						GTHardCodedBones.EBone ebone;
						Transform transform;
						if (GTHardCodedBones.TryGetFirstBoneInParents(gameObject.transform, out ebone, out transform) && ebone == GTHardCodedBones.EBone.body)
						{
							this._anchorOverrides.CurrentBadgeTransform = gameObject.transform;
						}
					}
					else
					{
						this._anchorOverrides.CurrentBadgeTransform = gameObject.transform;
					}
				}
			}
			if (flag)
			{
				foreach (GameObject gameObject2 in this.leftObjects)
				{
					this.EnableItem(gameObject2, true);
				}
			}
			if (flag2)
			{
				foreach (GameObject gameObject3 in this.rightObjects)
				{
					this.EnableItem(gameObject3, true);
				}
			}
			this.ApplyClippingOffsets(true);
		}

		public List<GameObject> leftObjects = new List<GameObject>();

		public List<GameObject> rightObjects = new List<GameObject>();

		public List<GameObject> objects = new List<GameObject>();

		public List<GameObject> holdableObjects = new List<GameObject>();

		public CosmeticAnchorAntiIntersectOffsets clippingOffsets;

		public bool isHoldableItem;

		public string dbgname;

		private BodyDockPositions _bodyDockPositions;

		private VRRigAnchorOverrides _anchorOverrides;

		private CosmeticsController.CosmeticSlots activeSlot;
	}
}
