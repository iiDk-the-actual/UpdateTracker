using System;
using UnityEngine;

namespace GorillaLocomotion.Climbing
{
	public class GorillaClimbable : MonoBehaviour
	{
		private void Awake()
		{
			this.colliderCache = base.GetComponent<Collider>();
		}

		public bool snapX;

		public bool snapY;

		public bool snapZ;

		public float maxDistanceSnap = 0.05f;

		public AudioClip clip;

		public AudioClip clipOnFullRelease;

		public Action<GorillaHandClimber, GorillaClimbableRef> onBeforeClimb;

		public bool climbOnlyWhileSmall;

		public bool IsPlayerAttached;

		[NonSerialized]
		public bool isBeingClimbed;

		[NonSerialized]
		public Collider colliderCache;
	}
}
