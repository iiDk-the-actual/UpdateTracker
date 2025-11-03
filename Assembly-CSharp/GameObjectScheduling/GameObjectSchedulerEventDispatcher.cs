using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameObjectScheduling
{
	public class GameObjectSchedulerEventDispatcher : MonoBehaviour
	{
		public UnityEvent OnScheduledActivation
		{
			get
			{
				return this.onScheduledActivation;
			}
		}

		public UnityEvent OnScheduledDeactivation
		{
			get
			{
				return this.onScheduledDeactivation;
			}
		}

		[SerializeField]
		private UnityEvent onScheduledActivation;

		[SerializeField]
		private UnityEvent onScheduledDeactivation;
	}
}
