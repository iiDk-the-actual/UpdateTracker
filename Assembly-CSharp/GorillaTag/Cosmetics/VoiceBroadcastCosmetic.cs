using System;
using GorillaTag.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(LoudSpeakerActivator))]
	public class VoiceBroadcastCosmetic : MonoBehaviour, IGorillaSliceableSimple
	{
		private void Awake()
		{
			this.loudSpeaker = base.GetComponent<LoudSpeakerActivator>();
			this.animator = base.GetComponent<Animator>();
			this.talkAnimationTrigger = Animator.StringToHash(this.talkAnimationTriggerName);
			this.gsl = base.GetComponentInParent<GorillaSpeakerLoudness>();
		}

		public void SetWearable(VoiceBroadcastCosmeticWearable wearable)
		{
			this.wearable = wearable;
		}

		private void StartBroadcast()
		{
			this.loudSpeaker.StartLocalBroadcast();
			UnityEvent unityEvent = this.onStartListening;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this.wearable.OnCosmeticStartListening();
			this.lastSliceUpdateTime = Time.time;
			GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		}

		private void StopBroadcast()
		{
			this.loudSpeaker.StopLocalBroadcast();
			UnityEvent unityEvent = this.onStopListening;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this.wearable.OnCosmeticStopListening();
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		}

		public void OnEnable()
		{
			this.isListening = false;
			this.speakingTime = 0f;
		}

		public void OnDisable()
		{
			this.isListening = false;
			this.speakingTime = 0f;
			this.StopBroadcast();
		}

		public void SetListenState(bool listening)
		{
			if (this.isListening == listening || !base.enabled || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.isListening = listening;
			this.speakingTime = 0f;
			if (listening)
			{
				this.StartBroadcast();
				return;
			}
			this.StopBroadcast();
		}

		public void SliceUpdate()
		{
			float num = Time.time - this.lastSliceUpdateTime;
			this.lastSliceUpdateTime = Time.time;
			if (this.gsl != null && this.gsl.IsSpeaking && this.gsl.LoudnessNormalized >= this.minVolume)
			{
				this.speakingTime += num;
				if (this.speakingTime >= this.minSpeakingTime)
				{
					if (this.animator != null)
					{
						this.animator.SetTrigger(this.talkAnimationTrigger);
					}
					if (this.simpleAnimation != null && !this.simpleAnimation.isPlaying)
					{
						this.simpleAnimation.Play();
						return;
					}
				}
			}
			else
			{
				this.speakingTime = 0f;
			}
		}

		private void ResetToFirstFrame()
		{
			this.simpleAnimation.Rewind();
			this.simpleAnimation.Play();
			this.simpleAnimation.Sample();
			this.simpleAnimation.Stop();
		}

		public TalkingCosmeticType talkingCosmeticType;

		[Tooltip("How loud the Gorilla voice should be before detecting as talking.")]
		[SerializeField]
		public float minVolume = 0.1f;

		[Tooltip("How long the initial speaking section needs to last to trigger the talking animation.")]
		[SerializeField]
		public float minSpeakingTime = 0.15f;

		[SerializeField]
		private Animation simpleAnimation;

		[SerializeField]
		private string talkAnimationTriggerName;

		private int talkAnimationTrigger;

		private const string EVENTS = "Events";

		[SerializeField]
		private UnityEvent onStartListening;

		[SerializeField]
		private UnityEvent onStartSpeaking;

		[SerializeField]
		private UnityEvent onStopSpeaking;

		[SerializeField]
		private UnityEvent onStopListening;

		private float speakingTime;

		private bool isListening;

		private bool isSpeaking;

		private VoiceBroadcastCosmeticWearable wearable;

		private LoudSpeakerActivator loudSpeaker;

		private GorillaSpeakerLoudness gsl;

		private Animator animator;

		private float lastSliceUpdateTime;
	}
}
