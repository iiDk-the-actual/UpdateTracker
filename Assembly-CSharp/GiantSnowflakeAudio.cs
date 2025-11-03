using System;
using System.Collections.Generic;
using UnityEngine;

public class GiantSnowflakeAudio : MonoBehaviour
{
	private void Start()
	{
		foreach (GiantSnowflakeAudio.SnowflakeScaleOverride snowflakeScaleOverride in this.audioOverrides)
		{
			if (base.transform.lossyScale.x < snowflakeScaleOverride.scaleMax)
			{
				base.GetComponent<GorillaSurfaceOverride>().overrideIndex = snowflakeScaleOverride.newOverrideIndex;
			}
		}
	}

	public List<GiantSnowflakeAudio.SnowflakeScaleOverride> audioOverrides;

	[Serializable]
	public struct SnowflakeScaleOverride
	{
		public float scaleMax;

		[GorillaSoundLookup]
		public int newOverrideIndex;
	}
}
