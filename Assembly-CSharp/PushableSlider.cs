using System;
using UnityEngine;

public class PushableSlider : MonoBehaviour
{
	public void Awake()
	{
		this.Initialize();
	}

	private void Initialize()
	{
		if (this._initialized)
		{
			return;
		}
		this._initialized = true;
		this._localSpace = base.transform.worldToLocalMatrix;
		this._startingPos = base.transform.localPosition;
	}

	private void OnTriggerStay(Collider other)
	{
		if (!base.enabled)
		{
			return;
		}
		GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent == null)
		{
			return;
		}
		Vector3 vector = this._localSpace.MultiplyPoint3x4(other.transform.position);
		Vector3 vector2 = base.transform.localPosition - this._startingPos - vector;
		float num = Mathf.Abs(vector2.x);
		if (num < this.farPushDist)
		{
			Vector3 currentVelocity = componentInParent.currentVelocity;
			if (Mathf.Sign(vector2.x) != Mathf.Sign((this._localSpace.rotation * currentVelocity).x))
			{
				return;
			}
			vector2.x = Mathf.Sign(vector2.x) * (this.farPushDist - num);
			vector2.y = 0f;
			vector2.z = 0f;
			Vector3 vector3 = base.transform.localPosition - this._startingPos + vector2;
			vector3.x = Mathf.Clamp(vector3.x, this.minXOffset, this.maxXOffset);
			base.transform.localPosition = this.GetXOffsetVector(vector3.x + this._startingPos.x);
			GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		}
	}

	private Vector3 GetXOffsetVector(float x)
	{
		return new Vector3(x, this._startingPos.y, this._startingPos.z);
	}

	public void SetProgress(float value)
	{
		this.Initialize();
		value = Mathf.Clamp(value, 0f, 1f);
		float num = Mathf.Lerp(this.minXOffset, this.maxXOffset, value);
		base.transform.localPosition = this.GetXOffsetVector(this._startingPos.x + num);
		this._previousLocalPosition = new Vector3(num, 0f, 0f);
		this._cachedProgress = value;
	}

	public float GetProgress()
	{
		this.Initialize();
		Vector3 vector = base.transform.localPosition - this._startingPos;
		if (vector == this._previousLocalPosition)
		{
			return this._cachedProgress;
		}
		this._previousLocalPosition = vector;
		this._cachedProgress = (vector.x - this.minXOffset) / (this.maxXOffset - this.minXOffset);
		return this._cachedProgress;
	}

	[SerializeField]
	private float farPushDist = 0.015f;

	[SerializeField]
	private float maxXOffset;

	[SerializeField]
	private float minXOffset;

	private Matrix4x4 _localSpace;

	private Vector3 _startingPos;

	private Vector3 _previousLocalPosition;

	private float _cachedProgress;

	private bool _initialized;
}
