using System;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class VRRigAnchorOverrides : MonoBehaviour
{
	[DebugOption]
	public Transform CurrentBadgeTransform
	{
		get
		{
			return this.currentBadgeTransform;
		}
		set
		{
			if (value != this.currentBadgeTransform)
			{
				this.ResetBadge();
				this.currentBadgeTransform = value;
				this.badgeDefaultRot = this.currentBadgeTransform.localRotation;
				this.badgeDefaultPos = this.currentBadgeTransform.localPosition;
				this.UpdateBadge();
			}
		}
	}

	public Transform HuntDefaultAnchor
	{
		get
		{
			return this.huntComputerDefaultAnchor;
		}
	}

	public Transform HuntComputer
	{
		get
		{
			return this.huntComputer;
		}
	}

	public Transform BuilderWatchAnchor
	{
		get
		{
			return this.builderResizeButtonDefaultAnchor;
		}
	}

	public Transform BuilderWatch
	{
		get
		{
			return this.builderResizeButton;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < 8; i++)
		{
			this.overrideAnchors[i] = null;
		}
		int num = this.MapPositionToIndex(TransferrableObject.PositionState.OnChest);
		this.overrideAnchors[num] = this.chestDefaultTransform;
		this.huntDefaultTransform = this.huntComputer;
		this.builderResizeButtonDefaultTransform = this.builderResizeButton;
		this.activeAntiClippingOffsets = default(CosmeticAnchorAntiIntersectOffsets);
	}

	private void OnEnable()
	{
		if (this.nameDefaultAnchor && this.nameDefaultAnchor.parent)
		{
			this.nameTransform.parent = this.nameDefaultAnchor.parent;
		}
		else
		{
			Debug.LogError("VRRigAnchorOverrides: could not set parent `nameTransform` because `nameDefaultAnchor` or its parent was null!" + base.transform.GetPathQ(), this);
		}
		this.huntComputer = this.huntDefaultTransform;
		if (this.huntComputerDefaultAnchor && this.huntComputerDefaultAnchor.parent)
		{
			this.huntComputer.parent = this.huntComputerDefaultAnchor.parent;
		}
		else
		{
			Debug.LogError("VRRigAnchorOverrides: could not set parent `huntComputer` because `huntComputerDefaultAnchor` or its parent was null!" + base.transform.GetPathQ(), this);
		}
		this.builderResizeButton = this.builderResizeButtonDefaultTransform;
		if (this.builderResizeButtonDefaultAnchor && this.builderResizeButtonDefaultAnchor.parent)
		{
			this.builderResizeButton.parent = this.builderResizeButtonDefaultAnchor.parent;
			return;
		}
		Debug.LogError("VRRigAnchorOverrides: could not set parent `builderResizeButton` because `builderResizeButtonDefaultAnchor` or its parent was null! Path: " + base.transform.GetPathQ(), this);
	}

	private int MapPositionToIndex(TransferrableObject.PositionState pos)
	{
		int num = (int)pos;
		int num2 = 0;
		while ((num >>= 1) != 0)
		{
			num2++;
		}
		return num2;
	}

	public void ApplyAntiClippingOffsets(TransferrableObject.PositionState pos, XformOffset offset, bool enable, Transform defaultAnchor)
	{
		int num = this.MapPositionToIndex(pos);
		if (pos != TransferrableObject.PositionState.OnLeftArm)
		{
			if (pos != TransferrableObject.PositionState.OnRightArm)
			{
				if (pos != TransferrableObject.PositionState.OnChest)
				{
					GTDev.LogWarning<string>(string.Format("Anti Clipping offset for position {0} is not implemented", pos), null);
					return;
				}
				this.activeAntiClippingOffsets.chest.enabled = enable;
				this.activeAntiClippingOffsets.chest.offset = (enable ? offset : XformOffset.Identity);
			}
			else
			{
				this.activeAntiClippingOffsets.rightArm.enabled = enable;
				this.activeAntiClippingOffsets.rightArm.offset = (enable ? offset : XformOffset.Identity);
			}
		}
		else
		{
			this.activeAntiClippingOffsets.leftArm.enabled = enable;
			this.activeAntiClippingOffsets.leftArm.offset = (enable ? offset : XformOffset.Identity);
		}
		if (enable && (this.overrideAnchors[num] == null || (pos == TransferrableObject.PositionState.OnChest && this.overrideAnchors[num] == this.chestDefaultTransform)))
		{
			if (this.clippingOffsetTransforms[num] == null)
			{
				GameObject gameObject = new GameObject("Anti Clipping Offset");
				gameObject.transform.SetParent(defaultAnchor);
				this.clippingOffsetTransforms[num] = gameObject.transform;
			}
			Transform transform = this.clippingOffsetTransforms[num];
			transform.SetParent(defaultAnchor);
			transform.localPosition = offset.pos;
			transform.localRotation = offset.rot;
			transform.localScale = Vector3.one;
			this.OverrideAnchor(pos, transform);
			return;
		}
		if (!enable && this.overrideAnchors[num] == this.clippingOffsetTransforms[num])
		{
			if (pos == TransferrableObject.PositionState.OnChest)
			{
				this.OverrideAnchor(pos, this.chestDefaultTransform);
				return;
			}
			this.OverrideAnchor(pos, null);
		}
	}

	public void OverrideAnchor(TransferrableObject.PositionState pos, Transform anchor)
	{
		int num = this.MapPositionToIndex(pos);
		if (this.overrideAnchors[num] == this.chestDefaultTransform)
		{
			foreach (object obj in this.overrideAnchors[num])
			{
				Transform transform = (Transform)obj;
				if (!transform.name.Equals("DropZoneChest") && transform != anchor)
				{
					transform.parent = null;
				}
			}
			this.overrideAnchors[num] = anchor;
			return;
		}
		if (this.overrideAnchors[num])
		{
			foreach (object obj2 in this.overrideAnchors[num])
			{
				Transform transform2 = (Transform)obj2;
				if (transform2 != anchor)
				{
					transform2.parent = null;
				}
			}
		}
		this.overrideAnchors[num] = anchor;
	}

	public Transform AnchorOverride(TransferrableObject.PositionState pos, Transform fallback)
	{
		int num = this.MapPositionToIndex(pos);
		Transform transform = this.overrideAnchors[num];
		if (transform != null)
		{
			return transform;
		}
		return fallback;
	}

	public void UpdateHuntWatchOffset(XformOffset offset, bool enable)
	{
		this.activeAntiClippingOffsets.huntComputer.enabled = enable;
		this.activeAntiClippingOffsets.huntComputer.offset = (enable ? offset : XformOffset.Identity);
		this.huntComputer.parent = this.HuntDefaultAnchor;
		this.huntComputer.localPosition = this.activeAntiClippingOffsets.huntComputer.offset.pos;
		this.huntComputer.localRotation = this.activeAntiClippingOffsets.huntComputer.offset.rot;
	}

	public void UpdateBuilderWatchOffset(XformOffset offset, bool enable)
	{
		this.activeAntiClippingOffsets.builderWatch.enabled = enable;
		this.activeAntiClippingOffsets.builderWatch.offset = (enable ? offset : XformOffset.Identity);
		this.BuilderWatch.parent = this.BuilderWatchAnchor;
		this.BuilderWatch.localPosition = this.activeAntiClippingOffsets.builderWatch.offset.pos;
		this.BuilderWatch.localRotation = this.activeAntiClippingOffsets.builderWatch.offset.rot;
	}

	public void UpdateFriendshipBraceletOffset(XformOffset offset, bool left, bool enable)
	{
		if (left)
		{
			this.activeAntiClippingOffsets.friendshipBraceletLeft.enabled = enable;
			this.activeAntiClippingOffsets.friendshipBraceletLeft.offset = (enable ? offset : XformOffset.Identity);
			this.friendshipBraceletLeftAnchor.parent = this.friendshipBraceletLeftDefaultAnchor;
			this.friendshipBraceletLeftAnchor.localPosition = this.activeAntiClippingOffsets.friendshipBraceletLeft.offset.pos;
			this.friendshipBraceletLeftAnchor.localRotation = this.activeAntiClippingOffsets.friendshipBraceletLeft.offset.rot;
			this.friendshipBraceletLeftAnchor.localScale = this.activeAntiClippingOffsets.friendshipBraceletLeft.offset.scale;
			return;
		}
		this.activeAntiClippingOffsets.friendshipBraceletRight.enabled = enable;
		this.activeAntiClippingOffsets.friendshipBraceletRight.offset = (enable ? offset : XformOffset.Identity);
		this.friendshipBraceletRightAnchor.parent = this.friendshipBraceletRightDefaultAnchor;
		this.friendshipBraceletRightAnchor.localPosition = this.activeAntiClippingOffsets.friendshipBraceletRight.offset.pos;
		this.friendshipBraceletRightAnchor.localRotation = this.activeAntiClippingOffsets.friendshipBraceletRight.offset.rot;
		this.friendshipBraceletRightAnchor.localScale = this.activeAntiClippingOffsets.friendshipBraceletRight.offset.scale;
	}

	public void UpdateNameTagOffset(XformOffset offset, bool enable, CosmeticsController.CosmeticSlots slot)
	{
		switch (slot)
		{
		case CosmeticsController.CosmeticSlots.Hat:
			this.nameOffsets[5].enabled = enable;
			this.nameOffsets[5].offset = offset;
			break;
		case CosmeticsController.CosmeticSlots.Badge:
			this.nameOffsets[6].enabled = enable;
			this.nameOffsets[6].offset = offset;
			break;
		case CosmeticsController.CosmeticSlots.Face:
			this.nameOffsets[4].enabled = enable;
			this.nameOffsets[4].offset = offset;
			break;
		default:
			switch (slot)
			{
			case CosmeticsController.CosmeticSlots.Fur:
				this.nameOffsets[1].enabled = enable;
				this.nameOffsets[1].offset = offset;
				break;
			case CosmeticsController.CosmeticSlots.Shirt:
				this.nameOffsets[0].enabled = enable;
				this.nameOffsets[0].offset = offset;
				break;
			case CosmeticsController.CosmeticSlots.Pants:
				this.nameOffsets[2].enabled = enable;
				this.nameOffsets[2].offset = offset;
				break;
			case CosmeticsController.CosmeticSlots.Back:
				this.nameOffsets[3].enabled = enable;
				this.nameOffsets[3].offset = offset;
				break;
			}
			break;
		}
		this.UpdateName();
	}

	[Obsolete("Use UpdateNameOffset", true)]
	public void UpdateNameAnchor(GameObject nameAnchor, CosmeticsController.CosmeticSlots slot)
	{
		if (slot != CosmeticsController.CosmeticSlots.Badge)
		{
			if (slot != CosmeticsController.CosmeticSlots.Face)
			{
				switch (slot)
				{
				case CosmeticsController.CosmeticSlots.Fur:
					this.nameAnchors[1] = nameAnchor;
					break;
				case CosmeticsController.CosmeticSlots.Shirt:
					this.nameAnchors[0] = nameAnchor;
					break;
				case CosmeticsController.CosmeticSlots.Pants:
					this.nameAnchors[2] = nameAnchor;
					break;
				case CosmeticsController.CosmeticSlots.Back:
					this.nameAnchors[3] = nameAnchor;
					break;
				}
			}
			else
			{
				this.nameAnchors[4] = nameAnchor;
			}
		}
		else
		{
			this.nameAnchors[5] = nameAnchor;
		}
		this.UpdateName();
	}

	private void UpdateName()
	{
		for (int i = 0; i < this.nameOffsets.Length; i++)
		{
			if (this.nameOffsets[i].enabled)
			{
				this.nameTransform.parent = this.nameDefaultAnchor;
				this.nameTransform.localRotation = this.nameOffsets[i].offset.rot;
				this.nameTransform.localPosition = this.nameOffsets[i].offset.pos;
				return;
			}
		}
		if (this.nameDefaultAnchor)
		{
			this.nameTransform.parent = this.nameDefaultAnchor;
			this.nameTransform.localRotation = Quaternion.identity;
			this.nameTransform.localPosition = Vector3.zero;
			return;
		}
		Debug.LogError("VRRigAnchorOverrides: could not set parent for `nameTransform` because `nameDefaultAnchor` or its parent was null! Path: " + base.transform.GetPathQ(), this);
	}

	public void UpdateBadgeOffset(XformOffset offset, bool enable, CosmeticsController.CosmeticSlots slot)
	{
		if (slot != CosmeticsController.CosmeticSlots.Hat)
		{
			if (slot != CosmeticsController.CosmeticSlots.Face)
			{
				switch (slot)
				{
				case CosmeticsController.CosmeticSlots.Fur:
					this.badgeOffsets[1].enabled = enable;
					this.badgeOffsets[1].offset = offset;
					break;
				case CosmeticsController.CosmeticSlots.Shirt:
					this.badgeOffsets[0].enabled = enable;
					this.badgeOffsets[0].offset = offset;
					break;
				case CosmeticsController.CosmeticSlots.Pants:
					this.badgeOffsets[2].enabled = enable;
					this.badgeOffsets[2].offset = offset;
					break;
				case CosmeticsController.CosmeticSlots.Back:
					this.badgeOffsets[3].enabled = enable;
					this.badgeOffsets[3].offset = offset;
					break;
				}
			}
			else
			{
				this.badgeOffsets[4].enabled = enable;
				this.badgeOffsets[4].offset = offset;
			}
		}
		else
		{
			this.badgeOffsets[5].enabled = enable;
			this.badgeOffsets[5].offset = offset;
		}
		this.UpdateBadge();
	}

	[Obsolete("Use UpdateBadgeOffset", true)]
	public void UpdateBadgeAnchor(GameObject badgeAnchor, CosmeticsController.CosmeticSlots slot)
	{
		switch (slot)
		{
		case CosmeticsController.CosmeticSlots.Fur:
			this.badgeAnchors[1] = badgeAnchor;
			break;
		case CosmeticsController.CosmeticSlots.Shirt:
			this.badgeAnchors[0] = badgeAnchor;
			break;
		case CosmeticsController.CosmeticSlots.Pants:
			this.badgeAnchors[2] = badgeAnchor;
			break;
		case CosmeticsController.CosmeticSlots.Back:
			this.badgeAnchors[3] = badgeAnchor;
			break;
		}
		this.UpdateBadge();
	}

	private void UpdateBadge()
	{
		if (!this.currentBadgeTransform)
		{
			return;
		}
		for (int i = 0; i < this.badgeOffsets.Length; i++)
		{
			if (this.badgeOffsets[i].enabled)
			{
				Matrix4x4 matrix4x = Matrix4x4.TRS(this.badgeDefaultPos, this.badgeDefaultRot, this.currentBadgeTransform.localScale);
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(this.badgeOffsets[i].offset.pos, this.badgeOffsets[i].offset.rot, Vector3.one) * matrix4x;
				this.currentBadgeTransform.localRotation = matrix4x2.rotation;
				this.currentBadgeTransform.localPosition = matrix4x2.Position();
				return;
			}
		}
		foreach (GameObject gameObject in this.badgeAnchors)
		{
			if (gameObject)
			{
				this.currentBadgeTransform.localRotation = gameObject.transform.localRotation;
				this.currentBadgeTransform.localPosition = gameObject.transform.localPosition;
				return;
			}
		}
		this.ResetBadge();
	}

	private void ResetBadge()
	{
		if (!this.currentBadgeTransform)
		{
			return;
		}
		this.currentBadgeTransform.localRotation = this.badgeDefaultRot;
		this.currentBadgeTransform.localPosition = this.badgeDefaultPos;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < this.clippingOffsetTransforms.Length; i++)
		{
			if (this.clippingOffsetTransforms[i] != null)
			{
				foreach (object obj in this.clippingOffsetTransforms[i])
				{
					((Transform)obj).parent = null;
				}
				Object.Destroy(this.clippingOffsetTransforms[i].gameObject);
			}
		}
	}

	[SerializeField]
	public Transform nameDefaultAnchor;

	[SerializeField]
	public Transform nameTransform;

	[SerializeField]
	public Transform chestDefaultTransform;

	[SerializeField]
	public Transform huntComputer;

	[SerializeField]
	public Transform huntComputerDefaultAnchor;

	public Transform huntDefaultTransform;

	[SerializeField]
	protected Transform builderResizeButton;

	[SerializeField]
	protected Transform builderResizeButtonDefaultAnchor;

	private Transform builderResizeButtonDefaultTransform;

	private readonly Transform[] overrideAnchors = new Transform[8];

	private CosmeticAnchorAntiIntersectOffsets activeAntiClippingOffsets;

	private Transform[] clippingOffsetTransforms = new Transform[8];

	private GameObject nameLastObjectToAttach;

	private Transform currentBadgeTransform;

	private Vector3 badgeDefaultPos;

	private Quaternion badgeDefaultRot;

	private GameObject[] badgeAnchors = new GameObject[4];

	private GameObject[] nameAnchors = new GameObject[6];

	private CosmeticAnchorAntiClipEntry[] badgeOffsets = new CosmeticAnchorAntiClipEntry[6];

	private CosmeticAnchorAntiClipEntry[] nameOffsets = new CosmeticAnchorAntiClipEntry[7];

	[SerializeField]
	public Transform friendshipBraceletLeftDefaultAnchor;

	public Transform friendshipBraceletLeftAnchor;

	[SerializeField]
	public Transform friendshipBraceletRightDefaultAnchor;

	public Transform friendshipBraceletRightAnchor;
}
