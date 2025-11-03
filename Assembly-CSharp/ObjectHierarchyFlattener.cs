using System;
using GorillaTag;
using UnityEngine;

[DefaultExecutionOrder(2001)]
public class ObjectHierarchyFlattener : MonoBehaviour
{
	private void ResetTransform()
	{
		if (this.originalParentGO.activeInHierarchy)
		{
			return;
		}
		base.transform.SetParent(this.originalParentTransform);
		this.isAttachedToOverride = false;
		base.transform.localPosition = this.originalLocalPosition;
		base.transform.localRotation = this.originalLocalRotation;
		base.transform.localScale = this.originalScale;
	}

	public void CrumbDisabled()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (this.trackTransformOfParent)
		{
			ObjectHierarchyFlattenerManager.UnregisterOHF(this);
		}
		base.Invoke("ResetTransform", 0f);
	}

	public void InvokeLateUpdate()
	{
		if (this.maintainRelativeScale)
		{
			base.transform.localScale = Vector3.Scale(this.originalParentTransform.lossyScale, this.originalScale);
		}
		base.transform.rotation = this.originalParentTransform.rotation * this.originalLocalRotation;
		base.transform.position = this.originalParentTransform.position + base.transform.rotation * this.calcOffset * (this.originalParentTransform.lossyScale.x / this.originalParentScale) * this.originalParentScale;
	}

	private void OnEnable()
	{
		if (this.trackTransformOfParent)
		{
			ObjectHierarchyFlattenerManager.RegisterOHF(this);
		}
		if (!this.isAttachedToOverride)
		{
			this.originalParentTransform = base.transform.parent;
			this.originalParentGO = this.originalParentTransform.gameObject;
			this.originalLocalPosition = base.transform.localPosition;
			this.originalLocalRotation = base.transform.localRotation;
			this.originalParentScale = base.transform.parent.lossyScale.x;
			this.originalScale = base.transform.localScale;
			this.calcOffset = Vector3.Scale(this.originalLocalPosition, this.originalScale);
			FlattenerCrumb flattenerCrumb = this.originalParentGO.GetComponent<FlattenerCrumb>();
			if (flattenerCrumb == null)
			{
				flattenerCrumb = this.originalParentGO.AddComponent<FlattenerCrumb>();
			}
			flattenerCrumb.AddFlattenerReference(this);
		}
		base.transform.SetParent((this.overrideParentTransform != null) ? this.overrideParentTransform : null);
		this.isAttachedToOverride = true;
	}

	private void OnDisable()
	{
		ObjectHierarchyFlattenerManager.UnregisterOHF(this);
		base.Invoke("ResetTransformIfStillDisabled", 0f);
	}

	private void ResetTransformIfStillDisabled()
	{
		if (!base.isActiveAndEnabled)
		{
			this.ResetTransform();
		}
	}

	public const int k_monoDefaultExecutionOrder = 2001;

	[DebugReadout]
	private GameObject originalParentGO;

	private Transform originalParentTransform;

	private Vector3 originalLocalPosition;

	private Vector3 calcOffset;

	private Quaternion originalLocalRotation;

	private Vector3 originalScale;

	private float originalParentScale;

	public bool trackTransformOfParent;

	public bool maintainRelativeScale;

	private FlattenerCrumb crumb;

	public Transform overrideParentTransform;

	private bool isAttachedToOverride;
}
