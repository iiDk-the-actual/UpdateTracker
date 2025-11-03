using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioMixVar
{
	public float value
	{
		get
		{
			if (!this.group)
			{
				return 0f;
			}
			if (!this.mixer)
			{
				return 0f;
			}
			float num;
			if (!this.mixer.GetFloat(this.name, out num))
			{
				return 0f;
			}
			return num;
		}
		set
		{
			if (this.mixer)
			{
				this.mixer.SetFloat(this.name, value);
			}
		}
	}

	public void ReturnToPool()
	{
		if (this._pool != null)
		{
			this._pool.Return(this);
		}
	}

	public AudioMixerGroup group;

	public AudioMixer mixer;

	public string name;

	[NonSerialized]
	public bool taken;

	[SerializeField]
	private AudioMixVarPool _pool;
}
