using System;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	[CreateAssetMenu(fileName = "GorillaRopeSwingSettings", menuName = "ScriptableObjects/GorillaRopeSwingSettings", order = 0)]
	public class GorillaRopeSwingSettings : ScriptableObject
	{
		public float inheritVelocityMultiplier = 1f;

		public float frictionWhenNotHeld = 0.25f;
	}
}
