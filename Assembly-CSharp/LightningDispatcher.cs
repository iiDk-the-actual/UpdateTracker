using System;
using UnityEngine;

public class LightningDispatcher : MonoBehaviour
{
	public static event LightningDispatcher.DispatchLightningEvent RequestLightningStrike;

	public void DispatchLightning(Vector3 p1, Vector3 p2)
	{
		if (LightningDispatcher.RequestLightningStrike != null)
		{
			LightningStrike lightningStrike = LightningDispatcher.RequestLightningStrike(p1, p2);
			float num = Mathf.Max(new float[]
			{
				base.transform.lossyScale.x,
				base.transform.lossyScale.y,
				base.transform.lossyScale.z
			});
			lightningStrike.Play(p1, p2, this.beamWidthCM * 0.01f * num, this.soundVolumeMultiplier / num, LightningStrike.rand.NextFloat(this.minDuration, this.maxDuration), this.colorOverLifetime);
		}
	}

	[SerializeField]
	private float beamWidthCM = 1f;

	[SerializeField]
	private float soundVolumeMultiplier = 1f;

	[SerializeField]
	private float minDuration = 0.05f;

	[SerializeField]
	private float maxDuration = 0.12f;

	[SerializeField]
	private Gradient colorOverLifetime;

	public delegate LightningStrike DispatchLightningEvent(Vector3 p1, Vector3 p2);
}
