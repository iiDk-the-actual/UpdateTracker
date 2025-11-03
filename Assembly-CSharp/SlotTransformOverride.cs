using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

[Serializable]
public class SlotTransformOverride
{
	private XformOffset _EdXformOffsetRepresenationOf_overrideTransformMatrix
	{
		get
		{
			return new XformOffset(this.overrideTransformMatrix);
		}
		set
		{
			this.overrideTransformMatrix = Matrix4x4.TRS(value.pos, value.rot, value.scale);
		}
	}

	public void Initialize(Component component, Transform anchor)
	{
		if (!this.useAdvancedGrab)
		{
			return;
		}
		this.AdvOriginLocalToParentAnchorLocal = anchor.worldToLocalMatrix * this.advancedGrabPointOrigin.localToWorldMatrix;
		this.AdvAnchorLocalToAdvOriginLocal = this.advancedGrabPointOrigin.worldToLocalMatrix * this.advancedGrabPointAnchor.localToWorldMatrix;
		foreach (SubGrabPoint subGrabPoint in this.multiPoints)
		{
			if (subGrabPoint == null)
			{
				break;
			}
			subGrabPoint.InitializePoints(anchor, this.advancedGrabPointAnchor, this.advancedGrabPointOrigin);
		}
	}

	public void AddLineButton()
	{
		this.multiPoints.Add(new SubLineGrabPoint());
	}

	public void AddSubGrabPoint(TransferrableObjectGripPosition togp)
	{
		SubGrabPoint subGrabPoint = togp.CreateSubGrabPoint(this);
		this.multiPoints.Add(subGrabPoint);
	}

	[Obsolete("(2024-08-20 MattO) Cosmetics use xformOffsets now which fills in the appropriate data for this component. If you are doing something weird then `overrideTransformMatrix` must be used instead. This will probably be removed after 2024-09-15.")]
	public Transform overrideTransform;

	[Obsolete("(2024-08-20 MattO) Cosmetics use xformOffsets now which fills in the appropriate data for this component. If you are doing something weird then `overrideTransformMatrix` must be used instead. This will probably be removed after 2024-09-15.")]
	[Delayed]
	public string overrideTransform_path;

	public TransferrableObject.PositionState positionState;

	public bool useAdvancedGrab;

	public Matrix4x4 overrideTransformMatrix = Matrix4x4.identity;

	public Transform advancedGrabPointAnchor;

	public Transform advancedGrabPointOrigin;

	[SerializeReference]
	public List<SubGrabPoint> multiPoints = new List<SubGrabPoint>();

	public Matrix4x4 AdvOriginLocalToParentAnchorLocal;

	public Matrix4x4 AdvAnchorLocalToAdvOriginLocal;
}
