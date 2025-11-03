using System;
using UnityEngine;

namespace GorillaTag
{
	[CreateAssetMenu(fileName = "WatchableIntSO", menuName = "ScriptableObjects/WatchableIntSO")]
	public class WatchableIntSO : WatchableGenericSO<int>
	{
		private int currentValue
		{
			get
			{
				return base.Value;
			}
		}
	}
}
