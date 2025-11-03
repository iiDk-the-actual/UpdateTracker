using System;

internal class CircularBuffer<T>
{
	public int Count { get; private set; }

	public int Capacity { get; private set; }

	public CircularBuffer(int capacity)
	{
		this.backingArray = new T[capacity];
		this.Capacity = capacity;
		this.Count = 0;
	}

	public void Add(T value)
	{
		this.backingArray[this.nextWriteIdx] = value;
		this.lastWriteIdx = this.nextWriteIdx;
		this.nextWriteIdx = (this.nextWriteIdx + 1) % this.Capacity;
		if (this.Count < this.Capacity)
		{
			int count = this.Count;
			this.Count = count + 1;
		}
	}

	public void Clear()
	{
		this.Count = 0;
	}

	public T Last()
	{
		return this.backingArray[this.lastWriteIdx];
	}

	public T this[int logicalIdx]
	{
		get
		{
			if (logicalIdx < 0 || logicalIdx >= this.Count)
			{
				throw new ArgumentOutOfRangeException("logicalIdx", logicalIdx, string.Format("Out of bounds index {0} into CircularBuffer with length {1}", logicalIdx, this.Count));
			}
			int num = (this.lastWriteIdx + this.Capacity - logicalIdx) % this.Capacity;
			return this.backingArray[num];
		}
	}

	private T[] backingArray;

	private int nextWriteIdx;

	private int lastWriteIdx;
}
