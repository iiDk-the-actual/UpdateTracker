using System;
using UnityEngine;

namespace GameObjectScheduling
{
	[CreateAssetMenu(fileName = "New CountdownText Date", menuName = "Game Object Scheduling/CountdownText Date", order = 1)]
	public class CountdownTextDate : ScriptableObject
	{
		public string CountdownTo = "1/1/0001 00:00:00";

		public string FormatString = "{0} {1}";

		public string DefaultString = "";

		public int DaysThreshold = 365;
	}
}
