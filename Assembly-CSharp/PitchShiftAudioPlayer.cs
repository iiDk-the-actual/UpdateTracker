using System;
using UnityEngine;

public class PitchShiftAudioPlayer : MonoBehaviour
{
	private void Awake()
	{
		if (this._source == null)
		{
			this._source = base.GetComponent<AudioSource>();
		}
		if (this._pitch == null)
		{
			this._pitch = base.GetComponent<RangedFloat>();
		}
	}

	private void OnEnable()
	{
		this._pitchMixVars.Rent(out this._pitchMix);
		this._source.outputAudioMixerGroup = this._pitchMix.group;
	}

	private void OnDisable()
	{
		this._source.Stop();
		this._source.outputAudioMixerGroup = null;
		AudioMixVar pitchMix = this._pitchMix;
		if (pitchMix == null)
		{
			return;
		}
		pitchMix.ReturnToPool();
	}

	private void Update()
	{
		if (this.apply)
		{
			this.ApplyPitch();
		}
	}

	private void ApplyPitch()
	{
		this._pitchMix.value = this._pitch.curved;
	}

	public bool apply = true;

	[SerializeField]
	private AudioSource _source;

	[SerializeField]
	private AudioMixVarPool _pitchMixVars;

	[SerializeReference]
	private AudioMixVar _pitchMix;

	[SerializeField]
	private RangedFloat _pitch;
}
