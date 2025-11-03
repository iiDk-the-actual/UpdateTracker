using System;
using UnityEngine;

namespace GorillaTag.Sports
{
	public class SportGoalExitTrigger : MonoBehaviour
	{
		private void OnTriggerExit(Collider other)
		{
			SportBall componentInParent = other.GetComponentInParent<SportBall>();
			if (componentInParent != null && this.goalTrigger != null)
			{
				this.goalTrigger.BallExitedGoalTrigger(componentInParent);
			}
		}

		[SerializeField]
		private SportGoalTrigger goalTrigger;
	}
}
