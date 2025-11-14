using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class ContinuousPropertyTimeline : MonoBehaviour, ITickSystemTick
	{
		private bool IsBackward
		{
			get
			{
				return !this.IsForward;
			}
			set
			{
				this.IsForward = !value;
			}
		}

		private bool IsPaused
		{
			get
			{
				return !this.IsPlaying;
			}
			set
			{
				this.IsPlaying = !value;
			}
		}

		public void TimelinePlay()
		{
			this.IsPlaying = true;
			TickSystem<object>.AddTickCallback(this);
		}

		public void TimelinePause()
		{
			this.IsPaused = true;
			TickSystem<object>.RemoveTickCallback(this);
		}

		public void TimelineToggleDirection()
		{
			this.IsForward = !this.IsForward;
		}

		public void TimelineTogglePlay()
		{
			if (this.IsPlaying)
			{
				this.TimelinePause();
				return;
			}
			this.TimelinePlay();
		}

		public void TimelinePlayForward()
		{
			this.IsForward = true;
			this.TimelinePlay();
		}

		public void TimelinePlayBackward()
		{
			this.IsBackward = true;
			this.TimelinePlay();
		}

		public void TimelinePlayFromBeginning()
		{
			this.time = 0f;
			this.TimelinePlayForward();
			this.OnReachedBeginning();
		}

		public void TimelinePlayFromEnd()
		{
			this.time = this.durationSeconds;
			this.TimelinePlayBackward();
			this.OnReachedEnd();
		}

		public void TimelineScrubToTime(float t)
		{
			if (t <= 0f)
			{
				this.time = 0f;
				this.OnReachedBeginning();
				return;
			}
			if (t >= this.durationSeconds)
			{
				this.time = this.durationSeconds;
				this.OnReachedEnd();
				return;
			}
			this.time = t;
		}

		public void TimelineScrubToFraction(float f)
		{
			this.TimelineScrubToTime(f * this.durationSeconds);
		}

		public void TimelineSetDuration(float d)
		{
			this.durationSeconds = d;
			this.inverseDuration = 1f / this.durationSeconds;
			this.backwardDeltaMult = this.durationSeconds / this.backwardDuration;
		}

		public void TimelineSetBackwardDuration(float d)
		{
			this.separateBackwardDuration = true;
			this.backwardDuration = d;
			this.backwardDeltaMult = this.durationSeconds / this.backwardDuration;
		}

		private void Awake()
		{
			this.IsPlaying = this.startPlaying;
		}

		private void OnEnable()
		{
			this.inverseDuration = 1f / this.durationSeconds;
			this.backwardDeltaMult = this.durationSeconds / this.backwardDuration;
			this.events.InvokeAll(ContinuousPropertyTimeline.TimelineEvent.OnEnable);
			if (this.IsPlaying)
			{
				TickSystem<object>.AddTickCallback(this);
			}
		}

		private void OnDisable()
		{
			this.events.InvokeAll(ContinuousPropertyTimeline.TimelineEvent.OnDisable);
			TickSystem<object>.RemoveTickCallback(this);
		}

		private void OnReachedEnd()
		{
			if (this.IsForward)
			{
				switch (this.endBehavior)
				{
				case ContinuousPropertyTimeline.TimelineEndBehavior.Stop:
					this.TimelinePause();
					this.time = this.durationSeconds;
					break;
				case ContinuousPropertyTimeline.TimelineEndBehavior.Loop:
					this.TimelinePlayFromBeginning();
					break;
				case ContinuousPropertyTimeline.TimelineEndBehavior.PingPong:
					this.IsBackward = true;
					this.time = this.durationSeconds;
					break;
				}
			}
			this.continuousProperties.ApplyAll(1f);
			this.events.InvokeAll(ContinuousPropertyTimeline.TimelineEvent.OnReachedEnd);
		}

		private void OnReachedBeginning()
		{
			if (this.IsBackward)
			{
				switch (this.endBehavior)
				{
				case ContinuousPropertyTimeline.TimelineEndBehavior.Stop:
					this.TimelinePause();
					this.time = 0f;
					break;
				case ContinuousPropertyTimeline.TimelineEndBehavior.Loop:
					this.TimelinePlayFromEnd();
					break;
				case ContinuousPropertyTimeline.TimelineEndBehavior.PingPong:
					this.IsForward = true;
					this.time = 0f;
					break;
				}
			}
			this.continuousProperties.ApplyAll(0f);
			this.events.InvokeAll(ContinuousPropertyTimeline.TimelineEvent.OnReachedBeginning);
		}

		private void InBetween()
		{
			float num = this.time * this.inverseDuration;
			this.continuousProperties.ApplyAll(num);
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (this.IsForward)
			{
				this.time += Time.deltaTime;
				if (this.time >= this.durationSeconds)
				{
					this.OnReachedEnd();
					return;
				}
				this.InBetween();
				return;
			}
			else
			{
				this.time -= Time.deltaTime * this.backwardDeltaMult;
				if (this.time <= 0f)
				{
					this.OnReachedBeginning();
					return;
				}
				this.InBetween();
				return;
			}
		}

		[SerializeField]
		private float durationSeconds = 1f;

		[SerializeField]
		private float backwardDuration = 1f;

		[Tooltip("If true, the the timeline can move at a different speed when playing backwards.")]
		[SerializeField]
		private bool separateBackwardDuration;

		[Tooltip("When this object is enabled for the first time, should it immediately start playing from the beginning?")]
		[SerializeField]
		private bool startPlaying;

		[Tooltip("Determine what happens when the timeline reaches the end (or beginning while playing backwards).")]
		[SerializeField]
		private ContinuousPropertyTimeline.TimelineEndBehavior endBehavior;

		[SerializeField]
		private ContinuousPropertyArray continuousProperties;

		[SerializeField]
		private FlagEvents<ContinuousPropertyTimeline.TimelineEvent> events;

		private float time;

		private float inverseDuration;

		private float backwardDeltaMult;

		private bool IsForward = true;

		private bool IsPlaying;

		private enum TimelineEndBehavior
		{
			Stop,
			Loop,
			PingPong
		}

		[Flags]
		private enum TimelineEvent
		{
			OnReachedEnd = 1,
			OnReachedBeginning = 2,
			OnEnable = 4,
			OnDisable = 8
		}
	}
}
