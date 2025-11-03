using System;
using UnityEngine;
using UnityEngine.Events;

public class GenericTriggerReactor : MonoBehaviour, IBuildValidation
{
	bool IBuildValidation.BuildValidationCheck()
	{
		if (this.ComponentName.Length == 0)
		{
			return true;
		}
		if (Type.GetType(this.ComponentName) == null)
		{
			Debug.LogError("GenericTriggerReactor :: ComponentName must specify a valid Component or be empty.");
			return false;
		}
		return true;
	}

	private void Awake()
	{
		this.componentType = Type.GetType(this.ComponentName);
		base.TryGetComponent<GorillaVelocityEstimator>(out this.gorillaVelocityEstimator);
	}

	private void OnTriggerEnter(Collider other)
	{
		this.OnTriggerTest(other, this.speedRangeEnter, this.GTOnTriggerEnter, this.idealMotionPlayRangeEnter);
	}

	private void OnTriggerExit(Collider other)
	{
		this.OnTriggerTest(other, this.speedRangeExit, this.GTOnTriggerExit, this.idealMotionPlayRangeExit);
	}

	private void OnTriggerTest(Collider other, Vector2 speedRange, UnityEvent unityEvent, Vector2 idealMotionPlay)
	{
		Component component;
		if (unityEvent != null && (this.componentType == null || other.TryGetComponent(this.componentType, out component)))
		{
			if (this.gorillaVelocityEstimator != null)
			{
				float magnitude = this.gorillaVelocityEstimator.linearVelocity.magnitude;
				if (magnitude < speedRange.x || magnitude > speedRange.y)
				{
					return;
				}
				if (this.idealMotion != null)
				{
					float num = Vector3.Dot(this.gorillaVelocityEstimator.linearVelocity.normalized, this.idealMotion.forward);
					if (num < idealMotionPlay.x || num > idealMotionPlay.y)
					{
						return;
					}
				}
			}
			unityEvent.Invoke();
		}
	}

	[SerializeField]
	private string ComponentName = string.Empty;

	[Space]
	[SerializeField]
	private Vector2 speedRangeEnter;

	[SerializeField]
	private Vector2 speedRangeExit;

	[Space]
	[SerializeField]
	private Transform idealMotion;

	[SerializeField]
	private Vector2 idealMotionPlayRangeEnter;

	[SerializeField]
	private Vector2 idealMotionPlayRangeExit;

	[Space]
	[SerializeField]
	private UnityEvent GTOnTriggerEnter;

	[SerializeField]
	private UnityEvent GTOnTriggerExit;

	private Type componentType;

	private GorillaVelocityEstimator gorillaVelocityEstimator;
}
