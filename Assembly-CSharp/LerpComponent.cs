using System;
using System.Diagnostics;
using UnityEngine;

public abstract class LerpComponent : MonoBehaviour
{
	public float Lerp
	{
		get
		{
			return this._lerp;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (!Mathf.Approximately(this._lerp, num))
			{
				LerpChangedEvent onLerpChanged = this._onLerpChanged;
				if (onLerpChanged != null)
				{
					onLerpChanged.Invoke(num);
				}
			}
			this._lerp = num;
		}
	}

	public float LerpTime
	{
		get
		{
			return this._lerpLength;
		}
		set
		{
			this._lerpLength = ((value < 0f) ? 0f : value);
		}
	}

	protected virtual bool CanRender
	{
		get
		{
			return true;
		}
	}

	protected abstract void OnLerp(float t);

	protected void RenderLerp()
	{
		this.OnLerp(this._lerp);
	}

	protected virtual int GetState()
	{
		return new ValueTuple<float, int>(this._lerp, 779562875).GetHashCode();
	}

	protected virtual void Validate()
	{
		if (this._lerpLength < 0f)
		{
			this._lerpLength = 0f;
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void OnDrawGizmosSelected()
	{
	}

	[Conditional("UNITY_EDITOR")]
	private void TryEditorRender(bool playModeCheck = true)
	{
	}

	[Conditional("UNITY_EDITOR")]
	private void LerpToOne()
	{
	}

	[Conditional("UNITY_EDITOR")]
	private void LerpToZero()
	{
	}

	[Conditional("UNITY_EDITOR")]
	private void StartPreview(float lerpFrom, float lerpTo)
	{
	}

	[SerializeField]
	[Range(0f, 1f)]
	protected float _lerp;

	[SerializeField]
	protected float _lerpLength = 1f;

	[Space]
	[SerializeField]
	protected LerpChangedEvent _onLerpChanged;

	[SerializeField]
	protected bool _previewInEditor = true;

	[NonSerialized]
	private bool _previewing;

	[NonSerialized]
	private bool _cancelPreview;

	[NonSerialized]
	private bool _rendering;

	[NonSerialized]
	private int _lastState;

	[NonSerialized]
	private float _prevLerpFrom;

	[NonSerialized]
	private float _prevLerpTo;
}
