using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New KeyValuePairSet", menuName = "Data/KeyValuePairSet", order = 0)]
public class KeyValuePairSet : ScriptableObject
{
	public KeyValueStringPair[] Entries
	{
		get
		{
			return this.m_entries;
		}
	}

	[SerializeField]
	private KeyValueStringPair[] m_entries;
}
