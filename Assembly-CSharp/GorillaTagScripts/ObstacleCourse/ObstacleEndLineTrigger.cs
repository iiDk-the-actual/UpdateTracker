using System;
using UnityEngine;

namespace GorillaTagScripts.ObstacleCourse
{
	public class ObstacleEndLineTrigger : MonoBehaviour
	{
		public event ObstacleEndLineTrigger.ObstacleCourseTriggerEvent OnPlayerTriggerEnter;

		private void OnTriggerEnter(Collider other)
		{
			VRRig vrrig;
			if (other.attachedRigidbody.gameObject.TryGetComponent<VRRig>(out vrrig))
			{
				ObstacleEndLineTrigger.ObstacleCourseTriggerEvent onPlayerTriggerEnter = this.OnPlayerTriggerEnter;
				if (onPlayerTriggerEnter == null)
				{
					return;
				}
				onPlayerTriggerEnter(vrrig);
			}
		}

		public delegate void ObstacleCourseTriggerEvent(VRRig vrrig);
	}
}
