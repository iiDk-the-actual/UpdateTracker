using System;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorBlendShapeTarget
{
	public SkinnedMeshRenderer SkinnedMeshRenderer;

	public int BlendShapeIndex;

	[Tooltip("Blend shape weight at minimum loudness ")]
	public float minValue;

	[Tooltip("Blend shape weight at maximum loudness (use 100 for full weighting)\nA number higher than 100 can be used to have full weighting at lower voice loudness")]
	public float maxValue = 1f;

	public bool UseSmoothedLoudness;
}
