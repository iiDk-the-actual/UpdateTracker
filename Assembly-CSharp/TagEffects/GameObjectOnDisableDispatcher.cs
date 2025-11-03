using System;
using UnityEngine;

namespace TagEffects
{
	public class GameObjectOnDisableDispatcher : MonoBehaviour
	{
		public event GameObjectOnDisableDispatcher.OnDisabledEvent OnDisabled;

		private void OnDisable()
		{
			if (this.OnDisabled != null)
			{
				this.OnDisabled(this);
			}
		}

		public delegate void OnDisabledEvent(GameObjectOnDisableDispatcher me);
	}
}
