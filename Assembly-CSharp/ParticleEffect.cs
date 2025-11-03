using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleEffect : MonoBehaviour
{
	public long effectID
	{
		get
		{
			return this._effectID;
		}
	}

	public bool isPlaying
	{
		get
		{
			return this.system && this.system.isPlaying;
		}
	}

	public virtual void Play()
	{
		base.gameObject.SetActive(true);
		this.system.Play(true);
	}

	public virtual void Stop()
	{
		this.system.Stop(true);
		base.gameObject.SetActive(false);
	}

	private void OnParticleSystemStopped()
	{
		base.gameObject.SetActive(false);
		if (this.pool)
		{
			this.pool.Return(this);
		}
	}

	public ParticleSystem system;

	[SerializeField]
	private long _effectID;

	public ParticleEffectsPool pool;

	[NonSerialized]
	public int poolIndex = -1;
}
