using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public interface IProjectile
	{
		void Launch(Vector3 startPosition, Quaternion startRotation, Vector3 velocity, float chargeFrac, VRRig ownerRig, int progressStep = -1);
	}
}
