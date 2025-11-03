using System;
using System.Linq;
using GorillaNetworking;
using UnityEngine;

public sealed class FlickerManager : MonoBehaviour
{
	private void Awake()
	{
		if (this.FlickerDurations.Length % 2 != 0)
		{
			Debug.LogWarning("FlickerManager should have an even number of steps; removing last entry.");
			this.FlickerDurations = this.FlickerDurations.Take(this.FlickerDurations.Length - 1).ToArray<float>();
		}
		if (this.FlickerDurations.Length == 0)
		{
			Debug.LogWarning("No flicker durations set for FlickerManager, disabling.");
			Object.Destroy(this);
			return;
		}
	}

	private void Update()
	{
		float serverTime = FlickerManager.GetServerTime();
		if (serverTime < this._nextFlickerTime)
		{
			return;
		}
		BetterDayNightManager.instance.AnimateLightFlash(this.LightmapIndex, this.FlickerFadeInDuration, this.FlickerDurations[this._flickerIndex], this.FlickerFadeOutDuration);
		this._nextFlickerTime = serverTime + this.FlickerDurations[this._flickerIndex + 1];
		this._flickerIndex = (this._flickerIndex + 2) % this.FlickerDurations.Length;
	}

	private static float GetServerTime()
	{
		return (float)(GorillaComputer.instance.GetServerTime() - GorillaComputer.instance.startupTime).TotalSeconds;
	}

	public float[] FlickerDurations;

	public float FlickerFadeInDuration;

	public float FlickerFadeOutDuration;

	public int LightmapIndex;

	private int _flickerIndex;

	private float _nextFlickerTime = float.MinValue;
}
