using System;
using System.Collections.Generic;

public class Watchable<T>
{
	public T value
	{
		get
		{
			return this._value;
		}
		set
		{
			T value2 = this._value;
			this._value = value;
			foreach (Action<T> action in this.callbacks)
			{
				action(value);
			}
		}
	}

	public Watchable()
	{
	}

	public Watchable(T initial)
	{
		this._value = initial;
	}

	public void AddCallback(Action<T> callback, bool shouldCallbackNow = false)
	{
		this.callbacks.Add(callback);
		if (shouldCallbackNow)
		{
			foreach (Action<T> action in this.callbacks)
			{
				action(this._value);
			}
		}
	}

	public void RemoveCallback(Action<T> callback)
	{
		this.callbacks.Remove(callback);
	}

	private T _value;

	private List<Action<T>> callbacks = new List<Action<T>>();
}
