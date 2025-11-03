using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WatchableStringSO", menuName = "ScriptableObjects/WatchableStringSO")]
public class WatchableStringSO : ScriptableObject
{
	private string _value { get; set; }

	public string Value
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
			foreach (Action<string> action in this.callbacks)
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
			this.callbacks = new List<Action<string>>();
			this.enterPlayID = EnterPlayID.GetCurrent();
		}
	}

	public void AddCallback(Action<string> callback, bool shouldCallbackNow = false)
	{
		this.EnsureInitialized();
		this.callbacks.Add(callback);
		if (shouldCallbackNow)
		{
			string value = this._value;
			foreach (Action<string> action in this.callbacks)
			{
				action(value);
			}
		}
	}

	public void RemoveCallback(Action<string> callback)
	{
		this.EnsureInitialized();
		this.callbacks.Remove(callback);
	}

	public override string ToString()
	{
		return this.Value;
	}

	[TextArea]
	public string InitialValue;

	private EnterPlayID enterPlayID;

	private List<Action<string>> callbacks;
}
