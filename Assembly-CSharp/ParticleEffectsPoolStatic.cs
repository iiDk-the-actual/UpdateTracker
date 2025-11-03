using System;
using UnityEngine;

public class ParticleEffectsPoolStatic<T> : ParticleEffectsPool where T : ParticleEffectsPool
{
	public static T Instance
	{
		get
		{
			return ParticleEffectsPoolStatic<T>.gInstance;
		}
	}

	protected override void OnPoolAwake()
	{
		if (ParticleEffectsPoolStatic<T>.gInstance && ParticleEffectsPoolStatic<T>.gInstance != this)
		{
			Object.Destroy(this);
			return;
		}
		ParticleEffectsPoolStatic<T>.gInstance = this as T;
	}

	protected static T gInstance;
}
