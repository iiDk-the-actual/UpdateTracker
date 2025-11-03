using System;
using UnityEngine;

public class BetterBakerSettings : MonoBehaviour
{
	[SerializeField]
	public GameObject[] lightMapMaps = new GameObject[9];

	[Serializable]
	public struct LightMapMap
	{
		[SerializeField]
		public string timeOfDayName;

		[SerializeField]
		public GameObject sceneLightObject;
	}
}
