using System;
using UnityEngine;

[Serializable]
public struct KeyValueStringPair
{
	public KeyValueStringPair(string key, string value)
	{
		this.Key = key;
		this.Value = value;
	}

	public string Key;

	[Multiline]
	public string Value;
}
