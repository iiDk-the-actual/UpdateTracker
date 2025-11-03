using System;
using UnityEngine;
using UnityEngine.Events;

public class MaskCyclopsEye : MonoBehaviour
{
	private void OnEnable()
	{
		this.ScheduleNextBlink();
	}

	private void OnDisable()
	{
	}

	public void Update()
	{
		if (Time.time >= this.nextBlinkTime)
		{
			UnityEvent onBlink = this.OnBlink;
			if (onBlink != null)
			{
				onBlink.Invoke();
			}
			this.ScheduleNextBlink();
		}
	}

	public void Tick()
	{
		if (Time.time >= this.nextBlinkTime)
		{
			UnityEvent onBlink = this.OnBlink;
			if (onBlink != null)
			{
				onBlink.Invoke();
			}
			this.ScheduleNextBlink();
		}
	}

	private void ScheduleNextBlink()
	{
		float num = Random.Range(this.minWaitTime, this.maxWaitTime);
		this.nextBlinkTime = Time.time + num;
	}

	[Tooltip("Invoked when it's time to trigger a blink (e.g., play animation one-shot).")]
	public UnityEvent OnBlink;

	[Tooltip("Minimum time in seconds between blinks.")]
	[SerializeField]
	private float minWaitTime = 3f;

	[Tooltip("Maximum time in seconds between blinks.")]
	[SerializeField]
	private float maxWaitTime = 5f;

	private float nextBlinkTime;
}
