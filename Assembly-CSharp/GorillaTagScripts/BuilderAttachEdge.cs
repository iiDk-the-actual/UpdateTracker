using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public class BuilderAttachEdge : MonoBehaviour
	{
		private void Awake()
		{
			if (this.center == null)
			{
				this.center = base.transform;
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Transform transform = this.center;
			if (transform == null)
			{
				transform = base.transform;
			}
			Vector3 vector = transform.rotation * Vector3.right;
			Gizmos.DrawLine(transform.position - vector * this.length * 0.5f, transform.position + vector * this.length * 0.5f);
		}

		public Transform center;

		public float length;
	}
}
