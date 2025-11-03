using System;
using UnityEngine;

public class HoverboardAudio : MonoBehaviour
{
	private void Start()
	{
		this.Stop();
	}

	public void PlayTurnSound(float angle)
	{
		if (Time.time > this.turnSoundCooldownUntilTimestamp && angle > this.minAngleDeltaForTurnSound)
		{
			this.turnSoundCooldownUntilTimestamp = Time.time + this.turnSoundCooldownDuration;
			this.turnSounds.Play();
		}
	}

	public void UpdateAudioLoop(float speed, float airspeed, float strainLevel, float grindLevel)
	{
		this.motorAnimator.UpdateValue(speed, false);
		this.windRushAnimator.UpdateValue(airspeed, false);
		if (grindLevel > 0f)
		{
			this.grindAnimator.UpdatePitchAndVolume(speed, grindLevel + 0.5f, false);
		}
		else
		{
			this.grindAnimator.UpdatePitchAndVolume(0f, 0f, false);
		}
		strainLevel = Mathf.Clamp01(strainLevel * 10f);
		if (!this.didInitHum1BaseVolume)
		{
			this.hum1BaseVolume = this.hum1.volume;
			this.didInitHum1BaseVolume = true;
		}
		this.hum1.volume = Mathf.MoveTowards(this.hum1.volume, this.hum1BaseVolume * strainLevel, this.fadeSpeed * Time.deltaTime);
	}

	public void Stop()
	{
		if (!this.didInitHum1BaseVolume)
		{
			this.hum1BaseVolume = this.hum1.volume;
			this.didInitHum1BaseVolume = true;
		}
		this.hum1.volume = 0f;
		this.windRushAnimator.UpdateValue(0f, true);
		this.motorAnimator.UpdateValue(0f, true);
		this.grindAnimator.UpdateValue(0f, true);
	}

	[SerializeField]
	private AudioSource hum1;

	[SerializeField]
	private SoundBankPlayer turnSounds;

	private bool didInitHum1BaseVolume;

	private float hum1BaseVolume;

	[SerializeField]
	private float fadeSpeed;

	[SerializeField]
	private AudioAnimator windRushAnimator;

	[SerializeField]
	private AudioAnimator motorAnimator;

	[SerializeField]
	private AudioAnimator grindAnimator;

	[SerializeField]
	private float turnSoundCooldownDuration;

	[SerializeField]
	private float minAngleDeltaForTurnSound;

	private float turnSoundCooldownUntilTimestamp;
}
