using System;
using UnityEngine;

public class LerpScale : LerpComponent
{
	protected override void OnLerp(float t)
	{
		this.current = Vector3.Lerp(this.start, this.end, this.scaleCurve.Evaluate(t));
		if (this.target)
		{
			this.target.localScale = this.current;
		}
	}

	[Space]
	public Transform target;

	[Space]
	public Vector3 start = Vector3.one;

	public Vector3 end = Vector3.one;

	public Vector3 current;

	[SerializeField]
	private AnimationCurve scaleCurve = AnimationCurves.EaseInOutBounce;
}
