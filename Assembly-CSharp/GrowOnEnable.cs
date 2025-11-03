using System;
using UnityEngine;

public class GrowOnEnable : MonoBehaviour, ITickSystemTick
{
	private void Awake()
	{
		this._targetScale = base.transform.localScale;
	}

	private void OnEnable()
	{
		this._lerpVal = 0f;
		this._curve = AnimationCurves.GetCurveForEase(this.easeType);
		this.UpdateScale();
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		base.transform.localScale = this._targetScale;
		TickSystem<object>.RemoveTickCallback(this);
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		this._lerpVal = Mathf.Clamp01(this._lerpVal + Time.deltaTime / this.growDuration);
		this.UpdateScale();
		if (this._lerpVal >= 1f)
		{
			TickSystem<object>.RemoveTickCallback(this);
		}
	}

	private void UpdateScale()
	{
		base.transform.localScale = this._targetScale * this._curve.Evaluate(this._lerpVal);
	}

	[SerializeField]
	private float growDuration = 1f;

	[SerializeField]
	private AnimationCurves.EaseType easeType = AnimationCurves.EaseType.EaseOutBack;

	private AnimationCurve _curve;

	private Vector3 _targetScale;

	private float _lerpVal;
}
