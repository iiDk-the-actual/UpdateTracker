using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;

public class TriggerOnSpeed : MonoBehaviour, ITickSystemTick
{
	private void OnEnable()
	{
		TickSystem<object>.AddCallbackTarget(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveCallbackTarget(this);
	}

	public void Tick()
	{
		bool flag = this.velocityEstimator.linearVelocity.IsLongerThan(this.speedThreshold);
		if (flag != this.wasFaster)
		{
			if (flag)
			{
				this.onFaster.Invoke();
			}
			else
			{
				this.onSlower.Invoke();
			}
			this.wasFaster = flag;
		}
	}

	public bool TickRunning { get; set; }

	[SerializeField]
	private float speedThreshold;

	[SerializeField]
	private UnityEvent onFaster;

	[SerializeField]
	private UnityEvent onSlower;

	[SerializeField]
	private GorillaVelocityEstimator velocityEstimator;

	private bool wasFaster;
}
