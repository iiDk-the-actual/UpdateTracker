using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SpinParametricAnimation : MonoBehaviour
{
	protected void OnEnable()
	{
		this.axis = this.axis.normalized;
	}

	protected void LateUpdate()
	{
		Transform transform = base.transform;
		this._animationProgress = (this._animationProgress + Time.deltaTime * this.revolutionsPerSecond) % 1f;
		float num = this.timeCurve.Evaluate(this._animationProgress) * 360f;
		float num2 = num - this._oldAngle;
		this._oldAngle = num;
		if (this.WorldSpaceRotation)
		{
			transform.rotation = Quaternion.AngleAxis(num2, this.axis) * transform.rotation;
			return;
		}
		transform.localRotation = Quaternion.AngleAxis(num2, this.axis) * transform.localRotation;
	}

	[Tooltip("Axis to rotate around.")]
	public Vector3 axis = Vector3.up;

	[Tooltip("Whether rotation is in World Space or Local Space")]
	public bool WorldSpaceRotation = true;

	[FormerlySerializedAs("speed")]
	[Tooltip("Speed of rotation.")]
	public float revolutionsPerSecond = 0.25f;

	[Tooltip("Affects the progress of the animation over time.")]
	public AnimationCurve timeCurve;

	private float _animationProgress;

	private float _oldAngle;
}
