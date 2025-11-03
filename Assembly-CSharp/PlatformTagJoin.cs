using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatformTagJoin", menuName = "ScriptableObjects/PlatformTagJoin", order = 0)]
public class PlatformTagJoin : ScriptableObject
{
	public override string ToString()
	{
		return this.PlatformTag;
	}

	public string PlatformTag = " ";
}
