using System;
using UnityEngine;

namespace GorillaLocomotion.Swimming
{
	public class WaterSplashOverride : MonoBehaviour
	{
		public bool suppressWaterEffects;

		public bool playBigSplash;

		public bool playDrippingEffect = true;

		public bool scaleByPlayersScale;

		public bool overrideBoundingRadius;

		public float boundingRadiusOverride = 1f;
	}
}
