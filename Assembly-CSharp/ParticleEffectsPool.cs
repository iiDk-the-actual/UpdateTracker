using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

public class ParticleEffectsPool : MonoBehaviour
{
	public void Awake()
	{
		this.OnPoolAwake();
		this.Setup();
	}

	protected virtual void OnPoolAwake()
	{
	}

	private void Setup()
	{
		this.MoveToSceneWorldRoot();
		this._pools = new RingBuffer<ParticleEffect>[this.effects.Length];
		this._effectToPool = new Dictionary<long, int>(this.effects.Length);
		for (int i = 0; i < this.effects.Length; i++)
		{
			ParticleEffect particleEffect = this.effects[i];
			this._pools[i] = this.InitPoolForPrefab(i, this.effects[i]);
			this._effectToPool.TryAdd(particleEffect.effectID, i);
		}
	}

	private void MoveToSceneWorldRoot()
	{
		Transform transform = base.transform;
		transform.parent = null;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}

	private RingBuffer<ParticleEffect> InitPoolForPrefab(int index, ParticleEffect prefab)
	{
		RingBuffer<ParticleEffect> ringBuffer = new RingBuffer<ParticleEffect>(this.poolSize);
		string text = prefab.name.Trim();
		for (int i = 0; i < this.poolSize; i++)
		{
			ParticleEffect particleEffect = Object.Instantiate<ParticleEffect>(prefab, base.transform);
			particleEffect.gameObject.SetActive(false);
			particleEffect.pool = this;
			particleEffect.poolIndex = index;
			particleEffect.name = ZString.Concat<string, string, int>(text, "*", i);
			ringBuffer.Push(particleEffect);
		}
		return ringBuffer;
	}

	public void PlayEffect(ParticleEffect effect, Vector3 worldPos)
	{
		this.PlayEffect(effect.effectID, worldPos);
	}

	public void PlayEffect(ParticleEffect effect, Vector3 worldPos, float delay)
	{
		this.PlayEffect(effect.effectID, worldPos, delay);
	}

	public void PlayEffect(long effectID, Vector3 worldPos)
	{
		this.PlayEffect(this.GetPoolIndex(effectID), worldPos);
	}

	public void PlayEffect(long effectID, Vector3 worldPos, float delay)
	{
		this.PlayEffect(this.GetPoolIndex(effectID), worldPos, delay);
	}

	public void PlayEffect(int index, Vector3 worldPos)
	{
		if (index == -1)
		{
			return;
		}
		ParticleEffect particleEffect;
		if (!this._pools[index].TryPop(out particleEffect))
		{
			return;
		}
		particleEffect.transform.localPosition = worldPos;
		particleEffect.Play();
	}

	public void PlayEffect(int index, Vector3 worldPos, float delay)
	{
		if (delay.Approx(0f, 1E-06f))
		{
			this.PlayEffect(index, worldPos);
			return;
		}
		base.StartCoroutine(this.PlayDelayed(index, worldPos, delay));
	}

	private IEnumerator PlayDelayed(int index, Vector3 worldPos, float delay)
	{
		yield return new WaitForSeconds(delay);
		this.PlayEffect(index, worldPos);
		yield break;
	}

	public void Return(ParticleEffect effect)
	{
		this._pools[effect.poolIndex].Push(effect);
	}

	public int GetPoolIndex(long effectID)
	{
		int num;
		if (this._effectToPool.TryGetValue(effectID, out num))
		{
			return num;
		}
		return -1;
	}

	public ParticleEffect[] effects = new ParticleEffect[0];

	[Space]
	public int poolSize = 10;

	[Space]
	private RingBuffer<ParticleEffect>[] _pools = new RingBuffer<ParticleEffect>[0];

	private Dictionary<long, int> _effectToPool = new Dictionary<long, int>();
}
