using System;
using System.Collections;
using System.Collections.Generic;

public class UniqueQueue<T> : IEnumerable<T>, IEnumerable
{
	public int Count
	{
		get
		{
			return this.queue.Count;
		}
	}

	public UniqueQueue()
	{
		this.queuedItems = new HashSet<T>();
		this.queue = new Queue<T>();
	}

	public UniqueQueue(int capacity)
	{
		this.queuedItems = new HashSet<T>(capacity);
		this.queue = new Queue<T>(capacity);
	}

	public void Clear()
	{
		this.queuedItems.Clear();
		this.queue.Clear();
	}

	public bool Enqueue(T item)
	{
		if (!this.queuedItems.Add(item))
		{
			return false;
		}
		this.queue.Enqueue(item);
		return true;
	}

	public T Dequeue()
	{
		T t = this.queue.Dequeue();
		this.queuedItems.Remove(t);
		return t;
	}

	public bool TryDequeue(out T item)
	{
		if (this.queue.Count < 1)
		{
			item = default(T);
			return false;
		}
		item = this.Dequeue();
		return true;
	}

	public T Peek()
	{
		return this.queue.Peek();
	}

	public bool Contains(T item)
	{
		return this.queuedItems.Contains(item);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return this.queue.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.queue.GetEnumerator();
	}

	private HashSet<T> queuedItems;

	private Queue<T> queue;
}
