using System;
using UnityEngine;

public class CloudUmbrellaCloud : MonoBehaviour
{
	protected void Awake()
	{
		this.umbrellaXform = this.umbrella.transform;
		this.cloudScaleXform = this.cloudRenderer.transform;
	}

	protected void LateUpdate()
	{
		float num = Vector3.Dot(this.umbrellaXform.up, Vector3.up);
		float num2 = Mathf.Clamp01(this.scaleCurve.Evaluate(num));
		this.rendererOn = ((num2 > 0.09f && num2 < 0.1f) ? this.rendererOn : (num2 > 0.1f));
		this.cloudRenderer.enabled = this.rendererOn;
		this.cloudScaleXform.localScale = new Vector3(num2, num2, num2);
		this.cloudRotateXform.up = Vector3.up;
	}

	public UmbrellaItem umbrella;

	public Transform cloudRotateXform;

	public Renderer cloudRenderer;

	public AnimationCurve scaleCurve;

	private const float kHideAtScale = 0.1f;

	private const float kHideAtScaleTolerance = 0.01f;

	private bool rendererOn;

	private Transform umbrellaXform;

	private Transform cloudScaleXform;
}
