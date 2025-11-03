using System;
using UnityEngine;

public abstract class RandomContainer<T> : ScriptableObject
{
	public T lastItem
	{
		get
		{
			return this._lastItem;
		}
	}

	public int lastItemIndex
	{
		get
		{
			return this._lastItemIndex;
		}
	}

	public void ResetRandom(int? seedValue = null)
	{
		if (!this.staticSeed)
		{
			this._seed = seedValue ?? StaticHash.Compute(DateTime.UtcNow.Ticks);
		}
		else
		{
			this._seed = this.seed;
		}
		this._rnd = new SRand(this._seed);
	}

	public void Reset()
	{
		this.ResetRandom(null);
		this._lastItem = default(T);
		this._lastItemIndex = -1;
	}

	private void Awake()
	{
		this.Reset();
	}

	public virtual T GetItem(int index)
	{
		return this.items[index];
	}

	public virtual T NextItem()
	{
		this._lastItemIndex = (this.distinct ? this._rnd.NextIntWithExclusion(0, this.items.Length, this._lastItemIndex) : this._rnd.NextInt(0, this.items.Length));
		T t = this.items[this._lastItemIndex];
		this._lastItem = t;
		return t;
	}

	public T[] items = new T[0];

	public int seed;

	public bool staticSeed;

	public bool distinct = true;

	[Space]
	[NonSerialized]
	private int _seed;

	[NonSerialized]
	private T _lastItem;

	[NonSerialized]
	private int _lastItemIndex = -1;

	[NonSerialized]
	private SRand _rnd;
}
