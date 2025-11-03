using System;
using UnityEngine;

public class WingsWearable : MonoBehaviour, IGorillaSliceableSimple
{
	private void Awake()
	{
		if (this.animator == null)
		{
			GTDev.LogError<string>("WingsWearable on " + base.gameObject.name + " missing animator", null);
			return;
		}
		this.xform = this.animator.transform;
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		this.oldPos = this.xform.localPosition;
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		Vector3 position = this.xform.position;
		float num = (position - this.oldPos).magnitude / Time.deltaTime;
		float num2 = this.flapSpeedCurve.Evaluate(Mathf.Abs(num));
		this.animator.SetFloat(this.flapSpeedParamID, num2);
		this.oldPos = position;
	}

	[Tooltip("This animator must have a parameter called 'FlapSpeed'")]
	public Animator animator;

	[Tooltip("X axis is move speed, Y axis is flap speed")]
	public AnimationCurve flapSpeedCurve;

	private Transform xform;

	private Vector3 oldPos;

	private readonly int flapSpeedParamID = Animator.StringToHash("FlapSpeed");
}
