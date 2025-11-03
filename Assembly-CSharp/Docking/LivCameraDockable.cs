using System;
using Liv.Lck.GorillaTag;
using UnityEngine;

namespace Docking
{
	public class LivCameraDockable : Dockable
	{
		protected override void OnTriggerEnter(Collider other)
		{
			LivCameraDock livCameraDock;
			if (other.TryGetComponent<LivCameraDock>(out livCameraDock))
			{
				this.livDock = livCameraDock;
				this.potentialDock = other.transform;
			}
		}

		protected override void OnTriggerExit(Collider other)
		{
			if (this.livDock != null && other.transform == this.potentialDock.transform)
			{
				this.potentialDock = null;
				this.livDock = null;
			}
		}

		public override void Dock()
		{
			base.Dock();
			if (this.livDock == null)
			{
				return;
			}
			GTLckController gtlckController = base.GetComponent<GTLckController>() ?? base.GetComponentInParent<GTLckController>();
			if (gtlckController != null)
			{
				gtlckController.ApplyCameraSettings(this.livDock.cameraSettings);
			}
			this.livDock = null;
		}

		private LivCameraDock livDock;
	}
}
