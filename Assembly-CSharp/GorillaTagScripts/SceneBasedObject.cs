using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public class SceneBasedObject : MonoBehaviour
	{
		public bool IsLocalPlayerInScene()
		{
			return (ZoneManagement.instance.GetAllLoadedScenes().Count <= 1 || this.zone != GTZone.forest) && ZoneManagement.instance.IsSceneLoaded(this.zone);
		}

		public GTZone zone;
	}
}
