using System;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	internal interface IGorillaGrabable
	{
		string name { get; }

		bool MomentaryGrabOnly();

		bool CanBeGrabbed(GorillaGrabber grabber);

		void OnGrabbed(GorillaGrabber grabber, out Transform grabbedTransform, out Vector3 localGrabbedPosition);

		void OnGrabReleased(GorillaGrabber grabber);
	}
}
