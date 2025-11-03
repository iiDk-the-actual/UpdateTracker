using System;
using GorillaTag;

public class LoopingArray<T> : ObjectPoolEvents
{
	public int Length
	{
		get
		{
			return this.m_length;
		}
	}

	public int CurrentIndex
	{
		get
		{
			return this.m_currentIndex;
		}
	}

	public T this[int index]
	{
		get
		{
			return this.m_array[index];
		}
		set
		{
			this.m_array[index] = value;
		}
	}

	public LoopingArray()
		: this(0)
	{
	}

	public LoopingArray(int capicity)
	{
		this.m_length = capicity;
		this.m_array = new T[capicity];
		this.Clear();
	}

	public int AddAndIncrement(in T value)
	{
		int currentIndex = this.m_currentIndex;
		this.m_array[this.m_currentIndex] = value;
		this.m_currentIndex = (this.m_currentIndex + 1) % this.m_length;
		return currentIndex;
	}

	public int IncrementAndAdd(in T value)
	{
		this.m_currentIndex = (this.m_currentIndex + 1) % this.m_length;
		this.m_array[this.m_currentIndex] = value;
		return this.m_currentIndex;
	}

	public void Clear()
	{
		this.m_currentIndex = 0;
		for (int i = 0; i < this.m_array.Length; i++)
		{
			this.m_array[i] = default(T);
		}
	}

	void ObjectPoolEvents.OnTaken()
	{
		this.Clear();
	}

	void ObjectPoolEvents.OnReturned()
	{
	}

	private int m_length;

	private int m_currentIndex;

	private T[] m_array;

	public class Pool : ObjectPool<LoopingArray<T>>
	{
		private Pool(int amount)
			: base(amount)
		{
		}

		public Pool(int size, int amount)
			: this(size, amount, amount)
		{
		}

		public Pool(int size, int initialAmount, int maxAmount)
		{
			this.m_size = size;
			base.InitializePool(initialAmount, maxAmount);
		}

		public override LoopingArray<T> CreateInstance()
		{
			return new LoopingArray<T>(this.m_size);
		}

		private readonly int m_size;
	}
}
