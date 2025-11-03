using System;
using GorillaLocomotion;
using UnityEngine;

public class GorillaVelocityEstimator : MonoBehaviour
{
	public Vector3 linearVelocity { get; private set; }

	public Vector3 angularVelocity { get; private set; }

	public Vector3 handPos { get; private set; }

	private void Awake()
	{
		this.history = new GorillaVelocityEstimator.VelocityHistorySample[this.numFrames];
	}

	private void OnEnable()
	{
		this.currentFrame = 0;
		for (int i = 0; i < this.history.Length; i++)
		{
			this.history[i] = default(GorillaVelocityEstimator.VelocityHistorySample);
		}
		this.lastPos = base.transform.position;
		this.lastRotation = base.transform.rotation;
		GorillaVelocityEstimatorManager.Register(this);
	}

	private void OnDisable()
	{
		GorillaVelocityEstimatorManager.Unregister(this);
	}

	private void OnDestroy()
	{
		GorillaVelocityEstimatorManager.Unregister(this);
	}

	public void TriggeredLateUpdate()
	{
		Vector3 vector;
		Quaternion quaternion;
		base.transform.GetPositionAndRotation(out vector, out quaternion);
		Vector3 vector2 = Vector3.zero;
		if (!this.useGlobalSpace)
		{
			vector2 = GTPlayer.Instance.InstantaneousVelocity;
		}
		Vector3 vector3 = (vector - this.lastPos) / Time.deltaTime - vector2;
		Vector3 vector4 = (quaternion * Quaternion.Inverse(this.lastRotation)).eulerAngles;
		if (vector4.x > 180f)
		{
			vector4.x -= 360f;
		}
		if (vector4.y > 180f)
		{
			vector4.y -= 360f;
		}
		if (vector4.z > 180f)
		{
			vector4.z -= 360f;
		}
		vector4 *= 0.017453292f / Time.fixedDeltaTime;
		this.linearVelocity += (vector3 - this.history[this.currentFrame].linear) / (float)this.numFrames;
		this.angularVelocity += (vector4 - this.history[this.currentFrame].angular) / (float)this.numFrames;
		this.history[this.currentFrame] = new GorillaVelocityEstimator.VelocityHistorySample
		{
			linear = vector3,
			angular = vector4
		};
		this.handPos = vector;
		this.currentFrame = (this.currentFrame + 1) % this.numFrames;
		this.lastPos = vector;
		this.lastRotation = quaternion;
	}

	[Min(1f)]
	[SerializeField]
	private int numFrames = 8;

	private GorillaVelocityEstimator.VelocityHistorySample[] history;

	private int currentFrame;

	private Vector3 lastPos;

	private Quaternion lastRotation;

	private Vector3 lastRotationVec;

	public bool useGlobalSpace;

	public struct VelocityHistorySample
	{
		public Vector3 linear;

		public Vector3 angular;
	}
}
