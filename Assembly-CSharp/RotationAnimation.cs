using System;
using UnityEngine;

public class RotationAnimation : MonoBehaviour, ITickSystemTick
{
	public bool TickRunning { get; set; }

	public void Tick()
	{
		Vector3 vector = Vector3.zero;
		vector.x = this.amplitude.x * this.x.Evaluate((Time.time - this.baseTime) * this.period.x % 1f);
		vector.y = this.amplitude.y * this.y.Evaluate((Time.time - this.baseTime) * this.period.y % 1f);
		vector.z = this.amplitude.z * this.z.Evaluate((Time.time - this.baseTime) * this.period.z % 1f);
		if (this.releaseSet)
		{
			float num = this.release.Evaluate(Time.time - this.releaseTime);
			vector *= num;
			if (num < Mathf.Epsilon)
			{
				base.enabled = false;
			}
		}
		base.transform.localRotation = Quaternion.Euler(vector) * this.baseRotation;
	}

	private void Awake()
	{
		this.baseRotation = base.transform.localRotation;
	}

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
		this.releaseSet = false;
		this.baseTime = Time.time;
	}

	public void ReleaseToDisable()
	{
		this.releaseSet = true;
		this.releaseTime = Time.time;
	}

	public void CancelRelease()
	{
		this.releaseSet = false;
	}

	private void OnDisable()
	{
		base.transform.localRotation = this.baseRotation;
		TickSystem<object>.RemoveTickCallback(this);
	}

	[SerializeField]
	private AnimationCurve x;

	[SerializeField]
	private AnimationCurve y;

	[SerializeField]
	private AnimationCurve z;

	[SerializeField]
	private AnimationCurve attack;

	[SerializeField]
	private AnimationCurve release;

	[SerializeField]
	private Vector3 amplitude = Vector3.one;

	[SerializeField]
	private Vector3 period = Vector3.one;

	private Quaternion baseRotation;

	private float baseTime;

	private float releaseTime;

	private bool releaseSet;
}
