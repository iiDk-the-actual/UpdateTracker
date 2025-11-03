using System;
using GorillaTag.Cosmetics;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	public class GorillaIntervalTimer : MonoBehaviourPun
	{
		private void Awake()
		{
			if (this.networkProvider == null)
			{
				this.networkProvider = base.GetComponentInParent<NetworkedRandomProvider>();
			}
			this.ResetElapsed();
			this.ResetRun();
		}

		private void OnEnable()
		{
			if (this.runOnEnable)
			{
				if (!this.isRegistered)
				{
					GorillaIntervalTimerManager.RegisterGorillaTimer(this);
					this.isRegistered = true;
				}
				this.StartTimer();
			}
		}

		private void OnDisable()
		{
			if (this.isRegistered)
			{
				GorillaIntervalTimerManager.UnregisterGorillaTimer(this);
				this.isRegistered = false;
			}
			this.StopTimer();
		}

		public void StartTimer()
		{
			if (!this.isRegistered)
			{
				GorillaIntervalTimerManager.RegisterGorillaTimer(this);
				this.isRegistered = true;
			}
			this.ResetRun();
			this.elapsed = 0f;
			this.isInPostFireDelay = false;
			if (this.useInitialDelay && this.initialDelay > 0f)
			{
				this.currentIntervalSeconds = Mathf.Max(0.001f, this.ToSeconds(this.initialDelay));
			}
			else
			{
				this.RollNextInterval();
			}
			this.isRunning = true;
			this.isPaused = false;
			UnityEvent unityEvent = this.onTimerStarted;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		public void StopTimer()
		{
			this.isRunning = false;
			this.isPaused = false;
			this.elapsed = 0f;
			this.isInPostFireDelay = false;
			UnityEvent unityEvent = this.onTimerStopped;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			if (this.isRegistered)
			{
				GorillaIntervalTimerManager.UnregisterGorillaTimer(this);
				this.isRegistered = false;
			}
		}

		public void Pause()
		{
			this.isPaused = true;
		}

		public void Resume()
		{
			this.isPaused = false;
		}

		public void SetFixedIntervalSeconds(float seconds)
		{
			this.useRandomDuration = false;
			this.fixedInterval = Mathf.Max(0f, seconds);
			this.currentIntervalSeconds = Mathf.Max(0.001f, this.ToSeconds(this.fixedInterval));
			this.elapsed = 0f;
		}

		public void OverrideNextIntervalSeconds(float seconds)
		{
			this.currentIntervalSeconds = Mathf.Max(0.001f, seconds);
			this.elapsed = 0f;
		}

		public void ResetRun()
		{
			this.runFiredSoFar = 0;
		}

		public void InvokeUpdate()
		{
			if (!this.isRunning || this.isPaused)
			{
				return;
			}
			this.elapsed += Time.deltaTime;
			if (this.elapsed >= this.currentIntervalSeconds)
			{
				if (this.isInPostFireDelay)
				{
					this.isInPostFireDelay = false;
					this.elapsed = 0f;
					this.RollNextInterval();
					return;
				}
				UnityEvent unityEvent = this.onIntervalFired;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
				this.runFiredSoFar++;
				if (this.runLength == GorillaIntervalTimer.RunLength.Finite && this.runFiredSoFar >= Mathf.Max(1, this.maxFiresPerRun))
				{
					if (this.requireManualReset)
					{
						this.StopTimer();
						return;
					}
					this.runFiredSoFar = 0;
				}
				if (this.usePostIntervalDelay && this.postIntervalDelay > 0f)
				{
					this.isInPostFireDelay = true;
					this.elapsed = 0f;
					this.currentIntervalSeconds = Mathf.Max(0.001f, this.ToSeconds(this.postIntervalDelay));
					return;
				}
				this.elapsed = 0f;
				this.RollNextInterval();
			}
		}

		private void ResetElapsed()
		{
			this.elapsed = 0f;
		}

		private void RollNextInterval()
		{
			if (!this.useRandomDuration)
			{
				this.currentIntervalSeconds = Mathf.Max(0.001f, this.ToSeconds(this.fixedInterval));
				return;
			}
			float num = Mathf.Max(0f, this.ToSeconds(this.randTimeMin));
			float num2 = Mathf.Max(num, this.ToSeconds(this.randTimeMax));
			float num3;
			if (this.intervalSource == GorillaIntervalTimer.IntervalSource.NetworkedRandom && this.networkProvider != null)
			{
				switch (this.distribution)
				{
				default:
					num3 = this.networkProvider.NextFloat(num, num2);
					break;
				case GorillaIntervalTimer.RandomDistribution.Normal:
				{
					double num4 = Math.Max(double.Epsilon, 1.0 - this.networkProvider.NextDouble(0.0, 1.0));
					double num5 = Math.Max(double.Epsilon, 1.0 - (double)this.networkProvider.NextFloat01());
					double num6 = Math.Sqrt(-2.0 * Math.Log(num4)) * Math.Sin(6.283185307179586 * num5);
					float num7 = 0.5f * (num + num2);
					float num8 = (num2 - num) / 6f;
					num3 = Mathf.Clamp(num7 + (float)(num6 * (double)num8), num, num2);
					break;
				}
				case GorillaIntervalTimer.RandomDistribution.Exponential:
				{
					double num9 = Math.Max(double.Epsilon, 1.0 - this.networkProvider.NextDouble(0.0, 1.0));
					double num10 = 0.5 * (double)(num + num2);
					double num11 = ((num10 > 0.0) ? (1.0 / num10) : 1.0);
					num3 = Mathf.Clamp((float)(-(float)Math.Log(num9) / num11), num, num2);
					break;
				}
				}
				this.currentIntervalSeconds = Mathf.Max(0.001f, num3);
				return;
			}
			switch (this.distribution)
			{
			default:
				num3 = Random.Range(num, num2);
				break;
			case GorillaIntervalTimer.RandomDistribution.Normal:
			{
				float num12 = Mathf.Max(float.Epsilon, 1f - Random.value);
				float num13 = 1f - Random.value;
				float num14 = Mathf.Sqrt(-2f * Mathf.Log(num12)) * Mathf.Sin(6.2831855f * num13);
				float num15 = 0.5f * (num + num2);
				float num16 = (num2 - num) / 6f;
				num3 = Mathf.Clamp(num15 + num14 * num16, num, num2);
				break;
			}
			case GorillaIntervalTimer.RandomDistribution.Exponential:
			{
				float num17 = 0.5f * (num + num2);
				float num18 = ((num17 > 0f) ? (1f / num17) : 1f);
				num3 = Mathf.Clamp(-Mathf.Log(Mathf.Max(float.Epsilon, 1f - Random.value)) / num18, num, num2);
				break;
			}
			}
			this.currentIntervalSeconds = Mathf.Max(0.001f, num3);
		}

		private float ToSeconds(float value)
		{
			switch (this.unit)
			{
			default:
				return value;
			case GorillaIntervalTimer.TimeUnit.Minutes:
				return value * 60f;
			case GorillaIntervalTimer.TimeUnit.Hours:
				return value * 3600f;
			}
		}

		public void RestartTimer()
		{
			this.ResetElapsed();
			this.RollNextInterval();
			this.StartTimer();
		}

		public float GetPassedTime()
		{
			return this.elapsed;
		}

		public float GetRemainingTime()
		{
			return Mathf.Max(0f, this.currentIntervalSeconds - this.elapsed);
		}

		[Header("Scheduling")]
		[Tooltip("If true, the timer will automatically start when this component is enabled.")]
		[SerializeField]
		private bool runOnEnable = true;

		[Tooltip("If true, apply an initial delay before the first interval is fired.")]
		[SerializeField]
		private bool useInitialDelay;

		[Tooltip("Delay (in seconds or minutes depending on Unit) before the first fire if 'Use Initial Delay' is enabled.")]
		[SerializeField]
		private float initialDelay;

		[Header("Interval")]
		[Tooltip("Unit of time for Fixed Interval, Min and Max values.")]
		[SerializeField]
		private GorillaIntervalTimer.TimeUnit unit;

		[Tooltip("Distribution type used for generating random intervals when Interval Source = LocalRandom.")]
		[SerializeField]
		private GorillaIntervalTimer.RandomDistribution distribution;

		[Tooltip("Fixed interval duration (interpreted by Unit) when Use Random Duration = false.")]
		[SerializeField]
		private float fixedInterval = 1f;

		[Space]
		[Tooltip("If false, 'Fixed Interval' is used. If true, a random interval is sampled each cycle.")]
		[SerializeField]
		private bool useRandomDuration;

		[Tooltip("Minimum interval time (in selected Unit).")]
		[SerializeField]
		private float randTimeMin = 0.5f;

		[Tooltip("Maximum interval time (in selected Unit).")]
		[SerializeField]
		private float randTimeMax = 2f;

		[Tooltip("Determines whether to use a local random generator or a networked random source.")]
		[SerializeField]
		private GorillaIntervalTimer.IntervalSource intervalSource;

		[Header("Networked Interval (optional)")]
		[Tooltip("If Interval Source = NetworkedRandom, the timer queries this component for the next interval")]
		[SerializeField]
		private NetworkedRandomProvider networkProvider;

		[Space]
		[Tooltip("If true, wait this additional delay after onIntervalFired() before starting the next interval.")]
		[SerializeField]
		private bool usePostIntervalDelay;

		[Tooltip("Additional delay (in selected Unit) to wait after onIntervalFired(), before the next interval begins.")]
		[SerializeField]
		private float postIntervalDelay;

		[Header("Run Length")]
		[Tooltip("Infinite runs forever. Finite stops after Max Fires Per Run.")]
		[SerializeField]
		private GorillaIntervalTimer.RunLength runLength;

		[Tooltip("Number of times the timer fires before the run completes (when Run Length = Finite).")]
		[SerializeField]
		private int maxFiresPerRun = 3;

		[Tooltip("If true, the timer stops at the end of a finite run and requires ResetRun() / StartTimer() to continue. If false, the run counter auto-resets and continues.")]
		[SerializeField]
		private bool requireManualReset = true;

		[Header("Events")]
		public UnityEvent onIntervalFired;

		public UnityEvent onTimerStarted;

		public UnityEvent onTimerStopped;

		private const float minIntervalEpsilon = 0.001f;

		private float currentIntervalSeconds = 1f;

		private float elapsed;

		private bool isRunning;

		private bool isPaused;

		private bool isRegistered;

		private int runFiredSoFar;

		private bool isInPostFireDelay;

		private enum TimeUnit
		{
			Seconds,
			Minutes,
			Hours
		}

		private enum RandomDistribution
		{
			Uniform,
			Normal,
			Exponential
		}

		private enum IntervalSource
		{
			LocalRandom,
			NetworkedRandom
		}

		private enum RunLength
		{
			Infinite,
			Finite
		}
	}
}
