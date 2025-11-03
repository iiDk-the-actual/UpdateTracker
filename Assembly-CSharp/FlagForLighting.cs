using System;
using UnityEngine;

public class FlagForLighting : MonoBehaviour
{
	public FlagForLighting.TimeOfDay myTimeOfDay;

	public enum TimeOfDay
	{
		Sunrise,
		TenAM,
		Noon,
		ThreePM,
		Sunset,
		Night,
		RainingDay,
		RainingNight,
		None
	}
}
