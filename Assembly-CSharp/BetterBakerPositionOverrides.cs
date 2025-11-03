using System;
using System.Collections.Generic;
using UnityEngine;

public class BetterBakerPositionOverrides : MonoBehaviour
{
	public List<BetterBakerPositionOverrides.OverridePosition> overridePositions;

	[Serializable]
	public struct OverridePosition
	{
		public GameObject go;

		public Transform bakingTransform;

		public Transform gameTransform;
	}
}
