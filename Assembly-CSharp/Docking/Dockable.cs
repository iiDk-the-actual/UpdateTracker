using System;
using UnityEngine;

namespace Docking
{
	public class Dockable : MonoBehaviour
	{
		protected virtual void OnTriggerEnter(Collider other)
		{
			Dock dock;
			if (other.TryGetComponent<Dock>(out dock))
			{
				this.potentialDock = other.transform;
			}
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (this.potentialDock == other.transform)
			{
				this.potentialDock = null;
			}
		}

		public virtual void Dock()
		{
			if (this.potentialDock == null)
			{
				return;
			}
			base.transform.position = this.potentialDock.position;
			base.transform.rotation = this.potentialDock.rotation;
			this.potentialDock = null;
		}

		protected Transform potentialDock;
	}
}
