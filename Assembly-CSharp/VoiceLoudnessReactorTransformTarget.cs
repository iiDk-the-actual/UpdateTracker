using System;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorTransformTarget
{
	public Vector3 Initial
	{
		get
		{
			return this.initial;
		}
		set
		{
			this.initial = value;
		}
	}

	public Transform transform;

	private Vector3 initial;

	public Vector3 Max = Vector3.one;

	public float Scale = 1f;

	public bool UseSmoothedLoudness;
}
