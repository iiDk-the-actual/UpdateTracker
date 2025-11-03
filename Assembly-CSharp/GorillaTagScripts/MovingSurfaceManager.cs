using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class MovingSurfaceManager : MonoBehaviour
	{
		private void Awake()
		{
			if (MovingSurfaceManager.instance != null && MovingSurfaceManager.instance != this)
			{
				GTDev.LogWarning<string>("Instance of MovingSurfaceManager already exists. Destroying.", null);
				Object.Destroy(this);
				return;
			}
			if (MovingSurfaceManager.instance == null)
			{
				MovingSurfaceManager.instance = this;
			}
		}

		public void RegisterMovingSurface(MovingSurface ms)
		{
			this.movingSurfaces.TryAdd(ms.GetID(), ms);
		}

		public void UnregisterMovingSurface(MovingSurface ms)
		{
			this.movingSurfaces.Remove(ms.GetID());
		}

		public void RegisterSurfaceMover(SurfaceMover sm)
		{
			if (!this.surfaceMovers.Contains(sm))
			{
				this.surfaceMovers.Add(sm);
				sm.InitMovingSurface();
			}
		}

		public void UnregisterSurfaceMover(SurfaceMover sm)
		{
			this.surfaceMovers.Remove(sm);
		}

		public bool TryGetMovingSurface(int id, out MovingSurface result)
		{
			return this.movingSurfaces.TryGetValue(id, out result) && result != null;
		}

		private void FixedUpdate()
		{
			foreach (SurfaceMover surfaceMover in this.surfaceMovers)
			{
				if (surfaceMover.isActiveAndEnabled)
				{
					surfaceMover.Move();
				}
			}
		}

		private List<SurfaceMover> surfaceMovers = new List<SurfaceMover>(5);

		private Dictionary<int, MovingSurface> movingSurfaces = new Dictionary<int, MovingSurface>(10);

		public static MovingSurfaceManager instance;
	}
}
