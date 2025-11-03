using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GTSerializableDict<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver where TKey : IComparable<TKey>
{
	public void OnBeforeSerialize()
	{
		this._m_serializedEntries.Clear();
		foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
		{
			this._m_serializedEntries.Add(new GTSerializableKeyValue<TKey, TValue>(keyValuePair.Key, keyValuePair.Value));
		}
		this._m_serializedEntries.Sort((GTSerializableKeyValue<TKey, TValue> entry1, GTSerializableKeyValue<TKey, TValue> entry2) => entry1.k.CompareTo(entry2.k));
	}

	public void OnAfterDeserialize()
	{
		base.Clear();
		foreach (GTSerializableKeyValue<TKey, TValue> gtserializableKeyValue in this._m_serializedEntries)
		{
			try
			{
				base.Add(gtserializableKeyValue.k, gtserializableKeyValue.v);
			}
			catch (ArgumentException ex)
			{
				Debug.LogError("ERROR!!! GTSerializableDict: " + string.Format("Duplicate key found during deserialization: '{0}'. Ignoring duplicate. ", gtserializableKeyValue.k) + "Exception: " + ex.Message);
			}
		}
		this._m_serializedEntries.Clear();
	}

	[SerializeField]
	[HideInInspector]
	private List<GTSerializableKeyValue<TKey, TValue>> _m_serializedEntries = new List<GTSerializableKeyValue<TKey, TValue>>();
}
