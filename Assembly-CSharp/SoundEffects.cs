using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
	public bool isPlaying
	{
		get
		{
			return this._lastClipIndex >= 0 && this._lastClipLength >= 0.0 && this._lastClipElapsedTime < this._lastClipLength;
		}
	}

	public void Clear()
	{
		this.audioClips.Clear();
		this._lastClipIndex = -1;
		this._lastClipLength = -1.0;
	}

	public void Stop()
	{
		if (this.source)
		{
			this.source.GTStop();
		}
		this._lastClipLength = -1.0;
	}

	public void PlayNext(float delayMin, float delayMax, float volMin, float volMax)
	{
		float num = this._rnd.NextFloat(delayMin, delayMax);
		float num2 = this._rnd.NextFloat(volMin, volMax);
		this.PlayNext(num, num2);
	}

	public void PlayNext(float delay = 0f, float volume = 1f)
	{
		if (!this.source)
		{
			return;
		}
		if (this.audioClips == null || this.audioClips.Count == 0)
		{
			return;
		}
		if (this.source.isPlaying)
		{
			this.source.GTStop();
		}
		int num = this._rnd.NextInt(this.audioClips.Count);
		while (this.distinct && this._lastClipIndex == num)
		{
			num = this._rnd.NextInt(this.audioClips.Count);
		}
		AudioClip audioClip = this.audioClips[num];
		this._lastClipIndex = num;
		this._lastClipLength = (double)audioClip.length;
		float num2 = delay;
		if (num2 < this._minDelay)
		{
			num2 = this._minDelay;
		}
		if (num2 < 0.0001f)
		{
			this.source.GTPlayOneShot(audioClip, volume);
			this._lastClipElapsedTime = 0f;
			return;
		}
		this.source.clip = audioClip;
		this.source.volume = volume;
		this.source.GTPlayDelayed(num2);
		this._lastClipElapsedTime = -num2;
	}

	[Conditional("UNITY_EDITOR")]
	private void OnValidate()
	{
		if (string.IsNullOrEmpty(this.seed))
		{
			this.seed = "0x1337C0D3";
		}
		this._rnd = new SRand(this.seed);
		if (this.audioClips == null)
		{
			this.audioClips = new List<AudioClip>();
		}
	}

	public AudioSource source;

	[Space]
	public List<AudioClip> audioClips = new List<AudioClip>();

	public string seed = "0x1337C0D3";

	[Space]
	public bool distinct = true;

	[SerializeField]
	private float _minDelay;

	[Space]
	[SerializeField]
	private SRand _rnd;

	[NonSerialized]
	private int _lastClipIndex = -1;

	[NonSerialized]
	private double _lastClipLength = -1.0;

	[NonSerialized]
	private TimeSince _lastClipElapsedTime;
}
