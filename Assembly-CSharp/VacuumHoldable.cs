using System;
using UnityEngine;

public class VacuumHoldable : TransferrableObject
{
	public override void OnSpawn(VRRig rig)
	{
		base.OnSpawn(rig);
		this.itemState = TransferrableObject.ItemStates.State0;
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		this.itemState = TransferrableObject.ItemStates.State0;
		this.hasAudioSource = this.audioSource != null && this.audioSource.clip != null;
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		this.itemState = TransferrableObject.ItemStates.State0;
		if (this.particleFX.isPlaying)
		{
			this.particleFX.Stop();
		}
		if (this.hasAudioSource && this.audioSource.isPlaying)
		{
			this.audioSource.GTStop();
		}
	}

	private void InitToDefault()
	{
		this.itemState = TransferrableObject.ItemStates.State0;
		if (this.particleFX.isPlaying)
		{
			this.particleFX.Stop();
		}
		if (this.hasAudioSource && this.audioSource.isPlaying)
		{
			this.audioSource.GTStop();
		}
	}

	public override void ResetToDefaultState()
	{
		base.ResetToDefaultState();
		this.InitToDefault();
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		if (!this.IsMyItem() && base.myOnlineRig != null && base.myOnlineRig.muted)
		{
			this.itemState = TransferrableObject.ItemStates.State0;
		}
		if (this.itemState == TransferrableObject.ItemStates.State0)
		{
			if (this.particleFX.isPlaying)
			{
				this.particleFX.Stop();
			}
			if (this.hasAudioSource && this.audioSource.isPlaying)
			{
				this.audioSource.GTStop();
				return;
			}
		}
		else
		{
			if (!this.particleFX.isEmitting)
			{
				this.particleFX.Play();
			}
			if (this.hasAudioSource && !this.audioSource.isPlaying)
			{
				this.audioSource.GTPlay();
			}
			if (this.IsMyItem() && Time.time > this.activationStartTime + this.activationVibrationStartDuration)
			{
				GorillaTagger.Instance.StartVibration(this.currentState == TransferrableObject.PositionState.InLeftHand, this.activationVibrationLoopStrength, Time.deltaTime);
			}
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		this.itemState = TransferrableObject.ItemStates.State1;
		if (this.IsMyItem())
		{
			this.activationStartTime = Time.time;
			GorillaTagger.Instance.StartVibration(this.currentState == TransferrableObject.PositionState.InLeftHand, this.activationVibrationStartStrength, this.activationVibrationStartDuration);
		}
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		this.itemState = TransferrableObject.ItemStates.State0;
	}

	[Tooltip("Emission rate will be increase when the trigger button is pressed.")]
	public ParticleSystem particleFX;

	[Tooltip("Sound will loop and fade in/out volume when trigger pressed.")]
	public AudioSource audioSource;

	private float activationVibrationStartStrength = 0.8f;

	private float activationVibrationStartDuration = 0.05f;

	private float activationVibrationLoopStrength = 0.005f;

	private float activationStartTime;

	private bool hasAudioSource;

	private enum VacuumState
	{
		None = 1,
		Active
	}
}
