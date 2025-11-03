using System;
using System.Collections.Generic;
using System.Linq;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class TransferrableItemSlotTransformOverride : MonoBehaviour, IGorillaSliceableSimple, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this.defaultPosition = new SlotTransformOverride
		{
			positionState = TransferrableObject.PositionState.None
		};
		this.lastPosition = TransferrableObject.PositionState.None;
		foreach (SlotTransformOverride slotTransformOverride in this.transformOverridesDeprecated)
		{
			slotTransformOverride.Initialize(this, this.anchor);
		}
	}

	void ISpawnable.OnDespawn()
	{
	}

	public void AddGripPosition(TransferrableObject.PositionState state, TransferrableObjectGripPosition togp)
	{
		foreach (SlotTransformOverride slotTransformOverride in this.transformOverridesDeprecated)
		{
			if (slotTransformOverride.positionState == state)
			{
				slotTransformOverride.AddSubGrabPoint(togp);
				return;
			}
		}
		SlotTransformOverride slotTransformOverride2 = new SlotTransformOverride
		{
			positionState = state
		};
		this.transformOverridesDeprecated.Add(slotTransformOverride2);
		slotTransformOverride2.AddSubGrabPoint(togp);
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		if (this.followingTransferrableObject == null)
		{
			return;
		}
		if (this.followingTransferrableObject.currentState != this.lastPosition)
		{
			SlotTransformOverride slotTransformOverride = this.transformOverridesDeprecated.Find((SlotTransformOverride x) => (x.positionState & this.followingTransferrableObject.currentState) > TransferrableObject.PositionState.None);
			if (slotTransformOverride != null && slotTransformOverride.positionState == TransferrableObject.PositionState.None)
			{
				slotTransformOverride = this.defaultPosition;
			}
		}
		this.lastPosition = this.followingTransferrableObject.currentState;
	}

	private void Awake()
	{
		this.GenerateTransformFromPositionState();
	}

	public void GenerateTransformFromPositionState()
	{
		this.transformFromPosition = new Dictionary<TransferrableObject.PositionState, Transform>();
		if (this.transformOverridesDeprecated.Count > 0)
		{
			this.transformFromPosition[TransferrableObject.PositionState.None] = this.transformOverridesDeprecated[0].overrideTransform;
		}
		foreach (TransferrableObject.PositionState positionState in Enum.GetValues(typeof(TransferrableObject.PositionState)).Cast<TransferrableObject.PositionState>())
		{
			if (positionState == TransferrableObject.PositionState.None)
			{
				this.transformFromPosition[positionState] = null;
			}
			else
			{
				Transform transform = null;
				foreach (SlotTransformOverride slotTransformOverride in this.transformOverridesDeprecated)
				{
					if ((slotTransformOverride.positionState & positionState) != TransferrableObject.PositionState.None)
					{
						transform = slotTransformOverride.overrideTransform;
						break;
					}
				}
				this.transformFromPosition[positionState] = transform;
			}
		}
	}

	[CanBeNull]
	public Transform GetTransformFromPositionState(TransferrableObject.PositionState currentState)
	{
		if (this.transformFromPosition == null)
		{
			this.GenerateTransformFromPositionState();
		}
		return this.transformFromPosition[currentState];
	}

	public bool GetTransformFromPositionState(TransferrableObject.PositionState currentState, AdvancedItemState advancedItemState, Transform targetDockXf, out Matrix4x4 matrix4X4)
	{
		if (currentState != TransferrableObject.PositionState.None)
		{
			foreach (SlotTransformOverride slotTransformOverride in this.transformOverridesDeprecated)
			{
				if ((slotTransformOverride.positionState & currentState) != TransferrableObject.PositionState.None)
				{
					if (!slotTransformOverride.useAdvancedGrab)
					{
						matrix4X4 = slotTransformOverride.overrideTransformMatrix;
						return true;
					}
					if (advancedItemState.index >= slotTransformOverride.multiPoints.Count)
					{
						matrix4X4 = slotTransformOverride.overrideTransformMatrix;
						return true;
					}
					SubGrabPoint subGrabPoint = slotTransformOverride.multiPoints[advancedItemState.index];
					matrix4X4 = subGrabPoint.GetTransformFromPositionState(advancedItemState, slotTransformOverride, targetDockXf);
					return true;
				}
			}
			matrix4X4 = Matrix4x4.identity;
			return false;
		}
		if (this.transformOverridesDeprecated.Count > 0)
		{
			matrix4X4 = this.transformOverridesDeprecated[0].overrideTransformMatrix;
			return true;
		}
		matrix4X4 = Matrix4x4.identity;
		return false;
	}

	public AdvancedItemState GetAdvancedItemStateFromHand(TransferrableObject.PositionState currentState, Transform handTransform, Transform targetDock)
	{
		foreach (SlotTransformOverride slotTransformOverride in this.transformOverridesDeprecated)
		{
			if ((slotTransformOverride.positionState & currentState) != TransferrableObject.PositionState.None && slotTransformOverride.multiPoints.Count != 0)
			{
				SubGrabPoint subGrabPoint = slotTransformOverride.multiPoints[0];
				float num = float.PositiveInfinity;
				int num2 = -1;
				for (int i = 0; i < slotTransformOverride.multiPoints.Count; i++)
				{
					SubGrabPoint subGrabPoint2 = slotTransformOverride.multiPoints[i];
					if (!(subGrabPoint2.gripPoint == null))
					{
						float num3 = subGrabPoint2.EvaluateScore(base.transform, handTransform, targetDock);
						if (num3 < num)
						{
							subGrabPoint = subGrabPoint2;
							num = num3;
							num2 = i;
						}
					}
				}
				AdvancedItemState advancedItemStateFromHand = subGrabPoint.GetAdvancedItemStateFromHand(base.transform, handTransform, targetDock, slotTransformOverride);
				advancedItemStateFromHand.index = num2;
				return advancedItemStateFromHand;
			}
		}
		return new AdvancedItemState();
	}

	public void Edit()
	{
		if (TransferrableItemSlotTransformOverride.OnBringUpWindow != null)
		{
			TransferrableItemSlotTransformOverride.OnBringUpWindow(base.GetComponent<TransferrableObject>());
		}
	}

	[FormerlySerializedAs("transformOverridesList")]
	public List<SlotTransformOverride> transformOverridesDeprecated;

	[SerializeReference]
	public List<SlotTransformOverride> transformOverrides;

	private TransferrableObject.PositionState lastPosition;

	[Tooltip("(2024-08-20 MattO) For cosmetics this is almost always assigned to the TransferrableObject component in the same prefab and almost always belonging to the same gameobject as this Component.")]
	public TransferrableObject followingTransferrableObject;

	[Tooltip("(2024-08-20 MattO) This is filled in automatically by the cosmetic spawner.")]
	public SlotTransformOverride defaultPosition;

	[Obsolete("(2024-08-2024) This used to be assigned to `defaultPosition.overrideTransform` before, but was there ever an instance where it wasn't null? Keeping it serialized just in case there is a reason for it.")]
	public Transform defaultTransform;

	public Transform anchor;

	public Dictionary<TransferrableObject.PositionState, Transform> transformFromPosition;

	public static Action<TransferrableObject> OnBringUpWindow;
}
