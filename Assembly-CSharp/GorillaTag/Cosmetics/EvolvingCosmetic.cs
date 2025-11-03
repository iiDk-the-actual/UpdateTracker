using System;
using System.Collections;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class EvolvingCosmetic : MonoBehaviour, ITickSystemTick
	{
		private int LoopMaxValue
		{
			get
			{
				return this.stages.Length;
			}
		}

		private void Awake()
		{
			base.gameObject.GetOrAddComponent(ref this.networkEvents);
			this.myRig = base.GetComponentInParent<VRRig>();
			for (int i = 0; i < this.stages.Length; i++)
			{
				this.totalDuration += this.stages[i].Duration;
				if (this.enableLooping)
				{
					if (i < this.loopToStageOnComplete - 1)
					{
						this.timeAtLoopStart += this.stages[i].Duration;
					}
					else
					{
						this.loopDuration += this.stages[i].Duration;
					}
				}
			}
		}

		private void OnEnable()
		{
			if (this.stages.Length == 0)
			{
				return;
			}
			NetPlayer netPlayer = this.myRig.creator ?? NetworkSystem.Instance.LocalPlayer;
			if (netPlayer != null)
			{
				this.networkEvents.Init(netPlayer);
				TickSystem<object>.AddTickCallback(this);
				NetworkSystem.Instance.OnPlayerJoined += this.SendElapsedTime;
				this.networkEvents.Activate += this.ReceiveElapsedTime;
				this.FirstStage();
				return;
			}
			Debug.LogError("Failed to get a reference to the Photon Player needed to hook up the cosmetic event");
		}

		private void OnDisable()
		{
			if (this.networkEvents != null)
			{
				TickSystem<object>.RemoveTickCallback(this);
				NetworkSystem.Instance.OnPlayerJoined -= this.SendElapsedTime;
				this.networkEvents.Activate -= this.ReceiveElapsedTime;
			}
			CallLimiter callLimiter = this.callLimiter;
			if (callLimiter == null)
			{
				return;
			}
			callLimiter.Reset();
		}

		private void Log(bool isComplete, bool isEvent)
		{
		}

		private void FirstStage()
		{
			this.activeStageIndex = 0;
			this.activeStage = this.stages[0];
			this.nextEventIndex = 0;
			this.nextEvent = this.activeStage.GetEventOrNull(0);
			this.totalElapsedTime = 0f;
			this.totalTimeOfPreviousStages = 0f;
			this.HandleStages();
		}

		private void HandleStages()
		{
			for (;;)
			{
				float num = this.totalElapsedTime - this.totalTimeOfPreviousStages;
				float num2 = Mathf.Min(num / this.activeStage.Duration, 1f);
				this.activeStage.continuousProperties.ApplyAll(num2);
				while (this.nextEvent != null && num >= this.nextEvent.absoluteTime)
				{
					UnityEvent onTimeReached = this.nextEvent.onTimeReached;
					if (onTimeReached != null)
					{
						onTimeReached.Invoke();
					}
					this.Log(false, true);
					EvolvingCosmetic.EvolutionStage evolutionStage = this.activeStage;
					int num3 = this.nextEventIndex + 1;
					this.nextEventIndex = num3;
					this.nextEvent = evolutionStage.GetEventOrNull(num3);
				}
				if (num < this.activeStage.Duration)
				{
					break;
				}
				this.activeStageIndex++;
				if (this.activeStageIndex >= this.stages.Length && !this.enableLooping)
				{
					goto Block_4;
				}
				if (this.activeStageIndex >= this.stages.Length)
				{
					this.activeStageIndex = this.loopToStageOnComplete - 1;
					this.totalTimeOfPreviousStages = this.timeAtLoopStart;
					this.totalElapsedTime -= this.loopDuration;
				}
				else
				{
					this.totalTimeOfPreviousStages += this.activeStage.Duration;
				}
				this.activeStage = this.stages[this.activeStageIndex];
				this.nextEventIndex = 0;
				this.nextEvent = this.activeStage.GetEventOrNull(0);
				if (!this.activeStage.HasDuration)
				{
					this.totalElapsedTime = this.totalTimeOfPreviousStages + this.activeStage.Duration * 0.5f;
					TickSystem<object>.RemoveTickCallback(this);
				}
				else
				{
					TickSystem<object>.AddTickCallback(this);
				}
				this.Log(false, false);
			}
			return;
			Block_4:
			this.totalElapsedTime = this.totalDuration;
			TickSystem<object>.RemoveTickCallback(this);
			this.Log(true, false);
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			this.totalElapsedTime = Mathf.Clamp(this.totalElapsedTime + this.activeStage.DeltaTime(Time.deltaTime), 0f, this.totalDuration * 1.01f);
			this.HandleStages();
		}

		public void CompleteManualStage()
		{
			if (!this.activeStage.HasDuration)
			{
				this.ForceNextStage();
			}
		}

		public void ForceNextStage()
		{
			this.totalElapsedTime = this.totalTimeOfPreviousStages + this.activeStage.Duration;
			this.HandleStages();
		}

		private void SendElapsedTime(NetPlayer player)
		{
			if (this.sendProgressDelayCoroutine != null)
			{
				base.StopCoroutine(this.sendProgressDelayCoroutine);
			}
			this.sendProgressDelayCoroutine = base.StartCoroutine(this.SendElapsedTimeDelayed());
		}

		private IEnumerator SendElapsedTimeDelayed()
		{
			yield return new WaitForSeconds(1f);
			this.sendProgressDelayCoroutine = null;
			this.networkEvents.Activate.RaiseOthers(new object[] { this.totalElapsedTime });
			yield break;
		}

		private void ReceiveElapsedTime(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "ReceiveElapsedTime");
			if (info.senderID == this.myRig.creator.ActorNumber && this.callLimiter.CheckCallServerTime((double)Time.unscaledTime) && args.Length == 1)
			{
				object obj = args[0];
				if (obj is float)
				{
					float num = (float)obj;
					if (float.IsFinite(num) && num <= this.totalDuration && num >= 0f)
					{
						this.totalElapsedTime = num;
						this.HandleStages();
						return;
					}
				}
			}
		}

		[SerializeField]
		private bool enableLooping;

		[SerializeField]
		private int loopToStageOnComplete = 1;

		[SerializeField]
		private EvolvingCosmetic.EvolutionStage[] stages;

		private RubberDuckEvents networkEvents;

		private VRRig myRig;

		private CallLimiter callLimiter = new CallLimiter(5, 10f, 0.5f);

		private int activeStageIndex;

		private EvolvingCosmetic.EvolutionStage activeStage;

		private int nextEventIndex;

		private EvolvingCosmetic.EvolutionStage.EventAtTime nextEvent;

		private float totalElapsedTime;

		private float totalTimeOfPreviousStages;

		private float totalDuration;

		private float timeAtLoopStart;

		private float loopDuration;

		private Coroutine sendProgressDelayCoroutine;

		[Serializable]
		private class EvolutionStage
		{
			private bool HasAnyFlag(EvolvingCosmetic.EvolutionStage.ProgressionFlags flag)
			{
				return (this.progressionFlags & flag) > EvolvingCosmetic.EvolutionStage.ProgressionFlags.None;
			}

			public bool HasDuration
			{
				get
				{
					return this.HasAnyFlag(EvolvingCosmetic.EvolutionStage.ProgressionFlags.Time | EvolvingCosmetic.EvolutionStage.ProgressionFlags.Temperature);
				}
			}

			public bool HasTime
			{
				get
				{
					return this.HasAnyFlag(EvolvingCosmetic.EvolutionStage.ProgressionFlags.Time);
				}
			}

			public bool HasTemperature
			{
				get
				{
					return this.HasAnyFlag(EvolvingCosmetic.EvolutionStage.ProgressionFlags.Temperature);
				}
			}

			public float Duration
			{
				get
				{
					if (!this.HasDuration)
					{
						return 1f;
					}
					return this.durationSeconds;
				}
			}

			public float DeltaTime(float deltaTime)
			{
				return (this.HasTime ? deltaTime : 0f) + (this.HasTemperature ? (deltaTime * this.celsiusSpeedupMult.Evaluate(this.thermalReceiver.celsius)) : 0f);
			}

			public EvolvingCosmetic.EvolutionStage.EventAtTime GetEventOrNull(int index)
			{
				if (this.events == null || index < 0 || index >= this.events.Length)
				{
					return null;
				}
				return this.events[index];
			}

			private const float MIN_STAGE_TIME = 0.01f;

			public string debugName;

			public EvolvingCosmetic.EvolutionStage.ProgressionFlags progressionFlags = EvolvingCosmetic.EvolutionStage.ProgressionFlags.Time;

			[SerializeField]
			private float durationSeconds = float.NaN;

			public ThermalReceiver thermalReceiver;

			public AnimationCurve celsiusSpeedupMult = AnimationCurve.Linear(0f, 0f, 100f, 2f);

			public ContinuousPropertyArray continuousProperties;

			[SerializeField]
			private EvolvingCosmetic.EvolutionStage.EventAtTime[] events;

			[Flags]
			public enum ProgressionFlags
			{
				None = 0,
				Time = 1,
				Temperature = 2
			}

			[Serializable]
			public class EventAtTime : IComparable<EvolvingCosmetic.EvolutionStage.EventAtTime>
			{
				private string DynamicTimeLabel
				{
					get
					{
						if (this.type != EvolvingCosmetic.EvolutionStage.EventAtTime.Type.DurationFraction)
						{
							return "Time";
						}
						return "Fraction";
					}
				}

				public int CompareTo(EvolvingCosmetic.EvolutionStage.EventAtTime other)
				{
					return this.absoluteTime.CompareTo(other.absoluteTime);
				}

				public string debugName;

				public float time;

				public EvolvingCosmetic.EvolutionStage.EventAtTime.Type type;

				public float absoluteTime;

				public UnityEvent onTimeReached;

				public enum Type
				{
					SecondsFromBeginning,
					SecondsBeforeEnd,
					DurationFraction
				}
			}
		}
	}
}
