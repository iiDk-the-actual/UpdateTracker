using System;
using System.Collections;
using UnityEngine;

public class CrittersNoiseMaker : CrittersToolThrowable
{
	protected override void OnImpact(Vector3 hitPosition, Vector3 hitNormal)
	{
		if (CrittersManager.instance.LocalAuthority())
		{
			if (this.destroyOnImpact || this.playOnce)
			{
				this.PlaySingleNoise();
				return;
			}
			this.StartPlayingRepeatNoise();
		}
	}

	protected override void OnImpactCritter(CrittersPawn impactedCritter)
	{
		this.OnImpact(impactedCritter.transform.position, impactedCritter.transform.up);
	}

	protected override void OnPickedUp()
	{
		this.StopPlayRepeatNoise();
	}

	private void PlaySingleNoise()
	{
		CrittersLoudNoise crittersLoudNoise = (CrittersLoudNoise)CrittersManager.instance.SpawnActor(CrittersActor.CrittersActorType.LoudNoise, this.soundSubIndex);
		if (crittersLoudNoise == null)
		{
			return;
		}
		crittersLoudNoise.MoveActor(base.transform.position, base.transform.rotation, false, true, true);
		crittersLoudNoise.SetImpulseVelocity(Vector3.zero, Vector3.zero);
		CrittersManager.instance.TriggerEvent(CrittersManager.CritterEvent.NoiseMakerTriggered, this.actorId, base.transform.position);
	}

	private void StartPlayingRepeatNoise()
	{
		this.StopPlayRepeatNoise();
		this.repeatPlayNoise = base.StartCoroutine(this.PlayRepeatNoise());
	}

	private void StopPlayRepeatNoise()
	{
		if (this.repeatPlayNoise != null)
		{
			base.StopCoroutine(this.repeatPlayNoise);
			this.repeatPlayNoise = null;
		}
	}

	private IEnumerator PlayRepeatNoise()
	{
		int num = Mathf.FloorToInt(this.repeatNoiseDuration / this.repeatNoiseRate);
		int num2;
		for (int i = num; i > 0; i = num2 - 1)
		{
			this.PlaySingleNoise();
			yield return new WaitForSeconds(this.repeatNoiseRate);
			num2 = i;
		}
		if (this.destroyAfterPlayingRepeatNoise)
		{
			this.shouldDisable = true;
		}
		yield break;
	}

	[Header("Noise Maker")]
	public int soundSubIndex = 3;

	public bool playOnce = true;

	public float repeatNoiseDuration;

	public float repeatNoiseRate;

	public bool destroyAfterPlayingRepeatNoise = true;

	private Coroutine repeatPlayNoise;
}
