using System;
using System.Collections.Generic;
using UnityEngine;

public class WatchableGenericSO<T> : ScriptableObject
{
	private T _value { get; set; }

	public T Value
	{
		get
		{
			this.EnsureInitialized();
			return this._value;
		}
		set
		{
			this.EnsureInitialized();
			this._value = value;
			foreach (Action<T> action in this.callbacks)
			{
				action(value);
			}
		}
	}

	private void EnsureInitialized()
	{
		if (!this.enterPlayID.IsCurrent)
		{
			this._value = this.InitialValue;
			this.callbacks = new List<Action<T>>();
			this.enterPlayID = EnterPlayID.GetCurrent();
		}
	}

	public void AddCallback(Action<T> callback, bool shouldCallbackNow = false)
	{
		this.EnsureInitialized();
		this.callbacks.Add(callback);
		if (shouldCallbackNow)
		{
			T value = this._value;
			foreach (Action<T> action in this.callbacks)
			{
				action(value);
			}
		}
	}

	public void RemoveCallback(Action<T> callback)
	{
		this.EnsureInitialized();
		this.callbacks.Remove(callback);
	}

	public T InitialValue;

	private EnterPlayID enterPlayID;

	private List<Action<T>> callbacks;
}
