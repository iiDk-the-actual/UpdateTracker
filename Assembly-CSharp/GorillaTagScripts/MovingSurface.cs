using System;
using GT_CustomMapSupportRuntime;
using UnityEngine;

namespace GorillaTagScripts
{
	[RequireComponent(typeof(Collider))]
	public class MovingSurface : MonoBehaviour
	{
		private void Start()
		{
			MovingSurfaceManager.instance == null;
			MovingSurfaceManager.instance.RegisterMovingSurface(this);
		}

		private void OnDestroy()
		{
			if (MovingSurfaceManager.instance != null)
			{
				MovingSurfaceManager.instance.UnregisterMovingSurface(this);
			}
		}

		public int GetID()
		{
			return this.uniqueId;
		}

		public void CopySettings(MovingSurfaceSettings movingSurfaceSettings)
		{
			this.uniqueId = movingSurfaceSettings.uniqueId;
		}

		[SerializeField]
		private int uniqueId = -1;
	}
}
