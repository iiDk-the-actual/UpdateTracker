using System;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorTransformRotationTarget
{
	public Quaternion Initial
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

	private Quaternion initial;

	public Quaternion Max = Quaternion.identity;

	public float Scale = 1f;

	public bool UseSmoothedLoudness;
}
