using System;
using UnityEngine;

public class TransferrableObjectGripPosition : MonoBehaviour
{
	private void Awake()
	{
		if (this.parentObject == null)
		{
			this.parentObject = base.transform.parent.GetComponent<TransferrableItemSlotTransformOverride>();
		}
		this.parentObject.AddGripPosition(this.attachmentType, this);
	}

	public SubGrabPoint CreateSubGrabPoint(SlotTransformOverride overrideContainer)
	{
		return new SubGrabPoint();
	}

	[SerializeField]
	private TransferrableItemSlotTransformOverride parentObject;

	[SerializeField]
	private TransferrableObject.PositionState attachmentType;
}
