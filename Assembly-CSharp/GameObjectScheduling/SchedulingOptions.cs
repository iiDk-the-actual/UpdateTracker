using System;
using UnityEngine;

namespace GameObjectScheduling
{
	[CreateAssetMenu(fileName = "New Options", menuName = "Game Object Scheduling/Options", order = 0)]
	public class SchedulingOptions : ScriptableObject
	{
		public DateTime DtDebugServerTime
		{
			get
			{
				return this.dtDebugServerTime.AddSeconds((double)(Time.time * this.timescale));
			}
		}

		[SerializeField]
		private string debugServerTime;

		[SerializeField]
		private DateTime dtDebugServerTime;

		[SerializeField]
		[Range(-60f, 3660f)]
		private float timescale = 1f;
	}
}
