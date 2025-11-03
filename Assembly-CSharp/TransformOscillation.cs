using System;
using GorillaNetworking;
using UnityEngine;

public class TransformOscillation : MonoBehaviour
{
	private void Awake()
	{
		if (this.useRigidbodyMotion && !this.targetRigidbody)
		{
			this.targetRigidbody = base.GetComponent<Rigidbody>();
		}
		this.lastRotOffs = Quaternion.identity;
		this.startTime = Time.time;
		this.isRunning = false;
	}

	private void OnEnable()
	{
		this.lastPosOffs = Vector3.zero;
		this.lastRotOffs = Quaternion.identity;
		if (this.startOnEnable)
		{
			this.StartOscillation();
			return;
		}
		this.isRunning = false;
	}

	public void StartOscillation()
	{
		this.startTime = Time.time;
		this.isRunning = true;
	}

	private float GetTimeSeconds()
	{
		if (!this.useServerTime)
		{
			return Time.timeSinceLevelLoad;
		}
		if (GorillaComputer.instance == null)
		{
			return Time.timeSinceLevelLoad;
		}
		this.dt = GorillaComputer.instance.GetServerTime();
		return (float)this.dt.Minute * 60f + (float)this.dt.Second + (float)this.dt.Millisecond / 1000f;
	}

	private void ComputeOffsets(float t)
	{
		this.offsPos.x = this.PosAmp.x * Mathf.Sin(t * this.PosFreq.x);
		this.offsPos.y = this.PosAmp.y * Mathf.Sin(t * this.PosFreq.y);
		this.offsPos.z = this.PosAmp.z * Mathf.Sin(t * this.PosFreq.z);
		this.offsRot.x = this.RotAmp.x * Mathf.Sin(t * this.RotFreq.x);
		this.offsRot.y = this.RotAmp.y * Mathf.Sin(t * this.RotFreq.y);
		this.offsRot.z = this.RotAmp.z * Mathf.Sin(t * this.RotFreq.z);
	}

	private void LateUpdate()
	{
		if (!this.isRunning)
		{
			return;
		}
		if (this.useTimeLimit && Time.time - this.startTime >= this.timer)
		{
			return;
		}
		if (this.useRigidbodyMotion && this.targetRigidbody)
		{
			return;
		}
		float timeSeconds = this.GetTimeSeconds();
		this.ComputeOffsets(timeSeconds);
		Transform transform = base.transform;
		Quaternion quaternion = Quaternion.Euler(this.offsRot);
		Vector3 vector = transform.localPosition - this.lastPosOffs;
		Quaternion quaternion2 = transform.localRotation * Quaternion.Inverse(this.lastRotOffs);
		transform.localPosition = vector + this.offsPos;
		transform.localRotation = quaternion2 * quaternion;
		this.lastPosOffs = this.offsPos;
		this.lastRotOffs = quaternion;
	}

	private void FixedUpdate()
	{
		if (!this.isRunning)
		{
			return;
		}
		if (this.useTimeLimit && Time.time - this.startTime >= this.timer)
		{
			return;
		}
		if (!this.useRigidbodyMotion || !this.targetRigidbody)
		{
			return;
		}
		float timeSeconds = this.GetTimeSeconds();
		this.ComputeOffsets(timeSeconds);
		Transform transform = base.transform;
		Quaternion quaternion = Quaternion.Euler(this.offsRot);
		Transform parent = transform.parent;
		Vector3 vector = (parent ? parent.TransformVector(this.lastPosOffs) : this.lastPosOffs);
		Quaternion quaternion2 = (parent ? (parent.rotation * this.lastRotOffs * Quaternion.Inverse(parent.rotation)) : this.lastRotOffs);
		Vector3 vector2 = transform.position - vector;
		Quaternion quaternion3 = transform.rotation * Quaternion.Inverse(quaternion2);
		Vector3 vector3 = (parent ? parent.TransformVector(this.offsPos) : this.offsPos);
		Quaternion quaternion4 = (parent ? (parent.rotation * quaternion * Quaternion.Inverse(parent.rotation)) : quaternion);
		this.targetRigidbody.MovePosition(vector2 + vector3);
		this.targetRigidbody.MoveRotation(quaternion3 * quaternion4);
		this.lastPosOffs = this.offsPos;
		this.lastRotOffs = quaternion;
	}

	[SerializeField]
	private Vector3 PosAmp;

	[SerializeField]
	private Vector3 PosFreq;

	[SerializeField]
	private Vector3 RotAmp;

	[SerializeField]
	private Vector3 RotFreq;

	[SerializeField]
	private bool useServerTime;

	[Header("Rigidbody Motion (optional)")]
	[Tooltip("If true and a Rigidbody is present, applies motion using Rigidbody.MovePosition/MoveRotation in FixedUpdate.")]
	[SerializeField]
	private bool useRigidbodyMotion;

	[SerializeField]
	private Rigidbody targetRigidbody;

	[Header("Activation Timer (optional)")]
	[Tooltip("If true, oscillation only runs for 'activeDurationSeconds' after OnEnable; otherwise it runs indefinitely.")]
	[SerializeField]
	private bool useTimeLimit;

	[SerializeField]
	private float timer = 2f;

	[Header("Start Behavior (optional)")]
	[Tooltip("If true, oscillation starts automatically on OnEnable(). If false, call StartOscillation() manually.")]
	[SerializeField]
	private bool startOnEnable = true;

	private Vector3 lastPosOffs = Vector3.zero;

	private Quaternion lastRotOffs = Quaternion.identity;

	private Vector3 offsPos;

	private Vector3 offsRot;

	private DateTime dt;

	private float startTime;

	private bool isRunning;
}
