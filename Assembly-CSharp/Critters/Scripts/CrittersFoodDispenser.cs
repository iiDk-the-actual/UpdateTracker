using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Critters.Scripts
{
	public class CrittersFoodDispenser : CrittersActor
	{
		public override void Initialize()
		{
			base.Initialize();
			this.heldByPlayer = false;
		}

		public override void GrabbedBy(CrittersActor grabbingActor, bool positionOverride = false, Quaternion localRotation = default(Quaternion), Vector3 localOffset = default(Vector3), bool disableGrabbing = false)
		{
			base.GrabbedBy(grabbingActor, positionOverride, localRotation, localOffset, disableGrabbing);
			this.heldByPlayer = grabbingActor.isOnPlayer;
		}

		protected override void RemoteGrabbedBy(CrittersActor grabbingActor)
		{
			base.RemoteGrabbedBy(grabbingActor);
			this.heldByPlayer = grabbingActor.isOnPlayer;
		}

		public override void Released(bool keepWorldPosition, Quaternion rotation = default(Quaternion), Vector3 position = default(Vector3), Vector3 impulseVelocity = default(Vector3), Vector3 impulseAngularVelocity = default(Vector3))
		{
			base.Released(keepWorldPosition, rotation, position, impulseVelocity, impulseAngularVelocity);
			this.heldByPlayer = false;
		}

		protected override void HandleRemoteReleased()
		{
			base.HandleRemoteReleased();
			this.heldByPlayer = false;
		}

		[FormerlySerializedAs("isHeldByPlayer")]
		public bool heldByPlayer;
	}
}
