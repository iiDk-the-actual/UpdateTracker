using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public class BuilderAttachPoint : MonoBehaviour
	{
		private void Awake()
		{
			if (this.center == null)
			{
				this.center = base.transform;
			}
		}

		public Transform center;
	}
}
