using System;
using UnityEngine;

[Serializable]
public struct GTOption<T>
{
	public T ResolvedValue
	{
		get
		{
			if (!this.enabled)
			{
				return this.defaultValue;
			}
			return this.value;
		}
	}

	public GTOption(T defaultValue)
	{
		this.enabled = false;
		this.value = defaultValue;
		this.defaultValue = defaultValue;
	}

	public void ResetValue()
	{
		this.value = this.defaultValue;
	}

	[Tooltip("When checked, the filter is applied; when unchecked (default), it is ignored.")]
	[SerializeField]
	public bool enabled;

	[SerializeField]
	public T value;

	[NonSerialized]
	public readonly T defaultValue;
}
