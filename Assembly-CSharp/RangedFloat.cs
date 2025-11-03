using System;
using UnityEngine;

public class RangedFloat : MonoBehaviour, IRangedVariable<float>, IVariable<float>, IVariable
{
	public AnimationCurve Curve
	{
		get
		{
			return this._curve;
		}
	}

	public float Range
	{
		get
		{
			return this._max - this._min;
		}
	}

	public float Min
	{
		get
		{
			return this._min;
		}
		set
		{
			this._min = value;
		}
	}

	public float Max
	{
		get
		{
			return this._max;
		}
		set
		{
			this._max = value;
		}
	}

	public float normalized
	{
		get
		{
			if (!this.Range.Approx0(1E-06f))
			{
				return (this._value - this._min) / (this._max - this.Min);
			}
			return 0f;
		}
		set
		{
			this._value = this._min + Mathf.Clamp01(value) * (this._max - this._min);
		}
	}

	public float curved
	{
		get
		{
			return this._min + this._curve.Evaluate(this.normalized) * (this._max - this._min);
		}
	}

	public float Get()
	{
		return this._value;
	}

	public void Set(float f)
	{
		this._value = Mathf.Clamp(f, this._min, this._max);
	}

	[SerializeField]
	private float _value = 0.5f;

	[SerializeField]
	private float _min;

	[SerializeField]
	private float _max = 1f;

	[SerializeField]
	private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
