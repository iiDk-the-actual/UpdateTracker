using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GTEnumValueMap<T> : ISerializationCallbackReceiver
{
	public bool TryGet(long i, out T o)
	{
		return this._enumValue_to_unityObject.TryGetValue(i, out o);
	}

	public IEnumerable<T> Values
	{
		get
		{
			return this._enumValue_to_unityObject.Values;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		this.Init();
	}

	public void Init()
	{
		if (this.m_enumValueAndUnityObjectPairs == null)
		{
			return;
		}
		if (this._enumValue_to_unityObject == null)
		{
			this._enumValue_to_unityObject = new Dictionary<long, T>();
		}
		this._enumValue_to_unityObject.Clear();
		foreach (GTEnumValueMap<T>.EnumValueToUnityObject enumValueToUnityObject in this.m_enumValueAndUnityObjectPairs)
		{
			if (enumValueToUnityObject.enabled && enumValueToUnityObject.value != null)
			{
				this._enumValue_to_unityObject[enumValueToUnityObject.enumKey] = enumValueToUnityObject.value;
			}
		}
		if (!Application.isEditor)
		{
			this.m_enumScriptGuid = null;
			this.m_enumValueAndUnityObjectPairs = null;
		}
	}

	[Tooltip("The GUID to the Enum script asset which is what is serialized in editor (not used at runtime). This is exposed and editable as a precaution but shouldn't be necessary to have to use.")]
	[SerializeField]
	private string m_enumScriptGuid;

	[SerializeField]
	private List<GTEnumValueMap<T>.EnumValueToUnityObject> m_enumValueAndUnityObjectPairs = new List<GTEnumValueMap<T>.EnumValueToUnityObject>();

	private Dictionary<long, T> _enumValue_to_unityObject = new Dictionary<long, T>();

	[Serializable]
	private struct EnumValueToUnityObject
	{
		public bool enabled;

		public long enumKey;

		public string enumName;

		public T value;
	}
}
