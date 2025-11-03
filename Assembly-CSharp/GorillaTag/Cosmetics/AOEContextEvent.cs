using System;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[Serializable]
	public class AOEContextEvent : UnityEvent<AOEReceiver.AOEContext>
	{
	}
}
