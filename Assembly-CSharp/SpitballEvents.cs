using System;
using UnityEngine;

public class SpitballEvents : SubEmitterListener
{
	protected override void OnSubEmit()
	{
		base.OnSubEmit();
		if (this._audioSource && this._sfxHit)
		{
			this._audioSource.GTPlayOneShot(this._sfxHit, 1f);
		}
	}

	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private AudioClip _sfxHit;
}
