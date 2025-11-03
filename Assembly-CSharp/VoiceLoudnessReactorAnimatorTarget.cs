using System;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorAnimatorTarget
{
	public Animator animator;

	public bool useSmoothedLoudness;

	public float animatorSpeedToLoudness = 1f;
}
