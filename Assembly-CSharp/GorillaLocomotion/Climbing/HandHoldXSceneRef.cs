using System;
using UnityEngine;

namespace GorillaLocomotion.Climbing
{
	public class HandHoldXSceneRef : MonoBehaviour
	{
		public HandHold target
		{
			get
			{
				HandHold handHold;
				if (this.reference.TryResolve<HandHold>(out handHold))
				{
					return handHold;
				}
				return null;
			}
		}

		public GameObject targetObject
		{
			get
			{
				GameObject gameObject;
				if (this.reference.TryResolve(out gameObject))
				{
					return gameObject;
				}
				return null;
			}
		}

		[SerializeField]
		public XSceneRef reference;
	}
}
