using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class WhackAMoleManager : MonoBehaviour, IGorillaSliceableSimple
	{
		private void Awake()
		{
			WhackAMoleManager.instance = this;
			this.allGames.Clear();
		}

		public void OnEnable()
		{
			GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		}

		public void OnDisable()
		{
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		}

		public void SliceUpdate()
		{
			foreach (WhackAMole whackAMole in this.allGames)
			{
				whackAMole.InvokeUpdate();
			}
		}

		private void OnDestroy()
		{
			WhackAMoleManager.instance = null;
		}

		public void Register(WhackAMole whackAMole)
		{
			this.allGames.Add(whackAMole);
		}

		public void Unregister(WhackAMole whackAMole)
		{
			this.allGames.Remove(whackAMole);
		}

		public static WhackAMoleManager instance;

		public HashSet<WhackAMole> allGames = new HashSet<WhackAMole>();
	}
}
