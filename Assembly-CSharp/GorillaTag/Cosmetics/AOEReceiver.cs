using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class AOEReceiver : MonoBehaviour
	{
		public void ReceiveAOE(in AOEReceiver.AOEContext AOEContext)
		{
			if (!this.enabledForAOE)
			{
				return;
			}
			AOEContextEvent onAOEReceived = this.OnAOEReceived;
			if (onAOEReceived == null)
			{
				return;
			}
			onAOEReceived.Invoke(AOEContext);
		}

		public AOEContextEvent OnAOEReceived;

		[Tooltip("Quick toggle to disable receiving without disabling the GameObject.")]
		[SerializeField]
		private bool enabledForAOE = true;

		[Serializable]
		public struct AOEContext
		{
			public Vector3 origin;

			public float radius;

			public GameObject instigator;

			public float baseStrength;

			public float finalStrength;

			public float distance;

			public float normalizedDistance;
		}
	}
}
