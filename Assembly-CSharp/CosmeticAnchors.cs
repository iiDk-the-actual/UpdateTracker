using System;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class CosmeticAnchors : MonoBehaviour, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnSpawn(VRRig rig)
	{
	}

	void ISpawnable.OnDespawn()
	{
	}

	private void AssignAnchorToPath(ref GameObject anchorGObjRef, string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		Transform transform;
		if (!base.transform.TryFindByPath(path, out transform, false))
		{
			this.vrRig = base.GetComponentInParent<VRRig>(true);
			if (this.vrRig && this.vrRig.isOfflineVRRig)
			{
				Debug.LogError("CosmeticAnchors: Could not find path: \"" + path + "\".\nPath to this component: " + base.transform.GetPathQ(), this);
			}
			return;
		}
		anchorGObjRef = transform.gameObject;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void TryUpdate()
	{
	}

	public void EnableAnchor(bool enable)
	{
	}

	private void SetHuntComputerAnchor(bool enable)
	{
		Transform huntComputer = this.anchorOverrides.HuntComputer;
		if (!GorillaTagger.Instance.offlineVRRig.huntComputer.activeSelf || !enable)
		{
			huntComputer.parent = this.anchorOverrides.HuntDefaultAnchor;
		}
		else
		{
			huntComputer.parent = this.huntComputerAnchor.transform;
		}
		huntComputer.transform.localPosition = Vector3.zero;
		huntComputer.transform.localRotation = Quaternion.identity;
	}

	private void SetBuilderWatchAnchor(bool enable)
	{
		Transform builderWatch = this.anchorOverrides.BuilderWatch;
		if (!GorillaTagger.Instance.offlineVRRig.builderResizeWatch.activeSelf || !enable)
		{
			builderWatch.parent = this.anchorOverrides.BuilderWatchAnchor;
		}
		else
		{
			builderWatch.parent = this.builderWatchAnchor.transform;
		}
		builderWatch.transform.localPosition = Vector3.zero;
		builderWatch.transform.localRotation = Quaternion.identity;
	}

	private void SetCustomAnchor(Transform target, bool enable, GameObject overrideAnchor, Transform defaultAnchor)
	{
		Transform transform = ((enable && overrideAnchor != null) ? overrideAnchor.transform : defaultAnchor);
		if (target != null && target.parent != transform)
		{
			target.parent = transform;
			target.transform.localPosition = Vector3.zero;
			target.transform.localRotation = Quaternion.identity;
			target.transform.localScale = Vector3.one;
		}
	}

	public Transform GetPositionAnchor(TransferrableObject.PositionState pos)
	{
		if (pos != TransferrableObject.PositionState.OnLeftArm)
		{
			if (pos != TransferrableObject.PositionState.OnRightArm)
			{
				if (pos != TransferrableObject.PositionState.OnChest)
				{
					return null;
				}
				if (!this.chestAnchor)
				{
					return null;
				}
				return this.chestAnchor.transform;
			}
			else
			{
				if (!this.rightArmAnchor)
				{
					return null;
				}
				return this.rightArmAnchor.transform;
			}
		}
		else
		{
			if (!this.leftArmAnchor)
			{
				return null;
			}
			return this.leftArmAnchor.transform;
		}
	}

	public Transform GetNameAnchor()
	{
		if (!this.nameAnchor)
		{
			return null;
		}
		return this.nameAnchor.transform;
	}

	public bool AffectedByHunt()
	{
		return this.huntComputerAnchor != null;
	}

	public bool AffectedByBuilder()
	{
		return this.builderWatchAnchor != null;
	}

	[SerializeField]
	private bool deprecatedWarning = true;

	[SerializeField]
	protected GameObject nameAnchor;

	[SerializeField]
	protected string nameAnchor_path;

	[SerializeField]
	protected GameObject leftArmAnchor;

	[SerializeField]
	protected string leftArmAnchor_path;

	[SerializeField]
	protected GameObject rightArmAnchor;

	[SerializeField]
	protected string rightArmAnchor_path;

	[SerializeField]
	protected GameObject chestAnchor;

	[SerializeField]
	protected string chestAnchor_path;

	[SerializeField]
	protected GameObject huntComputerAnchor;

	[SerializeField]
	protected string huntComputerAnchor_path;

	[SerializeField]
	protected GameObject builderWatchAnchor;

	[SerializeField]
	protected string builderWatchAnchor_path;

	[SerializeField]
	protected GameObject friendshipBraceletLeftOverride;

	[SerializeField]
	protected string friendshipBraceletLeftOverride_path;

	[SerializeField]
	protected GameObject friendshipBraceletRightOverride;

	[SerializeField]
	protected string friendshipBraceletRightOverride_path;

	[SerializeField]
	protected GameObject badgeAnchor;

	[SerializeField]
	protected string badgeAnchor_path;

	[SerializeField]
	public CosmeticsController.CosmeticSlots slot;

	private VRRig vrRig;

	private VRRigAnchorOverrides anchorOverrides;

	private bool anchorEnabled;

	private static GTLogErrorLimiter k_debugLogError_anchorOverridesNull = new GTLogErrorLimiter("The array `anchorOverrides` was null. Is the cosmetic getting initialized properly? ", 10, "\n- ");
}
