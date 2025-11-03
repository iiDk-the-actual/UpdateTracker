using System;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorGameObjectEnableTarget
{
	public GameObject GameObject;

	public float Threshold;

	public bool TurnOnAtThreshhold = true;

	public bool UseSmoothedLoudness;

	public float Scale = 1f;
}
