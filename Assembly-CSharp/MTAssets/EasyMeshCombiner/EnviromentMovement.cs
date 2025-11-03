using System;
using UnityEngine;

namespace MTAssets.EasyMeshCombiner
{
	public class EnviromentMovement : MonoBehaviour
	{
		private void Start()
		{
			this.thisTransform = base.gameObject.GetComponent<Transform>();
			this.nextPosition = this.pos1;
		}

		private void Update()
		{
			if (Vector3.Distance(this.thisTransform.position, this.nextPosition) > 0.5f)
			{
				base.transform.position = Vector3.Lerp(this.thisTransform.position, this.nextPosition, 2f * Time.deltaTime);
				return;
			}
			if (this.nextPosition == this.pos1)
			{
				this.nextPosition = this.pos2;
				return;
			}
			if (this.nextPosition == this.pos2)
			{
				this.nextPosition = this.pos1;
				return;
			}
		}

		private Vector3 nextPosition = Vector3.zero;

		private Transform thisTransform;

		public Vector3 pos1;

		public Vector3 pos2;
	}
}
