using System;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	[CreateAssetMenu(fileName = "GorillaZiplineSettings", menuName = "ScriptableObjects/GorillaZiplineSettings", order = 0)]
	public class GorillaZiplineSettings : ScriptableObject
	{
		public float minSlidePitch = 0.5f;

		public float maxSlidePitch = 1f;

		public float minSlideVolume;

		public float maxSlideVolume = 0.2f;

		public float maxSpeed = 10f;

		public float gravityMulti = 1.1f;

		[Header("Friction")]
		public float friction = 0.25f;

		public float maxFriction = 1f;

		public float maxFrictionSpeed = 15f;
	}
}
