using System;
using System.Collections.Generic;

public class RingBuffer<T>
{
	public int Size
	{
		get
		{
			return this._size;
		}
	}

	public int Capacity
	{
		get
		{
			return this._capacity;
		}
	}

	public bool IsFull
	{
		get
		{
			return this._size == this._capacity;
		}
	}

	public bool IsEmpty
	{
		get
		{
			return this._size == 0;
		}
	}

	public RingBuffer(int capacity)
	{
		if (capacity < 1)
		{
			throw new ArgumentException("Can't be zero or negative", "capacity");
		}
		this._size = 0;
		this._capacity = capacity;
		this._items = new T[capacity];
	}

	public RingBuffer(IList<T> list)
		: this(list.Count)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.CopyTo(this._items, 0);
	}

	public ref T PeekFirst()
	{
		return ref this._items[this._head];
	}

	public ref T PeekLast()
	{
		return ref this._items[this._tail];
	}

	public bool Push(T item)
	{
		if (this._size == this._capacity)
		{
			return false;
		}
		this._items[this._tail] = item;
		this._tail = (this._tail + 1) % this._capacity;
		this._size++;
		return true;
	}

	public T Pop()
	{
		if (this._size == 0)
		{
			return default(T);
		}
		T t = this._items[this._head];
		this._head = (this._head + 1) % this._capacity;
		this._size--;
		return t;
	}

	public bool TryPop(out T item)
	{
		if (this._size == 0)
		{
			item = default(T);
			return false;
		}
		item = this._items[this._head];
		this._head = (this._head + 1) % this._capacity;
		this._size--;
		return true;
	}

	public void Clear()
	{
		this._head = 0;
		this._tail = 0;
		this._size = 0;
		Array.Clear(this._items, 0, this._capacity);
	}

	public bool TryGet(int i, out T item)
	{
		if (this._size == 0)
		{
			item = default(T);
			return false;
		}
		item = this._items[this._head + i % this._size];
		return true;
	}

	public ArraySegment<T> AsSegment()
	{
		return new ArraySegment<T>(this._items);
	}

	private T[] _items;

	private int _head;

	private int _tail;

	private int _size;

	private readonly int _capacity;
}
