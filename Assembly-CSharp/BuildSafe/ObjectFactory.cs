using System;
using UnityEngine;

namespace BuildSafe
{
	public static class ObjectFactory
	{
		public static event Action<Component> componentWasAdded
		{
			add
			{
			}
			remove
			{
			}
		}
	}
}
