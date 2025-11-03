using System;
using UnityEngine;

[Serializable]
public struct MouthFlapLevel
{
	public Vector2[] faces;

	public float cycleDuration;

	public float minRequiredVolume;

	public float maxRequiredVolume;
}
