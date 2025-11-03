using System;
using UnityEngine;

public class BuilderPortraitEyes : MonoBehaviour, IGorillaSliceableSimple
{
	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		this.scale = base.transform.lossyScale.x;
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		this.eyes.transform.position = this.eyeCenter.transform.position;
	}

	public void SliceUpdate()
	{
		if (GorillaTagger.Instance == null)
		{
			return;
		}
		Vector3 vector = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(GorillaTagger.Instance.headCollider.transform.position - this.eyeCenter.position, this.eyeCenter.forward), this.moveRadius * this.scale);
		this.eyes.transform.position = this.eyeCenter.position + vector;
	}

	[SerializeField]
	private Transform eyeCenter;

	[SerializeField]
	private GameObject eyes;

	[SerializeField]
	private float moveRadius = 0.5f;

	private float scale = 1f;
}
