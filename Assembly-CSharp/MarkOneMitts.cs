using System;
using UnityEngine;

public class MarkOneMitts : HandTapBehaviour, ITickSystemTick, IProximityEffectReceiver
{
	private void Awake()
	{
		this.leftMitt.Init();
		this.rightMitt.Init();
		this.rig = base.GetComponentInParent<VRRig>();
		this.vibrateController = this.vibrateController && this.rig.isOfflineVRRig;
		this.proximityEffect.AddReceiver(this);
	}

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void OnProximityCalculated(float distance, float alignment, float parallel)
	{
		float num = distance * alignment * parallel;
		if (num > 0.1f)
		{
			float num2 = this.proximitySpeedCurve.Evaluate(num);
			float num3 = this.proximitySpreadCurve.Evaluate(num);
			ParticleSystem.MinMaxCurve minMaxCurve = new ParticleSystem.MinMaxCurve(-num3, num3);
			this.StartFlame(this.leftMitt, num, num2, minMaxCurve);
			this.StartFlame(this.rightMitt, num, num2, minMaxCurve);
			if (this.vibrateController && this.vibrationStrengthMult > 0f)
			{
				GorillaTagger.Instance.StartVibration(true, this.vibrationStrengthMult * 0.5f * num, Time.deltaTime);
				GorillaTagger.Instance.StartVibration(false, this.vibrationStrengthMult * 0.5f * num, Time.deltaTime);
			}
			this.SetInterferenceAudio(true);
			float num4 = 1f - Mathf.Exp(-this.proximityAudioReactionSpeed * Time.deltaTime);
			this.proximityAudioSource.pitch = Mathf.Lerp(this.proximityAudioSource.pitch, this.proximityAudioPitch.Evaluate(num), num4);
			this.proximityAudioSource.volume = Mathf.Lerp(this.proximityAudioSource.volume, this.proximityAudioVolume.Evaluate(num), num4);
			return;
		}
		if (this.leftMitt.thermalSource.enabled || this.rightMitt.thermalSource.enabled)
		{
			this.leftMitt.flame.Stop();
			this.leftMitt.thermalSource.enabled = false;
			this.rightMitt.flame.Stop();
			this.rightMitt.thermalSource.enabled = false;
			this.SetInterferenceAudio(false);
		}
	}

	private void StartFlame(MarkOneMitts.Mitt mitt, float scale, float speed, ParticleSystem.MinMaxCurve xy)
	{
		if (!mitt.thermalSource.enabled)
		{
			mitt.flame.Play();
			mitt.thermalSource.enabled = true;
		}
		mitt.flameTransform.localScale = this.flameScale * scale * Vector3.one;
		mitt.flameMain.startSpeed = speed;
		mitt.flameForce.x = xy;
		mitt.flameForce.y = xy;
		mitt.thermalSource.celsius = this.heatMultiplier * scale;
	}

	private void RunTimer(MarkOneMitts.Mitt mitt, bool isLeftHand)
	{
		if (mitt.timer <= 0f)
		{
			return;
		}
		mitt.timer -= Time.deltaTime;
		if (mitt.timer <= 0f)
		{
			mitt.timer = 0f;
			mitt.flame.Stop();
			mitt.thermalSource.enabled = false;
			if (this.leftMitt.timer <= 0f && this.rightMitt.timer <= 0f)
			{
				this.proximityEffect.enabled = true;
				return;
			}
		}
		else
		{
			float num = mitt.lastTapStrength * mitt.timer;
			mitt.flameTransform.localScale = this.flameScale * num * Vector3.one;
			mitt.thermalSource.celsius = this.heatMultiplier * num;
			if (this.vibrateController)
			{
				GorillaTagger.Instance.StartVibration(isLeftHand, this.vibrationStrengthMult * num, 0.1f);
			}
		}
	}

	private void TryPlayProximityStartStopAudio(AudioClip clip, float volume)
	{
		if (this.proximityStartStopAudioSource.isPlaying)
		{
			return;
		}
		this.proximityStartStopAudioSource.clip = clip;
		this.proximityStartStopAudioSource.volume = volume;
		this.proximityStartStopAudioSource.Play();
	}

	private void SetInterferenceAudio(bool active)
	{
		if (this.proximityAudioSource.isPlaying == active)
		{
			return;
		}
		if (active)
		{
			this.TryPlayProximityStartStopAudio(this.proximityStartAudioClip, this.proximityStartAudioVolume);
			this.proximityAudioSource.Play();
			return;
		}
		this.TryPlayProximityStartStopAudio(this.proximityStopAudioClip, this.proximityStopAudioVolume);
		this.proximityAudioSource.Stop();
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		if (this.leftMitt.timer <= 0f && this.rightMitt.timer <= 0f)
		{
			TickSystem<object>.RemoveTickCallback(this);
			return;
		}
		this.RunTimer(this.leftMitt, true);
		this.RunTimer(this.rightMitt, false);
	}

	internal override void OnTap(HandEffectContext handContext)
	{
		float num = this.handSpeedToEffectStrength.Evaluate(handContext.Speed);
		if (num >= this.minEffectStrength)
		{
			TickSystem<object>.AddTickCallback(this);
			MarkOneMitts.Mitt mitt = (handContext.isLeftHand ? this.leftMitt : this.rightMitt);
			mitt.lastTapStrength = num;
			mitt.timer = this.flameTime;
			mitt.bursts[0].count = num * 10f;
			mitt.bursts[1].count = num * 5f;
			mitt.burst.emission.SetBursts(mitt.bursts);
			mitt.burstTransform.localScale = num * Vector3.one;
			this.StartFlame(mitt, num * this.flameScale * this.flameTime, this.flameSpeed, this.emptyParticleCurve);
			mitt.burst.Play();
			Keyframe[] keys = this.handSpeedToEffectStrength.keys;
			float value = keys[keys.Length - 1].value;
			handContext.soundPitch = Mathf.Clamp(value / num, 1f, 3f);
			this.proximityEffect.enabled = false;
			return;
		}
		handContext.soundFX = null;
	}

	[SerializeField]
	private MarkOneMitts.Mitt leftMitt;

	[SerializeField]
	private MarkOneMitts.Mitt rightMitt;

	[SerializeField]
	private ProximityEffect proximityEffect;

	[SerializeField]
	private AnimationCurve handSpeedToEffectStrength;

	[SerializeField]
	private float minEffectStrength = 0.5f;

	[SerializeField]
	private float flameScale = 3f;

	[SerializeField]
	private float flameTime = 0.5f;

	[SerializeField]
	private float flameSpeed = 5f;

	[SerializeField]
	private float heatMultiplier = 100f;

	[SerializeField]
	private AnimationCurve proximitySpeedCurve;

	[SerializeField]
	private AnimationCurve proximitySpreadCurve;

	[Space]
	[SerializeField]
	private bool vibrateController;

	[SerializeField]
	private float vibrationStrengthMult = 1f;

	[Space]
	[SerializeField]
	private AudioSource proximityAudioSource;

	[SerializeField]
	private AnimationCurve proximityAudioPitch;

	[SerializeField]
	private AnimationCurve proximityAudioVolume;

	[SerializeField]
	private float proximityAudioReactionSpeed = 0.2f;

	[Space]
	[SerializeField]
	private AudioSource proximityStartStopAudioSource;

	[SerializeField]
	private AudioClip proximityStartAudioClip;

	[SerializeField]
	private float proximityStartAudioVolume = 0.5f;

	[SerializeField]
	private AudioClip proximityStopAudioClip;

	[SerializeField]
	private float proximityStopAudioVolume = 0.5f;

	private VRRig rig;

	private ParticleSystem.MinMaxCurve emptyParticleCurve = new ParticleSystem.MinMaxCurve(0f);

	[Serializable]
	private class Mitt
	{
		public void Init()
		{
			this.bursts = new ParticleSystem.Burst[2];
			this.burst.emission.GetBursts(this.bursts);
			this.burstTransform = this.burst.transform;
			this.flameTransform = this.flame.transform;
			this.flameMain = this.flame.main;
			this.flameForce = this.flame.forceOverLifetime;
		}

		public ParticleSystem burst;

		public ParticleSystem flame;

		public ThermalSourceVolume thermalSource;

		[NonSerialized]
		public float lastTapStrength;

		[NonSerialized]
		public ParticleSystem.Burst[] bursts;

		[NonSerialized]
		public Transform burstTransform;

		[NonSerialized]
		public Transform flameTransform;

		[NonSerialized]
		public float timer;

		[NonSerialized]
		public ParticleSystem.MainModule flameMain;

		[NonSerialized]
		public ParticleSystem.ForceOverLifetimeModule flameForce;
	}
}
