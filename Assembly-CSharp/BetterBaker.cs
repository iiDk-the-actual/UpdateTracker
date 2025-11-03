using System;
using UnityEngine;

public class BetterBaker : MonoBehaviour
{
	public string bakeryLightmapDirectory;

	public string dayNightLightmapsDirectory;

	public GameObject[] allLights;

	public struct LightMapMap
	{
		public string timeOfDayName;

		public GameObject lightObject;
	}
}
