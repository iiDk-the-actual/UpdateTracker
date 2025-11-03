using System;
using UnityEngine;

public class ProximityEffectScoreCurvesSO : ScriptableObject
{
	[Tooltip("How far apart the transforms are. A distance. Contributes 'red' to the debug line. Y value should be in the range 0-1.")]
	public AnimationCurve distanceModifierCurve;

	[Tooltip("How closely the transforms' Z vectors are pointed towards each other. A dot product. Contributes 'green' to the debug line. Y value should be in the range 0-1.")]
	public AnimationCurve alignmentModifierCurve;

	[Tooltip("Whether each transform is in front of the other transform. The average of two dot products. Contributes 'blue' to the debug line. Y value should be in the range 0-1.")]
	public AnimationCurve parallelModifierCurve;
}
