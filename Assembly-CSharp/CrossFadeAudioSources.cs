using System;
using UnityEngine;

public class CrossFadeAudioSources : MonoBehaviour, IRangedVariable<float>, IVariable<float>, IVariable
{
	public void Play()
	{
		if (this.source1)
		{
			this.source1.Play();
		}
		if (this.source2)
		{
			this.source2.Play();
		}
	}

	public void Stop()
	{
		if (this.source1)
		{
			this.source1.Stop();
		}
		if (this.source2)
		{
			this.source2.Stop();
		}
	}

	private void Update()
	{
		if (!this.source1 || !this.source2)
		{
			return;
		}
		float num = this._curve.Evaluate(this._lerp);
		float num2;
		if (this.tween)
		{
			num2 = MathUtils.Xlerp(this._lastT, num, Time.deltaTime, this.tweenSpeed);
		}
		else
		{
			num2 = (this.lerpByClipLength ? this._curve.Evaluate((float)this.source1.timeSamples / (float)this.source1.clip.samples) : num);
		}
		this._lastT = num2;
		this.source2.volume = num2;
		this.source1.volume = 1f - num2;
	}

	public float Get()
	{
		return this._lerp;
	}

	public void Set(float f)
	{
		this._lerp = Mathf.Clamp01(f);
	}

	public float Min
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float Max
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public float Range
	{
		get
		{
			return 1f;
		}
	}

	public AnimationCurve Curve
	{
		get
		{
			return this._curve;
		}
	}

	[SerializeField]
	private float _lerp;

	[SerializeField]
	private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Space]
	[SerializeField]
	private AudioSource source1;

	[SerializeField]
	private AudioSource source2;

	[Space]
	public bool lerpByClipLength;

	public bool tween;

	public float tweenSpeed = 16f;

	private float _lastT;
}
