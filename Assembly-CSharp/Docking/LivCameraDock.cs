using System;
using Liv.Lck.GorillaTag;

namespace Docking
{
	public class LivCameraDock : Dock
	{
		private void Reset()
		{
			this.cameraSettings.fov = 80f;
		}

		private void OnValidate()
		{
			if (this.cameraSettings.forceFov && (this.cameraSettings.fov < 30f || this.cameraSettings.fov > 110f))
			{
				this.cameraSettings.fov = 80f;
			}
		}

		public GtCameraDockSettings cameraSettings;
	}
}
